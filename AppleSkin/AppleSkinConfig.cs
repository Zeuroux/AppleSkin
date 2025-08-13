using OnixRuntime.Api.OnixClient;
namespace AppleSkin {
    public partial class AppleSkinConfig : OnixModuleSettingRedirector {
        [Button(nameof(real), "real")]
        public partial OnixSetting.SettingChangedDelegate fake { get; set; }
        public void real()
        {
            Console.WriteLine("real called from AppleSkinConfig.cs");
        }
    }
}