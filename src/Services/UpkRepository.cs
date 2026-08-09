using DDSLib;
using OpenCalligraphy.Core.GameData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UpkManager.Models.UpkFile;
using UpkManager.Models.UpkFile.Engine.Texture;
using UpkManager.Models.UpkFile.Objects;
using UpkManager.Models.UpkFile.Tables;
using UpkManager.Repository;

namespace MHAchievManager.Services
{
    public class UpkRepository
    {
        private Image _blankImage;
        private readonly UpkFileRepository _fileRepository = new();
        private readonly ConcurrentDictionary<string, UnrealHeader> _loadedHeaders = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, UnrealExportTableEntry> _exportIndexMap = new(StringComparer.OrdinalIgnoreCase);
        private readonly DdsFile ddsFile = new();
        public static UpkRepository Instance { get; private set; }
        public int LoadedExportsCount => _exportIndexMap.Values.Distinct().Count();

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
                    if (!string.Equals(export.ClassReferenceNameIndex.Name, "texture2d", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (export.SerialDataSize < 1100 || export.SerialDataSize > 2100)
                        continue;

                    /*
                    // Preload and filter icons
                    if (export.UnrealObject == null)
                        await export.ParseUnrealObject(false, false);

                    if (!IsValidIconTexture(export.UnrealObject as IUnrealObject)) 
                        continue;

                    // Save Min/Max
                    long currentSize = export.SerialDataSize;

                    lock (_sizeLock)
                    {
                        if (currentSize < _minIconDataSize) _minIconDataSize = currentSize;
                        if (currentSize > _maxIconDataSize) _maxIconDataSize = currentSize;
                    }
                    */

                    string pathName = export.GetPathName();
                    if (!string.IsNullOrEmpty(pathName))
                    {
                        _exportIndexMap[pathName] = export;
                    }
                    string shortName = export.ObjectNameIndex.Name;
                    if (!string.IsNullOrEmpty(shortName))
                    {
                        _exportIndexMap.TryAdd(shortName, export);
                    }
                }
            }

            return header;
        }

        public static bool IsValidIconTexture(IUnrealObject unrealObject)
        {
            return unrealObject.UObject is UTexture2D textureObject
                && textureObject.Mips.Count > 0
                && textureObject.Mips[0].SizeX == 40
                && textureObject.Mips[0].SizeY == 40;
        }

        public Image GetBlankIcon()
        {
            if (_blankImage == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"{assembly.GetName().Name}.Resources.Blank.png";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                _blankImage = Image.FromStream(stream);
            }

            return _blankImage;
        }

        public async Task<Image> GetIconImageAsync(AssetId iconPathAssetId)
        {
            string iconName = iconPathAssetId.GetName();

            if (!string.IsNullOrEmpty(iconName)
                && _exportIndexMap.TryGetValue(iconName, out var entry))
            {
                if (entry.UnrealObject == null)
                    await entry.ParseUnrealObject(false, false);

                if (entry.UnrealObject is IUnrealObject uObject
                    && uObject.UObject is UTexture2D textureObject)
                {
                    try
                    {
                        using Stream stream = textureObject.GetObjectStream(0);
                        if (stream != null)
                        {
                            ddsFile.Load(stream);
                            return BitmapSourceToBitmap(ddsFile.BitmapSource);
                        }
                    }
                    catch { }
                }
            }
            return GetBlankIcon();
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            using MemoryStream outStream = new();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(outStream);

            using var tempBitmap = new Bitmap(outStream);
            return new Bitmap(tempBitmap);
        }

        public bool HasAsset(string name) => _exportIndexMap.ContainsKey(name);
    }
}
