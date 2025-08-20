using AppleSkin.Extensions;
using OnixRuntime.Api;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;

namespace AppleSkin
{
    public struct HudAttributes
    {
        public int Armor;
        public int Health;
        public int HealthMax;
        public int Hunger;
        public float Saturation;
        public float Exhaustion;
    }
    public static class TooltipSizes
    {
        public static readonly Vec2 Padding = new(4f, 4f);
        public static readonly Vec2 Offset = new(10, -10);
    }

}
