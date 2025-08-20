using OnixRuntime.Api.Maths;

namespace AppleSkin.TextureUV
{
    public static class HungerTextureUV
    {
        private static readonly Vec2 size = new(9);

        private static readonly Rect[] Backgrounds =
        [
            Rect.FromSize(16, 27, size).NormalizeWith(256f),
                Rect.FromSize(133, 27, size).NormalizeWith(256f)
        ];

        private static readonly Rect[] Wholes =
        [
            Rect.FromSize(52, 27, size).NormalizeWith(256f),
                Rect.FromSize(88, 27, size).NormalizeWith(256f)
        ];

        private static readonly Rect[] Halves =
        [
            Rect.FromSize(61, 27, size).NormalizeWith(256f),
                Rect.FromSize(97, 27, size).NormalizeWith(256f)
        ];

        public static Rect Background(bool hungered = false) => Backgrounds[hungered ? 1 : 0];
        public static Rect Whole(bool hungered = false) => Wholes[hungered ? 1 : 0];
        public static Rect Half(bool hungered = false) => Halves[hungered ? 1 : 0];
    }
}
