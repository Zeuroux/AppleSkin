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
using System.ComponentModel.Design;
using System.Text;

namespace AppleSkin
{
    public record ArmorItem(int Protection, string Slot, bool IsEmpty = false);

    public class AppleSkin : OnixPluginBase
    {
        private static TexturePath? Outline;
        private static  Random Rand = new();
        private static float unclampedFlashAlpha = 0f, flashAlpha = 0f;
        private static sbyte alphaDir = 1;

        private static FoodItem HeldFood = new() { Hunger = 0, Saturation = 0f, IsValid = false };
        private static ArmorItem HeldArmor = new(0, "", true);
        private static GameMode Gamemode;

        private static HudElements? AttributeBars;
        private static HungerAttributes HungerAttrs = new();
        private static HealthAttributes HealthAttrs = new();
        private static ArmorAttributes ArmorAttrs = new();
        public static bool ReuploadOutline = false;
        private static Vec2 SaturationTextureSize = new(9);
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
            Console.WriteLine(Encoding.UTF8.GetString(Onix.Game.PackManagerBehavior.LoadContent(TexturePath.Game("items/diamond.json"))));
            Config.SaturationOutlineThicknessSetting.IsHidden = Config.UseBackgroundAsOutline;
        }


        Dictionary<string, ArmorItem> ArmorValues = new()
        {
            ["leather_helmet"] = new(1, "slot.armor.head"),
            ["leather_chestplate"] = new(3, "slot.armor.chest"),
            ["leather_leggings"] = new(2, "slot.armor.legs"),
            ["leather_boots"] = new(1, "slot.armor.feet"),

            ["copper_helmet"] = new(2, "slot.armor.head"),
            ["copper_chestplate"] = new(4, "slot.armor.chest"),
            ["copper_leggings"] = new(3, "slot.armor.legs"),
            ["copper_boots"] = new(1, "slot.armor.feet"),

            ["golden_helmet"] = new(2, "slot.armor.head"),
            ["golden_chestplate"] = new(5, "slot.armor.chest"),
            ["golden_leggings"] = new(3, "slot.armor.legs"),
            ["golden_boots"] = new(1, "slot.armor.feet"),

            ["chainmail_helmet"] = new(2, "slot.armor.head"),
            ["chainmail_chestplate"] = new(5, "slot.armor.chest"),
            ["chainmail_leggings"] = new(4, "slot.armor.legs"),
            ["chainmail_boots"] = new(1, "slot.armor.feet"),

            ["iron_helmet"] = new(2, "slot.armor.head"),
            ["iron_chestplate"] = new(6, "slot.armor.chest"),
            ["iron_leggings"] = new(5, "slot.armor.legs"),
            ["iron_boots"] = new(2, "slot.armor.feet"),

            ["turtle_shell"] = new(2, "slot.armor.head"),

            ["diamond_helmet"] = new(3, "slot.armor.head"),
            ["diamond_chestplate"] = new(8, "slot.armor.chest"),
            ["diamond_leggings"] = new(6, "slot.armor.legs"),
            ["diamond_boots"] = new(3, "slot.armor.feet"),

            ["netherite_helmet"] = new(3, "slot.armor.head"),
            ["netherite_chestplate"] = new(8, "slot.armor.chest"),
            ["netherite_leggings"] = new(6, "slot.armor.legs"),
            ["netherite_boots"] = new(3, "slot.armor.feet")
        };



        private void OnTick()
        {
            if (Onix.LocalPlayer is not LocalPlayer localPlayer) return;
            var HasResistance = localPlayer.Effects.Any(e => e.Id == MobEffectId.Resistance);
            ArmorAttrs.Armors.Clear();
            foreach (var armor in localPlayer.ArmorItems) if (!armor.IsEmpty) {
                var ArmorValue = ArmorValues.GetValueOrDefault(armor.Item!.Name, new(0, "", true));
                ArmorAttrs.Armors[ArmorValue.Slot] = ArmorValue.Protection;
                HasResistance = true;
            }
            HeldArmor = ArmorValues.GetValueOrDefault(localPlayer.MainHandItem.Item?.Name ?? "", new(0, "", true));
            if (localPlayer.GetAttribute(EntityAttributeId.Health) is EntityAttribute health) {
                HealthAttrs.LastHealth = HealthAttrs.Health;
                HealthAttrs.LastHealthMax = HealthAttrs.HealthMax;
                if (HealthAttrs.LastHealth > health.Value && (HasResistance || HealthAttrs.AbsorptionOffset > 0))
                    HealthAttrs.HealthOffset = 1;
                if (health.Value == 0 || health.Value == health.MaxValue)
                    HealthAttrs.HealthOffset = 0;
                HealthAttrs.Health = (int)health.Value + HealthAttrs.HealthOffset;
                HealthAttrs.HealthMax = (int)health.MaxValue;
            }
            if (localPlayer.GetAttribute(EntityAttributeId.Absorption) is EntityAttribute absorption)
            {
                HealthAttrs.LastAbsorption = HealthAttrs.Absorption;
                if (HealthAttrs.LastAbsorption > absorption.Value && HasResistance)
                    HealthAttrs.AbsorptionOffset = 1;
                if (absorption.Value == 0 || absorption.Value == absorption.MaxValue)
                    HealthAttrs.AbsorptionOffset = 0;
                HealthAttrs.Absorption = (int)absorption.Value + HealthAttrs.AbsorptionOffset;
                HealthAttrs.AbsorptionMax = (int)absorption.MaxValue;
            }
            HealthAttrs.Changed = HealthAttrs.LastHealth != HealthAttrs.Health || HealthAttrs.LastAbsorption != HealthAttrs.Absorption;
            HealthAttrs.ShouldJitter = HealthAttrs.Health <= 4 && Config.AnimateHudIcons;

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
            if (!TryCacheOutline(gfx)) return;
            if (!(Gamemode == GameMode.Survival || Gamemode == GameMode.Adventure)) return;
            if (screenName != Constants.hudScreen || AttributeBars == null || HeldFood == null) return;
            RenderHungerBar(gfx, AttributeBars.Hunger, HeldFood);
            RenderHealthAndArmorBar(gfx, AttributeBars.Health, HeldFood);
        }
        private static void RenderHealthAndArmorBar(RendererGame gfx, Vec2 position, FoodItem HeldFood)
        {
            Rand = new(Onix.Dimension!.World.CurrentTick * 312871); //lmao
            var Health = HealthAttrs.Health;
            var LastHealth = HealthAttrs.LastHealth;
            var Absorption = HealthAttrs.Absorption;
            var LastAbsorption = HealthAttrs.LastAbsorption;

            var FlashAbsorption = LastAbsorption > Absorption;
            var FlashHealth = LastHealth > Health;

            var EstimatedHealthIncrement = HeldFood.GetEstimatedHealthIncrement(HungerAttrs);
            var EstimatedHealth = Health + EstimatedHealthIncrement;
            var RenderExpectedHealth = Config.ShowEstimatedHealthOverlay && EstimatedHealth != Health && HeldFood.IsValid;
            var AbsorptionIncrement = HeldFood.GetAbsorptionIncrement();
            var EstimatedAbsorption = MathF.Min(Absorption + AbsorptionIncrement, HealthAttrs.AbsorptionMax);
            var EstimatedAbsorptionMid = MathF.Ceiling(MathF.Max(EstimatedAbsorption, LastAbsorption) / 2F);
            var HealthBoostIncrement = HeldFood.GetHealthBoostIncrement();
            var EstimatedMaxHealth = MathF.Max(HealthAttrs.HealthMax, 20 + (Config.ShowEstimatedHealthBoostOverlay ? HealthBoostIncrement : 0));
            var EstimatedMaxHealthMid = MathF.Ceiling(EstimatedMaxHealth / 2F);

            var RenderExpectedAbsorption = Config.ShowEstimatedAbsorptionOverlay && EstimatedAbsorption != Absorption && HeldFood.IsValid;
            var IsUITop = !AttributeBars!.IsCenteredBottom;
            int Total = (int)(EstimatedMaxHealthMid + EstimatedAbsorptionMid);

            if (!IsUITop)
                AttributeBars.SetActionBarOffsetY(-(Total - 1) + (Total - 1) % 10 - 15);

            for (int i = 0; i < Total; i++)
            {
                int idk = (i << 1) | 1;
                int idk2 = ((i - (int)EstimatedMaxHealthMid) << 1) | 1;
                var IsAbsorption = i >= EstimatedMaxHealthMid;
                var y = -i + i % 10;
                var rect = Rect.FromSize(position, Constants.IconSize).MoveBy(i % 10 * 8, (IsUITop ? -y : y) - (HealthAttrs.ShouldJitter ? Rand.Next(2) : 0));
                if (HealthAttrs.HasRegenerationEffect && i == HealthAttrs.BobIndex)
                    rect = rect.MoveDown(2);

                IconRender.HealthBackground(gfx, rect, idk <= HealthAttrs.HealthMax || idk > EstimatedMaxHealth ? 1f : flashAlpha, HealthAttrs.Changed);
                if (IsAbsorption) {
                    if (FlashAbsorption && idk2 <= LastAbsorption && idk2 >= Absorption)
                        IconRender.AbsorptionFlash(gfx, rect, 1f, HealthAttrs.LastAbsorption == idk2, HealthAttrs.IsHardcore);
                    if (RenderExpectedAbsorption && idk2 <= EstimatedAbsorption && idk2 >= Absorption)
                        IconRender.Absorption(gfx, rect, flashAlpha, EstimatedAbsorption == idk2, HealthAttrs.IsHardcore);
                    if (idk2 <= Absorption)
                        IconRender.Absorption(gfx, rect, 1f, Absorption == idk2, HealthAttrs.IsHardcore);
                } else {
                    if (FlashHealth && idk <= LastHealth && idk >= Health)
                        IconRender.HealthFlash(gfx, rect, 1f, LastHealth == idk, HealthAttrs);
                    if (RenderExpectedHealth && idk <= EstimatedHealth && idk >= Health)
                        IconRender.Health(gfx, rect, flashAlpha, EstimatedHealth == idk, HealthAttrs);
                    if (idk <= Health)
                        IconRender.Health(gfx, rect, 1f, Health == idk, HealthAttrs);
                }
            }

            var Armor = 0;
            var ExpectedArmor = 0;
            if (!ArmorAttrs.Armors.ContainsKey(HeldArmor.Slot))
                ExpectedArmor += HeldArmor.Protection;
            foreach (var armor in ArmorAttrs.Armors)
            {
                Armor += armor.Value;
                if (armor.Key == HeldArmor.Slot)
                    ExpectedArmor += HeldArmor.Protection;
                else
                    ExpectedArmor += armor.Value;
            }
            var RenderExpectedArmor = Config.ShowEstimatedArmorOverlay && ExpectedArmor != Armor;
            Total--;
            var ArmorY = -Total + Total % 10 - 10;
            for (int i = 0; i < 10; i++)
            {
                var idk = (i << 1) | 1;
                var rect = Rect.FromSize(position, Constants.IconSize).MoveBy(i * 8, ArmorY);
                RenderArmorBar(gfx, Armor, idk, rect, 1f);
                if (!RenderExpectedArmor) continue;
                RenderArmorBar(gfx, ExpectedArmor, idk, rect, flashAlpha);
            }

            static void RenderArmorBar(RendererGame gfx, int armor, int idk, Rect rect, float alpha)
            {
                if (idk > armor)
                    IconRender.ArmorEmpty(gfx, rect, alpha);
                else
                    IconRender.Armor(gfx, rect, alpha, idk == armor);
            }
        }

        private static void RenderHungerBar(RendererGame gfx, Vec2 position, FoodItem heldFood)
        {
            float expectedHunger = MathF.Min(HungerAttrs.Hunger + heldFood.Hunger, 20f);
            float expectedSaturation = MathF.Min(MathF.Min(expectedHunger, HungerAttrs.Saturation + heldFood.Saturation), 20f);
            if (Config.ShowExhaustionOverlay)
            {
                float exhaustion = Math.Min(HungerAttrs.Exhaustion, 4f) / 4f;
                float width = MathF.Round(Constants.ExhaustionWidth * exhaustion);
                float uvLeft = MathF.Floor((1f - exhaustion) * 80f) / 80f;
                var rect = Rect.FromSize(position, new(width, 9)).MoveLeft(width - 1);
                gfx.RenderTexture(rect, Textures.Exhaustionbackground, .7f, new(uvLeft, 0f, 1f, 1f));
            }
            bool renderExpectedHunger = Config.ShowHungerRestoredFromHeldFood && expectedHunger != HungerAttrs.Hunger;
            bool renderExpectedSaturation = Config.ShowHungerRestoredFromHeldFood && expectedSaturation != HungerAttrs.Saturation;
            for (int i = 0; i < 10; i++)
            {
                HungerAttrs.ShouldJitter = HungerAttrs.Saturation <= 0f && HungerAttrs.UpdateCounter % (HungerAttrs.Hunger * 3 + 1) == 0 && Config.AnimateHudIcons;
                Rect rect = Rect.FromSize(position, Constants.IconSize).MoveBy((-i - 1) * 8, HungerAttrs.ShouldJitter ? -Rand.Next(2) : 0);
                IconRender.HungerBackground(gfx, rect, HungerAttrs.HasHungerEffect);
                int idk = (i << 1) | 1;
                if (renderExpectedHunger && idk <= expectedHunger)
                    IconRender.Hunger(gfx, rect, flashAlpha, expectedHunger == idk, HungerAttrs.HasHungerEffect);
                if (idk <= HungerAttrs.Hunger)
                    IconRender.Hunger(gfx, rect, 1f, HungerAttrs.Hunger == idk, HungerAttrs.HasHungerEffect);
                if (!Config.ShowSaturationOverlay) continue;
                if (renderExpectedSaturation)
                    IconRender.Saturation(gfx, Outline!, rect, idk, expectedSaturation, Config.SaturationColor.WithOpacity(flashAlpha), SaturationTextureSize.X);
                IconRender.Saturation(gfx, Outline!, rect, idk, HungerAttrs.Saturation, Config.SaturationColor, SaturationTextureSize.X);
            }
            HungerAttrs.UpdateCounter++;
        }

        private static bool TryCacheOutline(RendererGame gfx)
        {
            if (Outline == null) return false;
            if (gfx.GetTextureStatus(Outline) != RendererTextureStatus.Missing && !ReuploadOutline) return true;
            ReuploadOutline = false;
            var HasHungerEffect = HungerAttrs.HasHungerEffect;
            TexturePath baseTexture;
            Rect baseRect;
            TexturePath? mask = null;
            Rect? maskRect = null;
            if (Config.UseBackgroundAsOutline)
            {
                baseTexture = Textures.GetHungerBackground(HasHungerEffect);
                baseRect = Hunger.GetBackground(HasHungerEffect);
                mask = Textures.GetHunger(hasHungerEffect: HasHungerEffect);
                maskRect = Hunger.GetHunger(hasHungerEffect: HasHungerEffect);
            }
            else
            {
                baseTexture = Textures.GetHunger(hasHungerEffect: HasHungerEffect);
                baseRect = Hunger.GetHunger(hasHungerEffect: HasHungerEffect);
            }

            RawImageData hungerWhole = ImageHelpers.CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(baseTexture)), baseRect);
            int width = hungerWhole.Width;
            int height = hungerWhole.Height;
            if (Config.UseBackgroundAsOutline)
                gfx.UploadTexture(Outline, ImageHelpers.Walter(hungerWhole, ImageHelpers.CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(mask!)), maskRect!.Value)));
            else
                gfx.UploadTexture(
                    Outline,
                    ImageHelpers.CreateOutline(
                        hungerWhole,
                        Enumerable.Range(0, width).Any(x =>
                        hungerWhole.GetPixel(x, 0).A > 0 || hungerWhole.GetPixel(x, height - 1).A > 0) ||
                        Enumerable.Range(0, height).Any(y =>
                        hungerWhole.GetPixel(0, y).A > 0 || hungerWhole.GetPixel(width - 1, y).A > 0),
                        Config.SaturationOutlineThickness
                    )
                );
            SaturationTextureSize = new(width, height);
            return true;
        }

        // Tooltip stuff
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
        private static bool HideOriginalTooltip = false;
        private static bool HoveringAnything = false;

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
                case InputKey.Type.Scroll when HoveringAnything && StartScrolling:
                    (ScrollHorizontally ? ref TargetScrollOffset.X : ref TargetScrollOffset.Y) += isDown ? 10 : -10;
                    if (HideOriginalTooltip)
                        return true;
                    break;
                default:
                    break;
            }

            return false;
        }
        private void OnContainerScreenTick(ContainerScreen container)
        {

            if ((!Config.ShowFoodValuesInTooltip || !ScrollHorizontally) && !Config.AlwaysShowFoodValuesInTooltip) return;
            if (!(HoveringAnything = container.HoveredSlot >= 0))
                TargetScrollOffset = Vec2.Zero;
            if (container.GetHoveredItem() is ItemStack itemStack)
            {
                if ((lastSlot != container.HoveredSlot || !itemStack.Matches(lastItemStack)))
                {
                    TargetScrollOffset = Vec2.Zero;
                    if (itemStack.Item is { } food && FoodHelper.GetFoodItem(food, out HoveredFood) && HoveredFood.Hunger > 0)
                    {
                        Onix.Gui.RootUiElement!.FindMatchRecursive(
                            what: e => e.Name == "hover_text" &&
                                      e.JsonProperties.Contains("#hover_text") &&
                                      e.Parent is { Name: "highlight" } highlight &&
                                      highlight.JsonProperties.Contains(Constants.visibleTrue) &&
                                      highlight.Rect is { Width: > 0, Height: > 0 } &&
                                      highlight.Parent is { Name: "hover" } hover &&
                                      hover.Children.Any(sibling => sibling.Name == "white_border" && sibling.JsonProperties.Contains(Constants.visibleTrue)),
                            whenFound: e =>
                            {
                                if (e.Parent?.Parent?.Parent?.Parent?.FindChildRecursive("overlay")?.JsonProperties is { } prop && RegexPatterns.ItemCategoryRegex().Match(prop).Success)
                                {
                                    ResetHovered();
                                    return true;
                                }
                                var match = RegexPatterns.HoverTextRegex().Match(e.JsonProperties);
                                if (match.Success)
                                {
                                    CurrentHoverText = match.Groups[1].Value.Replace("\\n", "\n");
                                    if (HideOriginalTooltip)
                                        e.JsonProperties = "{}";
                                    if (e.Parent?.Parent?.Parent is { } hover)
                                        HoveredRect = hover.Rect.MoveTo(hover.GetAbsolutePosition());
                                    Hovered = e;
                                }
                                return true;

                            });
                    }
                    else ResetHovered();
                    lastSlot = container.HoveredSlot;
                    lastItemStack = itemStack.Clone();
                }
            }
            
            if (Hovered != null)
            {
                if (HideOriginalTooltip)
                    if (Hovered.JsonProperties != "{}")
                        Hovered.JsonProperties = "{}";
                if (Hovered is { Parent.Rect: not { Width: > 0, Height: > 0 } })
                {
                    ResetHovered();
                    lastItemStack = null;
                    return;
                }
            }
        }
        private void OnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUI)
        {
            if (screenName == Constants.hudScreen) { ResetHovered(); return; }
            ScrollOffset = ScrollOffset.Lerp(TargetScrollOffset, MathF.Min(lerpSpeed * delta, 1f));
            if ((!Config.ShowFoodValuesInTooltip || !ScrollHorizontally) && !Config.AlwaysShowFoodValuesInTooltip) return;
            if (HoveredFood == null || HoveredFood.Hunger == 0) return;
            gfx.FontType = FontType.Mojangles;
            Vec2 mousePosition = Onix.Gui.MousePosition;
            var tooltipOffset = Constants.Tooltip.Offset;
            bool add10 = true; 
            if (HoveredRect is {} hoveredRect && !hoveredRect.Expand(1/9f).Contains(mousePosition))
            {
                tooltipOffset = Vec2.Zero;
                mousePosition = hoveredRect.Center;
            }
            Vec2 tooltipPos = mousePosition + tooltipOffset;
            Vec2 itemInfoSize = gfx.MeasureText(CurrentHoverText);
            int hungerMid = (int)MathF.Ceiling(HoveredFood.Hunger / 2f);
            int saturationMid = (int)MathF.Ceiling(HoveredFood.Saturation / 2f);
            var foodInfoWidth = MathF.Max(9 * hungerMid, 9 * saturationMid);
            itemInfoSize.X = MathF.Max(itemInfoSize.X, foodInfoWidth);

            Vec2 tooltipSize = itemInfoSize + Constants.Tooltip.Padding * 2;
            tooltipSize.Y += 18;
            tooltipSize.X++;
            Vec2 screenSize = Onix.Gui.ScreenSize;            
            if ((tooltipPos.X += (HideOriginalTooltip = tooltipPos.X + tooltipSize.X > screenSize.X) ? -tooltipSize.X - tooltipOffset.X * 2 : 0) < 0) // this is stupid lmao
            {
                tooltipPos.X += tooltipSize.X / 2 + tooltipOffset.X;
                tooltipPos.Y -= tooltipSize.Y + tooltipOffset.Y;
                add10 = false;

            }
            if (add10 && tooltipOffset == Vec2.Zero)
                tooltipPos += Constants.Tooltip.Offset10;
            
            if (tooltipPos.Y + tooltipSize.Y > screenSize.Y)
                tooltipPos.Y -= tooltipPos.Y + tooltipSize.Y - screenSize.Y;
            else if (tooltipPos.Y < 0)
            {
                tooltipPos.Y = 0;
                tooltipPos.X = mousePosition.X + tooltipOffset.X;
            }

            tooltipPos += ScrollOffset;
            Textures.TooltipBackground.Render(gfx, Rect.FromSize(tooltipPos, tooltipSize), 1f);
            gfx.RenderText(tooltipPos + Constants.Tooltip.Padding + Constants.Tooltip.TextPadding, ColorF.White, CurrentHoverText);
            var backgroundUV = Hunger.GetBackground();
            var baseIconPos = tooltipPos + Constants.Tooltip.Padding;
            var hungerBaseRect = Rect.FromSize(baseIconPos, Constants.IconSize).MoveDown(itemInfoSize.Y);
            var saturationBaseRect = hungerBaseRect.MoveDown(9);

            for (int i = 0; i < Math.Max(hungerMid, saturationMid); ++i)
            {
                int value = (i << 1) | 1;
                if (i < hungerMid)
                {
                    var hungerRect = hungerBaseRect.MoveRight(9 * (hungerMid - i - 1));
                    IconRender.HungerBackground(gfx, hungerRect);
                    IconRender.Hunger(gfx, hungerRect, 1f, value == HoveredFood.Hunger);
                }
                if (i < saturationMid)
                {
                    var saturationRect = saturationBaseRect.MoveRight(9 * (saturationMid - i - 1));
                    IconRender.HungerBackground(gfx, saturationRect);
                    IconRender.Saturation(gfx, Textures.NormalOutline, saturationRect, value, HoveredFood.Saturation, Config.SaturationColor, SaturationTextureSize.X);
                }
            }
        }
        // End of tooltip stuff

        private void OnSessionLeave()
        {
            AttributeBars = null;
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