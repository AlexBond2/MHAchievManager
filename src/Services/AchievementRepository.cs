using MHAchievManager.Forms;
using MHAchievManager.Locale;
using MHAchievManager.Models;
using OpenCalligraphy.Core.GameData;
using OpenCalligraphy.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MHAchievManager.Services
{

    public class StringMap : Dictionary<LocaleStringId, Dictionary<string, string>> { }

    public class CategoryNode
    {
        public LocaleStringId Id { get; set; } = LocaleStringId.Blank;
        public string Name { get; set; } = string.Empty;
        public List<CategoryNode> SubCategories { get; set; } = [];
        public int VisibleAchievementsCount { get; set; }
        public bool HasVisibleAchievements => VisibleAchievementsCount > 0;
        public override string ToString() => Name;
    }

    public class SavePatchReport
    {
        public string TargetInfoFilePath { get; set; } = string.Empty;
        public string TargetStringFilePath { get; set; } = string.Empty;
        public bool IsNewInfoFile { get; set; }
        public bool IsNewStringFile { get; set; }

        // Delta collections ready for save
        public Dictionary<uint, AchievementInfo> InfoDelta { get; } = [];
        public StringMap StringDelta { get; } = [];

        // Stats for UI
        public int InfoAddedOrModified { get; set; }
        public int StringsAddedOrModified { get; set; }
        public int InfoAdded { get; set; }
        public int InfoRemoved { get; set; }
        public int StringsAdded { get; set; }
        public int StringsRemoved { get; set; }

        // Warnings
        public List<string> InfoWarnings { get; } = [];
        public List<string> StringWarnings { get; } = [];

        public bool HasWarnings => InfoWarnings.Count > 0 || StringWarnings.Count > 0;
        public bool HasChanges => InfoDelta.Count > 0 || StringDelta.Count > 0;
    }

    public class AchievementRepository
    {
        private readonly Dictionary<uint, AchievementInfo> _infoMap = [];
        private readonly StringMap _stringMap = [];

        private readonly Dictionary<LocaleStringId, HashSet<LocaleStringId>> _categoryToSubCats = [];
        private readonly Dictionary<LocaleStringId, List<AchievementInfo>> _subCatToRootAchievements = [];
        private readonly Dictionary<int, List<AchievementInfo>> _parentToChildren = [];
        private readonly Dictionary<LocaleStringId, int> _localeUsageCount = [];

        public string CurrentLanguage { get; set; } = GameLocale.DefaultLocale; 
        public IReadOnlySet<string> AvailableLocales => _availableLocales;
        private readonly HashSet<string> _availableLocales = new(StringComparer.Ordinal);

        public static AchievementRepository Instance { get; private set; }

        public static void Initialize()
        {
            Instance ??= new();
        }

        private AchievementRepository() { }

        public IReadOnlyCollection<AchievementInfo> AllAchievements => _infoMap.Values;

        public bool IsInfoDirty { get; set; }
        public bool IsStringDirty { get; set; }

        public string GetLocale(LocaleStringId key, string defaultText = "")
        {
            if (key == LocaleStringId.Invalid) return defaultText;

            if (_stringMap.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(CurrentLanguage, out var text))
                    return text;

                // Fallback to en_us if current language is missing
                if (translations.TryGetValue(GameLocale.DefaultLocale, out var enText))
                    return enText;
            }

            return defaultText.Length > 0 ? defaultText : key.ToString();
        }

        public AchievementInfo GetAchievement(int id)
        {
            return _infoMap.TryGetValue((uint)id, out var achievement) ? achievement : null;
        }

        private void IncrementUsage(LocaleStringId id)
        {
            if (id == 0) return;

            if (_localeUsageCount.TryGetValue(id, out int count))
                _localeUsageCount[id] = count + 1;
            else
                _localeUsageCount[id] = 1;
        }

        public int GetLocaleUsageCount(LocaleStringId localeId)
        {
            return _localeUsageCount.TryGetValue(localeId, out int count) ? count : 0;
        }

        public void RebuildIndexes()
        {
            _categoryToSubCats.Clear();
            _subCatToRootAchievements.Clear();
            _parentToChildren.Clear();
            _localeUsageCount.Clear();

            foreach (var ach in AllAchievements)
            {
                LocaleStringId catId = ach.CategoryStr;
                LocaleStringId subCatId = ach.SubCategoryStr;

                IncrementUsage(ach.Name);
                IncrementUsage(ach.InProgressStr);
                IncrementUsage(ach.CompletedStr);
                IncrementUsage(ach.RewardStr);
                IncrementUsage(catId);
                IncrementUsage(subCatId);

                if (!_categoryToSubCats.TryGetValue(catId, out var subSet))
                {
                    subSet = [];
                    _categoryToSubCats[catId] = subSet;
                }
                subSet.Add(subCatId);

                if (ach.ParentId != 0)
                {
                    if (!_parentToChildren.TryGetValue(ach.ParentId, out var children))
                    {
                        children = [];
                        _parentToChildren[ach.ParentId] = children;
                    }
                    children.Add(ach);
                }
                else
                {
                    if (!_subCatToRootAchievements.TryGetValue(subCatId, out var roots))
                    {
                        roots = [];
                        _subCatToRootAchievements[subCatId] = roots;
                    }
                    roots.Add(ach);
                }
            }
        }

        /// <summary>
        /// Reloads all layers. Applies files in the order they appear in the list.
        /// </summary>
        public void ReloadLayers(IEnumerable<string> activeInfoFiles, IEnumerable<string> activeStringFiles)
        {
            _infoMap.Clear();
            IsInfoDirty = false;

            JsonSerializerOptions options = new();

            foreach (string filePath in activeInfoFiles)
            {
                try
                {
                    using FileStream fs = File.OpenRead(filePath);
                    AchievementInfo[] infos = JsonSerializer.Deserialize<AchievementInfo[]>(fs, options);
                    if (infos == null) continue;

                    Debug.WriteLine($"Parsed achievement data from {Path.GetFileName(filePath)}");

                    foreach (AchievementInfo info in infos)
                        _infoMap[info.Id] = info;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"ReloadLayers(): Achievement info map deserialization failed - {e.Message}");
                }
            }

            RebuildIndexes();

            _stringMap.Clear();
            IsStringDirty = false;
            _availableLocales.Clear();

            foreach (var filePath in activeStringFiles)
            {
                using FileStream fs = File.OpenRead(filePath);
                StringMap stringMap = JsonSerializer.Deserialize<StringMap>(fs);

                if (stringMap is null)
                {
                    Debug.WriteLine("ReloadLayers(): stringMap == null");
                    continue;
                }

                foreach (var (stringId, localeDict) in stringMap)
                {
                    if (localeDict is null) continue;

                    // Restore actual data assignment
                    _stringMap[stringId] = localeDict;

                    // Collect unique locale keys
                    foreach (var localeCode in localeDict.Keys)
                    {
                        _availableLocales.Add(localeCode);
                    }
                }

                Debug.WriteLine($"Loaded {stringMap.Count} achievement strings from {Path.GetFileName(filePath)}");
            }
        }

        /// <summary>
        /// Generates a precise patch report without layer contamination from higher patches.
        /// </summary>
        public SavePatchReport GenerateSaveReport(
            string targetInfoPath,
            string targetStringPath,
            IEnumerable<string> activeInfoFiles,
            IEnumerable<string> activeStringFiles)
        {
            var infoList = activeInfoFiles.ToList();
            var stringList = activeStringFiles.ToList();

            // Partition Info layers
            int infoTargetIndex = infoList.FindIndex(f => string.Equals(f, targetInfoPath, StringComparison.OrdinalIgnoreCase));

            IEnumerable<string> baseInfoFiles = infoTargetIndex >= 0
                ? infoList.Take(infoTargetIndex)
                : infoList;

            IEnumerable<string> upperInfoFiles = infoTargetIndex >= 0
                ? infoList.Skip(infoTargetIndex + 1)
                : [];

            // Partition String layers
            int stringTargetIndex = stringList.FindIndex(f => string.Equals(f, targetStringPath, StringComparison.OrdinalIgnoreCase));

            IEnumerable<string> baseStringFiles = stringTargetIndex >= 0
                ? stringList.Take(stringTargetIndex)
                : stringList;

            IEnumerable<string> upperStringFiles = stringTargetIndex >= 0
                ? stringList.Skip(stringTargetIndex + 1)
                : [];

            var report = new SavePatchReport
            {
                TargetInfoFilePath = targetInfoPath,
                TargetStringFilePath = targetStringPath,
                IsNewInfoFile = !File.Exists(targetInfoPath),
                IsNewStringFile = !File.Exists(targetStringPath)
            };

            JsonSerializerOptions options = new();

            // 1. Build maps
            var baseInfoMap = BuildTempInfoMap(baseInfoFiles, options);
            var baseStringMap = BuildTempStringMap(baseStringFiles);

            var fullOriginalInfoMap = BuildTempInfoMap(infoList, options);
            var fullOriginalStringMap = BuildTempStringMap(stringList);

            var upperInfoMap = BuildTempInfoMap(upperInfoFiles, options);
            var upperStringMap = BuildTempStringMap(upperStringFiles);

            // Load original contents of the target files being saved
            var targetOriginalInfo = File.Exists(targetInfoPath)
                ? LoadSingleInfoFile(targetInfoPath, options)
                : [];

            var targetOriginalString = File.Exists(targetStringPath)
                ? LoadSingleStringFile(targetStringPath)
                : [];

            // 2. Isolate User Edits (Current UI vs Full Original Merge)
            var userInfoEdits = new Dictionary<uint, AchievementInfo>();
            foreach (var (id, current) in _infoMap)
            {
                bool exists = fullOriginalInfoMap.TryGetValue(id, out var original);
                if (!exists || JsonSerializer.Serialize(current, options) != JsonSerializer.Serialize(original, options))
                {
                    userInfoEdits[id] = current;
                }
            }

            StringMap userStringEdits = [];
            foreach (var (key, current) in _stringMap)
            {
                bool exists = fullOriginalStringMap.TryGetValue(key, out var original);
                if (!exists || JsonSerializer.Serialize(current) != JsonSerializer.Serialize(original))
                {
                    userStringEdits[key] = current;
                }
            }

            // 3. Construct Target State for Info (Base + TargetOriginal + UserEdits)
            var isolatedTargetInfoState = new Dictionary<uint, AchievementInfo>(baseInfoMap);
            foreach (var (id, item) in targetOriginalInfo) isolatedTargetInfoState[id] = item;
            foreach (var (id, item) in userInfoEdits) isolatedTargetInfoState[id] = item;

            // 4. Calculate clean Info Delta (TargetState vs BaseMap)
            var shadowedAchievements = new List<(uint Id, string Name)>();
            foreach (var (id, targetItem) in isolatedTargetInfoState)
            {
                bool existsInBase = baseInfoMap.TryGetValue(id, out var baseItem);
                bool isChanged = !existsInBase ||
                                 JsonSerializer.Serialize(targetItem, options) != JsonSerializer.Serialize(baseItem, options);

                if (isChanged)
                {
                    report.InfoDelta[id] = targetItem;
                    report.InfoAddedOrModified++;

                    if (upperInfoMap.ContainsKey(id))
                    {
                        shadowedAchievements.Add((id, GetLocale(targetItem.Name)));
                    }
                }
            }

            // --- GitHub Diff ---
            int itemAdded = 0;
            int itemRemoved = 0;

            foreach (var (id, newItem) in report.InfoDelta)
            {
                if (!targetOriginalInfo.TryGetValue(id, out var oldItem))
                {
                    itemAdded++;
                }
                else if (JsonSerializer.Serialize(newItem, options) != JsonSerializer.Serialize(oldItem, options))
                {
                    itemAdded++;
                    itemRemoved++;
                }
            }

            itemRemoved += targetOriginalInfo.Keys.Count(id => !report.InfoDelta.ContainsKey(id));
            report.InfoAdded = itemAdded;
            report.InfoRemoved = itemRemoved;

            if (shadowedAchievements.Count > 0)
            {
                report.InfoWarnings.Add(shadowedAchievements.Count <= 10
                    ? $"Shadowed info: {string.Join(", ", shadowedAchievements.Select(x => $"[{x.Id}] '{x.Name}'"))}"
                    : $"Shadowed info ({shadowedAchievements.Count} items): {string.Join(", ", shadowedAchievements.Take(5).Select(x => x.Id))} and more.");
            }

            // 5. Construct Target State for Strings & Calculate Delta
            var isolatedTargetStringState = new Dictionary<LocaleStringId, Dictionary<string, string>>(baseStringMap);
            foreach (var (key, item) in targetOriginalString) isolatedTargetStringState[key] = item;
            foreach (var (key, item) in userStringEdits) isolatedTargetStringState[key] = item;

            var shadowedStrings = new List<LocaleStringId>();
            foreach (var (key, targetDict) in isolatedTargetStringState)
            {
                bool existsInBase = baseStringMap.TryGetValue(key, out var baseDict);
                bool isChanged = !existsInBase ||
                                 JsonSerializer.Serialize(targetDict) != JsonSerializer.Serialize(baseDict);

                if (isChanged)
                {
                    report.StringDelta[key] = targetDict;
                    report.StringsAddedOrModified++;

                    if (upperStringMap.ContainsKey(key))
                    {
                        shadowedStrings.Add(key);
                    }
                }
            }

            // --- GitHub Diff ---
            itemAdded = 0;
            itemRemoved = 0;

            foreach (var (id, newItem) in report.StringDelta)
            {
                if (!targetOriginalString.TryGetValue(id, out var oldItem))
                {
                    itemAdded++;
                }
                else if (JsonSerializer.Serialize(newItem) != JsonSerializer.Serialize(oldItem))
                {
                    itemAdded++;
                    itemRemoved++;
                }
            }

            itemRemoved += targetOriginalString.Keys.Count(id => !report.StringDelta.ContainsKey(id));
            report.StringsAdded = itemAdded;
            report.StringsRemoved = itemRemoved;

            if (shadowedStrings.Count > 0)
            {
                report.StringWarnings.Add(shadowedStrings.Count <= 10
                    ? $"Shadowed strings: {string.Join(", ", shadowedStrings)}"
                    : $"Shadowed strings ({shadowedStrings.Count} items).");
            }

            return report;
        }

        // Helpers for reading a single file directly
        private static Dictionary<uint, AchievementInfo> LoadSingleInfoFile(string file, JsonSerializerOptions options)
        {
            var result = new Dictionary<uint, AchievementInfo>();
            try
            {
                using FileStream fs = File.OpenRead(file);
                var infos = JsonSerializer.Deserialize<AchievementInfo[]>(fs, options);
                if (infos != null) foreach (var info in infos) result[info.Id] = info;
            }
            catch (Exception e) { Debug.WriteLine($"Error reading target info file: {e.Message}"); }
            return result;
        }

        private static StringMap LoadSingleStringFile(string file)
        {
            StringMap result = [];
            try
            {
                using FileStream fs = File.OpenRead(file);
                var stringMap = JsonSerializer.Deserialize<StringMap>(fs);
                if (stringMap != null) foreach (var kvp in stringMap) result[kvp.Key] = kvp.Value;
            }
            catch (Exception e) { Debug.WriteLine($"Error reading target string file: {e.Message}"); }
            return result;
        }

        // Helper methods to keep the builder clean
        private static Dictionary<uint, AchievementInfo> BuildTempInfoMap(IEnumerable<string> files, JsonSerializerOptions options)
        {
            var tempMap = new Dictionary<uint, AchievementInfo>();
            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;
                try
                {
                    using FileStream fs = File.OpenRead(file);
                    var infos = JsonSerializer.Deserialize<AchievementInfo[]>(fs, options);
                    if (infos != null)
                    {
                        foreach (var info in infos) tempMap[info.Id] = info;
                    }
                }
                catch (Exception e) { Debug.WriteLine($"Error reading {file}: {e.Message}"); }
            }
            return tempMap;
        }

        private static StringMap BuildTempStringMap(IEnumerable<string> files)
        {
            StringMap tempMap = [];
            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;
                try
                {
                    using FileStream fs = File.OpenRead(file);
                    var stringMap = JsonSerializer.Deserialize<StringMap>(fs);
                    if (stringMap != null)
                    {
                        foreach (var kvp in stringMap) tempMap[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception e) { Debug.WriteLine($"Error reading {file}: {e.Message}"); }
            }
            return tempMap;
        }

        /// <summary>
        /// Executes the actual save to disk, merging the delta into existing files if necessary.
        /// </summary>
        public void ExecuteSave(SavePatchReport report)
        {
            JsonSerializerOptions options = new() 
            { 
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // --- SAVE INFO MAP ---
            if (report.InfoDelta.Count > 0)
            {
                var targetInfoMap = new Dictionary<uint, AchievementInfo>();

                // If file exists, load its original content first
                if (!report.IsNewInfoFile && File.Exists(report.TargetInfoFilePath))
                {
                    try
                    {
                        using FileStream fs = File.OpenRead(report.TargetInfoFilePath);
                        var existingInfos = JsonSerializer.Deserialize<AchievementInfo[]>(fs, options);
                        if (existingInfos != null)
                        {
                            foreach (var info in existingInfos) targetInfoMap[info.Id] = info;
                        }
                    }
                    catch (Exception e) { Debug.WriteLine($"Failed to load existing patch file: {e.Message}"); }
                }

                // Apply delta (add new, update existing)
                foreach (var kvp in report.InfoDelta)
                {
                    targetInfoMap[kvp.Key] = kvp.Value;
                }

                // Clean up: Remove entries from the target file that are NO LONGER in the delta
                // (meaning the user reverted them to match the base layer)
                var keysToRemove = targetInfoMap.Keys.Where(k => !report.InfoDelta.ContainsKey(k)).ToList();
                foreach (var key in keysToRemove)
                {
                    targetInfoMap.Remove(key);
                }

                // Save back to disk
                string infoJson = JsonSerializer.Serialize(targetInfoMap.Values.ToArray(), options);
                File.WriteAllText(report.TargetInfoFilePath, infoJson);
                IsInfoDirty = false;
            }

            // --- SAVE STRING MAP (Same logic) ---
            if (report.StringDelta.Count > 0)
            {
                options = new()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var targetStringMap = new StringMap();

                if (!report.IsNewStringFile && File.Exists(report.TargetStringFilePath))
                {
                    try
                    {
                        using FileStream fs = File.OpenRead(report.TargetStringFilePath);
                        var existingStrings = JsonSerializer.Deserialize<StringMap>(fs, options);
                        if (existingStrings != null)
                        {
                            foreach (var kvp in existingStrings) targetStringMap[kvp.Key] = kvp.Value;
                        }
                    }
                    catch (Exception e) { Debug.WriteLine($"Failed to load existing string file: {e.Message}"); }
                }

                foreach (var kvp in report.StringDelta)
                {
                    targetStringMap[kvp.Key] = kvp.Value;
                }

                var stringKeysToRemove = targetStringMap.Keys.Where(k => !report.StringDelta.ContainsKey(k)).ToList();
                foreach (var key in stringKeysToRemove)
                {
                    targetStringMap.Remove(key);
                }
                string stringJson = JsonSerializer.Serialize(targetStringMap, options);
                File.WriteAllText(report.TargetStringFilePath, stringJson);
                IsStringDirty = false;
            }
        }

        public List<CategoryNode> GetCategoriesWithSubcategories()
        {
            var result = new List<CategoryNode>();

            foreach (var catKvp in _categoryToSubCats)
            {
                LocaleStringId catId = catKvp.Key;
                string catName = GetLocale(catId);

                var catNode = new CategoryNode
                {
                    Id = catId,
                    Name = catName
                };

                int totalCategoryVisibleCount = 0;

                foreach (LocaleStringId subId in catKvp.Value)
                {
                    string subName = GetLocale(subId);

                    int subVisibleCount = 0;
                    if (_subCatToRootAchievements.TryGetValue(subId, out var roots))
                    {
                        subVisibleCount = roots.Count(ach =>
                            ach.VisibleState == AchievementVisibleState.Visible && ach.Enabled && ach.ParentId == 0);
                    }

                    totalCategoryVisibleCount += subVisibleCount;

                    catNode.SubCategories.Add(new CategoryNode
                    {
                        Id = subId,
                        Name = subName,
                        VisibleAchievementsCount = subVisibleCount
                    });
                }

                catNode.VisibleAchievementsCount = totalCategoryVisibleCount;

                catNode.SubCategories = [.. catNode.SubCategories.OrderBy(s => s.Name)];

                result.Add(catNode);
            }

            return [.. result.OrderBy(c => c.Name)];
        }

        public List<AchievementNode> GetAchievementsForSubCategory(LocaleStringId subCatId)
        {
            var result = new List<AchievementNode>();

            if (!_subCatToRootAchievements.TryGetValue(subCatId, out var rootAchievements))
                return result;

            var sortedRoots = rootAchievements
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => GetLocale(a.Name));

            foreach (var rootAch in sortedRoots)
            {
                result.Add(BuildAchievementNodeRecursive(rootAch));
            }

            return result;
        }

        private AchievementNode BuildAchievementNodeRecursive(AchievementInfo ach)
        {
            string name = GetLocale(ach.Name);
            if (string.IsNullOrWhiteSpace(name))
                name = ach.Id.ToString();

            var node = new AchievementNode
            {
                Info = ach,
                DisplayName = name
            };

            if (_parentToChildren.TryGetValue((int)ach.Id, out var children))
            {
                var sortedChildren = children
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => GetLocale(c.Name));

                foreach (var childAch in sortedChildren)
                {
                    node.Children.Add(BuildAchievementNodeRecursive(childAch));
                }
            }

            return node;
        }

        public List<LocaleListItem> GetLocaleItemsForCurrentLanguage(string filter, int limit, out int totalMatches)
        {
            filter = filter?.Trim() ?? string.Empty;
            totalMatches = 0;

            var result = new List<LocaleListItem>();

            foreach (var kvp in _stringMap)
            {
                LocaleStringId key = kvp.Key;
                if (key == LocaleStringId.Invalid) continue;

                var translations = kvp.Value;
                if (!translations.TryGetValue(CurrentLanguage, out string text))
                {
                    translations.TryGetValue(GameLocale.DefaultLocale, out text);
                }

                text ??= string.Empty;
                string idStr = ((ulong)key).ToString();

                if (string.IsNullOrEmpty(filter) ||
                    idStr.Contains(filter) ||
                    text.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    totalMatches++;

                    if (limit <= 0 || result.Count < limit)
                    {
                        result.Add(new LocaleListItem
                        {
                            Id = key,
                            Text = text,
                            Used = GetLocaleUsageCount(key)
                        });
                    }
                }
            }

            return result;
        }

        public Dictionary<string, string> GetTranslationsForId(LocaleStringId id)
        {
            if (_stringMap.TryGetValue(id, out var translations))
            {
                return new Dictionary<string, string>(translations, StringComparer.Ordinal);
            }

            return [];
        }

        public LocaleStringId GenerateNewLocaleId(string path)
        {
            return (LocaleStringId)HashHelper.HashPath(path);
        }

        public void UpdateOrCreateLocale(LocaleStringId currentId, Dictionary<string, string> newTranslations)
        {
            if (currentId == LocaleStringId.Invalid || newTranslations == null)
                return;

            // Retrieve existing translation dictionary or initialize a new one for new IDs
            bool isNewId = !_stringMap.TryGetValue(currentId, out var existingDict);
            if (isNewId)
                existingDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            bool hasAnyChanges = false;

            foreach (var (lang, newText) in newTranslations)
            {
                // Normalize incoming string: trim whitespace and convert null to empty
                string cleanNewText = newText?.Trim() ?? string.Empty;

                // Fetch the original value for state delta comparison
                string originalText = existingDict.TryGetValue(lang, out var oldVal) ? (oldVal ?? string.Empty) : string.Empty;

                // Perform strict change detection against original snapshot
                if (cleanNewText != originalText)
                {
                    if (string.IsNullOrEmpty(cleanNewText))
                    {
                        // Remove entry if the string was cleared by user
                        existingDict.Remove(lang);
                    }
                    else
                    {
                        // Assign updated value and track newly introduced locale codes
                        existingDict[lang] = cleanNewText;
                        _availableLocales.Add(lang);
                    }

                    hasAnyChanges = true;
                }
            }

            // Register new dictionary mapping only if valid translation entries exist
            if (isNewId && existingDict.Count > 0)
                _stringMap[currentId] = existingDict;

            if (hasAnyChanges)
            {
                IsStringDirty = true;
            }
        }

        public int AddNew(AchievementInfo selectedAchievement)
        {
            uint newId = 1;
            while (_infoMap.ContainsKey(newId))
            {
                newId++;
            }

            long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var publishedTimeSpan = new TimeSpan(unixSeconds * TimeSpan.TicksPerSecond);

            var newNameId = GenerateNewLocaleId($"AchievementStrings.{newId}.Name");
            var existingDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [GameLocale.DefaultLocale] = "New Achievement"
            };
            _stringMap[newNameId] = existingDict;
            _localeUsageCount[newNameId] = 1;
            IsStringDirty = true;

            var newAchievement = new AchievementInfo
            {
                Id = newId,
                Enabled = true,
                Name = newNameId,
                CategoryStr = selectedAchievement.CategoryStr,
                SubCategoryStr = selectedAchievement.SubCategoryStr,
                EventType = ScoringEventType.Invalid,
                VisibleState = AchievementVisibleState.Visible,
                PublishedDateUS = publishedTimeSpan,
                Context = new AchievementContext() { EventData = [], EventContext = [] }
            };

            _infoMap.Add(newId, newAchievement);
            IsInfoDirty = true;

            _subCatToRootAchievements[selectedAchievement.SubCategoryStr].Add(newAchievement);

            return (int)newId;
        }
    }

    public class AchievementNode
    {
        public AchievementInfo Info { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public List<AchievementNode> Children { get; set; } = [];
    }
}