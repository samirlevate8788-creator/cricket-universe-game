using UnityEngine;

namespace CricketUniverse.Data
{
    [CreateAssetMenu(menuName = "Cricket Universe/Player", fileName = "Player")]
    public sealed class PlayerData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "New Player";
        [Range(1, 99)] public int age = 22;
        [Range(1, 100)] public int overall = 65;

        [Header("Batting")]
        [Range(1, 100)] public int timing = 65;
        [Range(1, 100)] public int power = 65;
        [Range(1, 100)] public int running = 65;
        [Range(1, 100)] public int againstPace = 65;
        [Range(1, 100)] public int againstSpin = 65;
        [Range(1, 100)] public int pressure = 65;

        [Header("Bowling and fielding")]
        [Range(1, 100)] public int accuracy = 60;
        [Range(1, 100)] public int stamina = 75;
        [Range(1, 100)] public int catching = 65;
        [Range(1, 100)] public int throwing = 65;
        [Range(1, 100)] public int speed = 65;
    }
}
