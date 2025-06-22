using OnixRuntime.Api.Items;
using OnixRuntime.Api.UI;

namespace AppleSkin
{
    public static class ContainerScreenExtensions
    {
        public static ItemStack? GetHoveredItem(this ContainerScreen container)
        {
            return container.HoveringItem ? container.GetItem(container.HoveredContainer, container.HoveredSlot): null;
        }
    }
}
