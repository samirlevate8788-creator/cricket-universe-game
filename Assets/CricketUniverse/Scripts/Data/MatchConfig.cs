using UnityEngine;

namespace CricketUniverse.Data
{
    [CreateAssetMenu(menuName = "Cricket Universe/Match Config", fileName = "MatchConfig")]
    public sealed class MatchConfig : ScriptableObject
    {
        [Min(1)] public int oversPerInnings = 2;
        [Min(1)] public int wicketsPerInnings = 3;
        [Min(1)] public int demoTarget = 36;
        [Min(0.2f)] public float deliveryFlightSeconds = 1.05f;
        [Range(0.02f, 0.25f)] public float perfectTimingWindow = 0.055f;
        [Range(0.05f, 0.4f)] public float goodTimingWindow = 0.16f;
        public string homeTeamName = "Harbor Hawks";
        public string awayTeamName = "Summit Strikers";
    }
}
