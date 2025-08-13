using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.OnixClient.Settings;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.ResourcePacks;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Transactions;
using static OnixRuntime.Api.Utils.ClientTranslationLayers.VersionTranslationLayerLookupBuilder;

namespace AppleSkin
{
    public static partial class RegexPatterns
    {
        [GeneratedRegex(@"#hover_text"":""([^""]+)""")]
        public static partial Regex HoverTextRegex();
    }
    public class AppleSkin : OnixPluginBase
    {
        private const float FrameThreshold = 1 / 60;
        private const string visibleFalse = "\"#visible\":false";
        private const string visibleTrue = "\"#visible\":true";
        private static readonly Random rand = new();
        private static readonly Vec2 tooltipPadding = new(4.1f, 4.2f);
        private static readonly Vec2 tooltipOffset = new(10, -10);
        private static readonly Vec2 size = new(9);
        private static Vec2 hungerbar_pos;
        private static float saturation = 0f, hunger = 0f, exhaustion = 0f, unclampedFlashAlpha = 0f, flashAlpha = 0f, accumulator = 0f;
        private static ushort updateCounter = 0;
        private static bool shouldUpload = true, shouldJitter = false;
        private static GameUIElement? hungerbar;
        private static FoodItem? selectedFood;
        private static FoodItem? hoveredFood;
        private static sbyte alphaDir = 1;
        private static GameMode gamemode;
        private static bool scrollable = false;
        private static Vec2 scrollOffset = Vec2.Zero;
        private static bool isHoldingShift = false;
        private const float ExhaustionWidth = 81f;
        private const float Full = 1f;
        private const string hudScreen = "hud_screen";
        static int UVXOffset;
        static int backgroundUVXOffset;
        static TexturePath outline;
        static Rect uvRectBackground;
        static Rect uvRectWhole;
        static Rect uvRectHalf;
        static string CurrentHoverText = string.Empty;
        static readonly Dictionary<string, float> saturationLevels = new()
        {
            { "poor", 0.1f },
            { "low", 0.3f },
            { "normal", 0.6f },
            { "good", 0.8f },
            { "supernatural", 1.2f }
        };
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
            hungerbar = null;
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


        private static Vec2 GetAbsolutePosition(GameUIElement element)
        {
            Vec2 pos = default;
            for (var current = element; current != null; current = current.Parent)
                pos += current.Position;
            return pos;
        }

        static readonly byte[] food = Encoding.UTF8.GetBytes("minecraft:food");

        private void OnTick()
        {
            if (Onix.LocalPlayer is not LocalPlayer localPlayer || Onix.Gui.ScreenName != hudScreen) return;

            if (localPlayer.GetAttribute(EntityAttributeId.Saturation) is EntityAttribute s) saturation = s.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Hunger) is EntityAttribute h) hunger = h.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Exhaustion) is EntityAttribute rawExhaustion)
                exhaustion = Math.Min(rawExhaustion.Value, 4) / 4;
            gamemode = localPlayer.GameMode;
            bool isOnHunger = localPlayer.Effects.Any(e => e.Id == MobEffectId.Hunger);

            UVXOffset = isOnHunger ? 36 : 0;
            backgroundUVXOffset = isOnHunger ? 133 : 16;
            outline = isOnHunger ? Textures.hungered_outline : Textures.normal_outline;

            uvRectBackground = Rect.FromSize(new(backgroundUVXOffset, 27), size).NormalizeWith(256f);
            uvRectWhole = Rect.FromSize(new(52 + UVXOffset, 27), size).NormalizeWith(256f);
            uvRectHalf = Rect.FromSize(new(61 + UVXOffset, 27), size).NormalizeWith(256f);
            if (hungerbar?.Position.X < Onix.Gui.ScreenSize.X || hungerbar == null || hungerbar.Parent!.JsonProperties.Contains(visibleFalse))
                Reload();

            if (!GetFoodItem(localPlayer.MainHandItem.Item, out selectedFood) || (!selectedFood.AlwaysConsumable && hunger == 20))
                selectedFood = new FoodItem { Hunger = 0, Saturation = 0f };

            unclampedFlashAlpha += alphaDir * 0.125f;
            if (unclampedFlashAlpha >= 1.5f)
                alphaDir = -1;
            else if (unclampedFlashAlpha <= -0.5f)
                alphaDir = 1;
            flashAlpha = MathF.Max(0F, MathF.Min(1F, unclampedFlashAlpha)) * MathF.Max(0F, MathF.Min(1F, .65f));
            shouldJitter = saturation <= 0f && updateCounter % (hunger * 3 + 1) == 0;
        }

        private static bool GetFoodItem(Item? item, out FoodItem output)
        {
            if (item == null)
            {
                output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };
                return false;
            }
            var prefix = $"{item.Namespace}_".Replace("minecraft_", "");
            var path = $"items/{prefix}{item.Name}.json";
            var jsonData = Onix.Game.PackManagerBehavior.LoadContent(TexturePath.Game(path));
            bool isFood = jsonData.AsSpan().IndexOf(food) >= 0;
            if (isFood)
            {
                JsonDocument doc = JsonDocument.Parse(jsonData);
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("minecraft:item", out JsonElement foodItem) &&
                    foodItem.TryGetProperty("components", out JsonElement components) &&
                    components.TryGetProperty("minecraft:food", out JsonElement food))
                {
                    var rawSaturation = food.GetProperty("saturation_modifier");
                    float saturation_modifier = rawSaturation.ValueKind == JsonValueKind.String
                        ?  saturationLevels.GetValueOrDefault(rawSaturation.GetString() ?? "normal", 0.6f)
                        : rawSaturation.GetSingle();
                    int nutrition = food.GetProperty("nutrition").GetInt32();

                    var canAlwaysEat = food.TryGetProperty(Encoding.UTF8.GetBytes("can_always_eat"), out JsonElement cae) && cae.GetBoolean();
                    output = new FoodItem { Hunger = nutrition, Saturation = nutrition * (saturation_modifier * 2), AlwaysConsumable = canAlwaysEat };
                    return true;
                }
            }

            output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };
            return false;
        }

        private void OnContainerScreenTick(ContainerScreen container)
        {
            if (container.GetHoveredItem() is ItemStack item && hoveredFood?.GetHashCode() != item.GetHashCode())
            {
                try //amazing
                {
                    bool isFood = item.Item is {} food && GetFoodItem(food, out hoveredFood);
                    if (!isFood) hoveredFood = null;
                    else GetHoveredText(Onix.Gui.RootUiElement!);
                }
                catch
                {
                    hoveredFood = null;
                }
                scrollOffset = Vec2.Zero;   
            }
        }

        private void OnPreRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi)
        {
            if (screenName != hudScreen || hungerbar == null) return;
            RenderHungerBar(gfx, delta);
        }

        private void OnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUI)
        {
            if (screenName == hudScreen) { hoveredFood = null; scrollOffset = Vec2.Zero; return; }
            if (hoveredFood == null) return;
            gfx.FontType = FontType.Mojangles;
            Vec2 mousePosition = Onix.Gui.MousePosition;
            Vec2 tooltipPos = mousePosition + tooltipOffset;
            Vec2 itemInfoSize = gfx.MeasureText(CurrentHoverText);
            int hungerMid = (int)MathF.Ceiling(hoveredFood.Hunger / 2f);
            int saturationMid = (int)MathF.Ceiling(hoveredFood.Saturation / 2f);
            var hungerWidth = 9 * hungerMid;
            var saturationWidth = 9 * saturationMid;
            var foodInfoWidth = MathF.Max(hungerWidth, saturationWidth);
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
            gfx.RenderText(tooltipPos + tooltipPadding, ColorF.White, CurrentHoverText);

            CacheOutline(gfx, uvRectWhole, Textures.normal_outline);

            void RenderBackground(Rect rect) { gfx.RenderTexture(rect, Textures.icons, 1f, uvRectBackground); }
            for (int i = 0; i < Math.Max(hungerMid, saturationMid); ++i)
            {
                int value = (i << 1) | 1;
                Rect rect = Rect.FromSize(tooltipPos + tooltipPadding, size).MoveDown(itemInfoSize.Y);
                RenderHunger(gfx, uvRectWhole, uvRectHalf, rect.MoveBy(new Vec2(9 * (hungerMid - i - 1), 0)), value, hoveredFood.Hunger, 1f, RenderBackground);
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
            if (!(gamemode == GameMode.Survival || gamemode == GameMode.Adventure) || (accumulator += delta) < FrameThreshold || selectedFood == null) return;

            CacheOutline(gfx, uvRectWhole, outline);

            float expectedHunger = MathF.Min(hunger + selectedFood.Hunger, 20f);
            float expectedSaturation = MathF.Min(MathF.Min(expectedHunger, saturation + selectedFood.Saturation), 20f);

            float exhaustionWidth = MathF.Round(ExhaustionWidth * exhaustion);
            float x = hungerbar_pos.X;
            float y = hungerbar_pos.Y;
            float left = Full - exhaustionWidth / ExhaustionWidth;
            gfx.RenderTexture(new(x - exhaustionWidth, y , x, y + 9), Textures.exhaustion_background, .7f, new(left, 0f, Full, Full));

            bool renderExpectedHunger = expectedHunger != hunger;
            bool renderExpectedSaturation = expectedSaturation != saturation;

            for (int i = 0; i < 10; ++i)
            {
                Rect rect = Rect.FromSize(hungerbar_pos, size).MoveBy(new(-i * 8 - 8, shouldJitter ? -rand.Next(2) : 0));
                gfx.RenderTexture(rect, Textures.icons, 1f, uvRectBackground);

                int idk = (i << 1) | 1;
                RenderHunger(gfx, uvRectWhole, uvRectHalf, rect, idk, hunger, 1f);
                if (renderExpectedHunger)
                    RenderHunger(gfx, uvRectWhole, uvRectHalf, rect, idk, expectedHunger, flashAlpha);
                RenderSaturation(gfx, outline, rect, idk, saturation, ColorF.Yellow);
                if (renderExpectedSaturation)
                    RenderSaturation(gfx, outline, rect, idk, expectedSaturation, ColorF.Lerp(ColorF.Transparent, ColorF.Yellow, flashAlpha));
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
            Reload();
            shouldUpload = true;
        }

        private static bool GetElement(GameUIElement element, Func<GameUIElement, bool> what, Func<GameUIElement, bool> found)
        {
            Stack<GameUIElement> stack = new();
            stack.Push(element);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (what(current) && !found(current))
                    return false;

                var children = current.Children;
                if (children != null && children.Length > 0)
                {
                    for (int i = children.Length - 1; i >= 0; i--)
                    {
                        stack.Push(children[i]);
                    }
                }
            }

            return true;
        }


        private static void GetHoveredText(GameUIElement element)
        {
            GetElement(element,
                what: e => e.Name == "hover_text" &&
                          e.JsonProperties.Contains("#hover_text") &&
                          e.Parent is { Name: "highlight" } highlight &&
                          highlight.JsonProperties.Contains(visibleTrue) &&
                          highlight.Rect is { X: not -1, Y: not -1, Z: not -1, W: not -1 } &&
                          highlight.Parent is { Name: "hover" } hover &&
                          hover.Children.Any(sibling => sibling.Name == "white_border" && sibling.JsonProperties.Contains(visibleTrue)),
                found: e => {
                    var match = RegexPatterns.HoverTextRegex().Match(e.JsonProperties);
                    if (match.Success)
                    {
                        CurrentHoverText = match.Groups[1].Value.Replace("\\n", "\n");
                        e.JsonProperties = "{}";
                    }
                    return true;
                });
        }

        private static void Reload()
        {
            if (Onix.Gui.RootUiElement is GameUIElement root){
                if (GetElement(root, e => e.Name == "hunger_rend" && e.Parent!.JsonProperties.Contains(visibleTrue), element =>
                {
                    //Onix.Gui.RelayoutScreen();
                    hungerbar = element;
                    hungerbar_pos = GetAbsolutePosition(element);
                    element.Position *= 69;
                    return false;
                })) hungerbar = null;
            }
        }

        protected override void OnDisabled()
        {
            //Onix.Gui.RelayoutScreen();
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