using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using System;
using System.Collections;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Serialization;

namespace LocationBasedDoF
{
    public class LocationBasedDoF : MonoBehaviour
    {
        private static Mod mod;
        public float intFocusDistance = 1.5f;
        public float intAperture = 20;
        public int intFocalLength = 70;
        public float extFocusDistance = 65;
        public float extAperture = 6.5f;
        public int extFocalLength = 250;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("Initializing mod LocationBasedDoF");
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<LocationBasedDoF>();

            mod.IsReady = true;
        }

        private void Start()
        {
            StartGameBehaviour.OnStartGame += OnStartGame;
            PlayerEnterExit.OnTransitionDungeonInterior += OnEnterDungeon;
            PlayerEnterExit.OnTransitionDungeonExterior += OnExitDungeon;
            SaveLoadManager.OnLoad += OnLoad;
        }

        private void LoadSettings(ModSettings settings)
        {
            intFocusDistance = settings.GetValue<float>("InteriorDoF", "interiorFocusDistance");
            intAperture = settings.GetValue<float>("InteriorDoF", "interiorAperture");
            intFocalLength = settings.GetValue<int>("InteriorDoF", "interiorFocalLength");

            extFocusDistance = settings.GetValue<float>("ExteriorDoF", "exteriorFocusDistance");
            extAperture = settings.GetValue<float>("ExteriorDoF", "exteriorAperture");
            extFocalLength = settings.GetValue<int>("ExteriorDoF", "exteriorFocalLength");
        }

        private void OnStartGame(object sender, EventArgs e)
        {
            StartCoroutine(UpdateDoF());
        }

        private void OnEnterDungeon(PlayerEnterExit.TransitionEventArgs args)
        {
            StartCoroutine(UpdateDoF());
        }

        private void OnExitDungeon(PlayerEnterExit.TransitionEventArgs args)
        {
            StartCoroutine(UpdateDoF());
        }

        private void OnLoad(SaveData_v1 saveData)
        {
            StartCoroutine(UpdateDoF());
        }


        private void SetExteriorDoF()
        {
            DaggerfallUnity.Settings.DepthOfFieldFocusDistance = extFocusDistance;
            DaggerfallUnity.Settings.DepthOfFieldAperture = extAperture;
            DaggerfallUnity.Settings.DepthOfFieldFocalLength = extFocalLength;
        }

        private void SetInteriorDoF()
        {
            DaggerfallUnity.Settings.DepthOfFieldFocusDistance = intFocusDistance;
            DaggerfallUnity.Settings.DepthOfFieldAperture = intAperture;
            DaggerfallUnity.Settings.DepthOfFieldFocalLength = intFocalLength;
        }

        private IEnumerator UpdateDoF()
        {
            // Wait a bit
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            LoadSettings(mod.GetSettings());

            var dungeon = GameManager.Instance.PlayerEnterExit.Dungeon;

            if (dungeon == null)
            {
                SetExteriorDoF();
            }
            else
            {
                SetInteriorDoF();
            }

            GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
        }
    }
}