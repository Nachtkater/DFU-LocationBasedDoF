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

using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using System.Collections.Generic;

namespace LocationBasedDoF
{
    public class LocationBasedDoF : MonoBehaviour
    {
        DaggerfallUnity dfUnity;
        PlayerEnterExit playerEnterExit;
        PlayerEntity playerEntity;
        ItemCollection playerItems;
        private static Mod mod;
        public GameObject PlayerTorch;
        public float intFocusDistance = 1.5f;
        public float intAperture = 20;
        public int intFocalLength = 65;
        public float extFocusDistance = 50;
        public float extAperture = 10;
        public int extFocalLength = 250;
        public float darkIntensity = 10;
        public float darkThreshold = 0.2f;
        public float darkDiffusion = 5;
        public float lightIntensity = 20;
        public float lightThreshold = 1;
        public float lightDiffusion = 4;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("Initializing mod Location Based Effects");
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<LocationBasedDoF>();

            mod.IsReady = true;
        }

        private void Start()
        {
            LoadSettings(mod.GetSettings());
        }

        private void Update()
        {
            dfUnity = DaggerfallUnity.Instance;
            playerEnterExit = GameManager.Instance.PlayerEnterExit;
            playerEntity = GameManager.Instance.PlayerEntity;
            playerItems = playerEntity.Items;

            // Depth of Field conditions
            var dungeon = GameManager.Instance.PlayerEnterExit.Dungeon;
            var interior = GameManager.Instance.PlayerEnterExit.Interior;

            // Player not in dungeon or other interior
            if (dungeon == null && interior == null)
            {
                SetExteriorDoF();
            }
            else
            {
                SetInteriorDoF();
            }
            GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);

            // Light and Bloom conditions
            // Player outside and night
            if ((!playerEnterExit.IsPlayerInside && dfUnity.WorldTime.Now.IsCityLightsOn))
            {
                // Has player lantern? Give if not
                if(!playerItems.Contains(ItemGroups.UselessItems2, (int)UselessItems2.Lantern))
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Lantern);

                    item.RenameItem("Travel Lantern");
                    item.maxCondition = 3;
                    item.currentCondition = 2;
                    playerItems.AddItem(item);
                }
                DaggerfallUnityItem lantern = playerItems.GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Lantern, true);

                // Keep condition in flickering range 0...3
                if(lantern.currentCondition <= 1)
                {
                    lantern.currentCondition = 2;
                }

                // Equip lantern
                if(playerEntity.LightSource != lantern)
                {
                    playerEntity.LightSource = lantern;
                }

                // Set bloom
                // Change bloom only after sun went down
                if(dfUnity.WorldTime.Now.IsNight)
                {
                    SetDarkBloom();
                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                }
                else
                {
                    SetLightBloom();
                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                }
                
            }
            // Player in dungeon
            else if (playerEnterExit.IsPlayerInsideDungeon)
            {
                // Has player torch? Give if not
                if(!playerItems.Contains(ItemGroups.UselessItems2, (int)UselessItems2.Torch))
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Torch);

                    item.RenameItem("Dungeon Torch");
                    item.maxCondition = 3;
                    item.currentCondition = 2;
                    playerItems.AddItem(item);
                }
                DaggerfallUnityItem torch = playerItems.GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Torch, true);

                // Keep condition in flickering range 0...3
                if(torch.currentCondition <= 1)
                {
                    torch.currentCondition = 2;
                }

                // Equip torch
                if(playerEntity.LightSource != torch)
                {
                    playerEntity.LightSource = torch;
                }

                // Set bloom
                SetDarkBloom();
                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
            }
            // Player outside and day or in other interior
            else
            {
                if(playerEntity.LightSource != null)
                {
                    playerEntity.LightSource = null;
                }

                // Set bloom
                SetLightBloom();
                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
            }
        }

        private void LoadSettings(ModSettings settings)
        {
            intFocusDistance = settings.GetValue<float>("InteriorDoF", "interiorFocusDistance");
            intAperture = settings.GetValue<float>("InteriorDoF", "interiorAperture");
            intFocalLength = settings.GetValue<int>("InteriorDoF", "interiorFocalLength");

            extFocusDistance = settings.GetValue<float>("ExteriorDoF", "exteriorFocusDistance");
            extAperture = settings.GetValue<float>("ExteriorDoF", "exteriorAperture");
            extFocalLength = settings.GetValue<int>("ExteriorDoF", "exteriorFocalLength");

            darkIntensity = settings.GetValue<float>("BloomInDark", "darkIntensity");
            darkThreshold = settings.GetValue<float>("BloomInDark", "darkThreshold");
            darkDiffusion = settings.GetValue<float>("BloomInDark", "darkDiffusion");

            lightIntensity = settings.GetValue<float>("BloomInLight", "lightIntensity");
            lightThreshold = settings.GetValue<float>("BloomInLight", "lightThreshold");
            lightDiffusion = settings.GetValue<float>("BloomInLight", "lightDiffusion");
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

        private void SetDarkBloom()
        {
            DaggerfallUnity.Settings.BloomIntensity = darkIntensity;
            DaggerfallUnity.Settings.BloomThreshold = darkThreshold;
            DaggerfallUnity.Settings.BloomDiffusion = darkDiffusion;
        }

        private void SetLightBloom()
        {
            DaggerfallUnity.Settings.BloomIntensity = lightIntensity;
            DaggerfallUnity.Settings.BloomThreshold = lightThreshold;
            DaggerfallUnity.Settings.BloomDiffusion = lightDiffusion;
        }
    }
}