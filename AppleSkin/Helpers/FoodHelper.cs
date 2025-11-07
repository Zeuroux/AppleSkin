using OnixRuntime.Api;
using OnixRuntime.Api.Items;
using OnixRuntime.Api.Rendering;
using System.Text;
using System.Text.Json;

namespace AppleSkin.Helpers
{
    internal class FoodHelper
    {
        private static readonly byte[] FoodComponentBytes = Encoding.UTF8.GetBytes("minecraft:food");
        private static readonly IReadOnlyDictionary<string, float> SaturationLevels = new Dictionary<string, float>
        {
            { "poor", 0.1f },
            { "low", 0.3f },
            { "normal", 0.6f },
            { "good", 0.8f },
            { "supernatural", 1.2f }
        };

        private const float DefaultSaturationModifier = 0.6f;
        

        public static bool GetFoodItem(Item? item, out FoodItem output)
        {
            output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };

            if (item == null)
                return false;

            try
            {
                var path = $"items/{item.DescriptionIdentifier.Replace("item.", "").Replace(":", "_")}.json";
                var jsonData = Onix.Game.PackManagerBehavior.LoadContent(TexturePath.Game(path));

                if (jsonData == null || jsonData.Length == 0)
                    jsonData = Onix.Game.PackManagerBehavior.LoadContent(TexturePath.Game($"items/{path[(path.IndexOf('_') + 1)..]}"));

                if (!IsFood(jsonData))
                    return false;

                return ParseFoodData(jsonData, out output);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsFood(byte[] jsonData) =>
            jsonData.AsSpan().IndexOf(FoodComponentBytes) >= 0;

        private static bool ParseFoodData(byte[] jsonData, out FoodItem output)
        {
            output = new FoodItem { Hunger = 0, Saturation = 0f, AlwaysConsumable = false };

            try
            {
                string jsonText = Encoding.UTF8.GetString(jsonData);
                jsonText = RemoveComments(jsonText);

                using var doc = JsonDocument.Parse(jsonText);
                var root = doc.RootElement;

                if (!TryGetFoodComponent(root, out var foodComponent))
                    return false;

                var saturationModifier = GetSaturationModifier(foodComponent);

                int nutrition = foodComponent.TryGetProperty("nutrition", out var e) && e.TryGetInt32(out var value)
                    ? value
                    : 0;

                var canAlwaysEat = foodComponent.TryGetProperty("can_always_eat", out _);
                var effects = ParseEffects(foodComponent);

                output = new FoodItem
                {
                    Hunger = nutrition,
                    Saturation = nutrition * (saturationModifier * 2),
                    AlwaysConsumable = canAlwaysEat,
                    Effects = [.. effects]
                };

                return true;
            }
            catch 
            {
                return false;
            }
        }

        private static string RemoveComments(string jsonText)
        {
            jsonText = RegexPatterns.SingleLineCommentRegex().Replace(jsonText, "");
            return RegexPatterns.MultiLineCommentRegex().Replace(jsonText, "");
        }

        private static bool TryGetFoodComponent(JsonElement root, out JsonElement foodComponent)
        {
            foodComponent = default;
            return root.TryGetProperty("minecraft:item", out var itemElement) &&
                   itemElement.TryGetProperty("components", out var components) &&
                   components.TryGetProperty("minecraft:food", out foodComponent);
        }

        private static float GetSaturationModifier(JsonElement foodComponent)
        {
            if (!foodComponent.TryGetProperty("saturation_modifier", out var rawSaturation))
                return DefaultSaturationModifier;

            return rawSaturation.ValueKind == JsonValueKind.String
                ? SaturationLevels.GetValueOrDefault(rawSaturation.GetString() ?? "normal", DefaultSaturationModifier)
                : rawSaturation.GetSingle();
        }

        private static List<Effect> ParseEffects(JsonElement foodComponent)
        {
            var effects = new List<Effect>();

            if (!foodComponent.TryGetProperty("effects", out var effectsArray) ||
                effectsArray.ValueKind != JsonValueKind.Array)
                return effects;

            foreach (var effectElem in effectsArray.EnumerateArray())
            {
                var effect = new Effect(effectElem.GetProperty("name").GetString() ?? "", GetFloatProperty(effectElem, "chance", 1.0f), GetIntProperty(effectElem, "duration", 0), GetIntProperty(effectElem, "amplifier", 0));
                effects.Add(effect);
            }

            return effects;
        }

        private static float GetFloatProperty(JsonElement element, string propertyName, float defaultValue)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
                ? prop.GetSingle()
                : defaultValue;
        }

        private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number
                ? prop.GetInt32()
                : defaultValue;
        }
    }
}