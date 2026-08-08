using System;
using System.IO;
using System.Text.Json;

namespace MHAchievManager.Services
{
    public static class AppConfig
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private class ConfigData
        {
            public string GameFolderPath { get; set; } = string.Empty;
            public string AchievFolderPath { get; set; } = string.Empty;
        }

        public static string GameFolderPath { get; set; } = string.Empty;
        public static string AchievFolderPath { get; set; } = string.Empty;

        public static void Load()
        {
            if (!File.Exists(ConfigPath)) return;

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var data = JsonSerializer.Deserialize<ConfigData>(json);

                if (data != null)
                {
                    GameFolderPath = data.GameFolderPath ?? string.Empty;
                    AchievFolderPath = data.AchievFolderPath ?? string.Empty;
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                var data = new ConfigData
                {
                    GameFolderPath = GameFolderPath,
                    AchievFolderPath = AchievFolderPath
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
