using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;

namespace StardewArmors.Framework
{
    /// <summary>Appends each armor item's stats to its description, so they show in item tooltips.</summary>
    /// <remarks>The lines are generated from <c>CustomFields</c>, so stat numbers are never written twice.</remarks>
    internal sealed class ArmorTooltipEditor
    {
        private readonly IMonitor monitor;
        private readonly ITranslationHelper translation;

        /// <summary>Whether to edit descriptions. Turned off when the mod can't run, so tooltips don't promise stats.</summary>
        public bool Enabled { get; set; } = true;

        public ArmorTooltipEditor(IMonitor monitor, ITranslationHelper translation)
        {
            this.monitor = monitor;
            this.translation = translation;
        }

        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            // Late priority, so this runs after content packs have added their armor.
            if (this.Enabled && e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
                e.Edit(this.EditObjects, AssetEditPriority.Late);
        }

        private void EditObjects(IAssetData asset)
        {
            IDictionary<string, ObjectData> objects = asset.AsDictionary<string, ObjectData>().Data;

            foreach ((string itemId, ObjectData data) in objects)
            {
                if (data.ContextTags?.Any(tag => tag.Equals(ArmorStats.ContextTag, StringComparison.OrdinalIgnoreCase)) != true)
                    continue;

                List<string> lines = ArmorStats.Parse("(O)" + itemId, data.CustomFields, this.monitor, this.translation)
                    .GetDescriptionLines(this.translation)
                    .ToList();

                if (lines.Count > 0)
                    data.Description = (data.Description ?? "") + "\n" + string.Join("\n", lines);
            }
        }
    }
}
