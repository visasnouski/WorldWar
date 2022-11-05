using Microsoft.AspNetCore.Components.Web;
using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Internal
{
    public class DraggableEventObject : DragEventArgs
    {
        public Item? Item { get; set; }
    }
}
