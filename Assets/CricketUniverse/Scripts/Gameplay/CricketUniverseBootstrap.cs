using CricketUniverse.Core;
using CricketUniverse.Data;
using UnityEngine;

namespace CricketUniverse.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class CricketUniverseBootstrap : MonoBehaviour
    {
        [SerializeField] private MatchConfig matchConfig;
        private CricketMatchController controller;

        private void Awake()
        {
            if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            SceneActors actors = DemoStadiumBuilder.Build();
            controller = gameObject.AddComponent<CricketMatchController>();
            controller.Initialize(matchConfig, actors);
            MatchHudView hud = MatchHudView.Create(controller);
            controller.AttachHud(hud);
        }
    }
}
