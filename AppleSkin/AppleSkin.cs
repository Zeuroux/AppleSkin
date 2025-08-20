using AppleSkin.Extensions;
using AppleSkin.Helpers;
using AppleSkin.TextureUV;
using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

namespace AppleSkin
{
    public class AppleSkin : OnixPluginBase
    {
        private static TexturePath? Outline;

        private const float ExhaustionWidth = 80f;
        private const float Full = 1f;

        private const string visibleTrue = "\"#visible\":true";
        private const string hudScreen = "hud_screen";

        private static readonly Random rand = new();
        private static readonly Vec2 size = new(9);

        private static float unclampedFlashAlpha = 0f, flashAlpha = 0f;
        private static sbyte alphaDir = 1;

        private static ushort UpdateCounter = 0;
        private static bool ShouldJitter = false;

        private static float exhaustion_percent = 0f, health_increment = 0f;
        private static FoodItem? heldFood;
        private static GameMode Gamemode;
        private static bool HasHungerEffect = false;


        private static HudElements? AttributeBars;
        private static HudAttributes PlayerAttributes = new();

        public static AppleSkin Instance { get; private set; } = null!;
        public static AppleSkinConfig Config { get; private set; } = null!;

        public AppleSkin(OnixPluginInitInfo initInfo) : base(initInfo)
        {
            Instance = this;
            DisablingShouldUnloadPlugin = false;
#if DEBUG
            //base.WaitForDebuggerToBeAttached();
#endif
        }
    
        protected override void OnLoaded()
        {
            Console.WriteLine("Gaming");
            Config = new AppleSkinConfig(PluginDisplayModule);
            Onix.Events.Rendering.PreRenderScreenGame += OnPreRenderScreenGame;
            Onix.Events.Rendering.RenderScreenGame += OnRenderScreenGame;
            Onix.Events.Gui.ContainerScreenTick += OnContainerScreenTick;
            Onix.Events.Common.Tick += OnTick;
            Onix.Events.Input.Input += OnInput;
            Onix.Events.Session.SessionLeft += OnSessionLeave;
        }

        private void OnSessionLeave()
        {
            AttributeBars = null;
        }

        private void OnTick()
        {
            if (Onix.Gui.ScreenName != hudScreen) return;
            if (Onix.LocalPlayer is not LocalPlayer localPlayer) return;
            HasHungerEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Hunger);
            Outline = HasHungerEffect ? Textures.hungered_outline : Textures.normal_outline;
            if (localPlayer.GetAttribute(EntityAttributeId.Health) is EntityAttribute health) { 
                PlayerAttributes.Health = (int)health.Value;
                PlayerAttributes.HealthMax = (int)health.MaxValue;
                Console.WriteLine(health.MaxValue);
            }
            if (localPlayer.GetAttribute(EntityAttributeId.Saturation) is EntityAttribute saturation) PlayerAttributes.Saturation = saturation.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Hunger) is EntityAttribute hunger) PlayerAttributes.Hunger = (int)hunger.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Exhaustion) is EntityAttribute rawExhaustion) {
                exhaustion_percent = Math.Min(rawExhaustion.Value, 4) / 4;
                PlayerAttributes.Exhaustion = rawExhaustion.Value;
            }
            Gamemode = localPlayer.GameMode;
            if (AttributeBars == null)
                AttributeBars = new();
            else
                AttributeBars.Update();
            var item = localPlayer.MainHandItem.Item;
            if (!FoodHelper.GetFoodItem(item, out heldFood) || (!(heldFood.AlwaysConsumable || item!.Name == "apple") && PlayerAttributes.Hunger == 20))
                heldFood = new FoodItem { Hunger = 0, Saturation = 0f };
            health_increment = heldFood.GetEstimatedHealthIncrement(PlayerAttributes);
            unclampedFlashAlpha += alphaDir * 0.125f;
            if (unclampedFlashAlpha >= 1.5f)
                alphaDir = -1;
            else if (unclampedFlashAlpha <= -0.5f)
                alphaDir = 1;
            flashAlpha = MathF.Max(0F, MathF.Min(1F, unclampedFlashAlpha)) * MathF.Max(0F, MathF.Min(1F, Config.MaxFlashingHudIconAlpha));
            ShouldJitter = PlayerAttributes.Saturation <= 0f && UpdateCounter % (PlayerAttributes.Hunger * 3 + 1) == 0 && Config.AnimateHudIcons;
        }

        private void OnPreRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi)
        {
            if (!(Gamemode == GameMode.Survival || Gamemode == GameMode.Adventure)) return;
            if (Outline == null) return;
            CacheOutline(gfx, HungerTextureUV.Whole(HasHungerEffect), Outline);
            if (screenName != hudScreen || AttributeBars == null) return;
            if (AttributeBars == null || heldFood == null) return;
            RenderHungerBar(gfx, AttributeBars.Hunger, heldFood);
            RenderHealthBar(gfx, AttributeBars.Armor, heldFood);
        }

        private static void RenderHealthBar(RendererGame gfx, Vec2 position, FoodItem heldFood)
        {
            var healthMid = MathF.Ceiling(PlayerAttributes.HealthMax / 2);
            var Background = HungerTextureUV.Background();

            for (int i = 0; i < healthMid; i++)
            {
                var x = i % 10 * 8;
                var y = MathF.Floor(i / 10) * -10;
                var rect = Rect.FromSize(position, size).MoveBy(x , y);
                gfx.RenderTexture(rect, Textures.icons, 1f, Background);

            }
        }

        private static void RenderHungerBar(RendererGame gfx, Vec2 position, FoodItem heldFood)
        {
            
            float expectedHunger = MathF.Min(PlayerAttributes.Hunger + heldFood.Hunger, 20f);
            float expectedSaturation = MathF.Min(MathF.Min(expectedHunger, PlayerAttributes.Saturation + heldFood.Saturation), 20f);
            if (Config.ShowExhaustionOverlay)
            {
                float exhaustionWidth = MathF.Round(ExhaustionWidth * exhaustion_percent);
                float x = position.X;
                float y = position.Y;
                float left = Full - exhaustionWidth / ExhaustionWidth;
                gfx.RenderTexture(new(x - exhaustionWidth, y, x + 1, y + 9), Textures.exhaustion_background, .7f, new(left, 0f, Full, Full));
            }

            bool renderExpectedHunger = Config.ShowHungerRestoredFromHeldFood && expectedHunger != PlayerAttributes.Hunger;
            bool renderExpectedSaturation = Config.ShowHungerRestoredFromHeldFood && expectedSaturation != PlayerAttributes.Saturation;
            var Background = HungerTextureUV.Background(HasHungerEffect);
            var Whole = HungerTextureUV.Whole(HasHungerEffect);
            var Half = HungerTextureUV.Half(HasHungerEffect);
            for (int i = 0; i < 10; i++)
            {
                Rect rect = Rect.FromSize(position, size).MoveBy((-i - 1) * 8, ShouldJitter ? -rand.Next(2) : 0);
                gfx.RenderTexture(rect, Textures.icons, 1f, Background);

                int idk = (i << 1) | 1;
                if (renderExpectedHunger)
                    IconRender.Hunger(gfx, Whole, Half, rect, idk, expectedHunger, flashAlpha);
                IconRender.Hunger(gfx, Whole, Half, rect, idk, PlayerAttributes.Hunger, 1f);

                if (!Config.ShowSaturationOverlay) continue;
                if (renderExpectedSaturation)
                    IconRender.Saturation(gfx, Outline!, rect, idk, expectedSaturation, ColorF.Lerp(ColorF.Transparent, ColorF.Yellow, flashAlpha));
                IconRender.Saturation(gfx, Outline!, rect, idk, PlayerAttributes.Saturation, ColorF.Yellow);
            }
            UpdateCounter++;
        }

        // Mainly tooltip stuff

        private static bool StartScrolling = false;
        private static Vec2 ScrollOffset = Vec2.Zero;
        private static Vec2 TargetScrollOffset = Vec2.Zero;
        private static bool ScrollHorizontally = false;

        private static GameUIElement? Hovered;
        private static Rect? HoveredRect;
        private static FoodItem? HoveredFood;
        private static string CurrentHoverText = string.Empty;
        private static ItemStack? lastItemStack;
        private static int lastSlot = -1;
        private static void ResetHovered()
        {
            Hovered = null;
            HoveredRect = null;
            HoveredFood = null;
        }

        private static readonly float lerpSpeed = 10f;

        private bool OnInput(InputKey key, bool isDown)
        {
            if (!Onix.Client.Modules.First(M => M.SaveName == "module.better_tooltips.name").Enabled)
                return false;

            if (key == InputKey.Type.Ctrl)
            {
                StartScrolling = isDown;
                if (!isDown)
                    TargetScrollOffset = Vec2.Zero;
            }
            else if (key == InputKey.Type.Shift)
                ScrollHorizontally = isDown;
            if (HoveredFood is { } && key == InputKey.Type.Scroll)
                if (StartScrolling)
                    (ScrollHorizontally ? ref TargetScrollOffset.X : ref TargetScrollOffset.Y) += isDown ? 10 : -10;
                else return true;

            return false;
        }
        private void OnContainerScreenTick(ContainerScreen container)
        {
            if ((!Config.ShowFoodValuesInTooltip || !ScrollHorizontally) && !Config.AlwaysShowFoodValuesInTooltip) return;

            if (container.GetHoveredItem() is ItemStack itemStack && (lastSlot != container.HoveredSlot || !itemStack.Matches(lastItemStack)))
            {
                if (itemStack.Item is { } food && FoodHelper.GetFoodItem(food, out HoveredFood))
                {
                    Onix.Gui.RootUiElement!.FindMatchRecursive(
                        what: e => e.Name == "hover_text" &&
                                  e.JsonProperties.Contains("#hover_text") &&
                                  e.Parent is { Name: "highlight" } highlight &&
                                  highlight.JsonProperties.Contains(visibleTrue) &&
                                  highlight.Rect is { Width: > 0, Height: > 0 } &&
                                  highlight.Parent is { Name: "hover" } hover &&
                                  hover.Children.Any(sibling => sibling.Name == "white_border" && sibling.JsonProperties.Contains(visibleTrue)),
                        whenFound: e => {
                            if (e.Parent?.Parent?.Parent?.Parent?.FindChildRecursive("overlay")?.JsonProperties is { } prop && RegexPatterns.ItemCategoryRegex().Match(prop).Success)
                            {
                                ResetHovered();
                                return true;
                            }
                            var match = RegexPatterns.HoverTextRegex().Match(e.JsonProperties);
                            if (match.Success)
                            {
                                CurrentHoverText = match.Groups[1].Value.Replace("\\n", "\n");
                                e.JsonProperties = "{}";
                                if (e.Parent?.Parent?.Parent is { } hover)
                                    HoveredRect = hover.Rect.MoveTo(hover.GetAbsolutePosition());
                                Hovered = e;
                            }
                            return true;
                        });
                }
                else ResetHovered();
                TargetScrollOffset = Vec2.Zero;
                lastSlot = container.HoveredSlot;
                lastItemStack = itemStack.Clone();
            }
            if (Hovered != null)
            {
                if (Hovered.JsonProperties != "{}")
                    Hovered.JsonProperties = "{}";
                if (Hovered is { Parent.Rect: not { Width: > 0, Height: > 0 } })
                {
                    ResetHovered();
                    lastItemStack = null;
                    TargetScrollOffset = Vec2.Zero;
                    return;
                }
            }
        }
        private void OnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUI)
        {
            if ((!Config.ShowFoodValuesInTooltip || !ScrollHorizontally) && !Config.AlwaysShowFoodValuesInTooltip) return;
            if (HoveredFood == null) return;
            if (screenName == hudScreen) { ResetHovered(); return; }
            ScrollOffset = ScrollOffset.Lerp(TargetScrollOffset, MathF.Min(lerpSpeed * delta, 1f));
            gfx.FontType = FontType.Mojangles;
            Vec2 startPosition = Onix.Gui.MousePosition;
            var tooltipOffset = TooltipSizes.Offset;
            bool add10 = true; 
            if (HoveredRect is {} hoveredRect && hoveredRect.Contains(startPosition) == false)
            {
                tooltipOffset = Vec2.Zero;
                startPosition = hoveredRect.Center;
            }
            Vec2 tooltipPos = startPosition + tooltipOffset;
            Vec2 itemInfoSize = gfx.MeasureText(CurrentHoverText);
            int hungerMid = (int)MathF.Ceiling(HoveredFood.Hunger / 2f);
            int saturationMid = (int)MathF.Ceiling(HoveredFood.Saturation / 2f);
            var foodInfoWidth = MathF.Max(9 * hungerMid, 9 * saturationMid);
            itemInfoSize.X = MathF.Max(itemInfoSize.X, foodInfoWidth);

            Vec2 tooltipSize = itemInfoSize + new Vec2(0, 18) + TooltipSizes.Padding * 2;

            Vec2 screenSize = Onix.Gui.ScreenSize;
            
            if ((tooltipPos.X += tooltipPos.X + tooltipSize.X > screenSize.X ? -tooltipSize.X - tooltipOffset.X * 2 : 0) < 0)
            {
                tooltipPos.X += tooltipSize.X / 2 + tooltipOffset.X;
                tooltipPos.Y -= tooltipSize.Y + tooltipOffset.Y;
                add10 = false;
            }
            if (add10 && tooltipOffset == Vec2.Zero)
                tooltipPos += new Vec2(10);
            
            if (tooltipPos.Y + tooltipSize.Y > screenSize.Y)
                tooltipPos.Y -= tooltipPos.Y + tooltipSize.Y - screenSize.Y;
            else if (tooltipPos.Y < 0)
            {
                tooltipPos.Y = 0;
                tooltipPos.X = startPosition.X + tooltipOffset.X;
            }
            
            tooltipPos += ScrollOffset;
            Textures.tooltip.Render(gfx, Rect.FromSize(tooltipPos, tooltipSize), 1f);
            gfx.RenderText(tooltipPos + TooltipSizes.Padding + new Vec2(0.4f, 0.15f), ColorF.White, CurrentHoverText);
            var Background = HungerTextureUV.Background();
            var Whole = HungerTextureUV.Whole();
            var Half = HungerTextureUV.Half();
            void RenderBackground(Rect rect) { gfx.RenderTexture(rect, Textures.icons, 1f, Background); }
            for (int i = 0; i < Math.Max(hungerMid, saturationMid); ++i)
            {
                int value = (i << 1) | 1;
                Rect rect = Rect.FromSize(tooltipPos + TooltipSizes.Padding, size).MoveDown(itemInfoSize.Y);
                IconRender.Hunger(gfx, Whole, Half, rect.MoveBy(new Vec2(9 * (hungerMid - i - 1), 0)), value, HoveredFood.Hunger, 1f, RenderBackground);
                IconRender.Saturation(gfx, Textures.normal_outline, rect.MoveBy(new Vec2(9 * (saturationMid - i - 1), 9)), value, HoveredFood.Saturation, ColorF.Yellow, RenderBackground);
            }
        }
        // End of Mainly tooltip stuff
        private static void CacheOutline(RendererGame gfx, Rect uvRectWhole, TexturePath outline)
        {

            if (gfx.GetTextureStatus(outline) != RendererTextureStatus.Missing) return;
            AttributeBars = new();

            RawImageData hungerWhole = ImageHelpers.CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(Textures.icons)), uvRectWhole);

            int width = hungerWhole.Width;
            int height = hungerWhole.Height;
            bool touchesEdge = Enumerable.Range(0, width).Any(x =>
                hungerWhole.GetPixel(x, 0).A > 0 || hungerWhole.GetPixel(x, height - 1).A > 0) ||
                Enumerable.Range(0, height).Any(y =>
                hungerWhole.GetPixel(0, y).A > 0 || hungerWhole.GetPixel(width - 1, y).A > 0);

            gfx.UploadTexture(outline, ImageHelpers.CreateOutline(hungerWhole, touchesEdge));
        }

        protected override void OnEnabled()
        {
            AttributeBars = new();
        }

        protected override void OnDisabled()
        {
            Onix.Gui.ScheduleScreenRelayout();
            Console.WriteLine("Disabled");
        }

        protected override void OnUnloaded()
        {
            Onix.Events.Rendering.PreRenderScreenGame -= OnPreRenderScreenGame;
            Onix.Events.Rendering.RenderScreenGame -= OnRenderScreenGame;
            Onix.Events.Gui.ContainerScreenTick -= OnContainerScreenTick;
            Onix.Events.Common.Tick -= OnTick;
            Onix.Events.Input.Input -= OnInput;
            Onix.Events.Session.SessionLeft -= OnSessionLeave;
        }
    }
}