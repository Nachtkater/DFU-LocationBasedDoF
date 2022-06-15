// Project:         LocationBasedEffects mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Jochen Birkle
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Jochen Birkle
// Created On:      13/6/2022, 6:00 PM
// Last Edit:       15/6/2022, 5:30 PM
// Version:         0.3
// Special Thanks:  Interkarma
// Modifier:

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace LocationBasedEffects
{
    public class LocationBasedEffects : MonoBehaviour
    {
        static LocationBasedEffects instance;

        public static LocationBasedEffects Insance
        {
            get { return instance ?? (instance = FindObjectOfType<LocationBasedEffects>()); }
        }

        static Mod mod;
        
        // Settings
        public static float intFocusDistance { get; set; }
        public static float intAperture { get; set; }
        public static int intFocalLength { get; set; }
        public static float extFocusDistance { get; set; }
        public static float extAperture { get; set; }
        public static int extFocalLength { get; set; }
        public static float darkIntensity { get; set; }
        public static float darkThreshold { get; set; }
        public static float darkDiffusion { get; set; }
        public static float lightIntensity { get; set; }
        public static float lightThreshold { get; set; }
        public static float lightDiffusion { get; set; }
        public static bool autoSwitch { get; set; }
        public static bool autoRefill { get; set; }
        public static bool autoGive { get; set; }

        DaggerfallUnity dfUnity;
        PlayerEnterExit playerEnterExit;
        PlayerEntity playerEntity;
        ItemCollection playerItems;
        GameObject PlayerTorch;
        
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("Initializing mod 'Location Based Effects'");
            mod = initParams.Mod;
            instance = new GameObject(mod.Title).AddComponent<LocationBasedEffects>();
            
            mod.LoadSettingsCallback = LoadSettings;
            mod.IsReady = true;
        }

        private void Start()
        {
            mod.LoadSettings();
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
                if(autoGive && !playerItems.Contains(ItemGroups.UselessItems2, (int)UselessItems2.Lantern))
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Lantern);

                    item.RenameItem("Travel Lantern");
                    item.maxCondition = 3;
                    item.currentCondition = 2;
                    playerItems.AddItem(item);
                }
                DaggerfallUnityItem lantern = playerItems.GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Lantern, true);

                if(lantern != null)
                { 
                    // Keep condition in flickering range 0...3
                    if(autoRefill && (lantern.currentCondition <= 1 || lantern.currentCondition >= 3))
                    {
                        lantern.currentCondition = 2;
                    }

                    // Equip lantern
                    if(autoSwitch && playerEntity.LightSource != lantern)
                    {
                        playerEntity.LightSource = lantern;
                    }
                }

                // Set bloom
                // Change bloom only after sun went down
                if(dfUnity.WorldTime.Now.Hour > 18 || dfUnity.WorldTime.Now.Hour < 6)
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
                if(autoGive && !playerItems.Contains(ItemGroups.UselessItems2, (int)UselessItems2.Torch))
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Torch);

                    item.RenameItem("Dungeon Torch");
                    item.maxCondition = 3;
                    item.currentCondition = 2;
                    playerItems.AddItem(item);
                }
                DaggerfallUnityItem torch = playerItems.GetItem(ItemGroups.UselessItems2, (int)UselessItems2.Torch, true);

                if(torch != null)
                {
                    // Keep condition in flickering range 0...3
                    if(autoRefill && (torch.currentCondition <= 1 || torch.currentCondition >= 3))
                    {
                        torch.currentCondition = 2;
                    }

                    // Equip torch
                    if(autoSwitch && playerEntity.LightSource != torch)
                    {
                        playerEntity.LightSource = torch;
                    }
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

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            intFocusDistance = mod.GetSettings().GetValue<float>("InteriorDoF", "interiorFocusDistance");
            intAperture = mod.GetSettings().GetValue<float>("InteriorDoF", "interiorAperture");
            intFocalLength = mod.GetSettings().GetValue<int>("InteriorDoF", "interiorFocalLength");

            extFocusDistance = mod.GetSettings().GetValue<float>("ExteriorDoF", "exteriorFocusDistance");
            extAperture = mod.GetSettings().GetValue<float>("ExteriorDoF", "exteriorAperture");
            extFocalLength = mod.GetSettings().GetValue<int>("ExteriorDoF", "exteriorFocalLength");

            darkIntensity = mod.GetSettings().GetValue<float>("BloomInDark", "darkIntensity");
            darkThreshold = mod.GetSettings().GetValue<float>("BloomInDark", "darkThreshold");
            darkDiffusion = mod.GetSettings().GetValue<float>("BloomInDark", "darkDiffusion");

            lightIntensity = mod.GetSettings().GetValue<float>("BloomInLight", "lightIntensity");
            lightThreshold = mod.GetSettings().GetValue<float>("BloomInLight", "lightThreshold");
            lightDiffusion = mod.GetSettings().GetValue<float>("BloomInLight", "lightDiffusion");

            autoSwitch = mod.GetSettings().GetValue<bool>("PlayerLightAuto", "autoSwitch");
            autoRefill = mod.GetSettings().GetValue<bool>("PlayerLightAuto", "autoRefill");
            autoGive = mod.GetSettings().GetValue<bool>("PlayerLightAuto", "autoGive");
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