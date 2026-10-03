import * as THREE from './vendor/three.module.js';

const TEAM_TONES = ['#dff8e9', '#b9d3f2', '#f5c8a8', '#e0c8fa', '#d4e7bd', '#f3c5d2', '#eee8cf'];

export function createMatchView(canvas, getState) {
  const renderer = new THREE.WebGLRenderer({
    canvas,
    antialias: true,
    alpha: false,
    powerPreference: 'high-performance'
  });
  renderer.outputColorSpace = THREE.SRGBColorSpace;
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.12;
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFShadowMap;
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.45));

  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(43, 1, 0.1, 180);
  const lookTarget = new THREE.Vector3(0, 1.1, -2);
  const cameraModes = [
    { label: 'BROADCAST', position: new THREE.Vector3(0, 6.8, 18.8), target: new THREE.Vector3(0, 0.95, -1.7) },
    { label: 'SIDE CAM', position: new THREE.Vector3(16.5, 7.4, 1.5), target: new THREE.Vector3(0, 1.0, -1.3) },
    { label: 'BOWLER CAM', position: new THREE.Vector3(0, 4.4, -13.7), target: new THREE.Vector3(0, 1.0, 4) },
    { label: 'BATTER CAM', position: new THREE.Vector3(0, 4.8, 12.6), target: new THREE.Vector3(0, 1.2, -3.5) }
  ];
  camera.position.copy(cameraModes[0].position);
  camera.lookAt(cameraModes[0].target);
  let cameraIndex = 0;
  let lastVenue = '';
  let lastSky = '';
  let lastPitch = '';

  const stadium = new THREE.Group();
  scene.add(stadium);
  const grassMaterial = new THREE.MeshStandardMaterial({ color: '#287548', roughness: 1 });
  const pitchMaterial = new THREE.MeshStandardMaterial({ color: '#c8a97d', roughness: 1 });
  const pitch = new THREE.Mesh(new THREE.PlaneGeometry(3.4, 20), pitchMaterial);
  pitch.rotation.x = -Math.PI / 2;
  pitch.position.set(0, 0.022, -0.15);
  pitch.receiveShadow = true;
  stadium.add(pitch);

  const field = new THREE.Mesh(new THREE.PlaneGeometry(88, 80), grassMaterial);
  field.rotation.x = -Math.PI / 2;
  field.position.y = -0.08;
  field.receiveShadow = true;
  stadium.add(field);
  addMowingRings(stadium);
  addCreases(stadium);
  addBoundary(stadium);
  addStadiumBowl(stadium);
  addCrowd(stadium, window.matchMedia('(max-width: 650px)').matches ? 42 : 76);
  addFloodlights(stadium);
  addScoreboard(stadium);
  addBoundaryBoards(stadium);

  const hemi = new THREE.HemisphereLight('#cce8ff', '#264631', 2.15);
  scene.add(hemi);
  const sun = new THREE.DirectionalLight('#fff0d1', 3.25);
  sun.position.set(-11, 20, 9);
  sun.castShadow = true;
  sun.shadow.mapSize.set(1024, 1024);
  sun.shadow.camera.left = -18;
  sun.shadow.camera.right = 18;
  sun.shadow.camera.top = 22;
  sun.shadow.camera.bottom = -12;
  sun.shadow.bias = -0.0005;
  sun.target.position.set(0, 0, -1);
  scene.add(sun, sun.target);
  const fill = new THREE.DirectionalLight('#8ebcff', 0.75);
  fill.position.set(10, 10, -13);
  scene.add(fill);

  const stumps = makeWickets();
  const strikerWickets = stumps.group;
  strikerWickets.position.set(0, 0, 5.8);
  stadium.add(strikerWickets);
  const farWickets = makeWickets().group;
  farWickets.position.set(0, 0, -6.1);
  stadium.add(farWickets);

  const ball = makeBall();
  stadium.add(ball.group);
  ball.group.visible = false;
  const ballShadow = new THREE.Mesh(
    new THREE.CircleGeometry(0.32, 20),
    new THREE.MeshBasicMaterial({ color: '#06140c', transparent: true, opacity: 0.32, depthWrite: false })
  );
  ballShadow.rotation.x = -Math.PI / 2;
  ballShadow.position.y = 0.035;
  stadium.add(ballShadow);
  ballShadow.visible = false;

  const batter = makePlayer('#34b881', true);
  batter.root.position.set(0.65, 0, 4.5);
  stadium.add(batter.root);
  const bowler = makePlayer('#287ed2', false);
  bowler.root.position.set(0, 0, -5.3);
  bowler.root.rotation.y = Math.PI;
  stadium.add(bowler.root);
  const fielders = [
    [-6.8, 0, -11.2, 0.78], [6.9, 0, -10.8, 0.78], [-11.5, 0, 0.1, 0.76],
    [11.3, 0, -1.2, 0.76], [-5.2, 0, 10.4, 0.72], [5.2, 0, 10.1, 0.72]
  ].map(([x, y, z, scale], index) => {
    const player = makePlayer(TEAM_TONES[index % TEAM_TONES.length], false, true);
    player.root.scale.setScalar(scale);
    player.root.position.set(x, y, z);
    player.root.rotation.y = Math.atan2(-x, 7 - z);
    stadium.add(player.root);
    return player;
  });

  function resize() {
    const rect = canvas.getBoundingClientRect();
    const w = Math.max(1, Math.floor(rect.width));
    const h = Math.max(1, Math.floor(rect.height));
    renderer.setSize(w, h, false);
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
  }

  function cycleCamera() {
    cameraIndex = (cameraIndex + 1) % cameraModes.length;
    return cameraModes[cameraIndex].label;
  }

  function ballPosition(progress) {
    const state = getState();
    const p = THREE.MathUtils.clamp(progress, 0, 1);
    const type = state.deliveryType?.label || '';
    const bounceAt = type === 'YORKER' ? 0.84 : type === 'SHORT BALL' ? 0.68 : 0.74;
    let y;
    if (p < bounceAt) {
      const t = p / bounceAt;
      y = 2.15 * (1 - t) + 0.14 * t + Math.sin(Math.PI * t) * 0.42;
    } else {
      const t = (p - bounceAt) / (1 - bounceAt);
      const rebound = state.settings.pitch === 'bouncy' ? 0.86 : state.settings.pitch === 'green' ? 0.52 : 0.62;
      y = 0.13 + Math.sin(Math.PI * t) * rebound - t * 0.05;
    }
    const swingFactor = (state.deliveryType?.swing || 0.25) * (state.settings.weather === 'cloudy' ? 1.25 : 1);
    const x = Math.sin(p * Math.PI) * swingFactor * 0.44 + (state.aim || 0) * 0.035;
    return new THREE.Vector3(x, Math.max(0.1, y), -5.1 + p * 10.25);
  }

  function render(now, state) {
    syncEnvironment(state);
    syncTeams(state);
    const seconds = now * 0.001;
    const shotT = state.phase === 'shot' ? THREE.MathUtils.clamp((now - state.shotStarted) / 1250, 0, 1) : 0;
    const idle = Math.sin(seconds * 2.3);
    batter.root.position.x = 0.65 + (state.batterMove || 0) * 0.46;
    batter.root.position.y = Math.sin(seconds * 3.2) * 0.025;
    batter.leftArm.rotation.z = -0.16 + (state.phase === 'delivery' ? Math.sin(seconds * 6) * 0.025 : 0);
    batter.rightArm.rotation.z = state.phase === 'shot' ? THREE.MathUtils.lerp(-0.1, -2.55, Math.sin(shotT * Math.PI) ** 0.8) : -0.1;
    batter.rightArm.rotation.x = state.phase === 'shot' ? Math.sin(shotT * Math.PI) * -0.85 : 0;
    batter.bat.rotation.z = state.phase === 'shot' ? THREE.MathUtils.lerp(-0.2, 1.28, Math.sin(shotT * Math.PI) ** 0.8) : -0.2;
    bowler.root.position.y = state.phase === 'delivery' ? Math.abs(Math.sin(seconds * 3.8)) * 0.13 : idle * 0.018;
    bowler.rightArm.rotation.z = state.phase === 'delivery' ? -Math.sin(seconds * 5.2) * 1.2 : -0.18;
    fielders.forEach((player, i) => { player.root.position.y = Math.sin(seconds * 1.7 + i) * 0.018; });

    let worldBall = null;
    if (state.phase === 'delivery') worldBall = ballPosition(state.progress);
    if (state.phase === 'shot' && state.shotStartPoint && state.shotEndPoint) {
      worldBall = new THREE.Vector3(
        THREE.MathUtils.lerp(state.shotStartPoint.x, state.shotEndPoint.x, shotT),
        THREE.MathUtils.lerp(state.shotStartPoint.y, state.shotEndPoint.y, shotT) + Math.sin(shotT * Math.PI) * (state.shotApex || 0),
        THREE.MathUtils.lerp(state.shotStartPoint.z, state.shotEndPoint.z, shotT)
      );
    }
    ball.group.rotation.x = state.phase === 'shot' ? shotT * 5.4 : 0;
    ball.group.rotation.y = seconds * 7;
    ball.group.rotation.z = state.phase === 'shot' ? shotT * 2.6 : 0;
    ball.group.visible = !!worldBall;
    ballShadow.visible = !!worldBall;
    if (worldBall) {
      ball.group.position.copy(worldBall);
      ballShadow.position.x = worldBall.x;
      ballShadow.position.z = worldBall.z;
      const shadowSize = THREE.MathUtils.clamp(1.35 - worldBall.y * 0.2, 0.55, 1.25);
      ballShadow.scale.setScalar(shadowSize);
      ballShadow.material.opacity = THREE.MathUtils.clamp(0.34 - worldBall.y * 0.075, 0.08, 0.31);
    }

    strikerWickets.rotation.z = state.phase === 'shot' && state.pendingWicket ? Math.sin(shotT * Math.PI) * 0.6 : 0;
    const mode = cameraModes[cameraIndex];
    camera.position.lerp(mode.position, 0.045);
    lookTarget.lerp(mode.target, 0.055);
    camera.lookAt(lookTarget);
    renderer.render(scene, camera);
  }

  function syncTeams(state) {
    const batting = state.teams[state.battingTeamIndex] || { color: '#34b881' };
    const fielding = state.teams[1 - state.battingTeamIndex] || { color: '#287ed2' };
    batter.kit.color.set(batting.color);
    batter.sleeves.color.set(batting.color);
    bowler.kit.color.set(fielding.color);
    bowler.sleeves.color.set(fielding.color);
  }

  function syncEnvironment(state) {
    const night = state.weather === 'dew' || state.stadium === 'city';
    const cloudy = state.weather === 'cloudy';
    const sky = night ? '#111c35' : cloudy ? '#60727b' : state.stadium === 'highland' ? '#72a9c2' : '#71b8d1';
    if (lastSky !== sky) {
      lastSky = sky;
      scene.background = new THREE.Color(sky);
      scene.fog = new THREE.Fog(sky, 46, 100);
    }
    const pitchColors = { flat: '#c9ad83', green: '#929d78', dry: '#b78b58', bouncy: '#d1b589' };
    if (lastPitch !== state.pitch) {
      lastPitch = state.pitch;
      pitchMaterial.color.set(pitchColors[state.pitch] || pitchColors.flat);
    }
    const grassColors = { harbor: '#287548', highland: '#397451', city: '#1d6343' };
    const fieldColor = state.weather === 'dew' ? '#347d4c' : grassColors[state.stadium] || grassColors.harbor;
    grassMaterial.color.set(fieldColor);
    const brightness = night ? 1.35 : cloudy ? 1.9 : 2.35;
    hemi.intensity = brightness;
    sun.intensity = night ? 0.8 : cloudy ? 1.75 : 3.25;
    renderer.toneMappingExposure = night ? 1.27 : 1.12;
    const venue = `${state.stadium}/${night}/${cloudy}`;
    if (lastVenue !== venue) {
      lastVenue = venue;
      stadium.children.filter((child) => child.userData.venueAccent).forEach((child) => child.material.color.set(night ? '#46415c' : state.stadium === 'highland' ? '#43535a' : '#345253'));
    }
  }

  resize();
  return {
    resize,
    render,
    ballPosition,
    cycleCamera,
    get cameraLabel() { return cameraModes[cameraIndex].label; }
  };
}

function addMowingRings(parent) {
  for (let i = 0; i < 7; i++) {
    const points = [];
    for (let step = 0; step < 128; step++) {
      const angle = step / 128 * Math.PI * 2;
      const rx = 10 + i * 2.55;
      const rz = 7.5 + i * 1.82;
      points.push(new THREE.Vector3(Math.cos(angle) * rx, -0.031, Math.sin(angle) * rz));
    }
    const line = new THREE.LineLoop(new THREE.BufferGeometry().setFromPoints(points), new THREE.LineBasicMaterial({ color: i % 2 ? '#64a866' : '#1f7447', transparent: true, opacity: 0.28 }));
    parent.add(line);
  }
}

function addCreases(parent) {
  const lineMaterial = new THREE.MeshBasicMaterial({ color: '#f4eddc' });
  for (const z of [-5.7, 5.7]) {
    const line = new THREE.Mesh(new THREE.BoxGeometry(3.25, 0.018, 0.055), lineMaterial);
    line.position.set(0, 0.045, z);
    parent.add(line);
    for (const side of [-1, 1]) {
      const sideLine = new THREE.Mesh(new THREE.BoxGeometry(0.055, 0.018, 0.48), lineMaterial);
      sideLine.position.set(side * 1.3, 0.045, z + (z > 0 ? -0.22 : 0.22));
      parent.add(sideLine);
    }
  }
  for (let i = 0; i < 4; i++) {
    const strip = new THREE.Mesh(new THREE.PlaneGeometry(2.4, 1.3), new THREE.MeshStandardMaterial({ color: i % 2 ? '#d6bd91' : '#c7a879', roughness: 1 }));
    strip.rotation.x = -Math.PI / 2;
    strip.position.set(0, 0.034, -6.9 + i * 3.7);
    parent.add(strip);
  }
}

function addBoundary(parent) {
  const pts = [];
  for (let i = 0; i < 160; i++) {
    const a = (i / 160) * Math.PI * 2;
    pts.push(new THREE.Vector3(Math.cos(a) * 28.5, 0.025, Math.sin(a) * 21.2));
  }
  const rope = new THREE.LineLoop(new THREE.BufferGeometry().setFromPoints(pts), new THREE.LineBasicMaterial({ color: '#e8d39c' }));
  parent.add(rope);
  const inner = pts.map((p) => new THREE.Vector3(p.x * 0.985, 0.027, p.z * 0.985));
  parent.add(new THREE.LineLoop(new THREE.BufferGeometry().setFromPoints(inner), new THREE.LineBasicMaterial({ color: '#f1e2b1', transparent: true, opacity: 0.52 })));
}

function ovalBand(parent, rx, rz, y, height, color, transparent = false) {
  const vertices = [];
  const indices = [];
  const steps = 160;
  for (let i = 0; i <= steps; i++) {
    const a = i / steps * Math.PI * 2;
    const x = Math.cos(a) * rx;
    const z = Math.sin(a) * rz;
    vertices.push(x, y, z, x * 1.035, y, z * 1.035, x * 1.035, y + height, z * 1.035, x, y + height, z);
  }
  for (let i = 0; i < steps; i++) {
    const b = i * 4;
    const next = (i + 1) * 4;
    // Join neighboring points around the oval into continuous wall surfaces.
    indices.push(
      b + 1, next + 1, next + 2, b + 1, next + 2, b + 2,
      b, b + 3, next + 3, b, next + 3, next,
      b + 3, b + 2, next + 2, b + 3, next + 2, next + 3,
      b, next, next + 1, b, next + 1, b + 1
    );
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
  geometry.setIndex(indices);
  geometry.computeVertexNormals();
  const material = new THREE.MeshStandardMaterial({ color, roughness: 0.92, side: THREE.DoubleSide, transparent, opacity: transparent ? 0.96 : 1 });
  const band = new THREE.Mesh(geometry, material);
  band.userData.venueAccent = true;
  band.receiveShadow = true;
  parent.add(band);
  return band;
}

function addStadiumBowl(parent) {
  ovalBand(parent, 37.2, 29.2, 0.45, 8.8, '#17313d');
  const tiers = ['#25454a', '#315655', '#263e49', '#355b5b', '#223b48', '#304d58', '#1b3540'];
  for (let row = 0; row < 9; row++) {
    const rx = 29.5 + row * 0.86;
    const rz = 21.8 + row * 0.66;
    const ring = makeEllipseSeat(rx, rz, 0.68, 1.55 + row * 0.78, tiers[row % tiers.length]);
    ring.userData.venueAccent = true;
    parent.add(ring);
  }
  for (const x of [-31, 31]) {
    const concourse = new THREE.Mesh(new THREE.BoxGeometry(1.2, 1.2, 43), new THREE.MeshStandardMaterial({ color: '#102832', roughness: 0.9 }));
    concourse.position.set(x * 1.03, 2.8, 0);
    parent.add(concourse);
  }
  // Slim roof trusses frame the sky while leaving the pitch and lights visible.
  for (const side of [-1, 1]) {
    const beam = new THREE.Mesh(new THREE.BoxGeometry(0.36, 0.45, 43), new THREE.MeshStandardMaterial({ color: '#304a55', metalness: 0.32, roughness: 0.58 }));
    beam.position.set(side * 37.6, 8.9, 0);
    parent.add(beam);
  }
}

function makeEllipseSeat(rx, rz, width, y, color) {
  const vertices = [];
  const indices = [];
  const steps = 160;
  for (let i = 0; i <= steps; i++) {
    const a = (i / steps) * Math.PI * 2;
    vertices.push(
      Math.cos(a) * rx, y, Math.sin(a) * rz,
      Math.cos(a) * (rx + width), y, Math.sin(a) * (rz + width * 0.78)
    );
    const n = i * 2;
    if (i < steps) indices.push(n, n + 1, n + 3, n, n + 3, n + 2);
  }
  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
  geometry.setIndex(indices);
  geometry.computeVertexNormals();
  return new THREE.Mesh(geometry, new THREE.MeshStandardMaterial({ color, roughness: 0.83, side: THREE.DoubleSide }));
}

function addCrowd(parent, across) {
  const countRows = 9;
  const crowdGeometry = new THREE.CapsuleGeometry(0.095, 0.24, 2, 5);
  const crowdMaterial = new THREE.MeshStandardMaterial({ color: '#ffffff', roughness: 0.98, vertexColors: true });
  const crowd = new THREE.InstancedMesh(crowdGeometry, crowdMaterial, countRows * across);
  const transform = new THREE.Object3D();
  let index = 0;
  for (let row = 0; row < countRows; row++) {
    for (let i = 0; i < across; i++) {
      const angle = i / across * Math.PI * 2 + (row % 2) * 0.017;
      const rx = 29.8 + row * 0.86;
      const rz = 22 + row * 0.66;
      transform.position.set(Math.cos(angle) * rx, 2.1 + row * 0.77 + Math.sin(i * 1.71 + row) * 0.1, Math.sin(angle) * rz);
      transform.scale.set(0.82 + Math.sin(i * 2 + row) * 0.22, 0.8 + Math.cos(i * 1.2) * 0.17, 0.72);
      transform.rotation.y = -angle;
      transform.updateMatrix();
      crowd.setMatrixAt(index, transform.matrix);
      crowd.setColorAt(index, new THREE.Color(TEAM_TONES[(i * 13 + row * 3) % TEAM_TONES.length]));
      index++;
    }
  }
  crowd.instanceMatrix.needsUpdate = true;
  if (crowd.instanceColor) crowd.instanceColor.needsUpdate = true;
  parent.add(crowd);
}

function addFloodlights(parent) {
  for (const [x, z] of [[-37, -20], [37, -20], [-37, 20], [37, 20]]) {
    const pole = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.19, 10, 7), new THREE.MeshStandardMaterial({ color: '#9cb2ad', metalness: 0.72, roughness: 0.33 }));
    pole.position.set(x, 5.4, z);
    parent.add(pole);
    const bank = new THREE.Group();
    for (let row = 0; row < 2; row++) {
      for (let column = 0; column < 5; column++) {
        const bulb = new THREE.Mesh(new THREE.BoxGeometry(0.3, 0.2, 0.18), new THREE.MeshBasicMaterial({ color: '#efffdd' }));
        bulb.position.set((column - 2) * 0.34, row * 0.27, 0);
        bank.add(bulb);
      }
    }
    bank.position.set(x, 10.7, z);
    parent.add(bank);
  }
}

function addScoreboard(parent) {
  const board = document.createElement('canvas');
  board.width = 768;
  board.height = 192;
  const context = board.getContext('2d');
  const gradient = context.createLinearGradient(0, 0, 768, 192);
  gradient.addColorStop(0, '#102c35');
  gradient.addColorStop(1, '#174c45');
  context.fillStyle = gradient;
  context.fillRect(0, 0, 768, 192);
  context.strokeStyle = '#68e1b48a';
  context.lineWidth = 5;
  context.strokeRect(9, 9, 750, 174);
  context.fillStyle = '#69e5b5';
  context.font = '800 26px system-ui, sans-serif';
  context.letterSpacing = '9px';
  context.fillText('CRICKET  UNIVERSE', 40, 53);
  context.fillStyle = '#f7f3e7';
  context.font = '900 67px system-ui, sans-serif';
  context.letterSpacing = '2px';
  context.fillText('PLAY YOUR MOMENT', 40, 135);
  const texture = new THREE.CanvasTexture(board);
  texture.colorSpace = THREE.SRGBColorSpace;
  const frame = new THREE.Mesh(new THREE.BoxGeometry(11, 2.9, 0.5), new THREE.MeshStandardMaterial({ color: '#102832', metalness: 0.28, roughness: 0.55 }));
  frame.position.set(0, 6.65, -30.8);
  parent.add(frame);
  const face = new THREE.Mesh(new THREE.PlaneGeometry(10.6, 2.65), new THREE.MeshBasicMaterial({ map: texture }));
  face.position.set(0, 6.65, -30.52);
  parent.add(face);
}

function addBoundaryBoards(parent) {
  const colors = ['#143a3b', '#19484a', '#16383f'];
  for (let i = 0; i < 56; i++) {
    const angle = i / 56 * Math.PI * 2;
    const board = new THREE.Mesh(new THREE.BoxGeometry(2.7, 0.52, 0.18), new THREE.MeshStandardMaterial({ color: colors[i % colors.length], roughness: 0.8, emissive: '#0a201e', emissiveIntensity: 0.18 }));
    board.position.set(Math.cos(angle) * 27.5, 0.34, Math.sin(angle) * 20.3);
    board.rotation.y = -angle + Math.PI / 2;
    board.userData.venueAccent = true;
    parent.add(board);
  }
}

function makeWickets() {
  const group = new THREE.Group();
  const wood = new THREE.MeshStandardMaterial({ color: '#f4e5c7', roughness: 0.68 });
  const stumpGeometry = new THREE.CylinderGeometry(0.035, 0.045, 0.78, 7);
  for (let i = -1; i <= 1; i++) {
    const stump = new THREE.Mesh(stumpGeometry, wood);
    stump.position.set(i * 0.13, 0.39, 0);
    stump.castShadow = true;
    group.add(stump);
  }
  const bail = new THREE.Mesh(new THREE.BoxGeometry(0.34, 0.045, 0.08), wood);
  bail.position.y = 0.8;
  group.add(bail);
  return { group };
}

function makeBall() {
  const group = new THREE.Group();
  const leather = new THREE.MeshStandardMaterial({ color: '#c72537', roughness: 0.42, metalness: 0.04 });
  const seam = new THREE.MeshStandardMaterial({ color: '#ffe7d9', roughness: 0.7 });
  const shell = new THREE.Mesh(new THREE.SphereGeometry(0.145, 20, 14), leather);
  shell.castShadow = true;
  group.add(shell);
  const seamCurve = new THREE.Mesh(new THREE.TorusGeometry(0.101, 0.008, 5, 30), seam);
  seamCurve.rotation.set(0.5, 0.25, 0.7);
  group.add(seamCurve);
  return { group };
}

function makePlayer(color, isBatter, simplified = false) {
  const root = new THREE.Group();
  const kit = new THREE.MeshStandardMaterial({ color, roughness: 0.78 });
  const sleeves = kit.clone();
  const trousers = new THREE.MeshStandardMaterial({ color: isBatter ? '#e4e8e4' : '#233136', roughness: 0.94 });
  const skin = new THREE.MeshStandardMaterial({ color: '#b77b57', roughness: 0.92 });
  const helmet = new THREE.MeshStandardMaterial({ color: isBatter ? '#15252b' : color, roughness: 0.7, metalness: 0.1 });
  const shoe = new THREE.MeshStandardMaterial({ color: '#e9ece4', roughness: 0.85 });
  const white = new THREE.MeshStandardMaterial({ color: '#f7f5e9', roughness: 0.8 });
  const capsule = (radius, length, material, position, scale = null) => {
    const mesh = new THREE.Mesh(new THREE.CapsuleGeometry(radius, length, 3, simplified ? 5 : 7), material);
    mesh.position.set(...position);
    if (scale) mesh.scale.set(...scale);
    mesh.castShadow = true;
    root.add(mesh);
    return mesh;
  };
  if (!simplified) {
    capsule(0.235, 0.48, kit, [0, 1.22, 0]);
    capsule(0.18, 0.22, trousers, [0, 0.78, 0]);
    const stripe = new THREE.Mesh(new THREE.BoxGeometry(0.27, 0.07, 0.02), white);
    stripe.position.set(0, 1.32, 0.224);
    root.add(stripe);
  } else {
    capsule(0.19, 0.42, kit, [0, 1.18, 0]);
  }
  capsule(0.09, 0.43, trousers, [-0.13, 0.35, 0], [1, 1, 1]);
  capsule(0.09, 0.43, trousers, [0.13, 0.35, 0], [1, 1, 1]);
  for (const x of [-0.14, 0.14]) {
    const foot = new THREE.Mesh(new THREE.BoxGeometry(0.19, 0.13, 0.35), shoe);
    foot.position.set(x, 0.075, 0.035);
    foot.castShadow = true;
    root.add(foot);
  }
  const neck = new THREE.Mesh(new THREE.CylinderGeometry(0.075, 0.085, 0.16, 8), skin);
  neck.position.y = 1.58;
  root.add(neck);
  const head = new THREE.Mesh(new THREE.SphereGeometry(0.155, 12, 9), skin);
  head.position.y = 1.77;
  head.castShadow = true;
  root.add(head);
  const lid = new THREE.Mesh(new THREE.SphereGeometry(0.18, 12, 7, 0, Math.PI * 2, 0, Math.PI * 0.52), helmet);
  lid.position.y = 1.79;
  lid.castShadow = true;
  root.add(lid);

  const buildArm = (side) => {
    const shoulder = new THREE.Group();
    shoulder.position.set(side * 0.23, 1.43, 0);
    root.add(shoulder);
    const upper = new THREE.Mesh(new THREE.CapsuleGeometry(0.078, 0.29, 3, 6), sleeves);
    upper.position.y = -0.2;
    upper.rotation.z = side * -0.1;
    upper.castShadow = true;
    shoulder.add(upper);
    const elbow = new THREE.Group();
    elbow.position.set(0, -0.4, 0);
    shoulder.add(elbow);
    const forearm = new THREE.Mesh(new THREE.CapsuleGeometry(0.063, 0.28, 3, 6), skin);
    forearm.position.y = -0.19;
    forearm.castShadow = true;
    elbow.add(forearm);
    const glove = new THREE.Mesh(new THREE.SphereGeometry(0.088, 8, 7), isBatter ? white : skin);
    glove.position.y = -0.39;
    elbow.add(glove);
    return shoulder;
  };
  const leftArm = buildArm(-1);
  const rightArm = buildArm(1);
  const batPivot = new THREE.Group();
  batPivot.position.set(0.05, -0.39, 0.02);
  rightArm.add(batPivot);
  const batBlade = new THREE.Mesh(new THREE.BoxGeometry(0.19, 0.88, 0.075), new THREE.MeshStandardMaterial({ color: '#c8995f', roughness: 0.72 }));
  batBlade.position.y = -0.63;
  batBlade.rotation.z = -0.12;
  batBlade.castShadow = true;
  batPivot.add(batBlade);
  const batGrip = new THREE.Mesh(new THREE.CylinderGeometry(0.044, 0.039, 0.45, 7), new THREE.MeshStandardMaterial({ color: '#d8d8cf', roughness: 0.6 }));
  batGrip.position.y = -0.12;
  batPivot.add(batGrip);
  batPivot.visible = isBatter;

  root.traverse((object) => { if (object.isMesh) object.castShadow = true; });
  return { root, kit, sleeves, leftArm, rightArm, bat: batPivot };
}
