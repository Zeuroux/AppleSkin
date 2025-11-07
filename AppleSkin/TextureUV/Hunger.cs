using OnixRuntime.Api.Maths;
using AppleSkin.Stuff;

namespace AppleSkin.TextureUV
{
    public static class Hunger
    {
        private static readonly Rect[] Backgrounds =
        [
            Rect.FromSize(16, 27, Constants.IconSize).NormalizeWith(256f),
            Rect.FromSize(133, 27, Constants.IconSize).NormalizeWith(256f)
        ];

        private static readonly Rect[][] Hungers =
        [
            [
                Rect.FromSize(52, 27, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(88, 27, Constants.IconSize).NormalizeWith(256f)],
            [
                Rect.FromSize(61, 27, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(97, 27, Constants.IconSize).NormalizeWith(256f)
            ]
        ];

        public static Rect GetBackground(bool hasHungerEffect = false)
        {
            if (!AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            return Backgrounds[hasHungerEffect ? 1 : 0];
        }

        public static Rect GetHunger(bool half = false, bool hasHungerEffect = false)
        {
            if (!AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            return Hungers[half ? 1 : 0][hasHungerEffect ? 1 : 0];
        }
    }
}