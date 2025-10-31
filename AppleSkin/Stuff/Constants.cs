using OnixRuntime.Api.Maths;

namespace AppleSkin.Stuff
{
    internal static class Constants
    {

        public const string visibleTrue = "\"#visible\":true";
        public const string visibleFalse = "\"#visible\":false";
        public const string hudScreen = "hud_screen";

        public const float ExhaustionWidth = 80f;
        public const float Full = 1f;

        public static class TooltipSizes
        {
            public static readonly Vec2 Padding = new(4f, 4f);
            public static readonly Vec2 Offset = new(10, -10);
            public static readonly Vec2 IconSize = new(9);
        }
    }
}