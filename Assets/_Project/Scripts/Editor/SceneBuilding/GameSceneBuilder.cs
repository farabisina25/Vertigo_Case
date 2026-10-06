using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vertigo.Wheel.Controllers.Game;
using Vertigo.Wheel.Controllers.Mapping;
using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Core.Zones;
using Vertigo.Wheel.Data.Wheels;
using Vertigo.Wheel.Editor.Validation;
using Vertigo.Wheel.Presentation.Hud;
using Vertigo.Wheel.Presentation.Popups;
using Vertigo.Wheel.Presentation.Rewards;
using Vertigo.Wheel.Presentation.Wheel;
using Vertigo.Wheel.Presentation.Zones;

namespace Vertigo.Wheel.Editor.SceneBuilding
{
    /// <summary>
    /// Builds the game scene from code so the hierarchy always follows the UI rules
    /// (naming, anchors, raycast targets, animated children) and every reference is wired.
    /// </summary>
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/game.unity";

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color BackgroundColor = new Color32(24, 27, 38, 255);
        private static readonly Color PanelTint = new Color32(40, 44, 58, 240);
        private static readonly Color BlockerColor = new Color(0f, 0f, 0f, 0.86f);
        private static readonly Color Gold = new Color32(255, 206, 84, 255);
        private static readonly Color Danger = new Color32(255, 82, 70, 255);

        private const float ScreenMargin = 40f;
        private const float ZoneBarTop = 20f;
        private const float ZoneBarHeight = 124f;
        private const float HudTop = ZoneBarTop + ZoneBarHeight + 20f;

        private const int ZoneCellCount = 15;
        private const int CurrentZoneCell = ZoneCellCount / 2;
        private const float ZoneCellSize = 92f;
        private const float ZoneCellStep = 108f;

        private const float WheelSize = 600f;
        private const float WheelCenterY = -90f;
        private const float SliceIconRadius = WheelSize * 0.3f;
        private const float SliceAmountRadius = SliceIconRadius - 46f;

        private const float RewardEntryHeight = 92f;

        [MenuItem("Vertigo/Build Game Scene", priority = 0)]
        public static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Build Game Scene",
                    $"{ScenePath} already exists and will be rebuilt from scratch.",
                    "Rebuild",
                    "Cancel"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            GameSceneAssets assets;
            try
            {
                assets = GameSceneAssets.Load();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError($"[GameSceneBuilder] {exception.Message}");
                return;
            }

            Scene scene = Build(assets);
            Debug.Log($"[GameSceneBuilder] Built {scene.path}.");
            UiHierarchyValidator.ValidateOpenScenes();
        }

        public static Scene Build(GameSceneAssets assets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            Transform canvas = CreateCanvas().transform;

            ZoneBarView zoneBar = BuildZoneBar(canvas, assets);
            WalletView wallet = BuildWallet(canvas, assets);
            RewardListView collectedRewards = BuildRewardsPanel(canvas, assets, out LeaveButtonView leaveButton);
            WheelView wheel = BuildWheel(canvas, assets);
            BombPopupView bombPopup = BuildBombPopup(canvas, assets);
            RunSummaryPopupView summaryPopup = BuildSummaryPopup(canvas, assets);

            var bootstrap = new GameObject("game_bootstrap").AddComponent<GameBootstrap>();
            using (var writer = new SerializedFieldWriter(bootstrap))
            {
                writer.Set("_progression", assets.Progression)
                    .Set("_rewardCatalog", assets.Catalog)
                    .Set("_settings", assets.Settings)
                    .Set("_texts", assets.Texts)
                    .Set("_wheelView", wheel)
                    .Set("_zoneBarView", zoneBar)
                    .Set("_collectedRewardsView", collectedRewards)
                    .Set("_bombPopupView", bombPopup)
                    .Set("_summaryPopupView", summaryPopup)
                    .Set("_leaveButtonView", leaveButton)
                    .Set("_walletView", wallet);
            }

            RenderPreview(assets, wheel, zoneBar, wallet);
            bombPopup.gameObject.SetActive(false);
            summaryPopup.gameObject.SetActive(false);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            return scene;
        }

        private static void CreateCamera()
        {
            var go = new GameObject("camera_main") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.cullingMask = 0;
        }

        private static void CreateEventSystem()
        {
            new GameObject("ui_event_system", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("ui_canvas_game", typeof(RectTransform)) { layer = 5 };

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // Top-stretched so the strip grows with wide screens while the cells stay centred.
        private static ZoneBarView BuildZoneBar(Transform parent, GameSceneAssets assets)
        {
            RectTransform bar = UiFactory.CreateRect("ui_zone_bar", parent);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = Vector2.one;
            bar.pivot = new Vector2(0.5f, 1f);
            bar.offsetMin = new Vector2(ScreenMargin, -(ZoneBarTop + ZoneBarHeight));
            bar.offsetMax = new Vector2(-ScreenMargin, -ZoneBarTop);

            Image background = UiFactory.CreateImage("ui_image_zone_bar_background", bar, assets.ZonePanelNormal);
            background.color = PanelTint;
            UiFactory.Stretch(background.rectTransform);

            RectTransform viewport = UiFactory.CreateRect("ui_zone_bar_viewport", bar);
            UiFactory.Stretch(viewport, 12f);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UiFactory.CreateRect("ui_zone_bar_content", viewport);
            UiFactory.PlaceCentered(content, Vector2.zero, Vector2.zero);

            var cells = new ZoneCellView[ZoneCellCount];
            for (int i = 0; i < ZoneCellCount; i++)
            {
                cells[i] = BuildZoneCell(content, i, assets);
            }

            Image frame = UiFactory.CreateImage("ui_image_zone_bar_frame", bar, assets.ZoneBarFrame);
            UiFactory.Stretch(frame.rectTransform);

            var view = bar.gameObject.AddComponent<ZoneBarView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_scrollContent", content)
                    .SetArray("_cells", cells)
                    .Set("_currentCellIndex", CurrentZoneCell);

                SerializedProperty styles = writer.GetProperty("_styles");
                styles.arraySize = 3;
                WriteZoneStyle(styles.GetArrayElementAtIndex(0), ZoneType.Normal, assets.ZonePanelNormal, Color.white);
                WriteZoneStyle(styles.GetArrayElementAtIndex(1), ZoneType.Safe, assets.ZonePanelSafe, new Color32(46, 160, 67, 255));
                WriteZoneStyle(styles.GetArrayElementAtIndex(2), ZoneType.Super, assets.ZonePanelSuper, Gold);
            }

            return view;
        }

        private static ZoneCellView BuildZoneCell(Transform parent, int index, GameSceneAssets assets)
        {
            RectTransform cell = UiFactory.CreateRect($"ui_zone_cell_{index}", parent);
            float x = (index - CurrentZoneCell) * ZoneCellStep;
            UiFactory.PlaceCentered(cell, new Vector2(x, 0f), new Vector2(ZoneCellSize, ZoneCellSize));

            Image background = UiFactory.CreateImage("ui_image_zone_cell_background_value", cell, assets.ZonePanelNormal, maskable: true);
            UiFactory.Stretch(background.rectTransform);

            Image currentFrame = UiFactory.CreateImage("ui_image_zone_cell_current_frame_value", cell, assets.ZoneCurrentFrame, maskable: true);
            currentFrame.color = Gold;
            UiFactory.Stretch(currentFrame.rectTransform, -4f);

            TextMeshProUGUI zone = UiFactory.CreateText("ui_text_zone_cell_value", cell, string.Empty, 40f, maskable: true);
            UiFactory.Stretch(zone.rectTransform);

            var view = cell.gameObject.AddComponent<ZoneCellView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_backgroundValue", background)
                    .Set("_currentFrameValue", currentFrame)
                    .Set("_zoneValue", zone);
            }

            return view;
        }

        private static void WriteZoneStyle(SerializedProperty style, ZoneType zoneType, Sprite background, Color textColor)
        {
            style.FindPropertyRelative("_zoneType").enumValueIndex = (int)zoneType;
            style.FindPropertyRelative("_background").objectReferenceValue = background;
            style.FindPropertyRelative("_textColor").colorValue = textColor;
        }

        private static WalletView BuildWallet(Transform parent, GameSceneAssets assets)
        {
            RectTransform wallet = UiFactory.CreateRect("ui_wallet", parent);
            UiFactory.Place(wallet, Vector2.one, Vector2.one, new Vector2(-ScreenMargin, -HudTop), new Vector2(280f, 72f));

            Image background = UiFactory.CreateImage("ui_image_wallet_background", wallet, assets.ZonePanelNormal);
            background.color = PanelTint;
            UiFactory.Stretch(background.rectTransform);

            Image icon = UiFactory.CreateImage("ui_image_wallet_currency_icon_value", wallet, assets.CurrencyIcon);
            UiFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(72f, 48f));

            TextMeshProUGUI balance = UiFactory.CreateText("ui_text_wallet_balance_value", wallet, "0", 40f, TextAlignmentOptions.Right);
            UiFactory.Stretch(balance.rectTransform);
            balance.rectTransform.offsetMin = new Vector2(96f, 0f);
            balance.rectTransform.offsetMax = new Vector2(-18f, 0f);

            var view = wallet.gameObject.AddComponent<WalletView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_currencyIconValue", icon).Set("_balanceValue", balance);
            }

            return view;
        }

        // Left-stretched column: grows with tall (4:3) screens, keeps its width on wide ones.
        private static RewardListView BuildRewardsPanel(Transform parent, GameSceneAssets assets, out LeaveButtonView leaveButton)
        {
            RectTransform panel = UiFactory.CreateRect("ui_panel_rewards", parent);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 0.5f);
            panel.offsetMin = new Vector2(ScreenMargin, ScreenMargin);
            panel.offsetMax = new Vector2(ScreenMargin + 360f, -HudTop);

            Image background = UiFactory.CreateImage("ui_image_panel_rewards_background", panel, assets.PanelGradient);
            UiFactory.Stretch(background.rectTransform);

            TextMeshProUGUI title = UiFactory.CreateText("ui_text_panel_rewards_title", panel, "REWARDS", 36f);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(320f, 48f));

            RewardListView list = BuildRewardList("ui_rewards", panel, assets, content =>
            {
                var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(4, 4, 4, 4);
                layout.spacing = 8f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            });
            var listRect = (RectTransform)list.transform;
            UiFactory.Stretch(listRect);
            listRect.offsetMin = new Vector2(12f, 130f);
            listRect.offsetMax = new Vector2(-12f, -76f);

            leaveButton = BuildLeaveButton(panel, assets);
            return list;
        }

        private static LeaveButtonView BuildLeaveButton(Transform parent, GameSceneAssets assets)
        {
            RectTransform root = UiFactory.CreateRect("ui_leave", parent);
            UiFactory.Place(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(300f, 90f));

            RectTransform anim = UiFactory.CreateRect("ui_leave_anim", root);
            UiFactory.Stretch(anim);
            var group = anim.gameObject.AddComponent<CanvasGroup>();

            Button button = UiFactory.CreateButton("ui_button_leave", anim, assets.ButtonOrange);
            UiFactory.Stretch((RectTransform)button.transform);
            AddButtonLabel(button, "ui_text_leave", "COLLECT", assets);

            var view = root.gameObject.AddComponent<LeaveButtonView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_leaveButton", button).Set("_animGroup", group);
            }

            return view;
        }

        private static RewardListView BuildRewardList(
            string name,
            Transform parent,
            GameSceneAssets assets,
            Action<RectTransform> addLayout)
        {
            RectTransform root = UiFactory.CreateRect(name, parent);

            // Transparent drag surface for the ScrollRect; the entries themselves never take raycasts.
            var dragSurface = root.gameObject.AddComponent<Image>();
            dragSurface.color = Color.clear;
            dragSurface.maskable = false;

            RectTransform viewport = UiFactory.CreateRect($"{name}_viewport", root);
            UiFactory.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UiFactory.CreateRect($"{name}_content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            addLayout(content);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RewardEntryView template = BuildRewardEntry(content, assets);
            template.gameObject.SetActive(false);

            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = viewport;
            scroll.content = content;

            var view = root.gameObject.AddComponent<RewardListView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_container", content).Set("_entryTemplate", template);
            }

            return view;
        }

        private static RewardEntryView BuildRewardEntry(Transform parent, GameSceneAssets assets)
        {
            RectTransform entry = UiFactory.CreateRect("ui_reward_entry_template", parent);
            entry.sizeDelta = new Vector2(320f, RewardEntryHeight);
            entry.gameObject.AddComponent<LayoutElement>().preferredHeight = RewardEntryHeight;

            RectTransform anim = UiFactory.CreateRect("ui_reward_entry_anim", entry);
            UiFactory.Stretch(anim);

            Image background = UiFactory.CreateImage("ui_image_reward_entry_background", anim, assets.ZonePanelNormal, maskable: true);
            UiFactory.Stretch(background.rectTransform);

            Image icon = UiFactory.CreateImage("ui_image_reward_entry_icon_value", anim, null, maskable: true);
            UiFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(130f, 72f));

            TextMeshProUGUI amount = UiFactory.CreateText("ui_text_reward_entry_amount_value", anim, "x0", 36f, TextAlignmentOptions.Right, maskable: true);
            UiFactory.Stretch(amount.rectTransform);
            amount.rectTransform.offsetMin = new Vector2(150f, 0f);
            amount.rectTransform.offsetMax = new Vector2(-14f, 0f);

            var view = entry.gameObject.AddComponent<RewardEntryView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_animRoot", anim).Set("_iconValue", icon).Set("_amountValue", amount);
            }

            return view;
        }

        // ui_wheel (view, never animated) > ui_wheel_anim > ui_wheel_rotation (spun by DOTween) > base + slices.
        private static WheelView BuildWheel(Transform parent, GameSceneAssets assets)
        {
            RectTransform wheel = UiFactory.CreateRect("ui_wheel", parent);
            UiFactory.PlaceCentered(wheel, new Vector2(0f, WheelCenterY), new Vector2(WheelSize, WheelSize));

            TextMeshProUGUI title = UiFactory.CreateText("ui_text_wheel_title_value", wheel, string.Empty, 64f);
            title.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(900f, 72f));

            TextMeshProUGUI subtitle = UiFactory.CreateText("ui_text_wheel_subtitle_value", wheel, string.Empty, 34f);
            subtitle.color = Gold;
            UiFactory.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(900f, 40f));

            RectTransform anim = UiFactory.CreateRect("ui_wheel_anim", wheel);
            UiFactory.Stretch(anim);

            Image glow = UiFactory.CreateImage("ui_image_wheel_glow", anim, assets.WheelGlow);
            glow.color = new Color(1f, 0.85f, 0.45f, 0.35f);
            UiFactory.PlaceCentered(glow.rectTransform, Vector2.zero, Vector2.one * WheelSize * 1.35f);

            RectTransform rotation = UiFactory.CreateRect("ui_wheel_rotation", anim);
            UiFactory.Stretch(rotation);

            Image wheelBase = UiFactory.CreateImage("ui_image_wheel_base_value", rotation, assets.WheelBase);
            UiFactory.Stretch(wheelBase.rectTransform);

            RectTransform slicesRoot = UiFactory.CreateRect("ui_wheel_slices", rotation);
            UiFactory.Stretch(slicesRoot);

            var slices = new WheelSliceView[WheelConfigSO.SliceCount];
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i] = BuildWheelSlice(slicesRoot, i, slices.Length, assets);
            }

            Image indicator = UiFactory.CreateImage("ui_image_wheel_indicator_value", anim, assets.WheelIndicator);
            UiFactory.Place(indicator.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(56f, 77f));

            Button spinButton = UiFactory.CreateButton("ui_button_spin", anim, assets.SpinButton);
            UiFactory.PlaceCentered((RectTransform)spinButton.transform, Vector2.zero, new Vector2(150f, 150f));
            AddButtonLabel(spinButton, "ui_text_spin", "SPIN", assets);

            var view = wheel.gameObject.AddComponent<WheelView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_spinButton", spinButton)
                    .Set("_rotationRoot", rotation)
                    .Set("_baseValue", wheelBase)
                    .Set("_indicatorValue", indicator)
                    .Set("_titleValue", title)
                    .Set("_subtitleValue", subtitle)
                    .SetArray("_slices", slices)
                    .Set("_spinSettings", assets.SpinSettings);
            }

            return view;
        }

        private static WheelSliceView BuildWheelSlice(Transform parent, int index, int count, GameSceneAssets assets)
        {
            RectTransform slice = UiFactory.CreateRect($"ui_wheel_slice_{index}", parent);
            UiFactory.Stretch(slice);
            slice.localRotation = Quaternion.Euler(0f, 0f, WheelAngles.GetSliceRestAngle(index, count));

            Image icon = UiFactory.CreateImage("ui_image_wheel_slice_icon_value", slice, null);
            UiFactory.PlaceCentered(icon.rectTransform, new Vector2(0f, SliceIconRadius), new Vector2(84f, 84f));

            TextMeshProUGUI amount = UiFactory.CreateText("ui_text_wheel_slice_amount_value", slice, string.Empty, 28f);
            amount.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.PlaceCentered(amount.rectTransform, new Vector2(0f, SliceAmountRadius), new Vector2(140f, 34f));

            var view = slice.gameObject.AddComponent<WheelSliceView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_iconValue", icon).Set("_amountValue", amount);
            }

            return view;
        }

        private static BombPopupView BuildBombPopup(Transform parent, GameSceneAssets assets)
        {
            PopupShell shell = BuildPopupShell("bomb", parent, new Vector2(820f, 720f));

            Image card = UiFactory.CreateImage("ui_image_popup_bomb_card", shell.Anim, assets.DeathCard);
            UiFactory.PlaceCentered(card.rectTransform, new Vector2(0f, 170f), new Vector2(300f, 300f));

            TextMeshProUGUI title = UiFactory.CreateText("ui_text_popup_bomb_title", shell.Anim, "BOMB!", 72f);
            title.color = Danger;
            title.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.PlaceCentered(title.rectTransform, new Vector2(0f, -20f), new Vector2(700f, 80f));

            TextMeshProUGUI info = UiFactory.CreateText(
                "ui_text_popup_bomb_info",
                shell.Anim,
                "You hit the bomb and lost every reward collected so far.",
                30f);
            info.enableWordWrapping = true;
            UiFactory.PlaceCentered(info.rectTransform, new Vector2(0f, -100f), new Vector2(760f, 80f));

            Button giveUp = UiFactory.CreateButton("ui_button_give_up", shell.Anim, assets.ButtonGrey);
            UiFactory.PlaceCentered((RectTransform)giveUp.transform, new Vector2(-170f, -250f), new Vector2(300f, 90f));
            AddButtonLabel(giveUp, "ui_text_give_up", "GIVE UP", assets);

            Button revive = UiFactory.CreateButton("ui_button_revive", shell.Anim, assets.ButtonOrange);
            UiFactory.PlaceCentered((RectTransform)revive.transform, new Vector2(170f, -250f), new Vector2(300f, 90f));

            TextMeshProUGUI reviveLabel = AddButtonLabel(revive, "ui_text_revive", "REVIVE", assets);
            reviveLabel.alignment = TextAlignmentOptions.Left;
            reviveLabel.rectTransform.offsetMin = new Vector2(26f, 0f);
            reviveLabel.rectTransform.offsetMax = new Vector2(-124f, 0f);

            Image costIcon = UiFactory.CreateImage("ui_image_revive_currency_icon", revive.transform, assets.CurrencyIcon);
            UiFactory.Place(costIcon.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-96f, 0f), new Vector2(46f, 30f));

            TextMeshProUGUI cost = UiFactory.CreateText("ui_text_revive_cost_value", revive.transform, "0", 34f, TextAlignmentOptions.Right);
            cost.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.Place(cost.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(76f, 40f));

            var view = shell.Root.gameObject.AddComponent<BombPopupView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_animRoot", shell.Anim)
                    .Set("_canvasGroup", shell.Fade)
                    .Set("_giveUpButton", giveUp)
                    .Set("_reviveButton", revive)
                    .Set("_reviveCostValue", cost);
            }

            return view;
        }

        private static RunSummaryPopupView BuildSummaryPopup(Transform parent, GameSceneAssets assets)
        {
            PopupShell shell = BuildPopupShell("summary", parent, new Vector2(1120f, 800f));

            Image panel = UiFactory.CreateImage("ui_image_popup_summary_panel", shell.Anim, assets.PanelGradient);
            UiFactory.Stretch(panel.rectTransform);

            Image frame = UiFactory.CreateImage("ui_image_popup_summary_frame", shell.Anim, assets.PanelFrame);
            frame.color = Gold;
            UiFactory.Stretch(frame.rectTransform);

            TextMeshProUGUI title = UiFactory.CreateText("ui_text_popup_summary_title_value", shell.Anim, string.Empty, 60f);
            title.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(1000f, 70f));

            TextMeshProUGUI info = UiFactory.CreateText("ui_text_popup_summary_info_value", shell.Anim, string.Empty, 32f);
            UiFactory.Place(info.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(1000f, 40f));

            RewardListView rewards = BuildRewardList("ui_popup_summary_rewards", shell.Anim, assets, content =>
            {
                var grid = content.gameObject.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(320f, RewardEntryHeight);
                grid.spacing = new Vector2(16f, 12f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;
                grid.childAlignment = TextAnchor.UpperCenter;
            });
            var rewardsRect = (RectTransform)rewards.transform;
            UiFactory.Stretch(rewardsRect);
            rewardsRect.offsetMin = new Vector2(40f, 160f);
            rewardsRect.offsetMax = new Vector2(-40f, -170f);

            Button playAgain = UiFactory.CreateButton("ui_button_play_again", shell.Anim, assets.ButtonOrange);
            UiFactory.Place((RectTransform)playAgain.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(340f, 96f));
            AddButtonLabel(playAgain, "ui_text_play_again", "PLAY AGAIN", assets);

            var view = shell.Root.gameObject.AddComponent<RunSummaryPopupView>();
            using (var writer = new SerializedFieldWriter(view))
            {
                writer.Set("_animRoot", shell.Anim)
                    .Set("_canvasGroup", shell.Fade)
                    .Set("_playAgainButton", playAgain)
                    .Set("_titleValue", title)
                    .Set("_infoValue", info)
                    .Set("_rewardList", rewards);
            }

            return view;
        }

        // ui_popup_x (view) > ui_popup_x_fade (CanvasGroup) > blocker + ui_popup_x_anim (scaled panel).
        private static PopupShell BuildPopupShell(string id, Transform parent, Vector2 panelSize)
        {
            RectTransform root = UiFactory.CreateRect($"ui_popup_{id}", parent);
            UiFactory.Stretch(root);

            RectTransform fade = UiFactory.CreateRect($"ui_popup_{id}_fade", root);
            UiFactory.Stretch(fade);
            var group = fade.gameObject.AddComponent<CanvasGroup>();

            UiFactory.CreateBlocker($"ui_image_popup_{id}_blocker", fade, BlockerColor);

            RectTransform anim = UiFactory.CreateRect($"ui_popup_{id}_anim", fade);
            UiFactory.PlaceCentered(anim, Vector2.zero, panelSize);

            return new PopupShell(root, group, anim);
        }

        private static TextMeshProUGUI AddButtonLabel(Button button, string name, string text, GameSceneAssets assets)
        {
            TextMeshProUGUI label = UiFactory.CreateText(name, button.transform, text, 36f);
            label.fontSharedMaterial = assets.OutlinedFont;
            UiFactory.Stretch(label.rectTransform);
            return label;
        }

        // Fills the views with zone 1 content so the saved scene looks like the game in edit mode.
        private static void RenderPreview(GameSceneAssets assets, WheelView wheel, ZoneBarView zoneBar, WalletView wallet)
        {
            IZoneRules rules = assets.Progression.CreateZoneRules();
            int zone = ZoneRules.FirstZone;
            WheelDefinition definition = new ScriptableWheelProvider(assets.Progression).GetWheel(zone, rules.GetZoneType(zone));

            wheel.Render(new WheelViewDataFactory(assets.Progression, assets.Catalog).Create(zone, definition));
            zoneBar.Render(zone, rules);
            wallet.SetCurrencyIcon(assets.CurrencyIcon);
            wallet.SetBalance(assets.Settings.StartingBalance, false);
        }

        private readonly struct PopupShell
        {
            public PopupShell(RectTransform root, CanvasGroup fade, RectTransform anim)
            {
                Root = root;
                Fade = fade;
                Anim = anim;
            }

            public RectTransform Root { get; }
            public CanvasGroup Fade { get; }
            public RectTransform Anim { get; }
        }
    }
}
