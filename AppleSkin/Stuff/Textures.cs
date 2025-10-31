using AppleSkin.Helpers;
using OnixRuntime.Api.Rendering;

namespace AppleSkin.Stuff
{
    internal static class Textures
    {
        public static readonly TexturePath Icons = TexturePath.Game("textures/gui/icons.png");

        public static readonly TexturePath AbsorptionHeart = TexturePath.Game("textures/ui/absorption_heart.png");
        public static readonly TexturePath AbsorptionHeartHalf = TexturePath.Game("textures/ui/absorption_heart_half.png");
        public static readonly TexturePath FreezeHeart = TexturePath.Game("textures/ui/freeze_heart.png");
        public static readonly TexturePath FreezeHeartFlash = TexturePath.Game("textures/ui/freeze_heart_flash.png");
        public static readonly TexturePath FreezeHeartFlashHalf = TexturePath.Game("textures/ui/freeze_heart_flash_half.png");
        public static readonly TexturePath FreezeHeartHalf = TexturePath.Game("textures/ui/freeze_heart_half.png");
        public static readonly TexturePath Heart = TexturePath.Game("textures/ui/heart.png");
        public static readonly TexturePath HeartBackground = TexturePath.Game("textures/ui/heart_background.png");
        public static readonly TexturePath HeartBlink = TexturePath.Game("textures/ui/heart_blink.png");
        public static readonly TexturePath HeartFlash = TexturePath.Game("textures/ui/heart_flash.png");
        public static readonly TexturePath HeartFlashHalf = TexturePath.Game("textures/ui/heart_flash_half.png");
        public static readonly TexturePath HeartHalf = TexturePath.Game("textures/ui/heart_half.png");
        public static readonly TexturePath PoisonHeart = TexturePath.Game("textures/ui/poison_heart.png");
        public static readonly TexturePath PoisonHeartFlash = TexturePath.Game("textures/ui/poison_heart_flash.png");
        public static readonly TexturePath PoisonHeartFlashHalf = TexturePath.Game("textures/ui/poison_heart_flash_half.png");
        public static readonly TexturePath PoisonHeartHalf = TexturePath.Game("textures/ui/poison_heart_half.png");
        public static readonly TexturePath WitherHeart = TexturePath.Game("textures/ui/wither_heart.png");
        public static readonly TexturePath WitherHeartFlash = TexturePath.Game("textures/ui/wither_heart_flash.png");
        public static readonly TexturePath WitherHeartFlashHalf = TexturePath.Game("textures/ui/wither_heart_flash_half.png");
        public static readonly TexturePath WitherHeartHalf = TexturePath.Game("textures/ui/wither_heart_half.png");

        public static readonly TexturePath HardcoreAbsorptionHeart = TexturePath.Game("textures/ui/hardcore/absorption_heart.png");
        public static readonly TexturePath HardcoreAbsorptionHeartHalf = TexturePath.Game("textures/ui/hardcore/absorption_heart_half.png");
        public static readonly TexturePath HardcoreFreezeHeart = TexturePath.Game("textures/ui/hardcore/freeze_heart.png");
        public static readonly TexturePath HardcoreFreezeHeartFlash = TexturePath.Game("textures/ui/hardcore/freeze_heart_flash.png");
        public static readonly TexturePath HardcoreFreezeHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/freeze_heart_flash_half.png");
        public static readonly TexturePath HardcoreFreezeHeartHalf = TexturePath.Game("textures/ui/hardcore/freeze_heart_half.png");
        public static readonly TexturePath HardcoreHeart = TexturePath.Game("textures/ui/hardcore/heart.png");
        public static readonly TexturePath HardcoreHeartFlash = TexturePath.Game("textures/ui/hardcore/heart_flash.png");
        public static readonly TexturePath HardcoreHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/heart_flash_half.png");
        public static readonly TexturePath HardcoreHeartHalf = TexturePath.Game("textures/ui/hardcore/heart_half.png");
        public static readonly TexturePath HardcorePoisonHeart = TexturePath.Game("textures/ui/hardcore/poison_heart.png");
        public static readonly TexturePath HardcorePoisonHeartFlash = TexturePath.Game("textures/ui/hardcore/poison_heart_flash.png");
        public static readonly TexturePath HardcorePoisonHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/poison_heart_flash_half.png");
        public static readonly TexturePath HardcorePoisonHeartHalf = TexturePath.Game("textures/ui/hardcore/poison_heart_half.png");
        public static readonly TexturePath HardcoreWitherHeart = TexturePath.Game("textures/ui/hardcore/wither_heart.png");
        public static readonly TexturePath HardcoreWitherHeartFlash = TexturePath.Game("textures/ui/hardcore/wither_heart_flash.png");
        public static readonly TexturePath HardcoreWitherHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/wither_heart_flash_half.png");
        public static readonly TexturePath HardcoreWitherHeartHalf = TexturePath.Game("textures/ui/hardcore/wither_heart_half.png");

        public static readonly TexturePath HungerBackground = TexturePath.Game("textures/ui/hunger_background.png");
        public static readonly TexturePath HungerBlink = TexturePath.Game("textures/ui/hunger_blink.png");
        public static readonly TexturePath HungerEffect = TexturePath.Game("textures/ui/hunger_effect.png");
        public static readonly TexturePath HungerEffectBackground = TexturePath.Game("textures/ui/hunger_effect_background.png");
        public static readonly TexturePath HungerEffectFlashFull = TexturePath.Game("textures/ui/hunger_effect_flash_full.png");
        public static readonly TexturePath HungerEffectFlashHalf = TexturePath.Game("textures/ui/hunger_effect_flash_half.png");
        public static readonly TexturePath HungerEffectFull = TexturePath.Game("textures/ui/hunger_effect_full.png");
        public static readonly TexturePath HungerEffectHalf = TexturePath.Game("textures/ui/hunger_effect_half.png");
        public static readonly TexturePath HungerFlashFull = TexturePath.Game("textures/ui/hunger_flash_full.png");
        public static readonly TexturePath HungerFlashHalf = TexturePath.Game("textures/ui/hunger_flash_half.png");
        public static readonly TexturePath HungerFull = TexturePath.Game("textures/ui/hunger_full.png");
        public static readonly TexturePath HungerHalf = TexturePath.Game("textures/ui/hunger_half.png");

        public static readonly NineSlice TooltipBackground = new(TexturePath.Game("textures/ui/purpleBorder"));
        public static readonly TexturePath NormalOutline = TexturePath.Assets("normal_outline.png");
        public static readonly TexturePath HungeredOutline = TexturePath.Assets("hungered_outline.png");
        public static readonly TexturePath Exhaustionbackground = TexturePath.Assets("exhaustion.png");

        public static TexturePath GetFreezeHeart(bool half = false, bool isHardcore = false)
        {
            if (isHardcore)
                return half ? HardcoreFreezeHeartHalf : HardcoreFreezeHeart;
            else
                return half ? FreezeHeartHalf : FreezeHeart;
        }

        public static TexturePath GetFreezeHeartFlash(bool half = false, bool isHardcore = false)
        {
            if (isHardcore)
                return half ? HardcoreFreezeHeartFlashHalf : HardcoreFreezeHeartFlash;
            else
                return half ? FreezeHeartFlashHalf : FreezeHeartFlash;
        }
    }
}