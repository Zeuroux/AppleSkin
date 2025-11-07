using OnixRuntime.Api.Maths;
using AppleSkin.Stuff;

namespace AppleSkin.TextureUV
{
    internal class Health
    {
        private static readonly Rect[] Backgrounds =
        [
            Rect.FromSize(16, 0, Constants.IconSize).NormalizeWith(256f),
            Rect.FromSize(25, 0, Constants.IconSize).NormalizeWith(256f)
        ];

        private static readonly Rect[][] Absorptions =
        [
            [//Hardcore
                Rect.FromSize(160, 45, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(169, 45, Constants.IconSize).NormalizeWith(256f)
            ],
            [
                Rect.FromSize(160, 0, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(169, 0, Constants.IconSize).NormalizeWith(256f)
            ]
        ];

        private static readonly Rect[][] AbsorptionFlashes =
        [
            [//Hardcore
                Rect.FromSize(106, 45, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(115, 45, Constants.IconSize).NormalizeWith(256f)
            ],
            [
                Rect.FromSize(106, 0, Constants.IconSize).NormalizeWith(256f),
                Rect.FromSize(115, 0, Constants.IconSize).NormalizeWith(256f)
            ]
        ];

        private static readonly Rect[][][] Hearts = [
            [//Hardcore
                [//Normal
                    Rect.FromSize(52, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(61, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(178, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(187, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(124, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(133, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(88, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(97, 45, Constants.IconSize).NormalizeWith(256f)
                ]
            ],
            [
                [//Normal
                    Rect.FromSize(52, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(61, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(178, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(187, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(124, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(133, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(88, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(97, 0, Constants.IconSize).NormalizeWith(256f)
                ]
            ]
        ];

        private static readonly Rect[][][] HeartFlashes = [
            [//Hardcore
                [//Normal
                    Rect.FromSize(70, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(79, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(196, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(205, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(142, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(151, 45, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(106, 45, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(115, 45, Constants.IconSize).NormalizeWith(256f)
                ]
            ],
            [
                [//Normal
                    Rect.FromSize(70, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(79, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(196, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(205, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(142, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(151, 0, Constants.IconSize).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(106, 0, Constants.IconSize).NormalizeWith(256f),
                    Rect.FromSize(115, 0, Constants.IconSize).NormalizeWith(256f)
                ]
            ]
        ];

        public static Rect GetBackground(bool changed = false)
        {
            if (!AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            return Backgrounds[changed ? 1 : 0];
        }

        public static Rect GetAbsorption(bool half = false, bool hardcore = false)
        {
            if (!AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            return Absorptions[hardcore ? 0 : 1][half ? 1 : 0];
        }

        public static Rect GetAbsorptionFlash(bool half = false, bool hardcore = false) {
            if (!AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            return AbsorptionFlashes[hardcore ? 0 : 1][half ? 1 : 0];
        }

        public static Rect GetHeart(HealthAttributes attr, bool half = false)
        {
            int statusIndex = 0;
            if (attr.IsFreezing || !AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            //if (attr.IsFreezing) statusIndex = 1; else  //Mojank
            if (attr.HasWitherEffect) statusIndex = 2;
            else if (attr.HasPoisonEffect) statusIndex = 3;
            return Hearts[attr.IsHardcore ? 0 : 1][statusIndex][half ? 1 : 0];
        }

        public static Rect GetHeartFlash(HealthAttributes attr, bool half = false)
        {
            int statusIndex = 0;
            if (attr.IsFreezing || !AppleSkin.Config.PreferTextureMap) return Rect.FullUV;
            //if (attr.IsFreezing) statusIndex = 1; else //Mojank
            if (attr.HasWitherEffect) statusIndex = 2;
            else if (attr.HasPoisonEffect) statusIndex = 3;
            return HeartFlashes[attr.IsHardcore ? 0 : 1][statusIndex][half ? 1 : 0];
        }
    }
}