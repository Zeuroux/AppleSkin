using OnixRuntime.Api.Items;
using OnixRuntime.Api.UI;

namespace AppleSkin.Extensions
{
    public static class ContainerScreenExtensions
    {
        public static ItemStack? GetHoveredItem(this ContainerScreen container) => container.IsHoveringItem ? container.GetItem(container.HoveredContainer, container.HoveredSlot): null;
    }
}
