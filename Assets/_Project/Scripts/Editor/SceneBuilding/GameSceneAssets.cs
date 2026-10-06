using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data.Rewards;
using Vertigo.Wheel.Data.Settings;
using Vertigo.Wheel.Data.Wheels;
using Vertigo.Wheel.Presentation.Common;
using Vertigo.Wheel.Presentation.Wheel;
using Object = UnityEngine.Object;

namespace Vertigo.Wheel.Editor.SceneBuilding
{
    /// <summary>
    /// Every asset the game scene is built from, loaded by path so a missing file fails the build up front.
    /// </summary>
    public sealed class GameSceneAssets
    {
        private const string UiSprites = "Assets/Sprites/UI/";
        private const string WheelSprites = "Assets/Sprites/Wheel/";
        private const string Data = "Assets/_Project/ScriptableObjects/";
        private const string Fonts = "Assets/TextMesh Pro/Resources/Fonts & Materials/";

        public Sprite ButtonOrange { get; private set; }
        public Sprite ButtonGrey { get; private set; }
        public Sprite PanelGradient { get; private set; }
        public Sprite PanelFrame { get; private set; }
        public Sprite ZonePanelNormal { get; private set; }
        public Sprite ZonePanelSafe { get; private set; }
        public Sprite ZonePanelSuper { get; private set; }
        public Sprite ZoneCurrentFrame { get; private set; }
        public Sprite ZoneBarFrame { get; private set; }
        public Sprite DeathCard { get; private set; }
        public Sprite SpinButton { get; private set; }
        public Sprite WheelGlow { get; private set; }
        public Sprite WheelBase { get; private set; }
        public Sprite WheelIndicator { get; private set; }

        public Material OutlinedFont { get; private set; }

        public ZoneProgressionSO Progression { get; private set; }
        public RewardCatalogSO Catalog { get; private set; }
        public GameSettingsSO Settings { get; private set; }
        public GameTextsSO Texts { get; private set; }
        public WheelSpinSettingsSO SpinSettings { get; private set; }

        public Sprite CurrencyIcon => Settings.WalletCurrency != null ? Settings.WalletCurrency.Icon : null;

        public static GameSceneAssets Load()
        {
            var missing = new List<string>();
            var assets = new GameSceneAssets
            {
                ButtonOrange = Load<Sprite>(UiSprites + "UI_button_orange_standard.png", missing),
                ButtonGrey = Load<Sprite>(UiSprites + "UI_button_grey_standard.png", missing),
                PanelGradient = Load<Sprite>(UiSprites + "ui_card_frame_gardient.png", missing),
                PanelFrame = Load<Sprite>(UiSprites + "ui_card_frame_12px_neutral.png", missing),
                ZonePanelNormal = Load<Sprite>(UiSprites + "ui_card_panel_zone_bg.png", missing),
                ZonePanelSafe = Load<Sprite>(UiSprites + "ui_card_panel_zone_white.png", missing),
                ZonePanelSuper = Load<Sprite>(UiSprites + "ui_card_panel_zone_super.png", missing),
                ZoneCurrentFrame = Load<Sprite>(UiSprites + "ui_card_frame_4px_zone.png", missing),
                ZoneBarFrame = Load<Sprite>(UiSprites + "ui_card_zone_map_frame.png", missing),
                DeathCard = Load<Sprite>(UiSprites + "ui_card_icon_death.png", missing),
                SpinButton = Load<Sprite>(WheelSprites + "ui_spin_generic_button.png", missing),
                WheelGlow = Load<Sprite>(WheelSprites + "star_glow_alpha.png", missing),
                WheelBase = Load<Sprite>(WheelSprites + "ui_spin_bronze_base.png", missing),
                WheelIndicator = Load<Sprite>(WheelSprites + "ui_spin_bronze_indicator.png", missing),
                OutlinedFont = Load<Material>(Fonts + "LiberationSans SDF - Outline.mat", missing),
                Progression = Load<ZoneProgressionSO>(Data + "Progression/zone_progression.asset", missing),
                Catalog = Load<RewardCatalogSO>(Data + "Rewards/reward_catalog.asset", missing),
                Settings = Load<GameSettingsSO>(Data + "Settings/game_settings.asset", missing),
                Texts = Load<GameTextsSO>(Data + "Settings/game_texts.asset", missing),
                SpinSettings = Load<WheelSpinSettingsSO>(Data + "Settings/wheel_spin_settings.asset", missing),
            };

            if (TMP_Settings.defaultFontAsset == null)
            {
                missing.Add("TMP Settings default font asset");
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException("Missing assets:\n" + string.Join("\n", missing));
            }

            return assets;
        }

        private static T Load<T>(string path, ICollection<string> missing) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                missing.Add($"{path} ({typeof(T).Name})");
            }

            return asset;
        }
    }
}
