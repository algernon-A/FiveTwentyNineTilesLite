// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Xml.Serialization;
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;

    /// <summary>
    /// The mod's settings.
    /// </summary>
    [FileLocation(Mod.ModName)]
    public class ModSettings : ModSetting
    {
        private bool _unlockAll = true;
        private bool _extraAtStart = false;
        private bool _milestones = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModSettings"/> class.
        /// </summary>
        /// <param name="mod"><see cref="IMod"/> instance.</param>
        public ModSettings(IMod mod)
            : base(mod)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("UnlockAll")]
        public bool UnlockAll
        {
            get => _unlockAll;

            set
            {
                _unlockAll = value;

                // Assign contra value to ensure that JSON contains at least one non-default value.
                Contra = value;

                // Clear conflicting settings.
                if (value)
                {
                    _extraAtStart = false;
                    _milestones = false;
                }

                // Ensure state.
                EnsureState();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("ExtraTilesAtStart")]
        public bool ExtraTilesAtStart
        {
            get => _extraAtStart;

            set
            {
                _extraAtStart = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockAll = false;
                    _milestones = false;
                }

                // Ensure state.
                EnsureState();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("AssignToMilestones")]
        public bool AssignToMilestones
        {
            get => _milestones;

            set
            {
                _milestones = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockAll = false;
                    _extraAtStart = false;
                }

                // Ensure state.
                EnsureState();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether, well, nothing really.
        /// This is just the inverse of <see cref="UnlockAll"/>, to ensure the the JSON contains at least one non-default value.
        /// This is to workaround a bug where the settings file isn't overwritten when there are no non-default settings.
        /// </summary>
        [SettingsUIHidden]
        public bool Contra { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the mod's settings should be reset.
        /// </summary>
        [XmlIgnore]
        [SettingsUIButton]
        [SettingsUISection("ResetModSettings")]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            // Dummy getter.
            get => false;

            set
            {
                // Apply defaults.
                SetDefaults();

                // Ensure contra is set correctly.
                Contra = UnlockAll;

                // Save.
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Restores mod settings to default.
        /// </summary>
        public override void SetDefaults()
        {
            _unlockAll = true;
            _extraAtStart = false;
            _milestones = false;
        }

        /// <summary>
        /// Enables Unlock All as the default option and that no options are duplicated.
        /// </summary>
        private void EnsureState()
        {
            if (!_unlockAll && !_extraAtStart && !_milestones)
            {
                UnlockAll = true;
            }
        }
    }
}