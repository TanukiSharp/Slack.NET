using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SlackDotNet.WebApi;
using SlackDotNet.RealTimeMessaging;

namespace SlackDotNet
{
    internal class ReactionItemTypeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string str)
            {
                if (string.Equals(str, "message", StringComparison.OrdinalIgnoreCase))
                    return ReactionItemType.Message;
                else if (string.Equals(str, "file", StringComparison.OrdinalIgnoreCase))
                    return ReactionItemType.File;
                else if (string.Equals(str, "file_comment", StringComparison.OrdinalIgnoreCase))
                    return ReactionItemType.FileComment;
            }

            throw new FormatException($"Invalid {nameof(ReactionItemType)} value '{reader.Value}'");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class ParseModeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string str)
            {
                if (string.Equals(str, StringConstants.None, StringComparison.OrdinalIgnoreCase))
                    return ParseMode.None;
                else if (string.Equals(str, StringConstants.Client, StringComparison.OrdinalIgnoreCase))
                    return ParseMode.Client;
                else if (string.Equals(str, StringConstants.Full, StringComparison.OrdinalIgnoreCase))
                    return ParseMode.Full;
            }

            throw new FormatException($"Invalid {nameof(ParseMode)} value '{reader.Value}'");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class WarningsJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string warnings)
                return warnings.Split(',');

            return new[] { $"Invalid JSON value: {reader.Value}" };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class UnixTimestampJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds((double)Convert.ChangeType(reader.Value, typeof(double)))
                .ToLocalTime();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class ColorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string color)
            {
                if (string.Equals(color, "good", StringComparison.OrdinalIgnoreCase))
                    return Color.Good;

                if (string.Equals(color, "warning", StringComparison.OrdinalIgnoreCase))
                    return Color.Warning;

                if (string.Equals(color, "danger", StringComparison.OrdinalIgnoreCase))
                    return Color.Danger;

                uint raw = Convert.ToUInt32(color, 16);

                uint alpha;
                uint red;
                uint green;
                uint blue;

                if (color.Length == 3)
                {
                    red = raw >> 8;
                    green = (raw >> 4) & 0xF;
                    blue = raw & 0xF;

                    return new Color(
                        255,
                        (byte)(red | red << 4),
                        (byte)(green | green << 4),
                        (byte)(blue | blue << 4)
                    );
                }
                else if (color.Length == 4)
                {
                    alpha = raw >> 12;
                    red = (raw >> 8) & 0xF;
                    green = (raw >> 4) & 0xF;
                    blue = raw & 0xF;

                    return new Color(
                        (byte)(alpha | alpha << 4),
                        (byte)(red | red << 4),
                        (byte)(green | green << 4),
                        (byte)(blue | blue << 4)
                    );
                }
                else if (color.Length == 6)
                {
                    return new Color(
                        255,
                        (byte)(raw >> 16),
                        (byte)((raw >> 8) & 0xFF),
                        (byte)(raw & 0xFF)
                    );
                }
                else if (color.Length == 8)
                {
                    return new Color(
                        (byte)(raw >> 24),
                        (byte)((raw >> 16) & 0xFF),
                        (byte)((raw >> 8) & 0xFF),
                        (byte)(raw & 0xFF)
                    );
                }
            }

            return default(Color); // or throw ?
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color c)
                writer.WriteValue(c.ToHexString());
            else
                throw new ArgumentException($"The value must be of type '{nameof(Color)}'"); // <- should never happen
        }
    }
}
