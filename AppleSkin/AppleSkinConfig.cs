using OnixRuntime.Api.Maths;
using OnixRuntime.Api.OnixClient;
using OnixRuntime.Api.Rendering;
namespace AppleSkin {
    public partial class AppleSkinConfig : OnixModuleSettingRedirector {
        [Category("Hunger Settings")]
        [Value(true)]
        [ChangeCallback(nameof(OnChangeShowFoodValuesInTooltip))]
        [Name("Show food values in tooltip", "If true, shows the hunger and saturation values of food in its tooltip while holding Show food values in tooltip SHIFT")]
        public partial bool ShowFoodValuesInTooltip { get; set; }
        public void OnChangeShowFoodValuesInTooltip()
        {
            AlwaysShowFoodValuesInTooltipSetting.IsHidden = !ShowFoodValuesInTooltip;   
        }

        [Value(true)]
        [Name("Always show food values in tooltip", "If true, shows the hunger and saturation values of food in its tooltip automatically (without needing to hold SHIFT)")]
        public partial bool AlwaysShowFoodValuesInTooltip { get; set; }

        [Value(true)]
        [Name("Show saturation overlay", "If true, shows your current saturation level overlayed on the hunger bar")]
        public partial bool ShowSaturationOverlay { get; set; }

        [Value(true)]
        [Name("Show hunger restored from held food", "If true, shows the hunger (and saturation if showSaturationHudOverlay is true) that would be restored by food you are currently holding")]
        public partial bool ShowHungerRestoredFromHeldFood { get; set; }

        [Value(true)]
        [Name("Show exhaustion overlay", "If true, shows your food exhaustion as a progress bar behind the hunger bar")]
        public partial bool ShowExhaustionOverlay { get; set; }

        [Value(1f, 1f, 0f, 1f)]
        [Name("Saturation Color", "Color of the saturation outline")]
        public partial ColorF SaturationColor { get; set; }

        [Value(true)]
        [ChangeCallback(nameof(OnChangeOutline))]
        [Name("Use hunger background for saturation outline", "Turn this on/off if the outline doesnt match the hunger icon")]
        public partial bool UseBackgroundAsOutline { get; set; }
        private bool? _lastOutlineSetting = null;
        public void OnChangeOutline()
        {
            if (_lastOutlineSetting != UseBackgroundAsOutline)
            {
                _lastOutlineSetting = UseBackgroundAsOutline;
                SaturationOutlineThicknessSetting.IsHidden = UseBackgroundAsOutline;
                AppleSkin.ReuploadOutline = true;
            }
        }

        [Value(1)]
        [MinMax(1, 20)]
        [ChangeCallback(nameof(OnThicknessChange))]
        [Name("Saturation Thinkness", "Increase or decrease this if it looks ugly/barely visible on your pack")]
        public partial int SaturationOutlineThickness { get; set; }
        private int _lastOutlineThickness = -1;
        public void OnThicknessChange()
        {
            if (_lastOutlineThickness != SaturationOutlineThickness)
            {
                _lastOutlineThickness = SaturationOutlineThickness;
                AppleSkin.ReuploadOutline = true;
            }
        }

        [Category("Health Settings")]

        [Value(true)]
        [Name("Show estimated health overlay", "If true, shows an overlay on the health bar that estimates your expected health based on your food and saturation levels")]
        public partial bool ShowEstimatedHealthOverlay { get; set; }

        [Value(true)]
        [Name("Show estimated absorption overlay", "If true, shows an overlay on the health bar that estimates your expected absorption based on the item you're holding")]
        public partial bool ShowEstimatedAbsorptionOverlay { get; set; }

        [Value(true)]
        [Name("Show estimated health boost overlay", "If true, shows an overlay on the health bar that shows your expected extra hearts based on the item you're holding")]
        public partial bool ShowEstimatedHealthBoostOverlay { get; set; }

        [CategoryStop()]

        [Category("Armor Settings")]

        [Value(true)]
        [Name("Show estimated armor overlay", "If true, shows an overlay on the armor bar that shows your expected armor based on the item you're holding")]
        public partial bool ShowEstimatedArmorOverlay { get; set; }

        [CategoryStop()]

        //ill do this when bedock supports off-hand items
        //[Value(true)]
        //[Name("Show overlays for food in off-hand", "If true, enables the hunger/saturation/health overlays for food in your off-hand")]
        //public partial bool ShowOffhandFoodOverlays { get; set; }

        [Value(true)]
        [Name("Animate HUD icons to match Minecraft", "If true, health/hunger overlay will shake to match Minecraft's icon animations")]
        public partial bool AnimateHudIcons { get; set; }

        [Value(.65f)]
        [MinMax(0f, 1f)]
        [Name("Max alpha of flashing HUD icons", "Alpha value of the flashing icons at their most visible point (1.0 = fully opaque, 0.0 = fully transparent)")]
        public partial float MaxFlashingHudIconAlpha { get; set; }

        [Value(true)]
        [ChangeCallback(nameof(OnChangePreferPreferedTexture))]
        [Name("Prefer icons texture map", "If true, uses the textures/gui/icons.png for rendering hud icons instead of the seperated hud icons")]
        public partial bool PreferTextureMap { get; set; }
        private bool? _lastPreferTexturePath = null;
        public void OnChangePreferPreferedTexture()
        {
            if (_lastPreferTexturePath != PreferTextureMap)
            {
                _lastPreferTexturePath = PreferTextureMap;
                AppleSkin.ReuploadOutline = true;
            }
        }
    }
}