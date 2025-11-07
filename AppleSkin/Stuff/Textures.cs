using AppleSkin.Helpers;
using OnixRuntime.Api.Rendering;

namespace AppleSkin.Stuff
{
    internal static class Textures
    {
        private static readonly TexturePath Icons = TexturePath.Game("textures/gui/icons.png");

        private static readonly TexturePath AbsorptionHeart = TexturePath.Game("textures/ui/absorption_heart.png");
        private static readonly TexturePath AbsorptionHeartHalf = TexturePath.Game("textures/ui/absorption_heart_half.png");
        private static readonly TexturePath FreezeHeart = TexturePath.Game("textures/ui/freeze_heart.png");
        private static readonly TexturePath FreezeHeartFlash = TexturePath.Game("textures/ui/freeze_heart_flash.png");
        private static readonly TexturePath FreezeHeartFlashHalf = TexturePath.Game("textures/ui/freeze_heart_flash_half.png");
        private static readonly TexturePath FreezeHeartHalf = TexturePath.Game("textures/ui/freeze_heart_half.png");
        private static readonly TexturePath Heart = TexturePath.Game("textures/ui/heart.png");
        private static readonly TexturePath HeartBackground = TexturePath.Game("textures/ui/heart_background.png");
        private static readonly TexturePath HeartBlink = TexturePath.Game("textures/ui/heart_blink.png");
        private static readonly TexturePath HeartFlash = TexturePath.Game("textures/ui/heart_flash.png");
        private static readonly TexturePath HeartFlashHalf = TexturePath.Game("textures/ui/heart_flash_half.png");
        private static readonly TexturePath HeartHalf = TexturePath.Game("textures/ui/heart_half.png");
        private static readonly TexturePath PoisonHeart = TexturePath.Game("textures/ui/poison_heart.png");
        private static readonly TexturePath PoisonHeartFlash = TexturePath.Game("textures/ui/poison_heart_flash.png");
        private static readonly TexturePath PoisonHeartFlashHalf = TexturePath.Game("textures/ui/poison_heart_flash_half.png");
        private static readonly TexturePath PoisonHeartHalf = TexturePath.Game("textures/ui/poison_heart_half.png");
        private static readonly TexturePath WitherHeart = TexturePath.Game("textures/ui/wither_heart.png");
        private static readonly TexturePath WitherHeartFlash = TexturePath.Game("textures/ui/wither_heart_flash.png");
        private static readonly TexturePath WitherHeartFlashHalf = TexturePath.Game("textures/ui/wither_heart_flash_half.png");
        private static readonly TexturePath WitherHeartHalf = TexturePath.Game("textures/ui/wither_heart_half.png");

        private static readonly TexturePath HardcoreAbsorptionHeart = TexturePath.Game("textures/ui/hardcore/absorption_heart.png");
        private static readonly TexturePath HardcoreAbsorptionHeartHalf = TexturePath.Game("textures/ui/hardcore/absorption_heart_half.png");
        private static readonly TexturePath HardcoreFreezeHeart = TexturePath.Game("textures/ui/hardcore/freeze_heart.png");
        private static readonly TexturePath HardcoreFreezeHeartFlash = TexturePath.Game("textures/ui/hardcore/freeze_heart_flash.png");
        private static readonly TexturePath HardcoreFreezeHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/freeze_heart_flash_half.png");
        private static readonly TexturePath HardcoreFreezeHeartHalf = TexturePath.Game("textures/ui/hardcore/freeze_heart_half.png");
        private static readonly TexturePath HardcoreHeart = TexturePath.Game("textures/ui/hardcore/heart.png");
        private static readonly TexturePath HardcoreHeartFlash = TexturePath.Game("textures/ui/hardcore/heart_flash.png");
        private static readonly TexturePath HardcoreHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/heart_flash_half.png");
        private static readonly TexturePath HardcoreHeartHalf = TexturePath.Game("textures/ui/hardcore/heart_half.png");
        private static readonly TexturePath HardcorePoisonHeart = TexturePath.Game("textures/ui/hardcore/poison_heart.png");
        private static readonly TexturePath HardcorePoisonHeartFlash = TexturePath.Game("textures/ui/hardcore/poison_heart_flash.png");
        private static readonly TexturePath HardcorePoisonHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/poison_heart_flash_half.png");
        private static readonly TexturePath HardcorePoisonHeartHalf = TexturePath.Game("textures/ui/hardcore/poison_heart_half.png");
        private static readonly TexturePath HardcoreWitherHeart = TexturePath.Game("textures/ui/hardcore/wither_heart.png");
        private static readonly TexturePath HardcoreWitherHeartFlash = TexturePath.Game("textures/ui/hardcore/wither_heart_flash.png");
        private static readonly TexturePath HardcoreWitherHeartFlashHalf = TexturePath.Game("textures/ui/hardcore/wither_heart_flash_half.png");
        private static readonly TexturePath HardcoreWitherHeartHalf = TexturePath.Game("textures/ui/hardcore/wither_heart_half.png");

        private static readonly TexturePath HungerBackground = TexturePath.Game("textures/ui/hunger_background.png");
        //unused in java and bedrock
        //private static readonly TexturePath HungerBlink = TexturePath.Game("textures/ui/hunger_blink.png");
        //private static readonly TexturePath HungerEffect = TexturePath.Game("textures/ui/hunger_effect.png");
        private static readonly TexturePath HungerEffectBackground = TexturePath.Game("textures/ui/hunger_effect_background.png");
        //unused in java and bedrock
        //private static readonly TexturePath HungerEffectFlashFull = TexturePath.Game("textures/ui/hunger_effect_flash_full.png");
        //private static readonly TexturePath HungerEffectFlashHalf = TexturePath.Game("textures/ui/hunger_effect_flash_half.png");
        private static readonly TexturePath HungerEffectFull = TexturePath.Game("textures/ui/hunger_effect_full.png");
        private static readonly TexturePath HungerEffectHalf = TexturePath.Game("textures/ui/hunger_effect_half.png");
        //unused in java and bedrock
        //private static readonly TexturePath HungerFlashFull = TexturePath.Game("textures/ui/hunger_flash_full.png");
        //private static readonly TexturePath HungerFlashHalf = TexturePath.Game("textures/ui/hunger_flash_half.png");
        private static readonly TexturePath HungerFull = TexturePath.Game("textures/ui/hunger_full.png");
        private static readonly TexturePath HungerHalf = TexturePath.Game("textures/ui/hunger_half.png");

        private static readonly TexturePath ArmorEmpty = TexturePath.Game("textures/ui/armor_empty.png");
        private static readonly TexturePath ArmorFull = TexturePath.Game("textures/ui/armor_full.png");
        private static readonly TexturePath ArmorHalf = TexturePath.Game("textures/ui/armor_half.png");

        public static readonly NineSlice TooltipBackground = new(TexturePath.Game("textures/ui/purpleBorder"));
        public static readonly TexturePath NormalOutline = TexturePath.Assets("normal_outline.png");
        public static readonly TexturePath HungeredOutline = TexturePath.Assets("hungered_outline.png");
        public static readonly TexturePath Exhaustionbackground = TexturePath.Assets("exhaustion.png");
        

        public static TexturePath GetHealthBackground(bool flash)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            return flash ? HeartBlink : HeartBackground;
        }

        public static TexturePath GetAbsorption(bool half = false, bool hardcore = false)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            if (hardcore)
                return half ? HardcoreAbsorptionHeartHalf : HardcoreAbsorptionHeart;
            else
                return half ? AbsorptionHeartHalf : AbsorptionHeart;
        }

        public static TexturePath GetAbsorptionFlash(bool half = false, bool hardcore = false)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            if (hardcore)
                return half ? HardcoreAbsorptionHeartHalf : HardcoreAbsorptionHeart;
            else
                return half ? AbsorptionHeartHalf : AbsorptionHeart;
        }

        public static TexturePath GetHeart(HealthAttributes attr, bool half = false)
        {
            if (AppleSkin.Config.PreferTextureMap) // freeze heart isnt on texture map for some reason
                if (attr.IsFreezing)
                    return half ? FreezeHeartHalf : FreezeHeart;
                else
                    return Icons;

            if (attr.IsHardcore)
            {
                if (attr.HasPoisonEffect)
                    return half ? HardcorePoisonHeartHalf : HardcorePoisonHeart;
                else if (attr.HasWitherEffect)
                    return half ? HardcoreWitherHeartHalf : HardcoreWitherHeart;
                else if (attr.IsFreezing)
                    return half ? HardcoreFreezeHeartHalf : HardcoreFreezeHeart;
                else
                    return half ? HardcoreHeartHalf : HardcoreHeart;
            }
            else
            {
                
                if (attr.HasPoisonEffect)
                    return half ? PoisonHeartHalf : PoisonHeart;
                else if (attr.HasWitherEffect)
                    return half ? WitherHeartHalf : WitherHeart;
                else if (attr.IsFreezing)
                    return half ? FreezeHeartHalf : FreezeHeart;
                else
                    return half ? HeartHalf : Heart;
            }
        }

        public static TexturePath GetHeartFlash(HealthAttributes attr, bool half = false)
        {
            if (AppleSkin.Config.PreferTextureMap) // freeze heart isnt on texture map for some reason
                if (attr.IsFreezing)
                    return half ? FreezeHeartFlashHalf : FreezeHeartFlash;
                else
                    return Icons;
            if (attr.IsHardcore)
            {
                if (attr.HasPoisonEffect)
                    return half ? HardcorePoisonHeartFlashHalf : HardcorePoisonHeartFlash;
                else if (attr.HasWitherEffect)
                    return half ? HardcoreWitherHeartFlashHalf : HardcoreWitherHeartFlash;
                else if (attr.IsFreezing)
                    return half ? HardcoreFreezeHeartFlashHalf : HardcoreFreezeHeartFlash;
                else
                    return half ? HardcoreHeartFlashHalf : HardcoreHeartFlash;
            }
            else
            {
                if (attr.HasPoisonEffect)
                    return half ? PoisonHeartFlashHalf : PoisonHeartFlash;
                else if (attr.HasWitherEffect)
                    return half ? WitherHeartFlashHalf : WitherHeartFlash;
                else if (attr.IsFreezing)
                    return half ? FreezeHeartFlashHalf : FreezeHeartFlash;
                else
                    return half ? HeartFlashHalf : HeartFlash;
            }
        }
    
        public static TexturePath GetHungerBackground(bool hasHungerEffect)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            return hasHungerEffect ? HungerEffectBackground : HungerBackground;
        }

        public static TexturePath GetHunger(bool half = false, bool hasHungerEffect = false)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            if (hasHungerEffect)
                return half ? HungerEffectHalf : HungerEffectFull;
            else
                return half ? HungerHalf : HungerFull;
        }

        public static TexturePath GetArmorEmpty()
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            return ArmorEmpty;
        }

        public static TexturePath GetArmor(bool half = false)
        {
            if (AppleSkin.Config.PreferTextureMap)
                return Icons;
            return half ? ArmorHalf : ArmorFull;
        }
    }
}