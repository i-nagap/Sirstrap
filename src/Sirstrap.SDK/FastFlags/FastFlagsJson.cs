namespace Sirstrap.SDK.FastFlags
{
    public static class FastFlagsJson
    {
        private static string ToDisplayValue(object value)
        {
            return value switch
            {
                bool booleanValue => booleanValue ? "True" : "False",
                long integerValue => integerValue.ToString(CultureInfo.InvariantCulture),
                double decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
                _ => (string)value
            };
        }

        private static object? ToScalar(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => element.TryGetInt64(out long integerValue) ? integerValue : element.GetDouble(),
                JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Null => null,
                _ => null
            };
        }

        private static object ToTypedValue(string value)
        {
            return value switch
            {
                _ when bool.TryParse(value, out bool booleanValue) => booleanValue,
                _ when long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integerValue) => integerValue,
                _ when double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double decimalValue) => decimalValue,
                _ => value
            };
        }

        private static void WriteFlag(Utf8JsonWriter writer, string name, object value)
        {
            switch (value)
            {
                case bool booleanValue:
                    writer.WriteBoolean(name, booleanValue);

                    break;

                case long integerValue:
                    writer.WriteNumber(name, integerValue);

                    break;

                case double decimalValue:
                    writer.WriteNumber(name, decimalValue);

                    break;

                default:
                    writer.WriteString(name, (string)value);

                    break;
            }
        }

        internal static SortedDictionary<string, object>? ParseTyped(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            SortedDictionary<string, object> typedFlags = new(StringComparer.Ordinal);

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                object? value = ToScalar(property.Value);

                if (value == null)
                {
                    Log.Warning("Ignored a FastFlag whose value is not a number, text or true/false (Flag: {0}).", property.Name);

                    continue;
                }

                typedFlags[property.Name] = value;
            }

            return typedFlags;
        }

        internal static string SerializeTyped(SortedDictionary<string, object> typedFlags)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions
            {
                Indented = true
            }))
            {
                writer.WriteStartObject();

                foreach ((string name, object value) in typedFlags)
                {
                    WriteFlag(writer, name, value);
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        internal static SortedDictionary<string, string> ToDisplayFlags(SortedDictionary<string, object> typedFlags)
        {
            SortedDictionary<string, string> displayFlags = new(StringComparer.Ordinal);

            foreach ((string name, object value) in typedFlags)
            {
                displayFlags[name] = ToDisplayValue(value);
            }

            return displayFlags;
        }

        internal static SortedDictionary<string, object> ToTypedFlags(IReadOnlyDictionary<string, string> displayFlags)
        {
            SortedDictionary<string, object> typedFlags = new(StringComparer.Ordinal);

            foreach ((string name, string value) in displayFlags)
            {
                string trimmedName = name.Trim();

                if (!string.IsNullOrEmpty(trimmedName))
                {
                    typedFlags[trimmedName] = ToTypedValue(value.Trim());
                }
            }

            return typedFlags;
        }

        public static string Serialize(IReadOnlyDictionary<string, string> flags)
        {
            return SerializeTyped(ToTypedFlags(flags));
        }

        public static IReadOnlyDictionary<string, string>? TryParse(string json)
        {
            try
            {
                SortedDictionary<string, object>? typedFlags = ParseTyped(json);

                return typedFlags == null ? null : ToDisplayFlags(typedFlags);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}