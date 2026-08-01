using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
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

    public class AchievementInfoViewModel
    {
        private readonly AchievementInfo _info;

        public AchievementInfoViewModel(AchievementInfo info)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info));
        }

        [Browsable(false)]
        public AchievementInfo Target => _info;

        #region 1. General

        [Category("1. General")]
        [DisplayName("Achievement ID")]
        [Description("Unique uint identifier of the achievement record.")]
        [ReadOnly(true)]
        public uint Id => _info.Id;

        [Category("1. General")]
        [DisplayName("Enabled")]
        [Description("Controls whether this achievement is active in the game logic.")]
        public bool Enabled
        {
            get => _info.Enabled;
            set => _info.Enabled = value;
        }

        [Category("1. General")]
        [DisplayName("Parent ID")]
        [Description("ID of the parent achievement component. 0 if this is a root achievement.")]
        public int ParentId
        {
            get => _info.ParentId;
            set => _info.ParentId = value;
        }

        [Category("1. General")]
        [DisplayName("Score")]
        [Description("Achievement points awarded to the player upon completion.")]
        public int Score
        {
            get => _info.Score;
            set => _info.Score = value;
        }

        #endregion

        #region 2. Localization

        [Category("2. Localization")]
        [DisplayName("Name")]
        [Description("Translated achievement name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId Name
        {
            get => _info.Name;
            set => _info.Name = value;
        }

        [Category("2. Localization")]
        [DisplayName("Category")]
        [Description("Translated category name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId CategoryStr
        {
            get => _info.CategoryStr;
            set => _info.CategoryStr = value;
        }

        [Category("2. Localization")]
        [DisplayName("SubCategory")]
        [Description("Translated subcategory name.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId SubCategoryStr
        {
            get => _info.SubCategoryStr;
            set => _info.SubCategoryStr = value;
        }

        [Category("2. Localization")]
        [DisplayName("In-Progress Text")]
        [Description("Translated in-progress objective description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId InProgressStr
        {
            get => _info.InProgressStr;
            set => _info.InProgressStr = value;
        }

        [Category("2. Localization")]
        [DisplayName("Completed Text")]
        [Description("Translated completion description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId CompletedStr
        {
            get => _info.CompletedStr;
            set => _info.CompletedStr = value;
        }

        [Category("2. Localization")]
        [DisplayName("Reward Text")]
        [Description("Translated reward description.")]
        [TypeConverter(typeof(LocaleStringConverter))]
        [Editor(typeof(LocaleStringEditor), typeof(UITypeEditor))]
        public LocaleStringId RewardStr
        {
            get => _info.RewardStr;
            set => _info.RewardStr = value;
        }
        #endregion

        #region 3. Evaluation & Logic

        [Category("3. Evaluation && Logic")]
        [DisplayName("Visible State")]
        [Description("Determines visibility rules in the client (Visible, Hidden, Invisible, etc.).")]
        public AchievementVisibleState VisibleState
        {
            get => _info.VisibleState;
            set => _info.VisibleState = value;
        }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Evaluation Type")]
        [Description("Specifies how progress is evaluated and triggered.")]
        public AchievementEvaluationType EvaluationType
        {
            get => _info.EvaluationType;
            set => _info.EvaluationType = value;
        }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Event Type")]
        [Description("The scoring event type tracked by this objective.")]
        public ScoringEventType EventType
        {
            get => _info.EventType;
            set => _info.EventType = value;
        }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Threshold")]
        [Description("Target counter requirement needed to complete this objective.")]
        public int Threshold
        {
            get => _info.Threshold;
            set => _info.Threshold = value;
        }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Dependent Achievement ID")]
        [Description("ID of an external achievement requirement that must be completed first.")]
        public int DependentAchievementId
        {
            get => _info.DependentAchievementId;
            set => _info.DependentAchievementId = value;
        }

        [Category("3. Evaluation && Logic")]
        [DisplayName("Context")]
        [Description("Additional context references, triggers, and reward prototypes.")]
        public AchievementContext Context
        {
            get => _info.Context;
            set => _info.Context = value;
        }

        #endregion

        #region 4. Visual & UI

        [Category("4. Visual && UI")]
        [DisplayName("Display Order")]
        [Description("Defines the sorting order of this component within lists.")]
        public int DisplayOrder
        {
            get => _info.DisplayOrder;
            set => _info.DisplayOrder = value;
        }

        [Category("4. Visual && UI")]
        [DisplayName("UI Display Option")]
        [Description("Controls UI component representation (CheckBox, ProgressBar, Counter, Invisible, etc.).")]
        public AchievementUIProgressDisplayOption UIProgressDisplayOption
        {
            get => _info.UIProgressDisplayOption;
            set => _info.UIProgressDisplayOption = value;
        }

        [Category("4. Visual && UI")]
        [DisplayName("Icon Asset ID")]
        [Description("Texture asset ID reference used for the main achievement icon.")]
        [TypeConverter(typeof(AssetIdConverter))]
        public AssetId IconPathAssetId
        {
            get => _info.IconPathAssetId;
            set => _info.IconPathAssetId = value;
        }

        [Category("4. Visual && UI")]
        [DisplayName("Hi-Res Icon Asset ID")]
        [Description("High-resolution texture asset ID reference (optional/unused in client).")]
        [TypeConverter(typeof(AssetIdConverter))]
        public AssetId IconPathHiResAssetId
        {
            get => _info.IconPathHiResAssetId;
            set => _info.IconPathHiResAssetId = value;
        }

        [Category("4. Visual && UI")]
        [DisplayName("Party Visible")]
        [Description("Broadcasts progress or completion notification to party members.")]
        public bool PartyVisible
        {
            get => _info.PartyVisible;
            set => _info.PartyVisible = value;
        }

        #endregion

        #region 5. System && Console

        [Category("5. System && Console")]
        [DisplayName("Published Date (US)")]
        [Description("Internal creation/publication date.")]
        [TypeConverter(typeof(UnixTimeConverter))]
        [Editor(typeof(UnixTimeEditor), typeof(UITypeEditor))]
        public TimeSpan PublishedDateUS
        {
            get => _info.PublishedDateUS;
            set => _info.PublishedDateUS = value;
        }

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy")]
        [Description("Indicates whether this record is linked to a PlayStation 4 trophy.")]
        public bool OrbisTrophy
        {
            get => _info.OrbisTrophy;
            set => _info.OrbisTrophy = value;
        }

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy ID")]
        [Description("PlayStation 4 internal trophy index (-1 if none).")]
        public int OrbisTrophyId
        {
            get => _info.OrbisTrophyId;
            set => _info.OrbisTrophyId = value;
        }

        [Category("5. System && Console")]
        [DisplayName("Orbis Trophy Shared")]
        [Description("Shared status for PlayStation 4 trophy evaluation.")]
        public bool OrbisTrophyShared
        {
            get => _info.OrbisTrophyShared;
            set => _info.OrbisTrophyShared = value;
        }

        #endregion
    }

    public class AchievementInfo
    {
        [JsonRequired]
        public uint Id { get; set; }
        public bool Enabled { get; set; }
        public int ParentId { get; set; }
        public LocaleStringId Name { get; set; }
        public LocaleStringId InProgressStr { get; set; }
        public LocaleStringId CompletedStr { get; set; }
        public LocaleStringId RewardStr { get; set; }
        public AssetId IconPathAssetId { get; set; }
        public int Score { get; set; }
        public LocaleStringId CategoryStr { get; set; }
        public LocaleStringId SubCategoryStr { get; set; }
        public int DisplayOrder { get; set; }
        public AchievementVisibleState VisibleState { get; set; }
        public AchievementEvaluationType EvaluationType { get; set; }
        public ScoringEventType EventType { get; set; }
        public int Threshold { get; set; }
        public int DependentAchievementId { get; set; }
        public AchievementUIProgressDisplayOption UIProgressDisplayOption { get; set; }
        public TimeSpan PublishedDateUS { get; set; }
        public AssetId IconPathHiResAssetId { get; set; } = AssetId.Invalid;
        public bool OrbisTrophy { get; set; } = false;
        public int OrbisTrophyId { get; set; } = -1;
        public bool OrbisTrophyShared { get; set; } = false;

        [JsonRequired]
        public bool PartyVisible { get; set; }
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

        [DisplayName("Event Data")]
        [TypeConverter(typeof(CleanArrayConverter))]
        public EventData[] EventDataArray => EventData?.ToArray();

        [DisplayName("Event Context")]
        [TypeConverter(typeof(CleanArrayConverter))]
        public EventContext[] EventContextArray => EventContext?.ToArray();
        public override string ToString() => $"Context";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EventData
    {
        [TypeConverter(typeof(GuidPrototypeConverter))]
        public long Prototype { get; set; }
        public bool IncludeChildren { get; set; }

        public override string ToString() => $"EventData";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EventContext
    {
        public EventContextType ContextType { get; set; }
        [TypeConverter(typeof(GuidPrototypeConverter))]
        public long Prototype { get; set; }
        public bool IncludeChildren { get; set; }

        public override string ToString() => $"EventContext";
    }

    public class AchievementStrings
    {
        public Dictionary<string, Dictionary<string, string>> Strings { get; set; } = [];
    }

    public class AchievementTreeNode
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int ParentId { get; set; }
        public AchievementInfo Info { get; set; }
    }
}
