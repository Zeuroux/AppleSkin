using OnixRuntime.Api.Rendering;

namespace AppleSkin
{
    internal static class Textures
    {
        public static readonly NineSlice tooltip = new(TexturePath.Game("textures/ui/purpleBorder"));
        public static readonly TexturePath icons = TexturePath.Game("textures/gui/icons.png");
        public static readonly TexturePath normal_outline = TexturePath.Assets("normal_outline.png");
        public static readonly TexturePath hungered_outline = TexturePath.Assets("hungered_outline.png");
        public static readonly TexturePath exhaustion_background = TexturePath.Assets("exhaustion.png");
    }
}