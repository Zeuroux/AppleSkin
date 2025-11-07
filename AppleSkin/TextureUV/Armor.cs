using OnixRuntime.Api.Maths;
using AppleSkin.Stuff;

namespace AppleSkin.TextureUV
{
    internal class Armor
    {
        private static readonly Rect Empty = Rect.FromSize(16, 9, Constants.IconSize).NormalizeWith(256f);
        private static readonly Rect Full  = Rect.FromSize(34, 9, Constants.IconSize).NormalizeWith(256f);
        private static readonly Rect Half  = Rect.FromSize(25, 9, Constants.IconSize).NormalizeWith(256f);

        public static Rect GetEmpty() => AppleSkin.Config.PreferTextureMap ? Empty : Rect.FullUV;
        public static Rect GetArmor(bool half = false) => AppleSkin.Config.PreferTextureMap ? (half ? Half : Full) : Rect.FullUV;

    }
}