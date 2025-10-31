using AppleSkin.Extensions;
using AppleSkin.Helpers;
using AppleSkin.Stuff;
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

namespace AppleSkin
{
    public class AppleSkin : OnixPluginBase
    {
        private static TexturePath? Outline;
        private static  Random Rand = new();
        private static readonly Vec2 size = new(9);

        private static float unclampedFlashAlpha = 0f, flashAlpha = 0f;
        private static sbyte alphaDir = 1;

        private static FoodItem? HeldFood;
        private static GameMode Gamemode;

        private static HudElements? AttributeBars;
        private static HungerAttributes HungerAttrs = new();
        private static HealthAttributes HealthAttrs = new();
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
            Config = new AppleSkinConfig(PluginDisplayModule, true);
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
            if (Onix.LocalPlayer is not LocalPlayer localPlayer) return;
            if (localPlayer.GetAttribute(EntityAttributeId.Health) is EntityAttribute health) {
                HealthAttrs.LastHealth = HealthAttrs.Health;
                HealthAttrs.LastHealthMax = HealthAttrs.HealthMax;
                HealthAttrs.Health = (int)health.Value;
                HealthAttrs.HealthMax = (int)health.MaxValue;
            }
            var HasResistance = localPlayer.Effects.Any(e => e.Id == MobEffectId.Resistance);
            if (localPlayer.GetAttribute(EntityAttributeId.Absorption) is EntityAttribute absorption)
            {
                HealthAttrs.LastAbsorption = HealthAttrs.Absorption;
                if (HealthAttrs.LastAbsorption > absorption.Value && HasResistance)
                    HealthAttrs.AbsorptionOffset = 1;
                if (absorption.Value == 0)
                    HealthAttrs.AbsorptionOffset = 0;
                var NewAbsorption = (int)MathF.Min(absorption.Value + HealthAttrs.AbsorptionOffset, absorption.MaxValue);
                HealthAttrs.Absorption = NewAbsorption;
            }
            HealthAttrs.Changed = HealthAttrs.LastHealth != HealthAttrs.Health || HealthAttrs.LastAbsorption != HealthAttrs.Absorption;

            HealthAttrs.IsHardcore = Onix.Dimension?.World.IsHardcore == true;
            HealthAttrs.IsFreezing = localPlayer.GetFlag(EntityFlag.Shaking);
            HealthAttrs.HasRegenerationEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Regeneration);
            HealthAttrs.HasWitherEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Wither);
            HealthAttrs.HasPoisonEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Poison);

            if (localPlayer.GetAttribute(EntityAttributeId.Hunger) is EntityAttribute hunger) HungerAttrs.Hunger = (int)hunger.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Saturation) is EntityAttribute saturation) HungerAttrs.Saturation = saturation.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Exhaustion) is EntityAttribute rawExhaustion) HungerAttrs.Exhaustion = rawExhaustion.Value;
            
            HungerAttrs.HasHungerEffect = localPlayer.Effects.Any(e => e.Id == MobEffectId.Hunger);
            Outline = HungerAttrs.HasHungerEffect ? Textures.HungeredOutline : Textures.NormalOutline;

            if (Onix.Gui.ScreenName != Constants.hudScreen) return;
            HealthAttrs.BobIndex = (HealthAttrs.BobIndex + 1) % (((HealthAttrs.HealthMax / 2) + (int)MathF.Ceiling(HealthAttrs.Absorption / 2f)) * 2 + 2);            

            Gamemode = localPlayer.GameMode;
            if (AttributeBars == null) AttributeBars = new(); else AttributeBars.Update();
            var item = localPlayer.MainHandItem.Item;
            if (!FoodHelper.GetFoodItem(item, out HeldFood) || (!(HeldFood.AlwaysConsumable || item!.Name == "apple") && HungerAttrs.Hunger == 20))
                HeldFood = new FoodItem { Hunger = 0, Saturation = 0f, IsValid = false };
            unclampedFlashAlpha += alphaDir * 0.125f;
            if (unclampedFlashAlpha >= 1.5f) alphaDir = -1; else if (unclampedFlashAlpha <= -0.5f) alphaDir = 1;
            flashAlpha = MathF.Max(0F, MathF.Min(1F, unclampedFlashAlpha)) * MathF.Max(0F, MathF.Min(1F, Config.MaxFlashingHudIconAlpha));
        }

        private void OnPreRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi)
        {
            if (Outline == null) return;
            CacheOutline(gfx, HungerTextureUV.Whole(HungerAttrs.HasHungerEffect), Outline);
            if (!(Gamemode == GameMode.Survival || Gamemode == GameMode.Adventure)) return;
            if (screenName != Constants.hudScreen || AttributeBars == null) return;
            if (AttributeBars == null || HeldFood == null) return;
            RenderHungerBar(gfx, AttributeBars.Hunger, HeldFood);
            RenderHealthBar(gfx, AttributeBars.Armor, HeldFood);
        }

        private static void RenderHealthBar(RendererGame gfx, Vec2 position, FoodItem HeldFood)
        {
            bool ShouldJitter = HealthAttrs.Health <= 4 && HealthAttrs.UpdateCounter % 4 == 0 && Config.AnimateHudIcons;
            Rand = new(Onix.Dimension!.World.CurrentTick * 312871); //lmao
            var HealthMid = MathF.Ceiling(Math.Max(HealthAttrs.LastHealthMax, HealthAttrs.HealthMax) / 2f);
            var AbsorptionMid = MathF.Ceiling(Math.Max(HealthAttrs.LastAbsorption, HealthAttrs.Absorption) / 2f);
            var Background = HealthTextureUV.Background(HealthAttrs.Changed);

            var EstimatedHealthIncrement = HeldFood.GetEstimatedHealthIncrement(HungerAttrs);
            var EstimatedHealth = HealthAttrs.Health + EstimatedHealthIncrement;
            var Health = HealthAttrs.Health;
            var RenderExpectedHealth = Config.ShowEstimatedHealthOverlay && EstimatedHealth != Health && HeldFood.IsValid;
            for (int i = 0; i < HealthMid + AbsorptionMid; i++)
            {
                int idk = (i << 1) | 1;
                int idk2 = ((i - (int)HealthMid) << 1) | 1;

                var IsAbsorption = i >= HealthMid;
                var x = i % 10 * 8;

                var y = (MathF.Floor(i / 10) * -10 - 1) + (ShouldJitter ? -Rand.Next(2) : 0);
                if (HealthAttrs.HasRegenerationEffect && i == HealthAttrs.BobIndex)
                    y += 2;
                var rect = Rect.FromSize(position, size).MoveBy(x , y);
                gfx.RenderTexture(rect, Textures.Icons, 1f, Background);

                if (IsAbsorption) {
                    if (idk2 <= HealthAttrs.LastAbsorption)
                        gfx.RenderTexture(rect, Textures.Icons, 1f, HealthTextureUV.AbsorptionFlash(HealthAttrs.LastAbsorption == idk2, HealthAttrs.IsHardcore));
                    if (idk2 <= HealthAttrs.Absorption)
                        gfx.RenderTexture(rect, Textures.Icons, 1f, HealthTextureUV.Absorption(HealthAttrs.Absorption == idk2, HealthAttrs.IsHardcore));
                } else {
                    if (idk <= HealthAttrs.LastHealth)
                        gfx.RenderTexture(
                            rect,
                            HealthAttrs.IsFreezing ? Textures.GetFreezeHeartFlash(HealthAttrs.LastHealth == idk, HealthAttrs.IsHardcore) : Textures.Icons,
                            1f,
                            HealthTextureUV.HeartFlash(HealthAttrs, HealthAttrs.LastHealth == idk)
                        );
                    if (RenderExpectedHealth && idk <= EstimatedHealth)
                        gfx.RenderTexture(
                            rect,
                            HealthAttrs.IsFreezing ? Textures.GetFreezeHeart(EstimatedHealth == idk, HealthAttrs.IsHardcore) : Textures.Icons,
                            flashAlpha,
                            HealthTextureUV.Heart(HealthAttrs, EstimatedHealth == idk)
                        );
                    if (idk <= Health)
                        gfx.RenderTexture(
                            rect,
                            HealthAttrs.IsFreezing ? Textures.GetFreezeHeart(Health == idk, HealthAttrs.IsHardcore) : Textures.Icons, 
                            1f, 
                            HealthTextureUV.Heart(HealthAttrs, Health == idk)
                        );
                }

            }
            HealthAttrs.UpdateCounter++;
        }

        private static void RenderHungerBar(RendererGame gfx, Vec2 position, FoodItem heldFood)
        {
            bool ShouldJitter = HungerAttrs.Saturation <= 0f && HungerAttrs.UpdateCounter % (HungerAttrs.Hunger * 3 + 1) == 0 && Config.AnimateHudIcons;

            float expectedHunger = MathF.Min(HungerAttrs.Hunger + heldFood.Hunger, 20f);
            float expectedSaturation = MathF.Min(MathF.Min(expectedHunger, HungerAttrs.Saturation + heldFood.Saturation), 20f);
            if (Config.ShowExhaustionOverlay)
            {
                float exhaustionWidth = MathF.Round(Constants.ExhaustionWidth * Math.Min(HungerAttrs.Exhaustion, 4) / 4);
                float x = position.X;
                float y = position.Y;
                float left = Constants.Full - exhaustionWidth / Constants.ExhaustionWidth;
                gfx.RenderTexture(new(x - exhaustionWidth, y, x + 1, y + 9), Textures.Exhaustionbackground, .7f, new(left, 0f, Constants.Full, Constants.Full));
            }

            bool renderExpectedHunger = Config.ShowHungerRestoredFromHeldFood && expectedHunger != HungerAttrs.Hunger;
            bool renderExpectedSaturation = Config.ShowHungerRestoredFromHeldFood && expectedSaturation != HungerAttrs.Saturation;
            var Background = HungerTextureUV.Background(HungerAttrs.HasHungerEffect);
            var Whole = HungerTextureUV.Whole(HungerAttrs.HasHungerEffect);
            var Half = HungerTextureUV.Half(HungerAttrs.HasHungerEffect);
            for (int i = 0; i < 10; i++)
            {
                Rect rect = Rect.FromSize(position, size).MoveBy((-i - 1) * 8, ShouldJitter ? -Rand.Next(2) : 0);
                gfx.RenderTexture(rect, Textures.Icons, 1f, Background);

                int idk = (i << 1) | 1;
                if (renderExpectedHunger)
                    IconRender.Hunger(gfx, Whole, Half, rect, idk, expectedHunger, flashAlpha);
                IconRender.Hunger(gfx, Whole, Half, rect, idk, HungerAttrs.Hunger, 1f);

                if (!Config.ShowSaturationOverlay) continue;
                if (renderExpectedSaturation)
                    IconRender.Saturation(gfx, Outline!, rect, idk, expectedSaturation, ColorF.Lerp(ColorF.Transparent, ColorF.Yellow, flashAlpha));
                IconRender.Saturation(gfx, Outline!, rect, idk, HungerAttrs.Saturation, ColorF.Yellow);
            }
            HungerAttrs.UpdateCounter++;
        }

        // Mainly tooltip stuff
        public static readonly Vec2 TooltipTextPadding = new(0.4f, 0.15f);
        public static readonly Vec2 Offset10 = new(10);
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
                
            switch (key.Value)
            {
                case InputKey.Type.Ctrl:
                    StartScrolling = isDown;
                    if (!isDown)
                        TargetScrollOffset = Vec2.Zero;
                    break;
                case InputKey.Type.Shift:
                    ScrollHorizontally = isDown;
                    break;
                case InputKey.Type.Scroll when HoveredFood is { }:
                    if (StartScrolling)
                        (ScrollHorizontally ? ref TargetScrollOffset.X : ref TargetScrollOffset.Y) += isDown ? 10 : -10;
                    return true;
                default:
                    break;
            }

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
                                  highlight.JsonProperties.Contains(Constants.visibleTrue) &&
                                  highlight.Rect is { Width: > 0, Height: > 0 } &&
                                  highlight.Parent is { Name: "hover" } hover &&
                                  hover.Children.Any(sibling => sibling.Name == "white_border" && sibling.JsonProperties.Contains(Constants.visibleTrue)),
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
            if (screenName == Constants.hudScreen) { ResetHovered(); return; }
            ScrollOffset = ScrollOffset.Lerp(TargetScrollOffset, MathF.Min(lerpSpeed * delta, 1f));
            gfx.FontType = FontType.Mojangles;
            Vec2 startPosition = Onix.Gui.MousePosition;
            var tooltipOffset = Constants.TooltipSizes.Offset;
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

            Vec2 tooltipSize = itemInfoSize + Constants.TooltipSizes.Padding * 2;
            tooltipSize.Y += 18;
            Vec2 screenSize = Onix.Gui.ScreenSize;
            
            if ((tooltipPos.X += tooltipPos.X + tooltipSize.X > screenSize.X ? -tooltipSize.X - tooltipOffset.X * 2 : 0) < 0)
            {
                tooltipPos.X += tooltipSize.X / 2 + tooltipOffset.X;
                tooltipPos.Y -= tooltipSize.Y + tooltipOffset.Y;
                add10 = false;
            }
            if (add10 && tooltipOffset == Vec2.Zero)
                tooltipPos += Offset10;
            
            if (tooltipPos.Y + tooltipSize.Y > screenSize.Y)
                tooltipPos.Y -= tooltipPos.Y + tooltipSize.Y - screenSize.Y;
            else if (tooltipPos.Y < 0)
            {
                tooltipPos.Y = 0;
                tooltipPos.X = startPosition.X + tooltipOffset.X;
            }
            
            tooltipPos += ScrollOffset;
            Textures.TooltipBackground.Render(gfx, Rect.FromSize(tooltipPos, tooltipSize), 1f);
            gfx.RenderText(tooltipPos + Constants.TooltipSizes.Padding + TooltipTextPadding, ColorF.White, CurrentHoverText);
            var backgroundUV = HungerTextureUV.Background();
            var wholeUV = HungerTextureUV.Whole();
            var halfUV = HungerTextureUV.Half();
            var baseIconPos = tooltipPos + Constants.TooltipSizes.Padding;
            var hungerBaseRect = Rect.FromSize(baseIconPos, Constants.TooltipSizes.IconSize).MoveDown(itemInfoSize.Y);
            var saturationBaseRect = hungerBaseRect.MoveDown(9);

            for (int i = 0; i < Math.Max(hungerMid, saturationMid); ++i)
            {
                int value = (i << 1) | 1;
                if (i < hungerMid)
                    IconRender.Hunger(gfx, wholeUV, halfUV, saturationBaseRect.MoveRight(9 * (hungerMid - i - 1)), value, HoveredFood.Hunger, 1f, rect => gfx.RenderTexture(rect, Textures.Icons, 1f, backgroundUV));
                if (i < saturationMid)
                    IconRender.Saturation(gfx, Textures.NormalOutline, hungerBaseRect.MoveRight(9 * (saturationMid - i - 1)), value, HoveredFood.Saturation, ColorF.Yellow, rect => gfx.RenderTexture(rect, Textures.Icons, 1f, backgroundUV));
            }
        }
        // End of Mainly tooltip stuff
        private static void CacheOutline(RendererGame gfx, Rect uvRectWhole, TexturePath outline)
        {

            if (gfx.GetTextureStatus(outline) != RendererTextureStatus.Missing) return;
            AttributeBars = new();

            RawImageData hungerWhole = ImageHelpers.CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(Textures.Icons)), uvRectWhole);

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
            AttributeBars = null;
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