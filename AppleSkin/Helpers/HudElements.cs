using AppleSkin.Extensions;
using AppleSkin.Stuff;
using OnixRuntime.Api;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppleSkin.Helpers
{
    public class HudElements
    {
        private static readonly Vec2 Limbo = new(float.MaxValue);
        public Vec2 Armor { get; protected set; }
        public Vec2 Health { get; protected set; }
        public Vec2 Hunger { get; protected set; }
        public bool IsCenteredBottom { get; protected set; }

        private GameUIElement? _armor, _health, _hunger, _profile, _exp;
        public HudElements()
        {
            GetProfile();
            ReloadPosition();
        }

        public void Update()
        {
            if (_profile == null || _profile.JsonProperties.Contains(Constants.visibleFalse))
                GetProfile();
            if (_armor == null || _health == null || _hunger == null)
                return;
            if (!IsLimbo(_armor.Position) || !IsLimbo(_health.Position) || !IsLimbo(_hunger.Position))
                ReloadPosition();
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

        public void SetActionBarOffsetY(int Y)
        {
            if (_profile == null || _exp == null) return;
            float oy, ny;
            if (_exp.Children.FirstOrDefault(c => c.Name == "popup_item_text") is { } popupText && (oy = popupText.Position.Y) != (ny = MathF.Min(Y, oy)))
                popupText.Position = popupText.Position.WithY(ny);
        }

        private void GetProfile()
        {
            AppleSkin.ReuploadOutline = true;
            Onix.Gui.RootUiElement?.FindMatchRecursive(e => e.Name == "hunger_rend" && e.Parent!.JsonProperties.Contains(Constants.visibleTrue), element => {
                Onix.Gui.ScheduleScreenRelayout();
                var profile = element.Parent!;
                _armor = profile.Children.FirstOrDefault(c => c.Name == "armor_rend")!;
                _health = profile.Children.FirstOrDefault(c => c.Name == "heart_rend")!;
                _hunger = profile.Children.FirstOrDefault(c => c.Name == "hunger_rend")!;
                _exp = profile.Children.FirstOrDefault(c => c.Name == "exp_rend")!;
                _profile = profile;
                IsCenteredBottom = _profile.Name.StartsWith('c');
                return true;
            });
        }
    }
}
