using MHAchievManager.Services;
using OpenCalligraphy.Core.GameData;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MHAchievManager.Models
{
    /// <summary>
    /// Serializes <see cref="TimeSpan"/> values using the underlying number of ticks.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Ticks);
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

    public class CleanArrayConverter : ArrayConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Array array)
            {
                string elemTypeName = array.GetType().GetElementType()?.Name ?? "Item";
                return $"{elemTypeName}[{array.Length}]";
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
