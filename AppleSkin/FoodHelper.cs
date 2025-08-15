using OnixRuntime.Api;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppleSkin
{
    internal class FoodHelper
    {
        static readonly byte[] food = Encoding.UTF8.GetBytes("minecraft:food");
        static readonly Dictionary<string, float> saturationLevels = new()
        {
            { "poor", 0.1f },
            { "low", 0.3f },
            { "normal", 0.6f },
            { "good", 0.8f },
            { "supernatural", 1.2f }
        };
        public static bool GetFoodItem(Item? item, out FoodItem output)
        {
            if (item == null)
            {
                output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };
                return false;
            }
            var prefix = $"{item.Namespace}_".Replace("minecraft_", "");
            var path = $"items/{prefix}{item.Name.Replace("enchanted_golden_apple", "appleEnchanted").Replace("cooked_mutton", "muttonCooked").Replace("raw_mutton", "muttonRaw")}.json";
            var jsonData = Onix.Game.PackManagerBehavior.LoadContent(TexturePath.Game(path));
            bool isFood = jsonData.AsSpan().IndexOf(food) >= 0;
            if (isFood)
            {
                string jsonText = Encoding.UTF8.GetString(jsonData);
                jsonText = RegexPatterns.SingleLineCommentRegex().Replace(jsonText, "");
                jsonText = RegexPatterns.MultiLineCommentRegex().Replace(jsonText, "");
                JsonDocument doc = JsonDocument.Parse(jsonText);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("minecraft:item", out JsonElement foodItem) &&
                    foodItem.TryGetProperty("components", out JsonElement components) &&
                    components.TryGetProperty("minecraft:food", out JsonElement food))
                {
                    var rawSaturation = food.GetProperty("saturation_modifier");
                    float saturation_modifier = rawSaturation.ValueKind == JsonValueKind.String
                        ? saturationLevels.GetValueOrDefault(rawSaturation.GetString() ?? "normal", 0.6f)
                        : rawSaturation.GetSingle();
                    int nutrition = food.GetProperty("nutrition").GetInt32();

                    var canAlwaysEat = food.TryGetProperty("can_always_eat", out JsonElement cae);

                    List<Effect> effects = [];
                    if (food.TryGetProperty("effects", out JsonElement effectsArray) && effectsArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var effectElem in effectsArray.EnumerateArray())
                        {
                            Effect effect = new()
                            {
                                Name = effectElem.GetProperty("name").GetString() ?? "",
                                Chance = effectElem.TryGetProperty("chance", out var chanceElem) && chanceElem.ValueKind == JsonValueKind.Number
                                    ? chanceElem.GetSingle()
                                    : 1.0f,
                                Duration = effectElem.TryGetProperty("duration", out var durationElem) && durationElem.ValueKind == JsonValueKind.Number
                                    ? durationElem.GetInt32()
                                    : 0,
                                Amplifier = effectElem.TryGetProperty("amplifier", out var ampElem) && ampElem.ValueKind == JsonValueKind.Number
                                    ? ampElem.GetInt32()
                                    : 0
                            };
                            effects.Add(effect);
                        }
                    }

                    output = new FoodItem
                    {
                        Hunger = nutrition,
                        Saturation = nutrition * (saturation_modifier * 2),
                        AlwaysConsumable = canAlwaysEat,
                        Effects = effects.ToArray()
                    };
                    return true;
                }
            }


            output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };
            return false;
        }

        public static float GetEstimatedHealthIncrement(FoodItem food)
        {
            int foodLevel = Math.Min((int)AppleSkin.hunger + food.Hunger, 20);
            float healthIncrement = 0;
            if (foodLevel >= 18)
            {
                float saturation_level = Math.Min(AppleSkin.saturation + food.Saturation, foodLevel);
                healthIncrement = GetEstimatedHealthIncrement(foodLevel, saturation_level, AppleSkin.exhaustion);
            }
            for (int i = 0; i < food.Effects.Length; i++)
            {
                Effect effect = food.Effects[i];
                if (effect.Name == "regeneration")
                {
                    int amplifier = effect.Amplifier % 32;
                    int durationTicks = effect.Duration * 20;

                    healthIncrement += (float)MathF.Floor(durationTicks / Math.Max(50 >> amplifier, 1));
                    break;
                }
            }
            return healthIncrement;
        }

        public static float REGEN_EXHAUSTION_INCREMENT = 6.0F;
        public static float MAX_EXHAUSTION = 4.0F;
        private static float GetEstimatedHealthIncrement(int food_level, float saturation_level, float exhaustion_level)
        {
            float health = 0;
            while (food_level >= 18)
            {
                while (exhaustion_level > MAX_EXHAUSTION)
                {
                    exhaustion_level -= MAX_EXHAUSTION;
                    if (saturation_level > 0)
                        saturation_level = Math.Max(saturation_level - 1, 0);
                    else
                        food_level -= 1;
                }
                if (food_level >= 20 && saturation_level > float.Epsilon)
                {
                    float limitedSaturationLevel = Math.Min(saturation_level, REGEN_EXHAUSTION_INCREMENT);
                    float exhaustionUntilAboveMax = FloatExtensions.NextUp(MAX_EXHAUSTION) - exhaustion_level;
                    int numIterationsUntilAboveMax = Math.Max(1, (int)Math.Ceiling(exhaustionUntilAboveMax / limitedSaturationLevel));

                    health += (limitedSaturationLevel / REGEN_EXHAUSTION_INCREMENT) * numIterationsUntilAboveMax;
                    exhaustion_level += limitedSaturationLevel * numIterationsUntilAboveMax;
                }
                else if (food_level >= 18)
                {
                    health += 1;
                    exhaustion_level += REGEN_EXHAUSTION_INCREMENT;
                }
            }
        
	    	return health;
        }
    }
}
