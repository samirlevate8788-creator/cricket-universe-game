using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CricketUniverse.Gameplay
{
    internal sealed class SceneActors
    {
        public Transform batter;
        public Transform bowler;
        public Transform ball;
        public Transform aimMarker;
        public Vector3 bowlerBallStart;
        public Vector3 contactPoint;
    }

    /// <summary>Self-contained, license-safe scene dressing made from primitive meshes.</summary>
    internal static class DemoStadiumBuilder
    {
        private static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();

        public static SceneActors Build()
        {
            materials.Clear();
            RenderSettings.ambientLight = new Color(0.50f, 0.57f, 0.60f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.33f, 0.47f, 0.49f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 65f;
            RenderSettings.fogEndDistance = 145f;

            Light sun = new GameObject("Stadium Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.91f, 0.75f);
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            GameObject cameraObject = new GameObject("Broadcast Camera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 46f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 180f;
            camera.backgroundColor = new Color(0.19f, 0.36f, 0.41f);
            cameraObject.transform.position = new Vector3(0f, 12.8f, -25f);
            cameraObject.transform.LookAt(new Vector3(0f, 2.2f, 3.5f));

            CreatePrimitive(PrimitiveType.Plane, "Outfield", new Vector3(0f, -0.08f, 0f), new Vector3(10f, 1f, 10f),
                new Color(0.09f, 0.34f, 0.19f));
            CreatePrimitive(PrimitiveType.Plane, "Pitch", new Vector3(0f, 0.005f, 0f), new Vector3(1.14f, 1f, 2.18f),
                new Color(0.60f, 0.45f, 0.28f));
            CreatePitchLines();
            CreateBoundary();
            CreateStandsAndCrowd();
            CreateFloodlights();
            CreateScoreboard();
            Transform batter = CreatePlayer("Batter", new Vector3(0f, 0f, -7.8f), new Color(0.07f, 0.36f, 0.81f), true);
            Transform bowler = CreatePlayer("Bowler", new Vector3(0f, 0f, 7.4f), new Color(0.89f, 0.34f, 0.12f), false);
            Transform ball = CreatePrimitive(PrimitiveType.Sphere, "Cricket Ball", new Vector3(0f, 1.3f, 7.4f), new Vector3(0.20f, 0.20f, 0.20f),
                new Color(0.69f, 0.07f, 0.08f)).transform;
            Transform marker = CreatePrimitive(PrimitiveType.Sphere, "Aim Marker", new Vector3(0f, 0.10f, -7.7f), new Vector3(0.18f, 0.035f, 0.18f),
                new Color(0.25f, 0.98f, 0.66f)).transform;
            return new SceneActors
            {
                batter = batter,
                bowler = bowler,
                ball = ball,
                aimMarker = marker,
                bowlerBallStart = new Vector3(0f, 1.3f, 7.4f),
                contactPoint = new Vector3(0f, 0.11f, -7.0f)
            };
        }

        private static void CreatePitchLines()
        {
            for (int end = -1; end <= 1; end += 2)
            {
                float z = end * 8.9f;
                CreatePrimitive(PrimitiveType.Cube, "Crease", new Vector3(0f, 0.025f, z), new Vector3(10.8f, 0.025f, 0.11f), Color.white);
                CreatePrimitive(PrimitiveType.Cube, "Return Crease Left", new Vector3(-4.9f, 0.025f, z + end * 0.43f), new Vector3(0.08f, 0.025f, 0.88f), Color.white);
                CreatePrimitive(PrimitiveType.Cube, "Return Crease Right", new Vector3(4.9f, 0.025f, z + end * 0.43f), new Vector3(0.08f, 0.025f, 0.88f), Color.white);
                CreateWickets(z - end * 0.10f);
            }
        }

        private static void CreateWickets(float z)
        {
            for (int i = -1; i <= 1; i++)
                CreatePrimitive(PrimitiveType.Cylinder, "Stump", new Vector3(i * 0.22f, 0.38f, z), new Vector3(0.055f, 0.38f, 0.055f),
                    new Color(0.92f, 0.87f, 0.68f));
            CreatePrimitive(PrimitiveType.Cube, "Bails", new Vector3(0f, 0.78f, z), new Vector3(0.58f, 0.055f, 0.10f),
                new Color(0.96f, 0.90f, 0.69f));
        }

        private static void CreateBoundary()
        {
            GameObject ring = new GameObject("Boundary Rope", typeof(LineRenderer));
            LineRenderer line = ring.GetComponent<LineRenderer>();
            line.positionCount = 120;
            line.loop = true;
            line.useWorldSpace = true;
            line.startWidth = 0.25f;
            line.endWidth = 0.25f;
            line.sharedMaterial = GetMaterial(new Color(0.88f, 0.83f, 0.50f));
            for (int i = 0; i < 120; i++)
            {
                float angle = i * Mathf.PI * 2f / 120f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * 42f, 0.02f, Mathf.Sin(angle) * 42f));
            }
        }

        private static void CreateStandsAndCrowd()
        {
            Color[] seats = { new Color(0.07f, 0.17f, 0.24f), new Color(0.11f, 0.24f, 0.30f), new Color(0.10f, 0.20f, 0.26f) };
            Color[] crowd = { new Color(0.90f, 0.46f, 0.19f), new Color(0.89f, 0.82f, 0.58f), new Color(0.27f, 0.66f, 0.55f), new Color(0.82f, 0.84f, 0.78f) };
            const int segments = 52;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                float yaw = -angle * Mathf.Rad2Deg;
                for (int row = 0; row < 3; row++)
                {
                    Vector3 position = outward * (49f + row * 1.8f) + Vector3.up * (1.5f + row * 1.15f);
                    GameObject seating = CreatePrimitive(PrimitiveType.Cube, "Stand Tier", position, new Vector3(3.0f, 0.9f, 2.4f), seats[row]);
                    seating.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                    for (int person = 0; person < 3; person++)
                    {
                        Vector3 offset = Quaternion.Euler(0f, yaw, 0f) * new Vector3((person - 1) * 0.66f, 0.63f, 0f);
                        GameObject spectator = CreatePrimitive(PrimitiveType.Capsule, "Distant Spectator",
                            position + offset, new Vector3(0.25f, 0.35f, 0.25f), crowd[(i + person + row) % crowd.Length]);
                        spectator.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                    }
                }
            }
        }

        private static void CreateFloodlights()
        {
            Vector3[] locations = { new Vector3(-43f, 0f, -35f), new Vector3(43f, 0f, -35f), new Vector3(-43f, 0f, 35f), new Vector3(43f, 0f, 35f) };
            foreach (Vector3 location in locations)
            {
                CreatePrimitive(PrimitiveType.Cylinder, "Floodlight Tower", location + Vector3.up * 10f, new Vector3(0.38f, 10f, 0.38f),
                    new Color(0.18f, 0.22f, 0.22f));
                GameObject rig = new GameObject("Stadium Floodlight");
                rig.transform.position = location + Vector3.up * 20f;
                Light light = rig.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = new Color(0.82f, 0.90f, 1f);
                light.intensity = 10f;
                light.range = 70f;
                light.spotAngle = 105f;
                rig.transform.rotation = Quaternion.Euler(25f, location.x > 0 ? 180f : 0f, 0f);
            }
        }

        private static void CreateScoreboard()
        {
            CreatePrimitive(PrimitiveType.Cube, "Stadium Scoreboard", new Vector3(0f, 10f, 51f), new Vector3(18f, 8f, 0.7f),
                new Color(0.025f, 0.08f, 0.10f));
            GameObject title = new GameObject("Scoreboard Title", typeof(TextMesh));
            title.transform.position = new Vector3(0f, 11.2f, 50.55f);
            title.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh mesh = title.GetComponent<TextMesh>();
            mesh.text = "CRICKET  |  UNIVERSE";
            mesh.fontSize = 80;
            mesh.characterSize = 0.14f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.color = new Color(0.36f, 0.98f, 0.72f);
        }

        private static Transform CreatePlayer(string name, Vector3 position, Color shirt, bool withBat)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            CreatePart(PrimitiveType.Capsule, "Player Body", root.transform, new Vector3(0f, 1.03f, 0f), new Vector3(0.54f, 0.72f, 0.39f), shirt);
            CreatePart(PrimitiveType.Sphere, "Helmet", root.transform, new Vector3(0f, 2.00f, 0.02f), new Vector3(0.45f, 0.40f, 0.43f),
                new Color(0.10f, 0.14f, 0.17f));
            CreatePart(PrimitiveType.Cube, "Helmet Grill", root.transform, new Vector3(0f, 1.88f, -0.23f), new Vector3(0.35f, 0.035f, 0.06f),
                new Color(0.72f, 0.78f, 0.76f));
            CreatePart(PrimitiveType.Capsule, "Left Leg", root.transform, new Vector3(-0.17f, 0.35f, 0.03f), new Vector3(0.20f, 0.50f, 0.22f),
                new Color(0.83f, 0.84f, 0.80f));
            CreatePart(PrimitiveType.Capsule, "Right Leg", root.transform, new Vector3(0.17f, 0.35f, 0.03f), new Vector3(0.20f, 0.50f, 0.22f),
                new Color(0.83f, 0.84f, 0.80f));
            CreatePart(PrimitiveType.Cube, "Shoes", root.transform, new Vector3(0f, 0.08f, -0.09f), new Vector3(0.58f, 0.16f, 0.39f),
                new Color(0.12f, 0.13f, 0.14f));
            if (withBat)
            {
                GameObject bat = CreatePart(PrimitiveType.Cube, "Bat", root.transform, new Vector3(0.42f, 0.92f, -0.34f),
                    new Vector3(0.22f, 0.95f, 0.10f), new Color(0.77f, 0.58f, 0.30f));
                bat.transform.localRotation = Quaternion.Euler(-12f, 0f, -18f);
                CreatePart(PrimitiveType.Sphere, "Glove", root.transform, new Vector3(0.34f, 1.36f, -0.19f), new Vector3(0.24f, 0.20f, 0.20f), Color.white);
            }
            return root.transform;
        }

        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject part = CreatePrimitive(type, name, parent.position + localPosition, scale, color);
            part.transform.SetParent(parent, true);
            part.transform.localPosition = localPosition;
            return part;
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetMaterial(color);
                if (name == "Distant Spectator" || name == "Stand Tier")
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            return go;
        }

        private static Material GetMaterial(Color color)
        {
            if (materials.TryGetValue(color, out Material material)) return material;
            string shaderName = GraphicsSettings.currentRenderPipeline != null ? "Universal Render Pipeline/Lit" : "Standard";
            Shader shader = Shader.Find(shaderName);
            if (shader == null) shader = Shader.Find("Unlit/Color");
            material = new Material(shader) { color = color, enableInstancing = true };
            materials.Add(color, material);
            return material;
        }
    }
}
