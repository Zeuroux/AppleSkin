using OnixRuntime.Api;
using OnixRuntime.Api.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleSkin
{
    public static class GameGuiInfoExtensions
    {
        public static void RelayoutScreen(this GameGuiInfo gui)
        {
            gui.GuiScale += 0.00001f;
            gui.GuiScale -= 0.00001f;
        }
    }
}
