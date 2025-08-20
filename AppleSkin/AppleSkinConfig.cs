using OnixRuntime.Api.OnixClient;
using OnixRuntime.Api.Rendering;
namespace AppleSkin {
    public partial class AppleSkinConfig : OnixModuleSettingRedirector {
        //[Button(nameof(Fubction), "Amazing button")]
        //public partial OnixSetting.SettingChangedDelegate bUTTON { get; set; }
        //void Fubction() {
        //    Console.WriteLine(AppleSkin.Hovered == null);
        //}



        //[Value(false)]
        //[Name("Disable", "asdasdas")]
        //public partial bool Disaple { get; set; }

        //[Value(false)]
        //[Name("Disresple", "asdasdas")]
        //public partial bool Disresple { get; set; }


        [Value(true)]
        [Name("Show food values in tooltip", "If true, shows the hunger and saturation values of food in its tooltip while holding Show food values in tooltip SHIFT")]
        public partial bool ShowFoodValuesInTooltip { get; set; }
        [Value(true)]
        [Name("Alwats show food values in tooltip", "If true, shows the hunger and saturation values of food in its tooltip automatically (without needing to hold SHIFT)")]
        public partial bool AlwaysShowFoodValuesInTooltip { get; set; }
        [Value(true)]
        [Name("Show saturation overlay", "If true, shows your current saturation level overlayed on the hunger bar")]
        public partial bool ShowSaturationOverlay { get; set; }
        [Value(true)]
        [Name("Show hunger restored from held food", "If true, shows the hunger (and saturation if showSaturationHudOverlay is true) that would be restored by food you are currently holding")]
        public partial bool ShowHungerRestoredFromHeldFood { get; set; }
        //ill do this when bedock supports off-hand items
        //[Value(true)]
        //[Name("Show overlays for food in off-hand", "If true, enables the hunger/saturation/health overlays for food in your off-hand")]
        //public partial bool ShowOffhandFoodOverlays { get; set; }
        [Value(true)]
        [Name("Show exhaustion overlay", "If true, shows your food exhaustion as a progress bar behind the hunger bar")]
        public partial bool ShowExhaustionOverlay { get; set; }
        [Value(true)]
        [Name("Show estimated health overlay", "If true, shows an overlay on the health bar that estimates your current health based on your food and saturation levels")]
        public partial bool ShowEstimatedHealthOverlay { get; set; }
        //not implemented yet
        //[Value(true)]
        //[Name("Show food values in Debug Screen", "If true, shows your hunger, saturation, and exhaustion level in Debug Screen")]
        //public partial bool ShowFoodValuesInDebugScreen { get; set; }
        [Value(true)]
        [Name("Animate HUD icons to match Minecraft", "If true, health/hunger overlay will shake to match Minecraft's icon animations")]
        public partial bool AnimateHudIcons { get; set; }
        [Value(.65f)]
        [MinMax(0f, 1f)]
        [Name("Max alpha of flashing HUD icons", "Alpha value of the flashing icons at their most visible point (1.0 = fully opaque, 0.0 = fully transparent)")]
        public partial float MaxFlashingHudIconAlpha { get; set; }

    }
}