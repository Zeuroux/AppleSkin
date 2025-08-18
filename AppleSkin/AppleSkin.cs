using AppleSkin.Extensions;
using AppleSkin.Helpers;
using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;

namespace AppleSkin
{
    public class AppleSkin : OnixPluginBase
    {
        private const float FrameThreshold = 1 / 60;
        private const string visibleTrue = "\"#visible\":true";
        private static readonly Random rand = new();
        private static readonly Vec2 tooltipPadding = new(4f, 4f);
        private static readonly Vec2 tooltipOffset = new(10, -10);
        private static readonly Vec2 size = new(9);
        private static GameUIElement? hovered;
        private static HudElements? AttributeBars;
        private static HudAttributes PlayerAttributes = new();
        private static float exhaustion_percent = 0f, health_increment = 0f, unclampedFlashAlpha = 0f, flashAlpha = 0f, accumulator = 0f;
        private static ushort updateCounter = 0;
        private static bool shouldUpload = true, shouldJitter = false;
        private static FoodItem? heldFood, hoveredFood;
        private static sbyte alphaDir = 1;
        private static GameMode gamemode;
        private static bool scrollable = false;
        private static Vec2 scrollOffset = Vec2.Zero;
        private static bool isHoldingShift = false;
        private const float ExhaustionWidth = 81f;
        private const float Full = 1f;
        private const string hudScreen = "hud_screen";
        static TexturePath? outline;
        static string CurrentHoverText = string.Empty;
        
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
            hovered = null;
        }

        private bool OnInput(InputKey key, bool isDown)
        {
            if (key == InputKey.Type.Scroll && hoveredFood is {} && scrollable)
            {
                (isHoldingShift ? ref scrollOffset.X : ref scrollOffset.Y) += (isDown ? 1 : -1) * 9;
                return true;
            }
            else if (key == InputKey.Type.Shift)
                isHoldingShift = isDown;
            return false;
        }

        private static bool HasHungerEffect = false;


        private void OnTick()
        {
            if (Onix.Gui.ScreenName != hudScreen) return;
            if (Onix.LocalPlayer is not LocalPlayer localPlayer) return;
            HasHungerEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Hunger);
            outline = HasHungerEffect ? Textures.hungered_outline : Textures.normal_outline;
            if (localPlayer.GetAttribute(EntityAttributeId.Saturation) is EntityAttribute s) PlayerAttributes.Saturation = s.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Hunger) is EntityAttribute h) PlayerAttributes.Hunger = (int)h.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Exhaustion) is EntityAttribute rawExhaustion) {
                exhaustion_percent = Math.Min(rawExhaustion.Value, 4) / 4;
                PlayerAttributes.Exhaustion = rawExhaustion.Value;
            }
            gamemode = localPlayer.GameMode;
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
            shouldJitter = PlayerAttributes.Saturation <= 0f && updateCounter % (PlayerAttributes.Hunger * 3 + 1) == 0 && Config.AnimateHudIcons;
        }

        ItemStack? lastItemStack;
        int lastSlot = -1;
        private void OnContainerScreenTick(ContainerScreen container)
        {
            if ((!Config.ShowFoodValuesInTooltip || !isHoldingShift) && !Config.AlwaysShowFoodValuesInTooltip) return;
            if (container.GetHoveredItem() is ItemStack itemStack && (lastSlot != container.HoveredSlot || !itemStack.Matches(lastItemStack)))
            {
                if (itemStack.Item is {} food && FoodHelper.GetFoodItem(food, out hoveredFood))
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
                                hoveredFood = null;
                                return true;
                            }
                            var match = RegexPatterns.HoverTextRegex().Match(e.JsonProperties);
                            if (match.Success)
                            {
                                CurrentHoverText = match.Groups[1].Value.Replace("\\n", "\n");
                                e.JsonProperties = "{}";
                                hovered = e;
                            }

                            return true;
                        });
                }
                else hoveredFood = null;
                scrollOffset = Vec2.Zero;
                lastSlot = container.HoveredSlot;
                lastItemStack = itemStack.Clone();
            }
            if (hovered != null)
            {
                if (hovered.JsonProperties != "{}")
                    hovered.JsonProperties = "{}";
                if (hovered is { Parent.Rect: not { Width: > 0, Height: > 0 } })
                {
                    hoveredFood = null;
                    lastItemStack = null;
                    scrollOffset = Vec2.Zero;
                    return;
                }
            }
        }

        private void OnPreRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi)
        {
            if (outline == null) return;
            CacheOutline(gfx, HungerTextureUV.Whole(HasHungerEffect), outline);
            if (screenName != hudScreen || AttributeBars == null) return;
            RenderHungerBar(gfx, delta);
        }

        private void OnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUI)
        {
            if ((!Config.ShowFoodValuesInTooltip || !isHoldingShift) && !Config.AlwaysShowFoodValuesInTooltip) return;
            if (screenName == hudScreen) { hoveredFood = null; }
            if (hoveredFood == null) return;
            gfx.FontType = FontType.Mojangles;
            Vec2 mousePosition = Onix.Gui.MousePosition;
            Vec2 tooltipPos = mousePosition + tooltipOffset;
            Vec2 itemInfoSize = gfx.MeasureText(CurrentHoverText);
            int hungerMid = (int)MathF.Ceiling(hoveredFood.Hunger / 2f);
            int saturationMid = (int)MathF.Ceiling(hoveredFood.Saturation / 2f);
            var foodInfoWidth = MathF.Max(9 * hungerMid, 9 * saturationMid);
            itemInfoSize.X = MathF.Max(itemInfoSize.X, foodInfoWidth);

            Vec2 tooltipSize = itemInfoSize + new Vec2(0, 18) + tooltipPadding * 2;

            Vec2 screenSize = Onix.Gui.ScreenSize;
            if ((tooltipPos.X += tooltipPos.X + tooltipSize.X > screenSize.X ? -tooltipSize.X - tooltipOffset.X * 2 : 0) < 0)
            {
                tooltipPos.X += tooltipSize.X / 2 + tooltipOffset.X;
                tooltipPos.Y -= tooltipSize.Y + tooltipOffset.Y;
            }

            if (tooltipPos.Y + tooltipSize.Y > screenSize.Y)
                tooltipPos.Y -= tooltipPos.Y + tooltipSize.Y - screenSize.Y;
            else if (tooltipPos.Y < 0)
            {
                tooltipPos.Y = 0;
                tooltipPos.X = mousePosition.X + tooltipOffset.X;
            }

            scrollable = tooltipSize.Y > screenSize.Y || tooltipSize.X > screenSize.X;
            if (scrollable)
            {
                if (scrollOffset.Y > 0)
                    scrollOffset.Y = 0;
                else if (scrollOffset.Y + tooltipSize.Y < screenSize.Y)
                    scrollOffset.Y = screenSize.Y - tooltipSize.Y;

                tooltipPos += scrollOffset;
            }

            Textures.tooltip.Render(gfx, Rect.FromSize(tooltipPos, tooltipSize), 1f);
            gfx.RenderText(tooltipPos + tooltipPadding + new Vec2(0.4f, 0.15f), ColorF.White, CurrentHoverText);
            var Background = HungerTextureUV.Background();
            var Whole = HungerTextureUV.Whole();
            var Half = HungerTextureUV.Half();
            void RenderBackground(Rect rect) { gfx.RenderTexture(rect, Textures.icons, 1f, Background); }
            for (int i = 0; i < Math.Max(hungerMid, saturationMid); ++i)
            {
                int value = (i << 1) | 1;
                Rect rect = Rect.FromSize(tooltipPos + tooltipPadding, size).MoveDown(itemInfoSize.Y);
                RenderHunger(gfx, Whole, Half, rect.MoveBy(new Vec2(9 * (hungerMid - i - 1), 0)), value, hoveredFood.Hunger, 1f, RenderBackground);
                RenderSaturation(gfx, Textures.normal_outline, rect.MoveBy(new Vec2(9 * (saturationMid - i - 1), 9)), value, hoveredFood.Saturation, ColorF.Yellow, RenderBackground);
            }
        }

        private static void CacheOutline(RendererGame gfx, Rect uvRectWhole, TexturePath outline)
        {

            if (gfx.GetTextureStatus(outline) != RendererTextureStatus.Missing && !shouldUpload) return;
            
            RawImageData hungerWhole = ImageHelpers.CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(Textures.icons)), uvRectWhole);

            int width = hungerWhole.Width;
            int height = hungerWhole.Height;
            bool touchesEdge = Enumerable.Range(0, width).Any(x =>
                hungerWhole.GetPixel(x, 0).A > 0 || hungerWhole.GetPixel(x, height - 1).A > 0) ||
                Enumerable.Range(0, height).Any(y =>
                hungerWhole.GetPixel(0, y).A > 0 || hungerWhole.GetPixel(width - 1, y).A > 0);

            gfx.UploadTexture(outline, ImageHelpers.CreateOutline(hungerWhole, touchesEdge));
            shouldUpload = false;
        }

        private static void RenderHungerBar(RendererGame gfx, float delta)
        {
            if (AttributeBars == null) return;
            if (!(gamemode == GameMode.Survival || gamemode == GameMode.Adventure) || (accumulator += delta) < FrameThreshold || heldFood == null) return;
            float expectedHunger = MathF.Min(PlayerAttributes.Hunger + heldFood.Hunger, 20f);
            float expectedSaturation = MathF.Min(MathF.Min(expectedHunger, PlayerAttributes.Saturation + heldFood.Saturation), 20f);
            if (Config.ShowExhaustionOverlay){
                float exhaustionWidth = MathF.Round(ExhaustionWidth * exhaustion_percent);
                float x = AttributeBars.Hunger.X;
                float y = AttributeBars.Hunger.Y;
                float left = Full - exhaustionWidth / ExhaustionWidth;
                gfx.RenderTexture(new(x - exhaustionWidth, y, x, y + 9), Textures.exhaustion_background, .7f, new(left, 0f, Full, Full));
            }

            bool renderExpectedHunger = Config.ShowHungerRestoredFromHeldFood && expectedHunger != PlayerAttributes.Hunger;
            bool renderExpectedSaturation = Config.ShowHungerRestoredFromHeldFood && expectedSaturation != PlayerAttributes.Saturation;
            var Background = HungerTextureUV.Background(HasHungerEffect);
            var Whole = HungerTextureUV.Whole(HasHungerEffect);
            var Half = HungerTextureUV.Half(HasHungerEffect);
            for (int i = 0; i < 10; ++i)
            {
                Rect rect = Rect.FromSize(AttributeBars.Hunger, size).MoveBy(new(-i * 8 - 8, shouldJitter ? -rand.Next(2) : 0));
                gfx.RenderTexture(rect, Textures.icons, 1f, Background);

                int idk = (i << 1) | 1;
                if (renderExpectedHunger)
                    RenderHunger(gfx, Whole, Half, rect, idk, expectedHunger, flashAlpha);
                RenderHunger(gfx, Whole, Half, rect, idk, PlayerAttributes.Hunger, 1f);
                
                if (!Config.ShowSaturationOverlay) continue;
                if (renderExpectedSaturation)
                    RenderSaturation(gfx, outline!, rect, idk, expectedSaturation, ColorF.Lerp(ColorF.Transparent, ColorF.Yellow, flashAlpha));
                RenderSaturation(gfx, outline!, rect, idk, PlayerAttributes.Saturation, ColorF.Yellow);
            }
            updateCounter++;
            accumulator = 0;
        }

        private static void RenderHunger(RendererCommon2D gfx, Rect uvRectWhole, Rect uvRectHalf, Rect rect, int idk, float hunger, float alpha, Action<Rect>? Before = null)
        {
            if (idk > hunger) return;
            Before?.Invoke(rect);
            gfx.RenderTexture(rect, Textures.icons, alpha, idk == hunger ? uvRectHalf : uvRectWhole);
        }

        private static void RenderSaturation(RendererCommon2D gfx, TexturePath outline, Rect rect, int idk, float saturation, ColorF color, Action<Rect>? Before = null)
        {
            var floorSat = MathF.Floor(saturation);
            var rounded = floorSat % 2 == 0 ? floorSat + 1 : floorSat;
            if (idk > rounded) return;
            float pixelWidth = MathF.Round(9 * ((saturation != rounded) ? (saturation - floorSat) : .6f));
            bool notWhole = idk == rounded;
            if (pixelWidth == 0 && notWhole) return;
            Before?.Invoke(rect);
            if (notWhole)
            {
                rect.X += 9 - pixelWidth;
                rect.Width = pixelWidth;
            }

            gfx.RenderTexture(rect, outline, color, new(notWhole ? 1f - (pixelWidth / 9) : 0, 0, 1, 1));
        }

        protected override void OnEnabled()
        {
            AttributeBars = new();
            shouldUpload = true;
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