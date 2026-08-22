using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Input;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;

namespace OsuTweaks.UI
{
    /// <summary>
    /// Модальное диалоговое окно для сохранения текущей раскладки тулбара в именованный JSON-пресет.
    /// </summary>
    public sealed partial class SavePresetDialog : CompositeDrawable
    {
        private readonly ToolbarLayoutConfig currentConfig;
        private readonly Action<string> onPresetSaved;

        private OsuTextBox textBox = null!;
        private OsuSpriteText errorText = null!;
        private Container dialogCard = null!;

        public SavePresetDialog(ToolbarLayoutConfig currentConfig, Action<string> onPresetSaved)
        {
            this.currentConfig = currentConfig;
            this.onPresetSaved = onPresetSaved;

            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            Depth = float.MinValue;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                // Затемняющий полноэкранный фон
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(0.65f)
                },
                // Центральная карточка диалога
                dialogCard = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 420,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 12,
                    BorderThickness = 1.5f,
                    BorderColour = colours.Pink.Opacity(0.6f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#16161c")
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 14),
                            Padding = new MarginPadding(20),
                            Children = new Drawable[]
                            {
                                // Заголовок с иконкой
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new SpriteIcon
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(18),
                                            Icon = FontAwesome.Solid.Save,
                                            Colour = colours.PinkLight
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Text = Localisation.OsuTweaksStrings.DialogSavePresetTitle,
                                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                                            Colour = Colour4.White
                                        }
                                    }
                                },
                                // Описание
                                new OsuSpriteText
                                {
                                    Text = Localisation.OsuTweaksStrings.DialogSavePresetPrompt,
                                    Font = OsuFont.GetFont(size: 13, weight: FontWeight.Regular),
                                    Colour = Colour4.White.Opacity(0.8f)
                                },
                                // Поле ввода
                                textBox = new OsuTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 36,
                                    PlaceholderText = "My Custom Layout",
                                    CommitOnFocusLost = false
                                },
                                // Сообщение об ошибке (при валидации)
                                errorText = new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                    Colour = Colour4.Red.Lighten(0.3f),
                                    Alpha = 0
                                },
                                // Кнопки Действий
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                    Children = new Drawable[]
                                    {
                                        new RoundedButton
                                        {
                                            Width = 180,
                                            Height = 38,
                                            Text = Localisation.OsuTweaksStrings.DialogSaveButton,
                                            BackgroundColour = colours.Pink,
                                            Action = trySave
                                        },
                                        new RoundedButton
                                        {
                                            Width = 180,
                                            Height = 38,
                                            Text = Localisation.OsuTweaksStrings.DialogCancelButton,
                                            BackgroundColour = Colour4.FromHex("#2a2a36"),
                                            Action = Dismiss
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            textBox.OnCommit += (_, _) => trySave();
        }

        public void ShowDialog()
        {
            this.FadeIn(200);
            dialogCard.ScaleTo(0.9f).ScaleTo(1f, 250, Easing.OutBack);
            Schedule(() => GetContainingFocusManager()?.ChangeFocus(textBox));
        }

        public void Dismiss()
        {
            this.FadeOut(150).Expire();
        }

        private void trySave()
        {
            string name = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                showError("Пожалуйста, введите название пресета.");
                return;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (name.Any(c => invalidChars.Contains(c)))
            {
                showError("Название содержит недопустимые символы (\\ / : * ? \" < > |).");
                return;
            }

            ToolbarPresetManager.SaveCustomPreset(name, currentConfig);
            onPresetSaved.Invoke(name);
            Dismiss();
        }

        private void showError(string message)
        {
            errorText.Text = message;
            errorText.FadeIn(150);
            dialogCard.FlashColour(Colour4.Red.Opacity(0.3f), 200);
        }

        public override bool HandlePositionalInput => true;
        public override bool HandleNonPositionalInput => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // Закрываем при клике вне карточки диалога
            if (!dialogCard.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition))
            {
                Dismiss();
                return true;
            }
            return true;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.Escape)
            {
                Dismiss();
                return true;
            }
            return base.OnKeyDown(e);
        }
    }
}
