using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;
using System.Collections.Immutable;
using System.Text;

namespace AppleSkin
{

    public class AppleSkin : OnixPluginBase
    {
        private const float FrameThreshold = 1 / 60;
        private const string visibleFalse = "\"#visible\":false";
        private const string visibleTrue = "\"#visible\":true";
        private static readonly Dictionary<string, FoodItem> foods = new()
        {
            { "apple", new FoodItem { Hunger = 4, Saturation = 2.4f, AlwaysConsumable = true } },
            { "baked_potato", new FoodItem { Hunger = 5, Saturation = 6 } },
            { "beef", new FoodItem { Hunger = 3, Saturation = 1.8f } },
            { "beetroot", new FoodItem { Hunger = 1, Saturation = 1 } },
            { "beetroot_soup", new FoodItem { Hunger = 6, Saturation = 7.2f } },
            { "bread", new FoodItem { Hunger = 5, Saturation = 6 } },
            { "cake", new FoodItem { Hunger = 2, Saturation = 0.4f } },
            { "carrot", new FoodItem { Hunger = 3, Saturation = 3.6f } },
            { "chicken", new FoodItem { Hunger = 2, Saturation = 1.2f } },
            { "chorus_fruit", new FoodItem { Hunger = 4, Saturation = 2.4f, AlwaysConsumable = true } },
            { "cod", new FoodItem { Hunger = 2, Saturation = 0.4f } },
            { "cooked_beef", new FoodItem { Hunger = 8, Saturation = 12.8f } },
            { "cooked_chicken", new FoodItem { Hunger = 6, Saturation = 7.2f } },
            { "cooked_cod", new FoodItem { Hunger = 5, Saturation = 6 } },
            { "cooked_mutton", new FoodItem { Hunger = 6, Saturation = 9.6f } },
            { "cooked_porkchop", new FoodItem { Hunger = 8, Saturation = 12.8f } },
            { "cooked_rabbit", new FoodItem { Hunger = 5, Saturation = 6 } },
            { "cooked_salmon", new FoodItem { Hunger = 6, Saturation = 9.6f } },
            { "cookie", new FoodItem { Hunger = 2, Saturation = 0.4f } },
            { "dried_kelp", new FoodItem { Hunger = 1, Saturation = 0.2f } },
            { "enchanted_golden_apple", new FoodItem { Hunger = 4, Saturation = 9.6f, AlwaysConsumable = true } },
            { "glow_berries", new FoodItem { Hunger = 2, Saturation = 0.4f } },
            { "golden_apple", new FoodItem { Hunger = 4, Saturation = 9.6f, AlwaysConsumable = true } },
            { "golden_carrot", new FoodItem { Hunger = 6, Saturation = 14.4f } },
            { "honey_bottle", new FoodItem { Hunger = 6, Saturation = 1.2f, AlwaysConsumable = true } },
            { "melon_slice", new FoodItem { Hunger = 2, Saturation = 1.2f } },
            { "mushroom_stew", new FoodItem { Hunger = 6, Saturation = 7.2f } },
            { "mutton", new FoodItem { Hunger = 2, Saturation = 1.2f } },
            { "poisonous_potato", new FoodItem { Hunger = 2, Saturation = 1.2f } },
            { "porkchop", new FoodItem { Hunger = 3, Saturation = 1.8f } },
            { "potato", new FoodItem { Hunger = 1, Saturation = 0.6f } },
            { "pufferfish", new FoodItem { Hunger = 1, Saturation = 0.2f } },
            { "pumpkin_pie", new FoodItem { Hunger = 8, Saturation = 4.8f } },
            { "rabbit", new FoodItem { Hunger = 3, Saturation = 1.8f } },
            { "rabbit_stew", new FoodItem { Hunger = 10, Saturation = 12 } },
            { "rotten_flesh", new FoodItem { Hunger = 4, Saturation = 0.8f } },
            { "salmon", new FoodItem { Hunger = 2, Saturation = 0.4f } },
            { "spider_eye", new FoodItem { Hunger = 2, Saturation = 3.2f } },
            { "suspicious_stew", new FoodItem { Hunger = 6, Saturation = 7.2f, AlwaysConsumable = true } },
            { "sweet_berries", new FoodItem { Hunger = 2, Saturation = 1.2f } },
            { "tropical_fish", new FoodItem { Hunger = 1, Saturation = 0.2f } }
        };
        private static readonly Random rand = new();
        private static readonly Vec2 tooltipPadding = new(4.5f, 4.2f);
        private static readonly Vec2 tooltipOffset = new(10, -10);
        private static readonly Vec2 size = new(9);
        private Vec2 hungerbar_pos;
        private float saturation = 0f, hunger = 0f, exhaustion = 0f, unclampedFlashAlpha = 0f, flashAlpha = 0f, accumulator = 0f;
        private int updateCounter = 0;
        private bool isOnHunger = false, shouldUpload = true, shouldJitter = false;
        private GameUIElement? hungerbar;
        private FoodItem? selectedFood;
        private ItemStack? hoveredFood;
        private sbyte alphaDir = 1;
        private GameMode gamemode;
        private int lastHoveredSlot = -1;
        private bool scrollable = false;
        private Vec2 scrollOffset = Vec2.Zero;
        private bool isHoldingShift = false;

        private readonly EnchantType[][] groups = [
            [//armor
                EnchantType.Protection,
                EnchantType.FireProtection,
                EnchantType.FeatherFalling,
                EnchantType.BlastProtection,
                EnchantType.ProjectileProtection,
                EnchantType.Thorns,
                EnchantType.Respiration,
                EnchantType.DepthStrider,
                EnchantType.AquaAffinity,
                EnchantType.FrostWalker,
                EnchantType.SoulSpeed,
                EnchantType.SwiftSneak
            ],
            [//universal enchants
                EnchantType.Sharpness,
                EnchantType.Smite,
                EnchantType.BaneOfArthropods,
                EnchantType.FireAspect,
                EnchantType.Looting,
                EnchantType.SilkTouch,
                EnchantType.Unbreaking,
                EnchantType.Fortune,
                EnchantType.Flame,
                EnchantType.LuckOfTheSea,
                EnchantType.Impaling,
                EnchantType.WindBurst,
                EnchantType.Density,
                EnchantType.Breach
            ]
        ];

        private static readonly EnchantType[] maxOne = [
            EnchantType.AquaAffinity,
            EnchantType.Flame,
            EnchantType.Infinity,
            EnchantType.Mending,
            EnchantType.CurseOfBinding,
            EnchantType.CurseOfVanishing,
            EnchantType.Channeling,
            EnchantType.Multishot
        ];

        public static AppleSkin Instance { get; private set; } = null!;
        public static AppleSkinConfig Config { get; private set; } = null!;

        public AppleSkin(OnixPluginInitInfo initInfo) : base(initInfo)
        {
            Instance = this;
            base.DisablingShouldUnloadPlugin = false;
#if DEBUG
            //base.WaitForDebuggerToBeAttached();
#endif
        }

        protected override void OnLoaded()
        {
            Console.WriteLine("Gaming");
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
            if (key == InputKey.ClickType.Scroll && hoveredFood is not null && scrollable)
            {
                ref var offset = ref (isHoldingShift ? ref scrollOffset.X : ref scrollOffset.Y);
                offset += (isDown ? 1 : -1) * 9;
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

        private static RawImageData CropUV(RawImageData source, Rect uvRect)
        {
            int x = (int)(uvRect.X * source.Width);
            int y = (int)(uvRect.Y * source.Height);
            int width = (int)(uvRect.Width * source.Width);
            int height = (int)(uvRect.Height * source.Height);

            x = Math.Clamp(x, 0, source.Width - 1);
            y = Math.Clamp(y, 0, source.Height - 1);
            width = Math.Clamp(width, 1, source.Width - x);
            height = Math.Clamp(height, 1, source.Height - y);

            RawImageData cropped = new(width, height);

            for (int yy = 0; yy < height; yy++)
            {
                for (int xx = 0; xx < width; xx++)
                {
                    uint pixel = source.GetRawPixel(x + xx, y + yy);
                    cropped.SetRawPixel(xx, yy, pixel);
                }
            }

            return cropped;
        }

        private static RawImageData CreateOutline(RawImageData imageData, bool touchesEdge)
        {
            int width = imageData.Width;
            int height = imageData.Height;

            RawImageData outline = new(width, height);
            ColorF red = ColorF.White;

            bool[] edges = new bool[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ColorF pixel = imageData.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        if (x == 0) edges[y * width] = true;
                        if (y == 0) edges[x] = true;
                        if (x == width - 1) edges[y * width + (width - 1)] = true;
                        if (y == height - 1) edges[(height - 1) * width + x] = true;
                        continue;
                    }
                    if (x > 0 && imageData.GetPixel(x - 1, y).A != 0)
                        edges[y * width + x - 1] = true;
                    if (x < width - 1 && imageData.GetPixel(x + 1, y).A != 0)
                        edges[y * width + x + 1] = true;
                    if (y > 0 && imageData.GetPixel(x, y - 1).A != 0)
                        edges[(y - 1) * width + x] = true;
                    if (y < height - 1 && imageData.GetPixel(x, y + 1).A != 0)
                        edges[(y + 1) * width + x] = true;
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!edges[y * width + x])
                        continue;

                    if (touchesEdge)
                    {
                        outline.SetPixel(x, y, red);
                    }
                    else
                    {
                        int neighborCount = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;

                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                                {
                                    if (imageData.GetPixel(nx, ny).A != 0)
                                        neighborCount++;
                                }
                            }
                        }

                        if (neighborCount == 1)
                        {
                            int tx = x + 1, ty = y - 1;
                            int bx = x - 1, by = y + 1;

                            if (tx >= 0 && ty >= 0 && tx < outline.Width && ty < outline.Height)
                                outline.SetPixel(tx, ty, red);

                            if (bx >= 0 && by >= 0 && bx < outline.Width && by < outline.Height)
                                outline.SetPixel(bx, by, red);
                        }

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (Math.Abs(dx) == Math.Abs(dy))
                                    continue;

                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && ny >= 0 && nx < outline.Width && ny < outline.Height)
                                {
                                    if (imageData.GetPixel(nx, ny).A == 0)
                                        outline.SetPixel(nx, ny, red);
                                }
                            }
                        }

                    }
                }
            }

            return outline;
        }

        private void OnTick()
        {
            if (Onix.LocalPlayer is not LocalPlayer localPlayer) return;
            if (localPlayer.GetAttribute(EntityAttributeId.Saturation) is EntityAttribute s) saturation = s.Value;
            if (localPlayer.GetAttribute(EntityAttributeId.Hunger) is EntityAttribute h) hunger = h.Value;
            gamemode = localPlayer.GameMode;

            if (localPlayer.GetAttribute(EntityAttributeId.Exhaustion) is EntityAttribute rawExhaustion)
                exhaustion = Math.Min(rawExhaustion.Value, 4) / 4;

            isOnHunger = localPlayer.Effects.Any(e => e.Id == MobEffectId.Hunger);

            if (hungerbar?.Position.X < Onix.Gui.ScreenSize.X || hungerbar == null || hungerbar.Parent!.JsonProperties.Contains(visibleFalse))
                Reload();

            if (!foods.TryGetValue(localPlayer.MainHandItem.Item?.Name ?? "", out selectedFood) || (!selectedFood.AlwaysConsumable && hunger == 20))
                selectedFood = new FoodItem { Hunger = 0, Saturation = 0f };

            unclampedFlashAlpha += alphaDir * 0.125f;
            if (unclampedFlashAlpha >= 1.5f)
                alphaDir = -1;
            else if (unclampedFlashAlpha <= -0.5f)
                alphaDir = 1;
            flashAlpha = MathF.Max(0F, MathF.Min(1F, unclampedFlashAlpha)) * MathF.Max(0F, MathF.Min(1F, .65f));
            shouldJitter = saturation <= 0f && updateCounter % (hunger * 3 + 1) == 0;
        }

        private void OnContainerScreenTick(ContainerScreen container)
        {
            if (container.GetHoveredItem() is ItemStack item)
            {
                hoveredFood = !container.HoveredContainer.StartsWith("recipe_") ? item.Clone() : null;
                int slot = container.HoveredSlot;
                if (lastHoveredSlot != slot)
                {
                    lastHoveredSlot = slot;
                    scrollOffset = Vec2.Zero;
                }
            }
        }

        private void OnPreRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUi)
        {
            if (screenName != "hud_screen" || hungerbar == null) return;
            RenderHungerBar(gfx, delta);
        }

        private void OnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden, bool isClientUI)
        {
            if (screenName == "hud_screen" || !(hoveredFood != null && hoveredFood.Item != null)) { hoveredFood = null; scrollOffset = Vec2.Zero; return; }
            if (hoveredFood == null || !foods.TryGetValue(hoveredFood.Item.Name, out FoodItem? foodItem)) return;
            bool showCategory = false;
            bool isOnSearchTab = false;
            if (Onix.Gui.RootUiElement is GameUIElement root && Onix.LocalPlayer is LocalPlayer localPlayer)
            {
                showCategory = root.FindChildRecursive("recipe_book")?.JsonProperties.Contains(visibleFalse) == true;
                var gameMode = localPlayer.GameMode;
                bool isRecipeBookLayout = root.FindChildRecursive("tab_navigation_panel")?.JsonProperties.Contains("\"#is_recipe_book_layout\":true") == true;
                var partialName = (gameMode == GameMode.Creative && isRecipeBookLayout ? "end_" : "tab_") + gameMode.ToString().ToLower();
                GetElement(root, e => e.Name.Equals("search_tab_toggle") && e.Parent!.Name.Contains(partialName) || e.Name.Equals("hover_text"), element =>
                {
                    if (element.Name.Equals("hover_text"))
                        element.JsonProperties = "{}";
                    else
                        isOnSearchTab = element.JsonProperties.Contains("\"#toggle_state\":true");
                    return true;
                });
            }
            gfx.FontType = FontType.Mojangles;
            Vec2 mousePosition = Onix.Gui.MousePosition;
            Vec2 tooltipPos = mousePosition + tooltipOffset;
            bool hasEnchants = hoveredFood.Enchants.Length > 0;
            bool isRenamed = hoveredFood.CustomName.Length > 0;
            StringBuilder itemInfo = new(hoveredFood.DisplayName);

            if (hoveredFood.Item.HasEnchantedOverlay || hasEnchants)
                itemInfo.Insert(0, "§b");
            if (isRenamed)
                itemInfo.Insert(0, "§o");
            if (isOnSearchTab && showCategory)
                itemInfo.Append("\n§r§9" + hoveredFood.Item.CreativeCategory);

            List<EnchantmentInstance> enchants = SortWithGroup(hoveredFood.Enchants, groups);

            foreach (var enchantInstance in enchants)
            {
                string enchant = EnchantToString(enchantInstance);
                itemInfo.Append("\n§r")
                    .Append(enchant.Contains("Curse") ? "§c" : "§7")
                    .Append($"{enchant}");
            }

            foreach (string lore in hoveredFood.Lore)
                itemInfo.Append("\n§o§5" + lore);

            Vec2 itemInfoSize = gfx.MeasureText(itemInfo.ToString());

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

            Textures.
                        tooltip.Render(gfx, Rect.FromSize(tooltipPos, tooltipSize), 1f);
            gfx.RenderText(tooltipPos + tooltipPadding, ColorF.White, itemInfo.ToString());

            Rect uvRectHalf = Rect.FromSize(new(61, 27), size).NormalizeWith(256f);
            Rect uvRectWhole = Rect.FromSize(new(52, 27), size).NormalizeWith(256f);
            Rect uvRectBackground = Rect.FromSize(new(16, 27), size).NormalizeWith(256f);
            TexturePath outline = isOnHunger ? Textures.hungered_outline : Textures.normal_outline;
            CacheOutline(gfx, uvRectWhole, outline);

            int hungerMid = (int)MathF.Ceiling(foodItem.Hunger / 2f);
            int saturationMid = (int)MathF.Ceiling(foodItem.Saturation / 2f);

            void RenderBackground(Rect rect) { gfx.RenderTexture(rect, Textures.icons, 1f, uvRectBackground); }
            for (int i = 0; i < 10; ++i)
            {
                int value = (i << 1) | 1;
                Rect rect = Rect.FromSize(tooltipPos + tooltipPadding + new Vec2(0, itemInfoSize.Y), size);
                RenderHunger(gfx, uvRectWhole, uvRectHalf, rect.MoveBy(new Vec2(9 * (hungerMid - i - 1), 0)), value, foodItem.Hunger, 1f, RenderBackground);
                RenderSaturation(gfx, outline, rect.MoveBy(new Vec2(9 * (saturationMid - i - 1), 9)), value, foodItem.Saturation, ColorF.Yellow, RenderBackground);
            }
        }

        private static List<EnchantmentInstance> SortWithGroup(EnchantmentInstance[] itemEnchants, EnchantType[][] byGroup)
        {
            var groupOrder = new Dictionary<EnchantType, int>();
            for (int i = 0; i < byGroup.Length; i++)
            {
                foreach (var enchant in byGroup[i])
                    groupOrder[enchant] = i;
            }

            var grouped = new List<EnchantmentInstance>[byGroup.Length];
            for (int i = 0; i < byGroup.Length; i++)
                grouped[i] = [];

            var ungrouped = new List<EnchantmentInstance>();

            foreach (var enchant in itemEnchants)
            {
                if (groupOrder.TryGetValue(enchant.Type, out int groupIndex))
                    grouped[groupIndex].Add(enchant);
                else
                    ungrouped.Add(enchant);
            }

            var finalOrder = new List<EnchantmentInstance>(itemEnchants.Length);
            foreach (var group in grouped)
                finalOrder.AddRange(group);
            finalOrder.AddRange(ungrouped);
            return finalOrder;
        }


        private static string EnchantToString(EnchantmentInstance enchant)
        {
            var sb = new StringBuilder();
            var type = enchant.Type;
            sb.Append(Onix.Game.GetTranslatedMessage(type.ToNameTranslationKey()));
            if (maxOne.Contains(type))
                return sb.ToString();
            int level = enchant.Level;
            string levelKey = "enchantment.level." + level;
            string levelStr = Onix.Game.GetTranslatedMessage(levelKey);
            return sb.Append(" " + (levelStr == levelKey ? level : levelStr)).ToString();
        }

        private void CacheOutline(RendererGame gfx, Rect uvRectWhole, TexturePath outline)
        {
            if (gfx.GetTextureStatus(outline) == RendererTextureStatus.Missing || shouldUpload)
            {
                RawImageData hungerWhole = CropUV(RawImageData.Load(Onix.Game.PackManager.LoadContent(Textures.icons)), uvRectWhole);

                int width = hungerWhole.Width;
                int height = hungerWhole.Height;
                bool touchesEdge = Enumerable.Range(0, width).Any(x =>
                    hungerWhole.GetPixel(x, 0).A > 0 || hungerWhole.GetPixel(x, height - 1).A > 0) ||
                    Enumerable.Range(0, height).Any(y =>
                    hungerWhole.GetPixel(0, y).A > 0 || hungerWhole.GetPixel(width - 1, y).A > 0);

                RawImageData created = CreateOutline(hungerWhole, touchesEdge);
                gfx.UploadTexture(outline, created);
                shouldUpload = false;
            }
        }

        private void RenderHungerBar(RendererGame gfx, float delta)
        {
            if ((accumulator += delta) < FrameThreshold || selectedFood == null || !(gamemode == GameMode.Survival || gamemode == GameMode.Adventure)) return;

            int xOffset = isOnHunger ? 36 : 0;
            Rect uvRectBackground = Rect.FromSize(new(isOnHunger ? 133 : 16, 27), size).NormalizeWith(256f);
            Rect uvRectWhole = Rect.FromSize(new(52 + xOffset, 27), size).NormalizeWith(256f);
            Rect uvRectHalf = Rect.FromSize(new(61 + xOffset, 27), size).NormalizeWith(256f);
            TexturePath outline = isOnHunger ? Textures.hungered_outline : Textures.normal_outline;
            CacheOutline(gfx, uvRectWhole, outline);

            float expectedHunger = MathF.Min(hunger + selectedFood.Hunger, 20);
            float expectedSaturation = MathF.Min(Math.Min(expectedHunger, saturation + selectedFood.Saturation), 20);

            float exhaustionWidth = MathF.Round(81 * exhaustion);
            gfx.RenderTexture(new(hungerbar_pos.X - exhaustionWidth + 1, hungerbar_pos.Y, hungerbar_pos.X + 1, hungerbar_pos.Y + 9), Textures.exhaustion_background, .7f, new(1f - (exhaustionWidth / 81f), 0f, 1f, 1f));

            for (int i = 0; i < 10; ++i)
            {
                Rect rect = Rect.FromSize(hungerbar_pos, size).MoveBy(new(-i * 8 - 8, shouldJitter ? -rand.Next(2) : 0));
                gfx.RenderTexture(rect, Textures.icons, 1f, uvRectBackground);

                int idk = (i << 1) | 1;
                RenderHunger(gfx, uvRectWhole, uvRectHalf, rect, idk, hunger, 1f);
                if (expectedHunger != hunger)
                    RenderHunger(gfx, uvRectWhole, uvRectHalf, rect, idk, expectedHunger, flashAlpha);
                if (gfx.GetTextureStatus(outline) != RendererTextureStatus.Loaded) continue;
                RenderSaturation(gfx, outline, rect, idk, saturation, ColorF.Yellow);
                if (expectedSaturation != saturation)
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
            if (element == null) return true;

            if (what(element) && !found(element))
                return false;

            var children = element.Children;
            if (children == null || children.Length == 0)
                return true;

            for (int i = 0; i < children.Length; i++)
                if (!GetElement(children[i], what, found))
                    return false;
            

            return true;
        }

        private void Reload()
        {
            if (Onix.Gui.RootUiElement is GameUIElement root){
                if (GetElement(root, e => e.Name == "hunger_rend" && e.Parent!.JsonProperties.Contains(visibleTrue), element =>
                {
                    ScheduleScreenRelayout();
                    hungerbar = element;
                    hungerbar_pos = GetAbsolutePosition(element);
                    element.Position *= 69;
                    return false;
                })) hungerbar = null;
            }
        }

        protected override void OnDisabled()
        {
            ScheduleScreenRelayout();
            Console.WriteLine("Disabled");
        }

        private static void ScheduleScreenRelayout()
        {
            Onix.Gui.GuiScale += 0.00001f;
            Onix.Gui.GuiScale -= 0.00001f;
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