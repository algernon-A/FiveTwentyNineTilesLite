﻿// <copyright file="Plugin.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Reflection;
    using BepInEx;
    using Game;
    using Game.Common;
    using Game.SceneFlow;
    using HarmonyLib;

    /// <summary>
    /// BepInEx plugin to substitute for IMod support.
    /// </summary>
    [BepInPlugin(GUID, "529 Tiles Lite", "1.0.2")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Plugin unique GUID.
        /// </summary>
        public const string GUID = "com.github.algernon-A.CS2.529TilesLite";

        // IMod instance reference.
        private Mod _mod;

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public void Awake()
        {
            // Ersatz IMod.OnLoad().
            _mod = new ();
            _mod.OnLoad();

            _mod.Log.Info("Plugin.Awake");

            // Apply Harmony patches.
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }

        /// <summary>
        /// Harmony postfix to <see cref="SystemOrder.Initialize"/> to substitute for IMod.OnCreateWorld.
        /// </summary>
        /// <param name="updateSystem"><see cref="GameManager"/> <see cref="UpdateSystem"/> instance.</param>
        [HarmonyPatch(typeof(SystemOrder), nameof(SystemOrder.Initialize))]
        [HarmonyPostfix]
        private static void InjectSystems(UpdateSystem updateSystem) => Mod.Instance.OnCreateWorld(updateSystem);
    }
}
