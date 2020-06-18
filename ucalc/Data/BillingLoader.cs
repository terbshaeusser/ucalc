using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        private static string AsString(JObject parent, string name, bool optional = false)
        {
            if (optional && (parent[name]?.Type ?? JTokenType.Null) == JTokenType.Null)
            {
                return "";
            }

            return parent[name]?.Value<string>() ??
                   throw new JsonException($"Cannot read {parent.Path}.{name} as string");
        }

        private static DateTime AsDate(JObject parent, string name)
        {
            return DateTime.ParseExact(AsString(parent, name), "dd.MM.yyyy", null);
        }

        private static DateTime? AsDateOptional(JObject parent, string name)
        {
            var str = AsString(parent, name, true);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return DateTime.ParseExact(str, "dd.MM.yyyy", null);
        }

        private static int AsInt(JObject parent, string name)
        {
            return parent[name]?.Value<int>() ??
                   throw new JsonException($"Cannot read {parent.Path}.{name} as integer");
        }

        private static bool AsBool(JObject parent, string name)
        {
            return parent[name]?.Value<bool>() ??
                   throw new JsonException($"Cannot read {parent.Path}.{name} as bool");
        }

        private static decimal AsDecimal(JObject parent, string name)
        {
            return decimal.Parse(parent[name]?.Value<string>() ??
                                 throw new JsonException($"Cannot read {parent.Path}.{name} as decimal"));
        }

        private static decimal? AsDecimalOptional(JObject parent, string name)
        {
            var str = AsString(parent, name, true);
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return decimal.Parse(str);
        }

        private static JObject AsObject(JObject parent, string name)
        {
            if ((parent[name]?.Type ?? JTokenType.Null) != JTokenType.Object)
            {
                throw new JsonException($"Cannot read {parent.Path}.{name} as object");
            }

            return (JObject) parent[name];
        }

        private static JArray AsArray(JObject parent, string name, bool optional = false)
        {
            var type = parent[name]?.Type ?? JTokenType.Object;

            if (optional && type == JTokenType.Null)
            {
                return null;
            }

            if (type != JTokenType.Array)
            {
                throw new JsonException($"Cannot read {parent.Path}.{name} as array");
            }

            return (JArray) parent[name];
        }

        private static void LoadAddress(JObject parent, string name, Address target)
        {
            var address = AsObject(parent, name);
            target.Street = AsString(address, "street");
            target.HouseNumber = AsString(address, "house_number");
            target.City = AsString(address, "city");
            target.Postcode = AsString(address, "plz");
        }

        private static void LoadBankAccount(JObject parent, string name, BankAccount target)
        {
            var bankAccount = AsObject(parent, name);
            target.Iban = AsString(bankAccount, "iban");
            target.Bic = AsString(bankAccount, "bic");
            target.BankName = AsString(bankAccount, "bank_name");
        }

        private Billing LoadFormat1(string path)
        {
            var content = File.ReadAllText(path);
            try
            {
                var root = JObject.Parse(content);
                var billing = new Billing
                {
                    StartDate = AsDate(root, "start_date"),
                    EndDate = AsDate(root, "end_date")
                };

                var landlord = AsObject(root, "owner");
                billing.Landlord.Salutation = (Salutation) AsInt(landlord, "salutation");
                billing.Landlord.Name = AsString(landlord, "name");
                billing.Landlord.Phone = AsString(landlord, "phone");
                billing.Landlord.MailAddress = AsString(landlord, "mail");
                LoadAddress(landlord, "address", billing.Landlord.Address);
                LoadBankAccount(landlord, "account", billing.Landlord.BankAccount);

                var house = AsObject(root, "house");
                LoadAddress(house, "address", billing.House.Address);

                var tenants = AsArray(root, "renters");
                foreach (var item in tenants)
                {
                    var tenant = (JObject) item;

                    var targetTenant = new Tenant
                    {
                        Salutation = (Salutation) AsInt(tenant, "salutation"),
                        Name = AsString(tenant, "name"),
                        PersonCount = AsInt(tenant, "person_count"),
                        EntryDate = AsDateOptional(tenant, "entry_date"),
                        DepartureDate = AsDateOptional(tenant, "departure_date"),
                        PaidRent = AsDecimal(tenant, "paid_rent"),
                        CustomMessage1 = AsString(tenant, "message1", true),
                        CustomMessage2 = AsString(tenant, "message2", true)
                    };
                    LoadBankAccount(tenant, "account", targetTenant.BankAccount);

                    var flat = new Flat
                    {
                        Name = $"Wohnung von {targetTenant.Name}",
                        Size = 1
                    };

                    targetTenant.RentedFlats.Add(flat);

                    billing.Tenants.Add(targetTenant);
                    billing.House.Flats.Add(flat);
                }

                var costs = AsArray(root, "costs");
                foreach (var item in costs)
                {
                    var cost = (JObject) item;

                    var targetCost = new Cost
                    {
                        Name = AsString(cost, "name"),
                        Division = (CostDivision) AsInt(cost, "division"),
                        AffectsAll = AsBool(cost, "affects_all_renters"),
                        AffectedFlats =
                            (AsArray(cost, "affected_renters", true) ?? new JArray()).Select(
                                renterItem =>
                                    billing.House.Flats[AsInt((JObject) renterItem, "id")]).ToHashSet(),
                        Entries = (AsArray(cost, "entries", true) ?? new JArray()).Select(entryItem =>
                        {
                            var parent = (JObject) entryItem;

                            var details = new CostEntryDetails
                            {
                                TotalAmount = AsDecimalOptional(parent, "cubic.sumprice") ?? 0,
                                UnitCount = AsDecimalOptional(parent, "cubic.sum") ?? 0
                            };

                            for (var i = 1; i <= 4; ++i)
                            {
                                var d = AsDecimalOptional(parent, $"cubic.discount{i}");

                                if (d != null)
                                {
                                    details.DiscountsInUnits.Add(d.Value);
                                }
                            }

                            return new CostEntry
                            {
                                StartDate = AsDate(parent, "start_date"),
                                EndDate = AsDate(parent, "end_date"),
                                Amount = AsDecimal(parent, "price"),
                                Details = details
                            };
                        }).ToList(),
                        DisplayInBill = AsBool(cost, "display")
                    };

                    billing.Costs.Add(targetCost);
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
            catch (FormatException e)
            {
                throw new IOException($"Could not load JSON with error:\n{e.Message}", e);
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private Billing LoadFormat2(string path)
        {
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

        public Billing Load(string path)
        {
            try
            {
                return LoadFormat2(path);
            }
            catch
            {
                return LoadFormat1(path);
            }
        }

        public void Store(string path, Billing billing)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(billing, Formatting.Indented));
        }
    }
}