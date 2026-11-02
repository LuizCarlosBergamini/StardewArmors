using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace StardewArmors.Integrations
{
    /// <summary>The subset of SpaceCore's API used by this mod. Signatures must match SpaceCore's <c>IApi</c> exactly.</summary>
    public interface ISpaceCoreApi
    {
        /// <summary>Register a custom equipment slot. The ID is global, so it must include the mod's unique ID.</summary>
        /// <remarks>SpaceCore calls <paramref name="slotValidator"/> with <c>null</c> when the player takes an item out of the slot.</remarks>
        void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null);

        /// <summary>Get the item in a custom equipment slot, or <c>null</c> if it's empty.</summary>
        Item GetItemInEquipmentSlot(Farmer farmer, string globalId);

        /// <summary>Set the item in a custom equipment slot. Only works for the local or offline farmer.</summary>
        void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item);

        /// <summary>Get whether an item passes a slot's validator.</summary>
        bool CanItemGoInEquipmentSlot(string globalId, Item item);
    }
}
