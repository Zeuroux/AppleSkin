using AppleSkin.Extensions;
using static AppleSkin.AppleSkin;

namespace AppleSkin.Helpers
{
    public class Effect
    {
        public required string Name { get; set; }
        public float Chance { get; set; }
        public int Duration { get; set; }
        public int Amplifier { get; set; }
    }
    public class FoodItem
    {
        required public int Hunger { get; set; }
        required public float Saturation { get; set; }

        public bool IsValid = true;
        public bool AlwaysConsumable { get; set; } = false;
        public Effect[] Effects { get; set; } = [];

        private const float RegenExhaustionIncrement = 6.0f;
        private const float MaxExhaustion = 4.0f;
        private const int MaxFoodLevel = 20;
        private const int HealthRegenThreshold = 18;
        private const int FullHealthThreshold = 20;
        public float GetEstimatedHealthIncrement(HungerAttributes player)
        {
            var foodLevel = Math.Min(player.Hunger + Hunger, MaxFoodLevel);
            float healthIncrement = 0;

            if (foodLevel >= HealthRegenThreshold)
            {
                var saturationLevel = Math.Min(player.Saturation + Saturation, foodLevel);
                healthIncrement = CalculateHealthFromFood(foodLevel, saturationLevel, player.Exhaustion);
            }

            healthIncrement += CalculateHealthFromRegeneration(Effects);
            return healthIncrement;
        }

        private static float CalculateHealthFromRegeneration(Effect[] effects)
        {
            var regenEffect = Array.Find(effects, e => e.Name == "regeneration");
            if (regenEffect == null)
                return 0f;

            var amplifier = regenEffect.Amplifier % 32;
            var durationTicks = regenEffect.Duration * 20;
            var tickInterval = Math.Max(50 >> amplifier, 1);

            return MathF.Floor(durationTicks / (float)tickInterval);
        }

        private static float CalculateHealthFromFood(int foodLevel, float saturationLevel, float exhaustionLevel)
        {
            float health = 0;

            while (foodLevel >= HealthRegenThreshold)
            {
                while (exhaustionLevel > MaxExhaustion)
                {
                    exhaustionLevel -= MaxExhaustion;
                    if (saturationLevel > 0)
                        saturationLevel = Math.Max(saturationLevel - 1, 0);
                    else
                        foodLevel--;
                }

                if (foodLevel >= FullHealthThreshold && saturationLevel > float.Epsilon)
                {
                    var limitedSaturationLevel = Math.Min(saturationLevel, RegenExhaustionIncrement);
                    var exhaustionUntilAboveMax = MaxExhaustion.NextUp() - exhaustionLevel;
                    var numIterationsUntilAboveMax = Math.Max(1, (int)Math.Ceiling(exhaustionUntilAboveMax / limitedSaturationLevel));

                    health += limitedSaturationLevel / RegenExhaustionIncrement * numIterationsUntilAboveMax;
                    exhaustionLevel += limitedSaturationLevel * numIterationsUntilAboveMax;
                }
                else if (foodLevel >= HealthRegenThreshold)
                {
                    health += 1;
                    exhaustionLevel += RegenExhaustionIncrement;
                }
            }

            return health;
        }
    }
}