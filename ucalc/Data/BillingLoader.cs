using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UCalc.Data
{
    internal class FlatSerializationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var set = (HashSet<Flat>) value;

            writer.WriteStartArray();

            foreach (var flat in set)
            {
                writer.WriteValue(flat.Id.ToString());
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonException("Expected [");
            }

            while (reader.Read() && reader.TokenType == JsonToken.String)
            {
            }

            if (reader.TokenType != JsonToken.EndArray)
            {
                throw new JsonException("Expected ]");
            }

            return new HashSet<Flat>();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new InvalidOperationException();
        }
    }

    internal class JsonNullToEmptyStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return reader.Value ?? "";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value ?? "");
        }
    }

    public class BillingLoader
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public Billing Load(string path)
        {
            // TODO: Support older format

            var content = File.ReadAllText(path);
            try
            {
                var billing = JsonConvert.DeserializeObject<Billing>(content);

                var idToFlat = new Dictionary<string, Flat>();
                foreach (var flat in billing.House.Flats)
                {
                    idToFlat.Add(flat.Id.ToString(), flat);
                }

                var root = JObject.Parse(content);
                var i = 0;
                foreach (var tenantItem in root["tenants"])
                {
                    foreach (var rentedFlatId in tenantItem["rentedFlats"])
                    {
                        billing.Tenants[i].RentedFlats.Add(idToFlat[rentedFlatId.Value<string>()]);
                    }

                    ++i;
                }

                i = 0;
                foreach (var tenantItem in root["costs"])
                {
                    foreach (var rentedFlatId in tenantItem["affectedFlats"])
                    {
                        billing.Costs[i].AffectedFlats.Add(idToFlat[rentedFlatId.Value<string>()]);
                    }

                    ++i;
                }

                return billing;
            }
            catch (JsonException e)
            {
                throw new IOException($"Could not load JSON with error:\n{e.Message}", e);
            }
            catch (InvalidCastException e)
            {
                throw new IOException($"Could not load JSON with error:\n{e.Message}", e);
            }
        }

        public void Store(string path, Billing billing)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(billing, Formatting.Indented));
        }
    }
}