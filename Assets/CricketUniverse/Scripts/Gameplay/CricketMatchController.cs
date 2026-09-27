using System.Collections;
using CricketUniverse.Core;
using CricketUniverse.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CricketUniverse.Gameplay
{
    internal sealed class CricketMatchController : MonoBehaviour
    {
        private MatchConfig config;
        private CricketMatchModel match;
        private SceneActors actors;
        private MatchHudView hud;
        private float flightTimer;
        private float shotTimer;
        private float shotDuration;
        private Vector3 shotStart;
        private Vector3 shotEnd;
        private float shotApex;
        private float aim;
        private float batterHomeX;
        private bool lofted;
        private bool quickRun;
        private bool deliveryResolved;
        private int pendingRuns;
        private bool pendingWicket;
        private ShotTiming lastTiming;
        private string status = "Your innings. Press BOWL when ready.";

        public void Initialize(MatchConfig settings, SceneActors sceneActors)
        {
            config = settings != null ? settings : ScriptableObject.CreateInstance<MatchConfig>();
            actors = sceneActors;
            match = new CricketMatchModel(config.oversPerInnings, config.wicketsPerInnings,
                config.demoTarget, config.homeTeamName, config.awayTeamName);
            batterHomeX = actors.batter.position.x;
            SetBallVisible(false);
            RefreshHud();
        }

        public void AttachHud(MatchHudView view)
        {
            hud = view;
            RefreshHud();
        }

        private void Update()
        {
            if (match == null) return;
            HandleInput();
            if (match.phase == MatchPhase.BallInFlight) UpdateIncomingBall();
            else if (match.phase == MatchPhase.ShotInFlight) UpdateShotFlight();
            UpdateAimVisual();
        }

        private void HandleInput()
        {
            if (match.phase == MatchPhase.ReadyToBowl && CricketInputRouter.BowlPressed) BowlDelivery();
            if (match.phase == MatchPhase.InningsBreak && CricketInputRouter.BowlPressed) ContinueInnings();
            if (match.phase == MatchPhase.MatchComplete && CricketInputRouter.BowlPressed) RestartMatch();

            float move = CricketInputRouter.MoveAxis;
            if (Mathf.Abs(move) > 0.05f && match.phase != MatchPhase.MatchComplete)
                actors.batter.position = new Vector3(Mathf.Clamp(actors.batter.position.x + move * 3.2f * Time.deltaTime, -1.35f, 1.35f),
                    actors.batter.position.y, actors.batter.position.z);

            float aimInput = CricketInputRouter.AimAxis;
            if (aimInput != 0f) SetAim(aim + aimInput * 0.12f);
            if (CricketInputRouter.LoftPressed) ToggleLoft();
            if (CricketInputRouter.ShotPressed) TryShot();
        }

        public void ActionButton()
        {
            if (match.phase == MatchPhase.ReadyToBowl) BowlDelivery();
            else if (match.phase == MatchPhase.InningsBreak) ContinueInnings();
            else if (match.phase == MatchPhase.MatchComplete) RestartMatch();
        }

        public void BowlDelivery()
        {
            if (match.phase != MatchPhase.ReadyToBowl) return;
            match.StartDelivery();
            flightTimer = 0f;
            deliveryResolved = false;
            quickRun = false;
            lastTiming = ShotTiming.None;
            actors.ball.gameObject.SetActive(true);
            actors.ball.position = actors.bowlerBallStart;
            status = "Watch the ball - time your shot!";
            RefreshHud();
        }

        private void UpdateIncomingBall()
        {
            flightTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(flightTimer / config.deliveryFlightSeconds);
            Vector3 end = actors.contactPoint + new Vector3(Mathf.Sin(flightTimer * 2.4f) * 0.08f, 0f, 0f);
            Vector3 position = Vector3.Lerp(actors.bowlerBallStart, end, progress);
            position.y += Mathf.Sin(progress * Mathf.PI) * 1.3f;
            actors.ball.position = position;
            if (hud != null) hud.SetBallProgress(progress, true);

            if (progress >= 1f)
            {
                lastTiming = ShotTiming.Miss;
                pendingWicket = Random.value < 0.28f;
                pendingRuns = 0;
                status = pendingWicket ? "BOWLED! The stumps are hit." : "Beaten by the delivery. Dot ball.";
                ResolveDelivery();
            }
        }

        public void TryShot()
        {
            if (match.phase != MatchPhase.BallInFlight) return;
            float progress = Mathf.Clamp01(flightTimer / config.deliveryFlightSeconds);
            float error = Mathf.Abs(progress - 0.82f);
            if (error <= config.perfectTimingWindow) lastTiming = ShotTiming.Perfect;
            else if (error <= config.goodTimingWindow) lastTiming = ShotTiming.Good;
            else lastTiming = ShotTiming.Poor;

            match.MarkShotInFlight();
            shotTimer = 0f;
            shotDuration = 1.25f;
            shotStart = actors.ball.position;
            float distance = lastTiming == ShotTiming.Perfect ? (lofted ? 48f : 38f)
                : lastTiming == ShotTiming.Good ? (lofted ? 37f : 28f) : 16f;
            float side = aim * (distance * 0.50f);
            float forward = distance * Mathf.Sqrt(1f - aim * aim * 0.25f);
            shotEnd = new Vector3(actors.batter.position.x + side, 0.08f, -7.5f - forward);
            shotApex = lofted ? 17f : lastTiming == ShotTiming.Perfect ? 7f : 3.2f;
            actors.ball.gameObject.SetActive(true);

            bool hitSix = lastTiming == ShotTiming.Perfect && lofted && Random.value < 0.72f;
            bool hitBoundary = lastTiming == ShotTiming.Perfect || (lastTiming == ShotTiming.Good && Random.value < (lofted ? 0.38f : 0.28f));
            if (lastTiming == ShotTiming.Perfect) pendingRuns = hitSix ? 6 : hitBoundary ? 4 : Random.Range(1, 4);
            else if (lastTiming == ShotTiming.Good) pendingRuns = hitBoundary ? 4 : Random.Range(1, lofted ? 3 : 4);
            else pendingRuns = Random.value < 0.55f ? 0 : 1;

            float wicketChance = lastTiming == ShotTiming.Poor ? 0.22f : lastTiming == ShotTiming.Good ? 0.045f : 0.015f;
            pendingWicket = Random.value < wicketChance;
            if (pendingWicket) pendingRuns = 0;
            status = $"{lastTiming.ToString().ToUpperInvariant()} contact - {(lofted ? "lofted" : "ground")}";
            RefreshHud();
        }

        public void ToggleLoft()
        {
            lofted = !lofted;
            if (hud != null) hud.SetLoft(lofted);
            RefreshHud();
        }

        public void AdjustAim(float delta) => SetAim(aim + delta);

        private void SetAim(float value)
        {
            aim = Mathf.Clamp(value, -1f, 1f);
            RefreshHud();
        }

        public void AttemptRun()
        {
            if (match.phase != MatchPhase.ShotInFlight || quickRun) return;
            quickRun = true;
            if (!pendingWicket) pendingRuns = Mathf.Min(6, pendingRuns + 1);
            if (Random.value < 0.06f) pendingWicket = true;
            status = pendingWicket ? "RUN OUT!" : "Quick single called.";
            RefreshHud();
        }

        private void UpdateShotFlight()
        {
            shotTimer += Time.deltaTime;
            float t = Mathf.Clamp01(shotTimer / shotDuration);
            actors.ball.position = Vector3.Lerp(shotStart, shotEnd, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * shotApex;
            actors.ball.Rotate(Vector3.right, 900f * Time.deltaTime, Space.World);
            if (t >= 1f) ResolveDelivery();
        }

        private void ResolveDelivery()
        {
            if (deliveryResolved) return;
            deliveryResolved = true;
            if (hud != null) hud.SetBallProgress(0f, false);
            SetBallVisible(false);
            if (pendingWicket) status = lastTiming == ShotTiming.Miss ? "WICKET!" : "CAUGHT! WICKET.";
            else if (pendingRuns == 6) status = "SIX! Into the stands!";
            else if (pendingRuns == 4) status = "FOUR! Finds the boundary.";
            else if (pendingRuns == 0) status = "Dot ball.";
            else status = $"{pendingRuns} run{(pendingRuns > 1 ? "s" : "")}.";

            match.RecordDelivery(pendingRuns, pendingWicket);
            if (match.phase == MatchPhase.InningsBreak) status = $"Innings complete. Target: {match.target}. Press NEXT INNINGS.";
            if (match.phase == MatchPhase.MatchComplete) status = match.result;
            RefreshHud();
            StartCoroutine(ReturnActorsToReady());
        }

        private IEnumerator ReturnActorsToReady()
        {
            yield return new WaitForSeconds(0.45f);
            if (match.phase == MatchPhase.ReadyToBowl || match.phase == MatchPhase.InningsBreak || match.phase == MatchPhase.MatchComplete)
            {
                actors.ball.gameObject.SetActive(false);
                actors.batter.position = new Vector3(batterHomeX, actors.batter.position.y, actors.batter.position.z);
            }
        }

        private void ContinueInnings()
        {
            if (match.phase != MatchPhase.InningsBreak) return;
            match.ContinueAfterBreak();
            status = "Chase is on. Press BOWL.";
            RefreshHud();
        }

        private void RestartMatch()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid()) SceneManager.LoadScene(scene.buildIndex);
        }

        private void UpdateAimVisual()
        {
            Vector3 basePosition = actors.batter.position + new Vector3(aim * 1.5f, 0.4f, -0.1f);
            actors.aimMarker.position = basePosition;
        }

        private void SetBallVisible(bool visible) => actors.ball.gameObject.SetActive(visible);

        private void RefreshHud()
        {
            if (hud != null && match != null) hud.Refresh(match, status, lastTiming, aim);
        }
    }
}
