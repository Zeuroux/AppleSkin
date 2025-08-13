using OnixRuntime.Api.OnixClient;
namespace AppleSkin {
    public partial class AppleSkinConfig : OnixModuleSettingRedirector {
        [Value("Reeal")]
        public partial OnixSetting.SettingChangedDelegate MyButton { get; set; }
        public void function()
        {
            Console.WriteLine("Button clicked!");
        }
    }
}