using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text.Json.Serialization;

namespace MHAchievManager.Models
{
    #region Enum
    public enum AchievementVisibleState
    {
        Invalid,
        Visible,
        Invisible,
        ParentComplete,
        Complete,
        InProgress,
        Objective
    }

    public enum AchievementEvaluationType
    {
        Invalid,
        Available,
        Children,
        Disabled,
        Parent
    }

    public enum AchievementUIProgressDisplayOption
    {
        Invalid = -1,
        Threshold,
        Hidden,
        Checkbox,
        ProgressBar,
        Max
    }

    public enum ScoringEventType
    {
        Invalid = -1,
        AreaEnter,
        AvatarLevel,
        AvatarPrestigeLevel,
        AvatarsUnlocked,
        AvatarUsedPower,
        CompleteMission,
        CurrencySpent,
        CurrencyCollected,
        DifficultyUnlocked, // Be The Hero // Removed in 1.52
        EntityDeath,
        EntityInteract,
        HotspotEnter,
        ItemBought,
        ItemCollected,
        ItemCrafted,
        ItemDonated,
        RegionEnter,
        VendorLevel,
        WaypointUnlocked,
        ChildrenComplete,
        MetaGameModeComplete,
        MetaGameStateComplete,
        MetaGameWaveComplete,
        ItemSpent,
        IsComplete, // Cow Tags
        EntityDeathViaPower,
        PvPMatchWon,
        PvPMatchLost,
        AvatarsAtPrestigeLevel,
        AvatarsAtPrestigeLevelCap,
        AvatarsAtLevelCap,
        AchievementScore,
        FullyUpgradedLegendaries,
        FullyUpgradedPetTech,
        HoursPlayed,
        HoursPlayedByAvatar,
        MinGearLevel,
        OrbsCollected,
        PowerRank, // Removed in 1.52
        PowerRankUltimate, // Removed in 1.52
        Dependent, // Legendary
        MetaGameStateCompleteDifficulty,
        MetaGameStateCompleteAffix,
        AvatarDeath,
        AvatarKill,
        AvatarKillAssist,
        CompletionTime,
        AvatarLevelTotal,
        AvatarLevelTotalAllAvatars,
        Max
    }

    public enum EventContextType
    {
        Avatar,
        Item,
        Party,
        Pet,
        Region,
        DifficultyTierMin,
        DifficultyTierMax,
        TeamUp,
        PublicEventTeam
    }

    #endregion

    [JsonConverter(typeof(AchievementInfoJsonConverter))]
    public class AchievementInfo
    {
        [Category("1. General")]
        [DisplayName("Achievement ID")]
        [Description("Unique uint identifier of the achievement record.")]
        [ReadOnly(true)]
        [JsonRequired]
        public uint Id { get; set; }

        [Category("1. General")]
        [DisplayName("Enabled")]
        [Description("Controls whether this achievement is active in the game logic.")]
        public bool Enabled { get; set; }

        [Category("1. General")]
        [DisplayName("Parent ID")]
        [Description("ID of the parent achievement component. 0 if this is a root achievement.")]
        [TypeConverter(typeof(AchievementIdConverter))]
        [Editor(typeof(AchievementIdEditor), typeof(UITypeEditor))]
        public int ParentId { get; set; }

        [Category("2. Localization")]
        [DisplayName("Name")]
        [Description("Translated achievement name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId Name { get; set; }

        [Category("2. Localization")]
        [DisplayName("In-Progress Text")]
        [Description("Translated in-progress objective description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId InProgressStr { get; set; }

        [Category("2. Localization")]
        [DisplayName("Completed Text")]
        [Description("Translated completion description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId CompletedStr { get; set; }

        [Category("2. Localization")]
        [DisplayName("Reward Text")]
        [Description("Translated reward description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId RewardStr { get; set; }

        [Category("4. Visual && UI")]
        [DisplayName("Icon Asset ID")]
        [Description("Texture asset ID reference used for the main achievement icon.")]
        [TypeConverter(typeof(AssetIdConverter))]
        [Editor(typeof(AssetIdEditor), typeof(UITypeEditor))]
        public AssetId IconPathAssetId { get; set; }

        [Category("1. General")]
        [DisplayName("Score")]
        [Description("Achievement points awarded to the player upon completion.")]
        public int Score { get; set; }

        [Category("2. Localization")]
        [DisplayName("Category")]
        [Description("Translated category name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId CategoryStr { get; set; }

        [Category("2. Localization")]
        [DisplayName("SubCategory")]
        [Description("Translated subcategory name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId SubCategoryStr { get; set; }

        [Category("4. Visual && UI")]
        [DisplayName("Display Order")]
        [Description("Defines the sorting order of this component within lists.")]
        public int DisplayOrder { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Visible State")]
        [Description("Determines visibility rules in the client (Visible, Hidden, Invisible, etc.).")]
        public AchievementVisibleState VisibleState { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Evaluation Type")]
        [Description("Specifies how progress is evaluated and triggered.")]
        public AchievementEvaluationType EvaluationType { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Event Type")]
        [Description("The scoring event type tracked by this objective.")]
        public ScoringEventType EventType { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Threshold")]
        [Description("Target counter requirement needed to complete this objective.")]
        public int Threshold { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Dependent Achievement ID")]
        [Description("ID of an external achievement requirement that must be completed first.")]
        [TypeConverter(typeof(AchievementIdConverter))]
        [Editor(typeof(AchievementIdEditor), typeof(UITypeEditor))]
        public int DependentAchievementId { get; set; }

        [Category("4. Visual && UI")]
        [DisplayName("UI Display Option")]
        [Description("Controls UI component representation (CheckBox, ProgressBar, Counter, Invisible, etc.).")]
        public AchievementUIProgressDisplayOption UIProgressDisplayOption { get; set; }

        [Category("5. System && Console")]
        [DisplayName("Published Date (US)")]
        [Description("Internal creation/publication date.")]
        [TypeConverter(typeof(UnixTimeConverter))]
        [Editor(typeof(UnixTimeEditor), typeof(UITypeEditor))]
        public TimeSpan PublishedDateUS { get; set; }

        [Category("4. Visual && UI")]
        [DisplayName("Hi-Res Icon Asset ID")]
        [Description("High-resolution texture asset ID reference (optional/unused in client).")]
        [TypeConverter(typeof(AssetIdConverter))]
        [Editor(typeof(AssetIdEditor), typeof(UITypeEditor))]
        public AssetId IconPathHiResAssetId { get; set; } = AssetId.Invalid;

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy")]
        [Description("Indicates whether this record is linked to a PlayStation 4 trophy.")]
        public bool OrbisTrophy { get; set; } = false;

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy ID")]
        [Description("PlayStation 4 internal trophy index (-1 if none).")]
        public int OrbisTrophyId { get; set; } = -1;

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy Shared")]
        [Description("Shared status for PlayStation 4 trophy evaluation.")]
        public bool OrbisTrophyShared { get; set; } = false;

        [Category("4. Visual && UI")]
        [DisplayName("Party Visible")]
        [Description("Broadcasts progress or completion notification to party members.")]
        [JsonRequired]
        public bool PartyVisible { get; set; }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Context")]
        [Description("Additional context references, triggers, and reward prototypes.")]
        public AchievementContext Context { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class AchievementContext
    {
        [DisplayName("Reward Prototype")]
        [TypeConverter(typeof(GuidPrototypeConverter))]
        [Editor(typeof(GuidPrototypeEditor), typeof(UITypeEditor))]
        public long RewardPrototype { get; set; }

        [Browsable(false)]
        public List<EventData> EventData { get; set; }
        [Browsable(false)]
        public List<EventContext> EventContext { get; set; }

        [JsonIgnore]
        [DisplayName("Event Data")]
        [TypeConverter(typeof(EventDataArrayConverter))]
        [Editor(typeof(EventDataCollectionEditor), typeof(UITypeEditor))]
        public EventData[] EventDataArray
        {
            get => EventData?.ToArray();
            set => EventData = value != null ? [.. value.Take(3)] : [];
        }

        [JsonIgnore]
        [DisplayName("Event Context")]
        [TypeConverter(typeof(EventContextArrayConverter))]
        [Editor(typeof(EventContextCollectionEditor), typeof(UITypeEditor))]
        public EventContext[] EventContextArray
        {
            get => EventContext?.ToArray();
            set => EventContext = value != null ? [.. value] : [];
        }

        [JsonIgnore]
        [Browsable(false)]
        public bool IsEmpty => RewardPrototype == 0
                    && (EventData == null || EventData.Count == 0)
                    && (EventContext == null || EventContext.Count == 0);
        public override string ToString() => $"Context";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EventData
    {
        [DisplayName("Event Prototype")]
        [Description("Prototype reference required to trigger and match this specific Event Type.")]
        [TypeConverter(typeof(GuidPrototypeConverter))]
        [Editor(typeof(GuidPrototypeEditor), typeof(UITypeEditor))]
        public long Prototype { get; set; }

        [DisplayName("Include Children")]
        [Description("If enabled, matches all child prototypes that inherit from the Event Prototype.")]
        public bool IncludeChildren { get; set; }

        public override string ToString() => $"EventData";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EventContext
    {
        [DisplayName("Context Type")]
        [Description("Specifies the target category of this event condition.")]
        public EventContextType ContextType { get; set; }

        [DisplayName("Target Prototype")]
        [Description("Prototype reference that must match to trigger or validate this event condition.")]
        [TypeConverter(typeof(GuidPrototypeConverter))]
        [Editor(typeof(GuidPrototypeEditor), typeof(UITypeEditor))]
        public long Prototype { get; set; }

        [DisplayName("Child Regions")]
        [Description("Evaluates true if the target region matches or inherits from the Target Region prototype.")]
        public bool IncludeChildren { get; set; }

        public override string ToString() => $"EventContext";
    }
}
