using OnixRuntime.Api.Maths;

namespace AppleSkin.TextureUV
{
    internal class HealthTextureUV
    {
        public static readonly Vec2 size = new(9);
        private static readonly Rect[] Backgrounds =
        [
            Rect.FromSize(16, 0, size).NormalizeWith(256f),
            Rect.FromSize(25, 0, size).NormalizeWith(256f)
        ];

        private static readonly Rect[][] Absorptions =
        [
            [//Hardcore
                Rect.FromSize(160, 45, size).NormalizeWith(256f),
                Rect.FromSize(169, 45, size).NormalizeWith(256f)
            ],
            [
                Rect.FromSize(160, 0, size).NormalizeWith(256f),
                Rect.FromSize(169, 0, size).NormalizeWith(256f)
            ]
        ];

        private static readonly Rect[][] AbsorptionFlashes =
        [
            [//Hardcore
                Rect.FromSize(106, 45, size).NormalizeWith(256f),
                Rect.FromSize(115, 45, size).NormalizeWith(256f)
            ],
            [
                Rect.FromSize(106, 0, size).NormalizeWith(256f),
                Rect.FromSize(115, 0, size).NormalizeWith(256f)
            ]
        ];

        private static readonly Rect[][][] Hearts = [
            [//Hardcore
                [//Normal
                    Rect.FromSize(52, 45, size).NormalizeWith(256f),
                    Rect.FromSize(61, 45, size).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(178, 45, size).NormalizeWith(256f),
                    Rect.FromSize(187, 45, size).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(124, 45, size).NormalizeWith(256f),
                    Rect.FromSize(133, 45, size).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(88, 45, size).NormalizeWith(256f),
                    Rect.FromSize(97, 45, size).NormalizeWith(256f)
                ]
            ],
            [
                [//Normal
                    Rect.FromSize(52, 0, size).NormalizeWith(256f),
                    Rect.FromSize(61, 0, size).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(178, 0, size).NormalizeWith(256f),
                    Rect.FromSize(187, 0, size).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(124, 0, size).NormalizeWith(256f),
                    Rect.FromSize(133, 0, size).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(88, 0, size).NormalizeWith(256f),
                    Rect.FromSize(97, 0, size).NormalizeWith(256f)
                ]
            ]
        ];

        private static readonly Rect[][][] HeartFlashes = [
            [//Hardcore
                [//Normal
                    Rect.FromSize(70, 45, size).NormalizeWith(256f),
                    Rect.FromSize(79, 45, size).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(196, 45, size).NormalizeWith(256f),
                    Rect.FromSize(205, 45, size).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(142, 45, size).NormalizeWith(256f),
                    Rect.FromSize(151, 45, size).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(106, 45, size).NormalizeWith(256f),
                    Rect.FromSize(115, 45, size).NormalizeWith(256f)
                ]
            ],
            [
                [//Normal
                    Rect.FromSize(70, 0, size).NormalizeWith(256f),
                    Rect.FromSize(79, 0, size).NormalizeWith(256f)
                ],
                [//Freeze
                    Rect.FromSize(196, 0, size).NormalizeWith(256f),
                    Rect.FromSize(205, 0, size).NormalizeWith(256f)
                ],
                [//Wither
                    Rect.FromSize(142, 0, size).NormalizeWith(256f),
                    Rect.FromSize(151, 0, size).NormalizeWith(256f)
                ],
                [//Poison
                    Rect.FromSize(106, 0, size).NormalizeWith(256f),
                    Rect.FromSize(115, 0, size).NormalizeWith(256f)
                ]
            ]
        ];

        public static Rect Background(bool changed = false) => Backgrounds[changed ? 1 : 0];

        public static Rect Absorption(bool half = false, bool hardcore = false) => Absorptions[hardcore ? 0 : 1][half ? 1 : 0];

        public static Rect AbsorptionFlash(bool half = false, bool hardcore = false) => AbsorptionFlashes[hardcore ? 0 : 1][half ? 1 : 0];

        public static Rect Heart(HealthAttributes attr, bool half = false)
        {
            int statusIndex = 0;
            if (attr.IsFreezing) return new Rect(Vec2.Zero, Vec2.One);
            //if (attr.IsFreezing) statusIndex = 1; else  //Mojank
            if (attr.HasWitherEffect) statusIndex = 2;
            else if (attr.HasPoisonEffect) statusIndex = 3;
            return Hearts[attr.IsHardcore ? 0 : 1][statusIndex][half ? 1 : 0];
        }

        public static Rect HeartFlash(HealthAttributes attr, bool half = false)
        {
            int statusIndex = 0;
            if (attr.IsFreezing) return new Rect(Vec2.Zero, Vec2.One);
            //if (attr.IsFreezing) statusIndex = 1; else //Mojank
            if (attr.HasWitherEffect) statusIndex = 2;
            else if (attr.HasPoisonEffect) statusIndex = 3;
            return HeartFlashes[attr.IsHardcore ? 0 : 1][statusIndex][half ? 1 : 0];
        }
    }
}