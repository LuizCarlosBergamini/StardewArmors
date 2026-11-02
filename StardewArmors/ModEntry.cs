using Microsoft.Xna.Framework.Graphics;
using StardewArmors.Framework;
using StardewArmors.Integrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewArmors
{
    /// <summary>The mod entry point. Only wires events and delegates; logic lives in <see cref="Framework"/>.</summary>
    internal sealed class ModEntry : Mod
    {
        private ArmorTooltipEditor tooltipEditor = null!;
        private ArmorBuffManager? buffManager;

        public override void Entry(IModHelper helper)
        {
            this.tooltipEditor = new ArmorTooltipEditor(this.Monitor, helper.Translation);

            helper.Events.Content.AssetRequested += (_, e) => this.tooltipEditor.OnAssetRequested(e);
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            ISpaceCoreApi? spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            if (spaceCore is null)
            {
                this.Monitor.Log(this.Helper.Translation.Get("log.spacecore-missing"), LogLevel.Error);
                this.tooltipEditor.Enabled = false;
                this.Helper.GameContent.InvalidateCache("Data/Objects");
                return;
            }

            // SpaceCore slot IDs are global, so the ID includes this mod's unique ID.
            string slotId = $"{this.ModManifest.UniqueID}_Armor";
            Texture2D slotBackground = this.Helper.ModContent.Load<Texture2D>("assets/slot.png");
            spaceCore.RegisterEquipmentSlot(
                this.ModManifest,
                slotId,
                ArmorStats.IsAllowedInSlot,
                () => this.Helper.Translation.Get("slot.name").ToString(),
                slotBackground
            );

            this.buffManager = new ArmorBuffManager(spaceCore, slotId, $"{this.ModManifest.UniqueID}_ArmorSet", this.Monitor, this.Helper.Translation);

            IModEvents events = this.Helper.Events;
            events.GameLoop.SaveLoaded += (_, _) => this.buffManager.OnSaveLoaded();
            events.GameLoop.DayStarted += (_, _) => this.buffManager.OnDayStarted();
            events.GameLoop.UpdateTicked += (_, args) => this.buffManager.OnUpdateTicked(args);
            events.GameLoop.ReturnedToTitle += (_, _) => this.buffManager.OnReturnedToTitle();
        }
    }
}
