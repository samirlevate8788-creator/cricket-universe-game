using System;
using UnityEngine;

namespace CricketUniverse.Core
{
    public enum MatchPhase { ReadyToBowl, BallInFlight, ShotInFlight, InningsBreak, MatchComplete }
    public enum ShotTiming { None, Perfect, Good, Poor, Miss }

    [Serializable]
    public sealed class InningsScore
    {
        public int runs;
        public int wickets;
        public int legalBalls;

        public string OversText => $"{legalBalls / 6}.{legalBalls % 6}";
    }

    public sealed class CricketMatchModel
    {
        public int oversPerInnings { get; }
        public int wicketsPerInnings { get; }
        public string battingTeam { get; private set; }
        public string bowlingTeam { get; private set; }
        public InningsScore[] innings { get; } = new[] { new InningsScore(), new InningsScore() };
        public int currentInnings { get; private set; }
        public int target { get; private set; }
        public MatchPhase phase { get; private set; } = MatchPhase.ReadyToBowl;
        public string result { get; private set; } = string.Empty;

        public InningsScore Current => innings[currentInnings];
        public int BallsRemaining => Mathf.Max(0, oversPerInnings * 6 - Current.legalBalls);
        public int RunsRequired => currentInnings == 0 ? 0 : Mathf.Max(0, target - Current.runs);

        public CricketMatchModel(int overs, int wickets, int firstInningsTarget, string home, string away)
        {
            oversPerInnings = Mathf.Max(1, overs);
            wicketsPerInnings = Mathf.Max(1, wickets);
            target = Mathf.Max(1, firstInningsTarget);
            battingTeam = home;
            bowlingTeam = away;
        }

        public void StartDelivery()
        {
            if (phase == MatchPhase.ReadyToBowl) phase = MatchPhase.BallInFlight;
        }

        public void MarkShotInFlight()
        {
            if (phase == MatchPhase.BallInFlight) phase = MatchPhase.ShotInFlight;
        }

        public void RecordDelivery(int runs, bool wicket, bool legalBall = true)
        {
            if (phase != MatchPhase.BallInFlight && phase != MatchPhase.ShotInFlight) return;

            Current.runs += Mathf.Max(0, runs);
            if (wicket) Current.wickets++;
            if (legalBall) Current.legalBalls++;

            if (currentInnings == 1 && Current.runs >= target)
            {
                int wicketsLeft = wicketsPerInnings - Current.wickets;
                result = $"{battingTeam} win by {wicketsLeft} wicket{(wicketsLeft == 1 ? "" : "s")}";
                phase = MatchPhase.MatchComplete;
            }
            else if (Current.wickets >= wicketsPerInnings || Current.legalBalls >= oversPerInnings * 6)
            {
                if (currentInnings == 0)
                {
                    target = Current.runs + 1;
                    (battingTeam, bowlingTeam) = (bowlingTeam, battingTeam);
                    currentInnings = 1;
                    phase = MatchPhase.InningsBreak;
                }
                else
                {
                    int first = innings[0].runs;
                    int second = innings[1].runs;
                    result = first == second ? "Match tied" : first > second ? $"{bowlingTeam} win by {first - second} runs" : $"{battingTeam} win";
                    phase = MatchPhase.MatchComplete;
                }
            }
            else phase = MatchPhase.ReadyToBowl;
        }

        public void ContinueAfterBreak()
        {
            if (phase == MatchPhase.InningsBreak) phase = MatchPhase.ReadyToBowl;
        }
    }
}
