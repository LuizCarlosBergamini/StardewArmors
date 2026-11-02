using System.Globalization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;

namespace StardewArmors.Framework
{
    /// <summary>The stats of one armor item, parsed from its <c>CustomFields</c>.</summary>
    internal sealed class ArmorStats
    {
        /// <summary>The context tag which marks an item as armor (part of the JSON contract).</summary>
        public const string ContextTag = "stardewarmors_armor";

        /// <summary>The prefix of every stat key in <c>CustomFields</c> (part of the JSON contract).</summary>
        public const string FieldPrefix = "StardewArmors/";

        /// <summary>Every supported stat, in display order. Adding a stat means adding a row here and in <c>CLAUDE.md</c>.</summary>
        private static readonly StatDefinition[] Definitions =
        {
            new("Defense", "stat.defense", false, (effects, value) => effects.Defense.Value += value),
            new("Immunity", "stat.immunity", false, (effects, value) => effects.Immunity.Value += value),
            new("Attack", "stat.attack", false, (effects, value) => effects.Attack.Value += value),
            new("CritChance", "stat.crit-chance", true, (effects, value) => effects.CriticalChanceMultiplier.Value += value),
            new("CritPower", "stat.crit-power", true, (effects, value) => effects.CriticalPowerMultiplier.Value += value),
            new("Luck", "stat.luck", false, (effects, value) => effects.LuckLevel.Value += value),
            new("Speed", "stat.speed", false, (effects, value) => effects.Speed.Value += value),
            new("Magnetism", "stat.magnetism", false, (effects, value) => effects.MagneticRadius.Value += value),
            new("MaxStamina", "stat.max-stamina", false, (effects, value) => effects.MaxStamina.Value += value)
        };

        /// <summary>The parsed values, keyed by stat definition.</summary>
        private readonly Dictionary<StatDefinition, float> values = new();

        /// <summary>Whether the item has no stats at all.</summary>
        public bool IsEmpty => this.values.Count == 0;

        /// <summary>Get whether an item may go in the Armor slot.</summary>
        /// <param name="item">The item, or <c>null</c> when the player takes an item out of the slot.</param>
        public static bool IsAllowedInSlot(Item? item)
        {
            return item is null || IsArmor(item);
        }

        /// <summary>Get whether an item is tagged as armor.</summary>
        public static bool IsArmor(Item item)
        {
            return item.HasContextTag(ContextTag);
        }

        /// <summary>Read the stats of an item instance from its data.</summary>
        public static ArmorStats Read(Item item, IMonitor monitor, ITranslationHelper translation)
        {
            object? rawData = ItemRegistry.GetData(item.QualifiedItemId)?.RawData;
            IDictionary<string, string>? fields = rawData switch
            {
                ObjectData data => data.CustomFields,
                BigCraftableData data => data.CustomFields,
                WeaponData data => data.CustomFields,
                ToolData data => data.CustomFields,
                ShirtData data => data.CustomFields,
                PantsData data => data.CustomFields,
                TrinketData data => data.CustomFields,
                _ => null
            };

            return Parse(item.QualifiedItemId, fields, monitor, translation);
        }

        /// <summary>Parse stats from a <c>CustomFields</c> dictionary. Problems are logged once and never thrown.</summary>
        /// <param name="itemId">The item ID, used in log messages.</param>
        public static ArmorStats Parse(string itemId, IDictionary<string, string>? fields, IMonitor monitor, ITranslationHelper translation)
        {
            ArmorStats stats = new();

            if (fields is not null)
            {
                foreach ((string key, string rawValue) in fields)
                {
                    if (!key.StartsWith(FieldPrefix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string statName = key.Substring(FieldPrefix.Length);
                    StatDefinition? definition = Definitions.FirstOrDefault(def => def.Key.Equals(statName, StringComparison.OrdinalIgnoreCase));
                    if (definition is null)
                    {
                        monitor.LogOnce(translation.Get("log.unknown-stat", new { item = itemId, field = key }), LogLevel.Warn);
                        continue;
                    }

                    if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        monitor.LogOnce(translation.Get("log.invalid-stat", new { item = itemId, field = key, value = rawValue }), LogLevel.Warn);
                        continue;
                    }

                    if (value != 0)
                        stats.values[definition] = stats.values.GetValueOrDefault(definition) + value;
                }
            }

            if (stats.IsEmpty)
                monitor.LogOnce(translation.Get("log.no-stats", new { item = itemId, prefix = FieldPrefix }), LogLevel.Warn);

            return stats;
        }

        /// <summary>Build the buff effects for these stats.</summary>
        public BuffEffects ToBuffEffects()
        {
            BuffEffects effects = new();
            foreach ((StatDefinition definition, float value) in this.values)
                definition.Apply(effects, value);
            return effects;
        }

        /// <summary>Get one translated line per stat, like "+3 Defense", in display order.</summary>
        public IEnumerable<string> GetDescriptionLines(ITranslationHelper translation)
        {
            foreach (StatDefinition definition in Definitions)
            {
                if (!this.values.TryGetValue(definition, out float value))
                    continue;

                float shown = definition.IsPercent ? value * 100 : value;
                string number = shown.ToString("0.##", CultureInfo.CurrentCulture);
                if (shown > 0)
                    number = "+" + number;

                yield return translation.Get(definition.TranslationKey, new { value = number });
            }
        }

        /// <summary>How one stat is named, shown and applied.</summary>
        /// <param name="Key">The key after <see cref="FieldPrefix"/> in <c>CustomFields</c>.</param>
        /// <param name="TranslationKey">The i18n key of the tooltip line.</param>
        /// <param name="IsPercent">Whether the value is a fraction shown as a percentage.</param>
        /// <param name="Apply">Add the value to buff effects.</param>
        private sealed record StatDefinition(string Key, string TranslationKey, bool IsPercent, Action<BuffEffects, float> Apply);
    }
}
