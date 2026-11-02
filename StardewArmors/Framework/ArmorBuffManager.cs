using StardewArmors.Integrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace StardewArmors.Framework
{
    /// <summary>Keeps one endless buff in sync with the item in the local player's Armor slot.</summary>
    internal sealed class ArmorBuffManager
    {
        /// <summary>How often to check the slot, in ticks. SpaceCore has no change event, so the slot is polled.</summary>
        private const uint CheckInterval = 15;

        private readonly ISpaceCoreApi spaceCore;
        private readonly string slotId;
        private readonly string buffId;
        private readonly IMonitor monitor;
        private readonly ITranslationHelper translation;

        /// <summary>The qualified ID of the item last applied for each split-screen player, or <c>null</c> for an empty slot.</summary>
        private readonly PerScreen<string?> lastItemId = new();

        /// <summary>Whether the buff must be rebuilt on the next check, for each split-screen player.</summary>
        private readonly PerScreen<bool> isDirty = new(() => true);

        public ArmorBuffManager(ISpaceCoreApi spaceCore, string slotId, string buffId, IMonitor monitor, ITranslationHelper translation)
        {
            this.spaceCore = spaceCore;
            this.slotId = slotId;
            this.buffId = buffId;
            this.monitor = monitor;
            this.translation = translation;
        }

        public void OnSaveLoaded()
        {
            this.Refresh(force: true);
        }

        public void OnDayStarted()
        {
            this.Refresh(force: true);
        }

        public void OnUpdateTicked(UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && e.IsMultipleOf(CheckInterval))
                this.Refresh(force: false);
        }

        public void OnReturnedToTitle()
        {
            this.lastItemId.ResetAllScreens();
            this.isDirty.ResetAllScreens();
        }

        /// <summary>Rebuild the buff if the slot contents changed, or always if <paramref name="force"/> is set.</summary>
        private void Refresh(bool force)
        {
            if (!Context.IsWorldReady)
                return;

            Item? item = this.spaceCore.GetItemInEquipmentSlot(Game1.player, this.slotId);
            string? itemId = item?.QualifiedItemId;

            if (!force && !this.isDirty.Value && itemId == this.lastItemId.Value)
                return;

            this.lastItemId.Value = itemId;
            this.isDirty.Value = false;
            this.Apply(item);
        }

        /// <summary>Replace the armor buff with one for the given item, or remove it if there's no armor.</summary>
        private void Apply(Item? item)
        {
            Game1.player.buffs.Remove(this.buffId);

            if (item is null || !ArmorStats.IsArmor(item))
            {
                this.monitor.Log("Armor slot is empty; armor buff removed.", LogLevel.Trace);
                return;
            }

            ArmorStats stats = ArmorStats.Read(item, this.monitor, this.translation);
            if (stats.IsEmpty)
                return;

            ParsedItemData data = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
            Buff buff = new(
                id: this.buffId,
                source: item.DisplayName,
                displaySource: item.DisplayName,
                duration: Buff.ENDLESS,
                iconTexture: data.GetTexture(),
                iconSheetIndex: data.SpriteIndex,
                effects: stats.ToBuffEffects(),
                isDebuff: false,
                displayName: item.DisplayName,
                description: null // the game lists the buff's stats when there's no description
            );

            Game1.player.applyBuff(buff);
            this.monitor.Log($"Applied armor buff for {item.QualifiedItemId}.", LogLevel.Trace);
        }
    }
}
