using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MHAchievManager.Models
{
    public class AchievementInfoJsonConverter : JsonConverter<AchievementInfo>
    {
        public override AchievementInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var info = new AchievementInfo
            {
                Id = root.GetProperty("Id").GetUInt32(),
                Enabled = root.GetProperty("Enabled").GetBoolean(),
                ParentId = root.GetProperty("ParentId").GetInt32(),
                Name = (LocaleStringId)root.GetProperty("Name").GetUInt64(),
                InProgressStr = (LocaleStringId)root.GetProperty("InProgressStr").GetUInt64(),
                CompletedStr = (LocaleStringId)root.GetProperty("CompletedStr").GetUInt64(),
                RewardStr = (LocaleStringId)root.GetProperty("RewardStr").GetUInt64(),
                IconPathAssetId = (AssetId)root.GetProperty("IconPathAssetId").GetUInt64(),
                Score = root.GetProperty("Score").GetInt32(),
                CategoryStr = (LocaleStringId)root.GetProperty("CategoryStr").GetUInt64(),
                SubCategoryStr = (LocaleStringId)root.GetProperty("SubCategoryStr").GetUInt64(),
                DisplayOrder = root.GetProperty("DisplayOrder").GetInt32(),
                VisibleState = (AchievementVisibleState)root.GetProperty("VisibleState").GetInt32(),
                EvaluationType = (AchievementEvaluationType)root.GetProperty("EvaluationType").GetInt32(),
                EventType = (ScoringEventType)root.GetProperty("EventType").GetInt32(),
                Threshold = root.GetProperty("Threshold").GetInt32(),
                DependentAchievementId = root.GetProperty("DependentAchievementId").GetInt32(),
                UIProgressDisplayOption = (AchievementUIProgressDisplayOption)root.GetProperty("UIProgressDisplayOption").GetInt32()
            };

            if (root.TryGetProperty("PublishedDateUS", out var dateProp))
                info.PublishedDateUS = new TimeSpan(dateProp.GetInt64());

            info.OrbisTrophy = root.TryGetProperty("OrbisTrophy", out var ot) && ot.GetBoolean();
            info.OrbisTrophyId = root.TryGetProperty("OrbisTrophyId", out var oti) ? oti.GetInt32() : -1;
            info.OrbisTrophyShared = root.TryGetProperty("OrbisTrophyShared", out var ots) && ots.GetBoolean();

            info.PartyVisible = root.TryGetProperty("PartyVisible", out var pv) && pv.GetBoolean();

            if (root.TryGetProperty("Context", out var ctx) && ctx.ValueKind != JsonValueKind.Null)
            {
                info.Context = ctx.Deserialize<AchievementContext>(options) ?? new AchievementContext();
            }
            else
            {
                info.Context = new AchievementContext();
            }

            return info;
        }

        public override void Write(Utf8JsonWriter writer, AchievementInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("Id", value.Id);
            writer.WriteBoolean("Enabled", value.Enabled);
            writer.WriteNumber("ParentId", value.ParentId);
            writer.WriteNumber("Name", (ulong)value.Name);
            writer.WriteNumber("InProgressStr", (ulong)value.InProgressStr);
            writer.WriteNumber("CompletedStr", (ulong)value.CompletedStr);
            writer.WriteNumber("RewardStr", (ulong)value.RewardStr);
            writer.WriteNumber("IconPathAssetId", (ulong)value.IconPathAssetId);
            writer.WriteNumber("Score", value.Score);
            writer.WriteNumber("CategoryStr", (ulong)value.CategoryStr);
            writer.WriteNumber("SubCategoryStr", (ulong)value.SubCategoryStr);
            writer.WriteNumber("DisplayOrder", value.DisplayOrder);
            writer.WriteNumber("VisibleState", (int)value.VisibleState);
            writer.WriteNumber("EvaluationType", (int)value.EvaluationType);
            writer.WriteNumber("EventType", (int)value.EventType);
            writer.WriteNumber("Threshold", value.Threshold);
            writer.WriteNumber("DependentAchievementId", value.DependentAchievementId);
            writer.WriteNumber("UIProgressDisplayOption", (int)value.UIProgressDisplayOption);
            writer.WriteNumber("PublishedDateUS", value.PublishedDateUS.Ticks);

            if (value.OrbisTrophy)
                writer.WriteBoolean("OrbisTrophy", value.OrbisTrophy);
            if (value.OrbisTrophyId != -1)
                writer.WriteNumber("OrbisTrophyId", value.OrbisTrophyId);
            if (value.OrbisTrophyShared)
                writer.WriteBoolean("OrbisTrophyShared", value.OrbisTrophyShared);

            writer.WriteBoolean("PartyVisible", value.PartyVisible);

            if (value.Context != null && !value.Context.IsEmpty)
            {
                writer.WritePropertyName("Context");
                JsonSerializer.Serialize(writer, value.Context, options);
            }

            writer.WriteEndObject();
        }
    }

    public class ReadOnlyPropertyConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string);
    }

    public class UnixTimeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is TimeSpan ts)
            {
                long totalSeconds = ts.Ticks / TimeSpan.TicksPerSecond;
                if (totalSeconds <= 0) return "N/A";
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(totalSeconds).UtcDateTime;
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AchievementIdConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is int id)
            {
                if (id == 0)
                    return "[0] None";

                var ach = AchievementRepository.Instance.GetAchievement(id);

                if (ach != null)
                {
                    string name = AchievementRepository.Instance.GetLocale(ach.Name);
                    return string.IsNullOrEmpty(name) ? $"[ID: {id}]" : $"[{id}] {name}";
                }

                return $"[{id}] Unknown Achievement";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AssetIdConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is AssetId assetId)
            {
                ulong numericVal = (ulong)assetId;

                if (numericVal == 0) return "Invalid";

                if (DataDirectory.Instance != null && DataDirectory.Instance.DataChecksum != 0)
                {
                    string assetPath = assetId.GetName();
                    if (!string.IsNullOrEmpty(assetPath)) return assetPath;
                }

                return numericVal.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class GuidPrototypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is long protoGuidVal)
            {
                if (protoGuidVal == 0) return "";

                if (DataDirectory.Instance != null && DataDirectory.Instance.DataChecksum != 0)
                {
                    var protoGuid = (PrototypeGuid)protoGuidVal;
                    var protoId = DataDirectory.Instance.GetPrototypeIdByGuid(protoGuid);

                    if (protoId != PrototypeId.Invalid) return protoId.GetName();
                }

                return protoGuidVal.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }        
    }

    public class EventDataArrayConverter : ArrayConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is EventData[] array)
            {
                return $"EventData[{array.Length}]";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class EventContextArrayConverter : ArrayConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is EventContext[] array)
            {
                return $"EventContext[{array.Length}]";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public sealed class LocaleStringConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is LocaleStringId localeId)
            {
                return AchievementRepository.Instance.GetLocale(localeId);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
