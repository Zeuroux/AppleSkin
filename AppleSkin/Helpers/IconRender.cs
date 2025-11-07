using AppleSkin.Stuff;
using AppleSkin.TextureUV;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using System.ComponentModel.Design;

namespace AppleSkin.Helpers
{
    internal static class IconRender
    {

        public static void Hunger(RendererCommon2D gfx, Rect rect, float alpha, bool half = false, bool hungered = false)
        {
            gfx.RenderTexture(rect, Textures.GetHunger(half, hungered), alpha, TextureUV.Hunger.GetHunger(half, hungered));
        }

        public static void Saturation(RendererCommon2D gfx, TexturePath outline, Rect rect, int idk, float saturation, ColorF color, float saturationWidth = 9f)
        {
            var nearestOdd = (int)(saturation - (saturation % 2) + 1);
            if (idk > nearestOdd) return;
            var rectUV = Rect.FullUV;
            if (idk == nearestOdd)
            {
                float pixelWidth = MathF.Round(saturationWidth * ((saturation / 2) - (int)(saturation / 2)));
                if (pixelWidth == 0) return;
                rectUV.X = 1f - (pixelWidth / saturationWidth);
                rect.X += 9 - 9 * (pixelWidth / saturationWidth);
                rect.Width = 9 * (pixelWidth / saturationWidth);
            }

            gfx.RenderTexture(rect, outline, color, rectUV);
        }

        public static void HungerBackground(RendererGame gfx, Rect rect, bool hungered = false)
        {
            gfx.RenderTexture(rect, Textures.GetHungerBackground(hungered), 1f, TextureUV.Hunger.GetBackground(hungered));
        }

        public static void HealthBackground(RendererGame gfx, Rect rect, float alpha, bool flash)
        {
            gfx.RenderTexture(rect, Textures.GetHealthBackground(flash), alpha, TextureUV.Health.GetBackground(flash));
        }

        public static void Absorption(RendererGame gfx, Rect rect, float alpha, bool half, bool hardcore)
        {
            gfx.RenderTexture(rect, Textures.GetAbsorption(half, hardcore), alpha, TextureUV.Health.GetAbsorption(half, hardcore));
        }

        public static void AbsorptionFlash(RendererGame gfx, Rect rect, float alpha, bool half, bool hardcore)
        {
            gfx.RenderTexture(rect, Textures.GetAbsorptionFlash(half, hardcore), alpha, TextureUV.Health.GetAbsorptionFlash(half, hardcore));
        }

        public static void Health(RendererGame gfx, Rect rect, float alpha, bool half, HealthAttributes healthAttrs)
        {
            gfx.RenderTexture(rect, Textures.GetHeart(healthAttrs, half), alpha, TextureUV.Health.GetHeart(healthAttrs, half));
        }

        public static void HealthFlash(RendererGame gfx, Rect rect, float alpha, bool half, HealthAttributes healthAttrs)
        {
            gfx.RenderTexture(rect, Textures.GetHeartFlash(healthAttrs, half), alpha, TextureUV.Health.GetHeartFlash(healthAttrs, half));
        }
    
        public static void ArmorEmpty(RendererGame gfx, Rect rect, float alpha)
        {
            gfx.RenderTexture(rect, Textures.GetArmorEmpty(), alpha, TextureUV.Armor.GetEmpty());
        }

        public static void Armor(RendererGame gfx, Rect rect, float alpha, bool half)
        {
            gfx.RenderTexture(rect, Textures.GetArmor(half), alpha, TextureUV.Armor.GetArmor(half));
        }
    }
}