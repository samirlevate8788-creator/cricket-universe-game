using UnityEngine;

namespace CricketUniverse.Data
{
    [CreateAssetMenu(menuName = "Cricket Universe/Team", fileName = "Team")]
    public sealed class TeamData : ScriptableObject
    {
        public string teamName = "New Team";
        public Color primaryColor = new Color(0.12f, 0.42f, 0.78f);
        [Range(1, 100)] public int battingStrength = 65;
        [Range(1, 100)] public int bowlingStrength = 65;
        [Range(1, 100)] public int fieldingStrength = 65;
        public PlayerData captain;
        public PlayerData[] squad = new PlayerData[11];
    }
}
