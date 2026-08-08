using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpkManager.Models;
using UpkManager.Models.UpkFile;
using UpkManager.Models.UpkFile.Tables;
using UpkManager.Repository;

namespace MHAchievManager.Services
{
    public class UpkRepository
    {
        private readonly UpkFileRepository _fileRepository = new();
        private readonly ConcurrentDictionary<string, UnrealHeader> _loadedHeaders = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, (UnrealHeader Header, UnrealExportTableEntry Entry)> _exportIndexMap = new(StringComparer.OrdinalIgnoreCase);
        public static UpkRepository Instance { get; private set; }
        public UnrealUpkFile UpkFile { get; private set; }
        public int LoadedExportsCount => _exportIndexMap.Count;

        private UpkRepository() { }

        public static void Initialize()
        {
            Instance ??= new();
        }

        public async Task PreloadPackagesAsync(IEnumerable<string> filePaths)
        {
            var tasks = filePaths.Distinct().Select(LoadAndIndexUpkAsync);
            await Task.WhenAll(tasks);
        }

        public async Task<UnrealHeader> LoadAndIndexUpkAsync(string fullPath)
        {
            if (_loadedHeaders.TryGetValue(fullPath, out var cached))
                return cached;

            var header = await _fileRepository.LoadUpkFile(fullPath);
            await header.ReadHeaderAsync(null);

            _loadedHeaders[fullPath] = header;

            if (header.ExportTable != null)
            {
                foreach (var export in header.ExportTable)
                {
                    string objectName = export.GetPathName();
                    if (string.IsNullOrEmpty(objectName))
                        continue;

                    _exportIndexMap[objectName] = (header, export);
                }
            }

            return header;
        }

        public bool TryGetExportByName(string iconName, out UnrealHeader header, out UnrealExportTableEntry entry)
        {
            if (_exportIndexMap.TryGetValue(iconName, out var result))
            {
                header = result.Header;
                entry = result.Entry;
                return true;
            }

            header = null;
            entry = null;
            return false;
        }
    }
}
