using AppleSkin.Extensions;
using OnixRuntime.Api;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;

namespace AppleSkin
{
    public class HudElements
    {
        private const string visibleTrue = "\"#visible\":true";
        private const string visibleFalse = "\"#visible\":false";
        private static readonly Vec2 Limbo = new(float.MaxValue);
        public Vec2 Armor { get; protected set; }
        public Vec2 Health { get; protected set; }
        public Vec2 Hunger { get; protected set; }

        private GameUIElement? _armor, _health, _hunger, _profile;
        public HudElements()
        {
            GetProfile();
            ReloadPosition();
        }
        
        public bool IsLoaded()
        {
            return _profile != null;
        }
        public void Update()
        {
            if (_profile == null || _profile.JsonProperties.Contains(visibleFalse))
                GetProfile();
            if (_armor == null || _health == null || _hunger == null)
                return;
            if (!IsLimbo(_armor.Position) || !IsLimbo(_health.Position) || !IsLimbo(_hunger.Position)) {
                ReloadPosition();
            }
        }

        private static bool IsLimbo(Vec2 pos) => pos == Limbo;

        private void ReloadPosition()
        {
            if (_profile == null || _armor == null || _health == null || _hunger == null) return;
            Onix.Gui.ScheduleScreenRelayout();
            Armor = _armor.GetAbsolutePosition();
            Health = _health.GetAbsolutePosition();
            Hunger = _hunger.GetAbsolutePosition();
            _armor.Position = Limbo;
            _health.Position = Limbo;
            _hunger.Position = Limbo;
        }

        private void GetProfile()
        {
            Onix.Gui.RootUiElement!.FindMatchRecursive(e => e.Name == "hunger_rend" && e.Parent!.JsonProperties.Contains(visibleTrue), element => {
                Onix.Gui.ScheduleScreenRelayout();
                var profile = element.Parent!;
                _armor = profile.FindChildRecursive("armor_rend")!;
                _health = profile.FindChildRecursive("heart_rend")!;
                _hunger = profile.FindChildRecursive("hunger_rend")!;
                _profile = profile;
                return true;
            });
        }
    }
}
