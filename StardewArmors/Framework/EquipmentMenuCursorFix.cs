using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace StardewArmors.Framework
{
    /// <summary>Carries the item on the player's cursor into SpaceCore's equipment menu when it opens.</summary>
    /// <remarks>
    /// SpaceCore opens its equipment menu from a button on the inventory page, as a child menu with its own empty
    /// <see cref="MenuWithInventory.heldItem"/>. The item the player was holding stays in <see cref="Farmer.CursorSlotItem"/>,
    /// where the new menu can't use it, so armor held on the cursor can't be dropped straight into the slot.
    /// This moves it across once, when the menu first appears. No Harmony: it only uses public game members, and it
    /// finds SpaceCore's menu by type name so it does nothing if SpaceCore changes.
    /// </remarks>
    internal sealed class EquipmentMenuCursorFix
    {
        /// <summary>The full type name of SpaceCore's equipment menu.</summary>
        private const string EquipmentMenuTypeName = "SpaceCore.EquipmentMenu";

        private readonly IMonitor monitor;

        /// <summary>The equipment menu already handled for each split-screen player, so the item is moved only when a menu opens.</summary>
        private readonly PerScreen<IClickableMenu?> handledMenu = new();

        public EquipmentMenuCursorFix(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void OnUpdateTicked()
        {
            if (!Context.IsWorldReady)
                return;

            IClickableMenu? childMenu = Game1.activeClickableMenu?.GetChildMenu();
            if (childMenu is not MenuWithInventory menu || childMenu.GetType().FullName != EquipmentMenuTypeName)
            {
                this.handledMenu.Value = null;
                return;
            }

            if (ReferenceEquals(this.handledMenu.Value, childMenu))
                return;
            this.handledMenu.Value = childMenu;

            Item? cursorItem = Game1.player.CursorSlotItem;
            if (cursorItem is null || menu.heldItem is not null)
                return;

            Game1.player.CursorSlotItem = null;
            menu.heldItem = cursorItem;
            this.monitor.Log($"Moved {cursorItem.QualifiedItemId} from the cursor into SpaceCore's equipment menu.", LogLevel.Trace);
        }
    }
}
