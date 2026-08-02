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
            options.Converters.Add(new TimeSpanJsonConverter());

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
    }

    public class AchievementNode
    {
        public AchievementInfo Info { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public List<AchievementNode> Children { get; set; } = [];
    }
}