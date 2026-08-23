using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Toolbar;
using osu.Game.Screens;
using osucc.Client;
using osucc.Plugin;
using osuTK;
using osuTK.Input;
using OsuTweaks.Models;
using OsuTweaks.UI;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Главный менеджер полностью модульного тулбара.
    /// Извлекает ВСЕ блоки (включая селектор режимов Rulesets, настройки, профиль, оверлеи)
    /// и позволяет свободно перемещать их мышкой между зонами Лево, Центр и Право.
    /// </summary>
    public partial class ModularToolbarManager : CompositeDrawable
    {
        public static ModularToolbarManager? Instance { get; private set; }

        private readonly IOsuCcPluginHost host;
        private string configFilePath;

        private Toolbar? toolbar;
        private Drawable? originalGridContainer;
        private FillFlowContainer? originalLeftFlow;
        private FillFlowContainer? originalRightFlow;
        private Drawable? originalRulesetSelector;

        private ToolbarZoneContainer leftZone = null!;
        private ToolbarZoneContainer centerZone = null!;
        private ToolbarZoneContainer rightZone = null!;

        private ToolbarContextMenu contextMenu = null!;
        private DragGhostContainer dragGhostContainer = null!;
        private DraggableEditHintBanner editHintBanner = null!;
        private DragGhostBadge? activeGhost;

        private readonly Dictionary<string, ToolbarBlockContainer> allBlocks = new();
        public IReadOnlyDictionary<string, ToolbarBlockContainer> AllBlocks => allBlocks;

        private readonly List<string> originalLeftItems = new();
        private readonly List<string> originalRightItems = new();

        private bool isEditMode;
        private ToolbarBlockContainer? draggingBlock;
        private ToolbarZoneContainer? currentTargetZone;
        private int currentTargetIndex;

        private ToolbarOverlayPositioner? overlayPositioner;
        private ToolbarStyleManager? styleManager;

        public ModularToolbarManager(IOsuCcPluginHost host)
        {
            Instance = this;
            this.host = host;

            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "osu-tweaks", "layout.json");

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Depth = float.MinValue;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            dragGhostContainer = new DragGhostContainer
            {
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Depth = float.MinValue
            };

            InternalChildren = new Drawable[]
            {
                leftZone = new ToolbarZoneContainer(ToolbarZone.Left),
                centerZone = new ToolbarZoneContainer(ToolbarZone.Center),
                rightZone = new ToolbarZoneContainer(ToolbarZone.Right),
                overlayPositioner = new ToolbarOverlayPositioner(this, host)
            };

            contextMenu = new ToolbarContextMenu();
            editHintBanner = new DraggableEditHintBanner(colours, SaveAndExitEditMode);

            leftZone.OnZoneRightClicked += onZoneRightClicked;
            centerZone.OnZoneRightClicked += onZoneRightClicked;
            rightZone.OnZoneRightClicked += onZoneRightClicked;
        }

        public void AttachToolbar(Toolbar newToolbar)
        {
            toolbar = newToolbar;
            TweaksLog.Info($"ModularToolbarManager.AttachToolbar called with toolbar HashCode={newToolbar.GetHashCode()}");

            host.Scheduler?.Add(() =>
            {
                try
                {
                    initManager();
                }
                catch (Exception ex)
                {
                    TweaksLog.Error("Exception in AttachToolbar scheduler", ex);
                }
            });
        }

        private void initManager()
        {
            if (toolbar == null)
                return;

            originalGridContainer = findGridContainer(toolbar);
            originalLeftFlow = findLeftFlow(toolbar);
            originalRightFlow = findRightFlow(toolbar);

            if (originalLeftFlow == null || originalRightFlow == null)
            {
                TweaksLog.Error($"initManager: originalLeftFlow={originalLeftFlow != null}, originalRightFlow={originalRightFlow != null}");
                return;
            }

            // Извлекаем все элементы тулбара (включая rulesetSelector)
            if (allBlocks.Count == 0)
            {
                originalLeftItems.Clear();

                // 1. Извлекаем левые кнопки (Settings, Home)
                var leftChildren = originalLeftFlow.Children.ToList();
                foreach (var child in leftChildren)
                {
                    string id = identifyDrawable(child);
                    originalLeftItems.Add(id);
                    ToolbarBlockContainer.DetachFromParent(child);

                    var block = new ToolbarBlockContainer(id, getFriendlyName(id, child), child);
                    bindBlockEvents(block);
                    allBlocks[id] = block;
                }

                // 2. Ищем и извлекаем селектор режимов (RulesetSelector)
                originalRulesetSelector = toolbar.GetType().GetField("rulesetSelector", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(toolbar) as Drawable
                                          ?? findVisualChild(toolbar, "ToolbarRulesetSelector")
                                          ?? findVisualChild(toolbar, "rulesetSelector");

                if (originalRulesetSelector != null)
                {
                    ToolbarBlockContainer.DetachFromParent(originalRulesetSelector);

                    string rulesetId = "rulesets";
                    originalLeftItems.Add(rulesetId);
                    var rulesetBlock = new ToolbarBlockContainer(rulesetId, getFriendlyName(rulesetId, originalRulesetSelector), originalRulesetSelector);
                    bindBlockEvents(rulesetBlock);
                    allBlocks[rulesetId] = rulesetBlock;
                    TweaksLog.Info("initManager: Extracted rulesetSelector successfully!");
                }

                // 3. Извлекаем правые кнопки (Rankings, News, Changelog, Wiki, Direct, Chat, Social, Music, Clock, Notifications, UserProfile, Plugins)
                originalRightItems.Clear();
                var rightChildren = originalRightFlow.Children.ToList();
                foreach (var child in rightChildren)
                {
                    string id = identifyDrawable(child);
                    originalRightItems.Add(id);
                    ToolbarBlockContainer.DetachFromParent(child);

                    var block = new ToolbarBlockContainer(id, getFriendlyName(id, child), child);
                    bindBlockEvents(block);
                    allBlocks[id] = block;
                }

                TweaksLog.Info($"initManager: Extracted total {allBlocks.Count} modular blocks.");
            }

            // Прячем ванильный GridContainer
            if (originalGridContainer != null)
            {
                originalGridContainer.Alpha = 0;
                originalGridContainer.AlwaysPresent = false;
            }

            if (Parent == null)
            {
                toolbar.Add(this);
            }

            // Прикрепляем плашку режима редактирования, контекстное меню и контейнер перетаскивания к корневому игровому контейнеру host.Game (полный экран 100vw x 100vh)
            if (host.Game is Container<Drawable> gameContainer)
            {
                if (editHintBanner.Parent == null) gameContainer.Add(editHintBanner);
                if (contextMenu.Parent == null) gameContainer.Add(contextMenu);
                if (dragGhostContainer.Parent == null) gameContainer.Add(dragGhostContainer);
            }
            else if (toolbar.Parent is Container<Drawable> gameRoot)
            {
                if (editHintBanner.Parent == null) gameRoot.Add(editHintBanner);
                if (contextMenu.Parent == null) gameRoot.Add(contextMenu);
                if (dragGhostContainer.Parent == null) gameRoot.Add(dragGhostContainer);
            }

            try
            {
                if (host.Data != null)
                {
                    string? path = host.Data.GetFullPath("layout.json");
                    if (path != null) configFilePath = path;
                    ToolbarPresetManager.Init(host.Data);
                }
            }
            catch { }

            // Загружаем сохраненный конфиг или схему по умолчанию
            var config = ToolbarLayoutConfig.Load(configFilePath);
            applyConfig(config);

            // Подписываемся на смену экранов для скрытия селектора рулсетов на ResultsScreen и других экранах, где он недопустим
            if (host.Game is OsuGame game)
            {
                var stack = game.ScreenStack;
                if (stack != null)
                {
                    stack.ScreenPushed += (_, next) => updateScreenRulesetVisibility(next);
                    stack.ScreenExited += (_, next) => updateScreenRulesetVisibility(next);
                    updateScreenRulesetVisibility(stack.CurrentScreen);
                }
            }

            overlayPositioner?.BindOverlays();

            if (OsuTweaksPlugin.Instance != null)
            {
                var plugin = OsuTweaksPlugin.Instance;
                plugin.UserProfileDisplayMode.BindValueChanged(e => ApplyUserProfileDisplayMode(e.NewValue, plugin.ProfileStatsPosition.Value), true);
                plugin.ProfileStatsPosition.BindValueChanged(e => ApplyUserProfileDisplayMode(plugin.UserProfileDisplayMode.Value, e.NewValue), true);

                styleManager?.Dispose();
                styleManager = new ToolbarStyleManager(host);
                styleManager.Attach(toolbar, plugin.FloatingIslandMode, plugin.ToolbarCornerRadius, plugin.ToolbarBackgroundOpacity, plugin.ToolbarHeight, plugin.NeonGlowLine, plugin.NeonGlowOffset, plugin.ToolbarAccentColor);
            }
        }

        private void updateScreenRulesetVisibility(IScreen? screen)
        {
            if (IsDisposed) return;

            if (isEditMode)
            {
                SaveAndExitEditMode();
            }

            if (allBlocks.TryGetValue("rulesets", out var rulesetBlock))
            {
                bool disallow = isScreenDisallowingRulesetChanges(screen);
                rulesetBlock.IsHiddenByScreen = disallow;
            }

            if (OsuTweaksPlugin.Instance != null)
            {
                ApplyUserProfileDisplayMode(OsuTweaksPlugin.Instance.UserProfileDisplayMode.Value, OsuTweaksPlugin.Instance.ProfileStatsPosition.Value);
            }
        }

        private static bool isScreenDisallowingRulesetChanges(IScreen? screen)
        {
            if (screen == null) return false;

            if (screen is IOsuScreen osuScreen)
            {
                return osuScreen.DisallowExternalBeatmapRulesetChanges;
            }

            string name = screen.GetType().Name;
            return name.Contains("Results") || name.Contains("Player") || name.Contains("Editor");
        }

        public void ApplyPreset(string presetName)
        {
            var config = ToolbarPresetManager.LoadPreset(presetName);
            applyConfig(config);
            config.Save(configFilePath);
            if (OsuTweaksPlugin.Instance != null)
            {
                OsuTweaksPlugin.Instance.ActivePresetName.Value = presetName;
            }
            host.Notify($"Пресет тулбара '{presetName}' применён", NotificationKind.Success);
        }

        public void ApplyConfig(ToolbarLayoutConfig config)
        {
            applyConfig(config);
            config.Save(configFilePath);
        }

        private void bindBlockEvents(ToolbarBlockContainer block)
        {
            block.OnBlockRightClicked += onBlockRightClicked;
            block.OnBlockDragStarted += onBlockDragStarted;
            block.OnBlockDragged += onBlockDragged;
            block.OnBlockDragEnded += onBlockDragEnded;
        }

        private ToolbarBlockContainer? getOrCreateBlock(ToolbarItemConfig item, ToolbarZone zone)
        {
            if (allBlocks.TryGetValue(item.Id, out var existing))
            {
                existing.IsHidden.Value = item.IsHidden;
                existing.CurrentZone = zone;
                return existing;
            }

            if (item.Id.StartsWith("spacer", StringComparison.OrdinalIgnoreCase))
            {
                var spacer = new ToolbarSpacer();
                var block = new ToolbarBlockContainer(item.Id, Localisation.OsuTweaksStrings.BlockSpacer.ToString(), spacer);
                block.IsHidden.Value = item.IsHidden;
                block.CurrentZone = zone;
                block.IsEditMode = isEditMode;
                bindBlockEvents(block);
                allBlocks[item.Id] = block;
                return block;
            }

            return null;
        }

        private void applyConfig(ToolbarLayoutConfig config)
        {
            leftZone.Flow.Clear(false);
            centerZone.Flow.Clear(false);
            rightZone.Flow.Clear(false);

            var incomingIds = new HashSet<string>(
                config.Left.Select(i => i.Id)
                .Concat(config.Center.Select(i => i.Id))
                .Concat(config.Right.Select(i => i.Id))
            );

            // Очищаем старые неиспользуемые спейсеры
            var spacersToRemove = allBlocks.Keys.Where(k => k.StartsWith("spacer", StringComparison.OrdinalIgnoreCase) && !incomingIds.Contains(k)).ToList();
            foreach (var k in spacersToRemove)
            {
                allBlocks.Remove(k);
            }

            var placed = new HashSet<string>();

            // Заполняем Лево
            int leftPos = 0;
            foreach (var item in config.Left)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Left);
                if (block != null)
                {
                    leftZone.Flow.Add(block);
                    leftZone.Flow.SetLayoutPosition(block, leftPos++);
                    placed.Add(item.Id);
                }
            }

            // Заполняем Центр
            int centerPos = 0;
            foreach (var item in config.Center)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Center);
                if (block != null)
                {
                    centerZone.Flow.Add(block);
                    centerZone.Flow.SetLayoutPosition(block, centerPos++);
                    placed.Add(item.Id);
                }
            }

            // Заполняем Право
            int rightPos = 0;
            foreach (var item in config.Right)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Right);
                if (block != null)
                {
                    rightZone.Flow.Add(block);
                    rightZone.Flow.SetLayoutPosition(block, rightPos++);
                    placed.Add(item.Id);
                }
            }

            // Все нераспределенные отправляем вправо
            foreach (var kvp in allBlocks)
            {
                if (!placed.Contains(kvp.Key))
                {
                    kvp.Value.CurrentZone = ToolbarZone.Right;
                    rightZone.Flow.Add(kvp.Value);
                    rightZone.Flow.SetLayoutPosition(kvp.Value, rightPos++);
                }
            }

            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();
            TweaksLog.Info($"applyConfig: Layout applied. Left={leftZone.Flow.Count}, Center={centerZone.Flow.Count}, Right={rightZone.Flow.Count}");
        }

        public void EnterEditMode()
        {
            if (isEditMode) return;
            isEditMode = true;

            leftZone.IsEditMode = true;
            centerZone.IsEditMode = true;
            rightZone.IsEditMode = true;

            if (editHintBanner.Parent == null)
            {
                if (toolbar?.Parent is Container<Drawable> parentContainer)
                {
                    parentContainer.Add(editHintBanner);
                }
                else if (host.Game is Container<Drawable> gameContainer)
                {
                    gameContainer.Add(editHintBanner);
                }
            }

            editHintBanner.FadeIn(200);
            TweaksLog.Info("EnterEditMode: Entered edit mode.");
        }

        public void SaveAndExitEditMode()
        {
            if (!isEditMode) return;

            var config = captureCurrentConfig();
            config.Save(configFilePath);

            isEditMode = false;
            leftZone.IsEditMode = false;
            centerZone.IsEditMode = false;
            rightZone.IsEditMode = false;

            editHintBanner.FadeOut(150);
            host.Notify(Localisation.OsuTweaksStrings.NotifyLayoutSaved, NotificationKind.Success);
            TweaksLog.Info("SaveAndExitEditMode: Saved and exited.");
        }

        public void CancelEditMode()
        {
            if (!isEditMode) return;

            var config = ToolbarLayoutConfig.Load(configFilePath);
            applyConfig(config);

            isEditMode = false;
            leftZone.IsEditMode = false;
            centerZone.IsEditMode = false;
            rightZone.IsEditMode = false;

            editHintBanner.FadeOut(150);
            host.Notify("Изменения отменены", NotificationKind.Info);
        }

        public void ResetToDefault()
        {
            if (IsDisposed) return;

            TweaksLog.Info("ResetToDefault: Applying default layout...");

            try
            {
                if (isEditMode)
                {
                    isEditMode = false;
                    leftZone.IsEditMode = false;
                    centerZone.IsEditMode = false;
                    rightZone.IsEditMode = false;
                    editHintBanner.FadeOut(150);
                }

                ApplyPreset("Default");
                host.Notify("Тулбар сброшен по умолчанию", NotificationKind.Info);
                TweaksLog.Info("ResetToDefault: Vanilla toolbar restored.");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("ResetToDefault error", ex);
            }
        }

        private ToolbarLayoutConfig captureCurrentConfig()
        {
            return new ToolbarLayoutConfig
            {
                Left = leftZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList(),
                Center = centerZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList(),
                Right = rightZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList()
            };
        }

        public ToolbarLayoutConfig GetCurrentConfig() => captureCurrentConfig();

        public void ShowSavePresetDialog(Action<string> onSaved)
        {
            var config = captureCurrentConfig();
            var dialog = new SavePresetDialog(config, name =>
            {
                if (string.IsNullOrWhiteSpace(name) || name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    host.Notify(Localisation.OsuTweaksStrings.NotifyDefaultPresetProtected, NotificationKind.Error);
                    return;
                }

                if (!ToolbarPresetManager.SaveCustomPreset(name, config))
                {
                    host.Notify(Localisation.OsuTweaksStrings.NotifyDefaultPresetProtected, NotificationKind.Error);
                    return;
                }

                onSaved(name);
                if (OsuTweaksPlugin.Instance != null)
                {
                    OsuTweaksPlugin.Instance.ActivePresetName.Value = name;
                }
                host.Notify($"Пресет \"{name}\" успешно сохранён!", NotificationKind.Success);
            });

            if (host.Game is Container<Drawable> gameContainer)
            {
                gameContainer.Add(dialog);
                dialog.ShowDialog();
            }
            else if (toolbar?.Parent is Container<Drawable> parentContainer)
            {
                parentContainer.Add(dialog);
                dialog.ShowDialog();
            }
        }

        public void ApplyUserProfileDisplayMode(UserProfileDisplayMode mode, ProfileStatsPosition statsPos = ProfileStatsPosition.Right)
        {
            if (IsDisposed) return;

            Scheduler.AddOnce(() =>
            {
                if (IsDisposed) return;

                try
                {
                    if (!allBlocks.TryGetValue("user_profile", out var userBlock))
                        return;

                    var userButton = userBlock.ContentDrawable;
                    if (userButton == null || !userButton.IsAlive)
                        return;

                    // Получаем точные поля кнопки ToolbarUserButton
                    var avatar = ReflectionHelper.GetFieldValue<Drawable>(userButton, "avatar")
                                 ?? userButton.ChildrenOfType<Drawable>().FirstOrDefault(d => d.GetType().Name.Contains("Avatar"));

                    var usernameText = ReflectionHelper.GetFieldValue<Drawable>(userButton, "usernameText")
                                       ?? userButton.ChildrenOfType<OsuSpriteText>().FirstOrDefault(t => 
                                           !t.Text.ToString().Contains("Ranking") && 
                                           !t.Text.ToString().Contains("Performance") &&
                                           !t.Text.ToString().StartsWith('#'));

                    if (avatar == null || usernameText == null)
                        return;

                    // Контейнер 32x32, оборачивающий аватарку
                    var avatarContainer = avatar.Parent ?? avatar;

                    // Находим непосредственный родительский FillFlowContainer для аватарки и ника
                    var parentFlow = (avatarContainer.Parent as FillFlowContainer) 
                                     ?? (usernameText.Parent as FillFlowContainer)
                                     ?? userButton.ChildrenOfType<FillFlowContainer>().FirstOrDefault(f => f.Contains(avatarContainer) && f.Contains(usernameText));

                    if (parentFlow == null)
                        return;

                    // Находим блок со статистикой (ранг # и PP)
                    var stats = ReflectionHelper.GetFieldValue<Drawable>(userButton, "statsContainer")
                                ?? ReflectionHelper.GetFieldValue<Drawable>(userButton, "stats")
                                ?? userButton.ChildrenOfType<Drawable>().FirstOrDefault(d => 
                                    d != avatarContainer && 
                                    d != usernameText && 
                                    (d.GetType().Name.Contains("Stat") || d.GetType().Name.Contains("Rank")));

                    int avatarPos = 1;
                    int namePos = 0;

                    switch (mode)
                    {
                        case UserProfileDisplayMode.Default:
                            avatarContainer.Alpha = 1;
                            avatarContainer.Width = 32;
                            avatarContainer.BypassAutoSizeAxes = Axes.None;
                            usernameText.Alpha = 1;
                            usernameText.BypassAutoSizeAxes = Axes.None;
                            usernameText.Margin = new MarginPadding { Right = 5f, Left = 0 };

                            namePos = 0;
                            avatarPos = 1;
                            break;

                        case UserProfileDisplayMode.AvatarLeft:
                            avatarContainer.Alpha = 1;
                            avatarContainer.Width = 32;
                            avatarContainer.BypassAutoSizeAxes = Axes.None;
                            usernameText.Alpha = 1;
                            usernameText.BypassAutoSizeAxes = Axes.None;
                            usernameText.Margin = new MarginPadding { Left = 5f, Right = 0 };

                            avatarPos = 0;
                            namePos = 1;
                            break;

                        case UserProfileDisplayMode.AvatarOnly:
                            avatarContainer.Alpha = 1;
                            avatarContainer.Width = 32;
                            avatarContainer.BypassAutoSizeAxes = Axes.None;
                            usernameText.Alpha = 0;
                            usernameText.BypassAutoSizeAxes = Axes.Both;
                            usernameText.Margin = new MarginPadding(0);

                            avatarPos = 0;
                            namePos = 1;
                            break;

                        case UserProfileDisplayMode.UsernameOnly:
                            avatarContainer.Alpha = 0;
                            avatarContainer.Width = 0;
                            avatarContainer.BypassAutoSizeAxes = Axes.Both;
                            usernameText.Alpha = 1;
                            usernameText.BypassAutoSizeAxes = Axes.None;
                            usernameText.Margin = new MarginPadding(0);

                            namePos = 0;
                            avatarPos = 1;
                            break;

                        case UserProfileDisplayMode.WithSeparator:
                        case UserProfileDisplayMode.AvatarLeftWithSep:
                            avatarContainer.Alpha = 1;
                            avatarContainer.Width = 32;
                            avatarContainer.BypassAutoSizeAxes = Axes.None;
                            usernameText.Alpha = 1;
                            usernameText.BypassAutoSizeAxes = Axes.None;

                            if (mode == UserProfileDisplayMode.AvatarLeftWithSep)
                            {
                                usernameText.Margin = new MarginPadding { Left = 8f, Right = 0 };
                                avatarPos = 0;
                                namePos = 1;
                            }
                            else
                            {
                                usernameText.Margin = new MarginPadding { Right = 8f, Left = 0 };
                                namePos = 0;
                                avatarPos = 1;
                            }
                            break;
                    }

                    if (parentFlow.Contains(avatarContainer)) parentFlow.SetLayoutPosition(avatarContainer, avatarPos);
                    if (parentFlow.Contains(usernameText)) parentFlow.SetLayoutPosition(usernameText, namePos);

                    // Управляем позицией блока статистики (ранг и PP): слева или справа
                    if (stats != null)
                    {
                        var targetFlow = (parentFlow.Contains(stats) ? parentFlow : null)
                                         ?? userButton.ChildrenOfType<FillFlowContainer>().FirstOrDefault(f => f.Contains(stats));

                        if (targetFlow != null)
                        {
                            targetFlow.SetLayoutPosition(stats, statsPos == ProfileStatsPosition.Left ? -10 : 10);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TweaksLog.Error($"ApplyUserProfileDisplayMode exception for mode {mode}", ex);
                }
            });
        }

        private void onZoneRightClicked(ToolbarZoneContainer zone, MouseDownEvent e)
        {
            showGlobalMenu(e.ScreenSpaceMouseDownPosition, zone);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                showGlobalMenu(e.ScreenSpaceMouseDownPosition, null);
                return true;
            }
            return base.OnMouseDown(e);
        }

        private void showGlobalMenu(Vector2 pos, ToolbarZoneContainer? clickedZone)
        {
            var menuItems = new List<ContextMenuItemData>();

            if (!isEditMode)
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonEnterEditMode,
                    Icon = FontAwesome.Solid.SlidersH,
                    Action = EnterEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonSavePreset,
                    Icon = FontAwesome.Solid.Save,
                    Action = () => ShowSavePresetDialog(_ => { })
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonOpenPresetsFolder,
                    Icon = FontAwesome.Solid.FolderOpen,
                    Action = ToolbarPresetManager.OpenPresetsFolder
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonResetToDefault,
                    Icon = FontAwesome.Solid.Undo,
                    Action = ResetToDefault
                });
            }
            else
            {
                // В РЕЖИМЕ РЕДАКТИРОВАНИЯ: Добавление спейсера на самом верху меню!
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuAddSpacer,
                    Icon = FontAwesome.Solid.Plus,
                    Action = () => addSpacer(clickedZone ?? rightZone)
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.EditBannerSaveButton,
                    Icon = FontAwesome.Solid.Check,
                    Action = SaveAndExitEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonSavePreset,
                    Icon = FontAwesome.Solid.Save,
                    Action = () => ShowSavePresetDialog(_ => { })
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonOpenPresetsFolder,
                    Icon = FontAwesome.Solid.FolderOpen,
                    Action = ToolbarPresetManager.OpenPresetsFolder
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.DialogCancelButton,
                    Icon = FontAwesome.Solid.Times,
                    Action = CancelEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ButtonResetToDefault,
                    Icon = FontAwesome.Solid.Undo,
                    IsDangerous = true,
                    Action = ResetToDefault
                });
            }

            contextMenu.ShowAt(pos, menuItems);
        }

        private void onBlockRightClicked(ToolbarBlockContainer block, MouseDownEvent e)
        {
            if (!isEditMode)
            {
                showGlobalMenu(e.ScreenSpaceMouseDownPosition, null);
                return;
            }

            var menuItems = new List<ContextMenuItemData>
            {
                new ContextMenuItemData
                {
                    Title = block.IsHidden.Value ? Localisation.OsuTweaksStrings.ContextMenuShow : Localisation.OsuTweaksStrings.ContextMenuHide,
                    Icon = block.IsHidden.Value ? FontAwesome.Solid.Eye : FontAwesome.Solid.EyeSlash,
                    Action = () => block.IsHidden.Value = !block.IsHidden.Value
                },
                new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuMoveLeft,
                    Icon = FontAwesome.Solid.ArrowLeft,
                    Action = () => moveBlockToZone(block, ToolbarZone.Left)
                },
                new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuMoveCenter,
                    Icon = FontAwesome.Solid.AlignCenter,
                    Action = () => moveBlockToZone(block, ToolbarZone.Center)
                },
                new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuMoveRight,
                    Icon = FontAwesome.Solid.ArrowRight,
                    Action = () => moveBlockToZone(block, ToolbarZone.Right)
                }
            };

            if (block.ContentDrawable is ToolbarSpacer)
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuRemoveSpacer,
                    Icon = FontAwesome.Solid.Trash,
                    IsDangerous = true,
                    Action = () => removeSpacer(block)
                });
            }
            else
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = Localisation.OsuTweaksStrings.ContextMenuResetBlock,
                    Icon = FontAwesome.Solid.Undo,
                    Action = () => resetBlockToDefault(block)
                });
            }

            contextMenu.ShowAt(e.ScreenSpaceMouseDownPosition, menuItems);
        }

        private void resetBlockToDefault(ToolbarBlockContainer block)
        {
            var def = ToolbarLayoutConfig.CreateDefault();
            int leftIdx = def.Left.FindIndex(i => i.Id == block.ItemId);
            if (leftIdx >= 0)
            {
                insertBlockIntoZone(block, leftZone, leftIdx);
                block.IsHidden.Value = false;
                host.Notify(Localisation.OsuTweaksStrings.NotifyBlockReset(block.DisplayName), NotificationKind.Info);
                return;
            }

            int rightIdx = def.Right.FindIndex(i => i.Id == block.ItemId);
            if (rightIdx >= 0)
            {
                insertBlockIntoZone(block, rightZone, rightIdx);
                block.IsHidden.Value = false;
                host.Notify(Localisation.OsuTweaksStrings.NotifyBlockReset(block.DisplayName), NotificationKind.Info);
                return;
            }

            insertBlockIntoZone(block, rightZone, rightZone.Flow.Count);
            block.IsHidden.Value = false;
        }

        private void insertBlockIntoZone(ToolbarBlockContainer block, ToolbarZoneContainer targetZone, int targetIndex)
        {
            var sourceZone = getZone(block.CurrentZone);
            sourceZone.Flow.Remove(block, false);

            var targetFlow = targetZone.Flow;
            var targetList = targetZone.GetVisualOrderedChildren();
            targetList.Remove(block);

            int insertIdx = Math.Clamp(targetIndex, 0, targetList.Count);
            targetList.Insert(insertIdx, block);

            targetFlow.Clear(false);
            for (int i = 0; i < targetList.Count; i++)
            {
                targetFlow.Add(targetList[i]);
                targetFlow.SetLayoutPosition(targetList[i], i);
            }

            block.CurrentZone = targetZone.Zone;

            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();

            TweaksLog.Info($"insertBlockIntoZone: '{block.DisplayName}' placed into zone {targetZone.Zone} at index {insertIdx} (total: {targetList.Count})");
        }

        private void moveBlockToZone(ToolbarBlockContainer block, ToolbarZone targetZone)
        {
            var targetContainer = getZone(targetZone);
            insertBlockIntoZone(block, targetContainer, targetContainer.GetVisualOrderedChildren().Count);
        }

        private void addSpacer(ToolbarZoneContainer zone)
        {
            string id = "spacer_" + Guid.NewGuid().ToString("N")[..6];
            var spacer = new ToolbarSpacer();
            var block = new ToolbarBlockContainer(id, "Разделитель", spacer);
            block.CurrentZone = zone.Zone;
            block.IsEditMode = true;
            bindBlockEvents(block);

            allBlocks[id] = block;
            insertBlockIntoZone(block, zone, zone.GetVisualOrderedChildren().Count);
        }

        private void removeSpacer(ToolbarBlockContainer block)
        {
            var flow = getZone(block.CurrentZone).Flow;
            flow.Remove(block, true);
            allBlocks.Remove(block.ItemId);
            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();
        }

        private ToolbarZoneContainer getZone(ToolbarZone zone) => zone switch
        {
            ToolbarZone.Left => leftZone,
            ToolbarZone.Center => centerZone,
            ToolbarZone.Right => rightZone,
            _ => leftZone
        };

        private void onBlockDragStarted(ToolbarBlockContainer block, DragStartEvent e)
        {
            draggingBlock = block;
            block.FadeTo(0.3f, 100);

            if (activeGhost != null) dragGhostContainer.Remove(activeGhost, true);

            activeGhost = new DragGhostBadge(block.DisplayName);
            activeGhost.Position = dragGhostContainer.ToLocalSpace(e.ScreenSpaceMouseDownPosition);
            dragGhostContainer.Add(activeGhost);

            TweaksLog.Info($"onBlockDragStarted: Dragging '{block.DisplayName}' ({block.ItemId})");
        }

        private void onBlockDragged(ToolbarBlockContainer block, DragEvent e)
        {
            if (draggingBlock == null) return;

            Vector2 mousePos = e.ScreenSpaceMousePosition;

            if (activeGhost != null)
            {
                activeGhost.Position = dragGhostContainer.ToLocalSpace(mousePos);
            }

            var hoverZone = getZoneUnderMouse(mousePos);

            if (hoverZone != null)
            {
                currentTargetZone = hoverZone;
                currentTargetIndex = hoverZone.GetInsertionIndexForPosition(mousePos);

                leftZone.HideInsertIndicator();
                centerZone.HideInsertIndicator();
                rightZone.HideInsertIndicator();

                hoverZone.ShowInsertIndicator(currentTargetIndex);
            }
        }

        private void onBlockDragEnded(ToolbarBlockContainer block, DragEndEvent e)
        {
            if (draggingBlock == null) return;

            block.FadeTo(block.IsHidden.Value ? 0.35f : 1f, 100);
            leftZone.HideInsertIndicator();
            centerZone.HideInsertIndicator();
            rightZone.HideInsertIndicator();

            if (activeGhost != null)
            {
                activeGhost.FadeOut(100).Expire();
                activeGhost = null;
            }

            var mousePos = e.ScreenSpaceMousePosition;
            var targetZone = currentTargetZone ?? getZoneUnderMouse(mousePos) ?? getZone(block.CurrentZone);
            int targetIndex = currentTargetZone != null ? currentTargetIndex : targetZone.GetInsertionIndexForPosition(mousePos);

            insertBlockIntoZone(block, targetZone, targetIndex);

            draggingBlock = null;
            currentTargetZone = null;
        }

        private ToolbarZoneContainer? getZoneUnderMouse(Vector2 screenSpacePos)
        {
            if (leftZone.ReceivePositionalInputAt(screenSpacePos)) return leftZone;
            if (centerZone.ReceivePositionalInputAt(screenSpacePos)) return centerZone;
            if (rightZone.ReceivePositionalInputAt(screenSpacePos)) return rightZone;

            Vector2 local = ToLocalSpace(screenSpacePos);
            float width = DrawWidth;
            if (local.X < width / 3f) return leftZone;
            if (local.X > (width * 2f) / 3f) return rightZone;
            return centerZone;
        }

        private static string identifyDrawable(Drawable d)
        {
            string typeName = d.GetType().Name;

            if (typeName.Contains("Settings")) return "settings";
            if (typeName.Contains("Home")) return "home";
            if (typeName.Contains("Ruleset")) return "rulesets";
            if (typeName.Contains("Clock")) return "clock";
            if (typeName.Contains("Notification")) return "notifications";
            if (typeName.Contains("Ranking") || typeName.Contains("Performance") || typeName.Contains("Historical")) return "rankings";
            if (typeName.Contains("News")) return "news";
            if (typeName.Contains("Changelog")) return "changelog";
            if (typeName.Contains("Wiki")) return "wiki";
            if (typeName.Contains("BeatmapListing") || typeName.Contains("Direct")) return "beatmap_listing";
            if (typeName.Contains("Chat")) return "chat";
            if (typeName.Contains("Social")) return "social";
            if (typeName.Contains("Music")) return "music";
            if (typeName.Contains("User")) return "user_profile";
            if (d is ToolbarSpacer) return "spacer_" + Guid.NewGuid().ToString("N")[..6];

            return typeName.ToLowerInvariant();
        }

        private static string getFriendlyName(string id, Drawable d) => id switch
        {
            "settings" => Localisation.OsuTweaksStrings.BlockSettings.ToString(),
            "home" => Localisation.OsuTweaksStrings.BlockHome.ToString(),
            "rulesets" => Localisation.OsuTweaksStrings.BlockRulesets.ToString(),
            "clock" => Localisation.OsuTweaksStrings.BlockClock.ToString(),
            "notifications" => Localisation.OsuTweaksStrings.BlockNotifications.ToString(),
            "rankings" => Localisation.OsuTweaksStrings.BlockRankings.ToString(),
            "news" => Localisation.OsuTweaksStrings.BlockNews.ToString(),
            "changelog" => Localisation.OsuTweaksStrings.BlockChangelog.ToString(),
            "wiki" => Localisation.OsuTweaksStrings.BlockWiki.ToString(),
            "beatmap_listing" => Localisation.OsuTweaksStrings.BlockBeatmaps.ToString(),
            "chat" => Localisation.OsuTweaksStrings.BlockChat.ToString(),
            "social" => Localisation.OsuTweaksStrings.BlockSocial.ToString(),
            "music" => Localisation.OsuTweaksStrings.BlockMusic.ToString(),
            "user_profile" => Localisation.OsuTweaksStrings.BlockUserProfile.ToString(),
            _ => d is ToolbarSpacer ? Localisation.OsuTweaksStrings.BlockSpacer.ToString() : d.GetType().Name
        };

        private static Drawable? findGridContainer(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child is GridContainer gc)
                    return gc;

                var nested = findGridContainer(child);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static Drawable? findVisualChild(Drawable root, string typeOrName)
        {
            foreach (var child in getChildren(root))
            {
                if (child.GetType().Name.Contains(typeOrName) || child.Name == typeOrName)
                    return child;

                var nested = findVisualChild(child, typeOrName);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static FillFlowContainer? findRightFlow(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child.Name == "Right buttons" || child.Name == "Right flow")
                {
                    var flow = getChildren(child).OfType<FillFlowContainer>().FirstOrDefault() ?? (child as FillFlowContainer);
                    if (flow != null)
                        return flow;
                }

                if (child is FillFlowContainer f && (f.Anchor == Anchor.TopRight || f.Origin == Anchor.TopRight))
                    return f;

                var found = findRightFlow(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static FillFlowContainer? findLeftFlow(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child.Name == "Left buttons" || child.Name == "Left flow")
                {
                    var flow = getChildren(child).OfType<FillFlowContainer>().FirstOrDefault() ?? (child as FillFlowContainer);
                    if (flow != null)
                        return flow;
                }

                if (child is FillFlowContainer f && (f.Anchor == Anchor.TopLeft || f.Origin == Anchor.TopLeft))
                    return f;

                var found = findLeftFlow(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static IEnumerable<Drawable> getChildren(Drawable drawable)
        {
            if (drawable == null) yield break;

            var childrenProp = drawable.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (childrenProp?.GetValue(drawable) is IEnumerable<Drawable> children)
            {
                foreach (var child in children)
                    yield return child;
            }

            var contentProp = drawable.GetType().GetProperty("Content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (contentProp?.GetValue(drawable) is IEnumerable content)
            {
                foreach (var row in content)
                {
                    if (row is Drawable cellDrawable)
                    {
                        yield return cellDrawable;
                    }
                    else if (row is IEnumerable rowContent)
                    {
                        foreach (var cell in rowContent)
                        {
                            if (cell is Drawable inner)
                                yield return inner;
                        }
                    }
                }
            }
        }

        private sealed partial class DragGhostContainer : Container
        {
            public override bool HandlePositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => false;
        }

        private sealed partial class DragGhostBadge : CompositeDrawable
        {
            public override bool HandlePositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => false;

            public DragGhostBadge(string title)
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;
                Depth = float.MinValue;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderThickness = 2,
                        BorderColour = Colour4.FromHex("#ff66aa"),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.Black.Opacity(0.85f)
                            },
                            new OsuSpriteText
                            {
                                Padding = new MarginPadding { Horizontal = 10, Vertical = 6 },
                                Text = title,
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold),
                                Colour = Colour4.White
                            }
                        }
                    }
                };
            }
        }
    }
}
