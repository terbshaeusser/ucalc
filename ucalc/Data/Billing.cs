using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable 659

namespace UCalc.Data
{
    public enum Salutation
    {
        Sir,
        SirDr,
        Madam,
        MadamDr,
        SirAndMadam,
        Family
    }

    public static class Salutations
    {
        public static string AsString(this Salutation salutation)
        {
            switch (salutation)
            {
                case Salutation.Sir:
                    return "Herr";
                case Salutation.SirDr:
                    return "Herr Dr.";
                case Salutation.Madam:
                    return "Frau";
                case Salutation.MadamDr:
                    return "Frau Dr.";
                case Salutation.SirAndMadam:
                    return "Herr und Frau";
                case Salutation.Family:
                    return "Familie";
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public class Address
    {
        [JsonProperty(PropertyName = "street"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Street { get; set; }

        [JsonProperty(PropertyName = "houseNumber"), JsonRequired,
         JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string HouseNumber { get; set; }

        [JsonProperty(PropertyName = "city"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string City { get; set; }

        [JsonProperty(PropertyName = "postCode"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Postcode { get; set; }

        public Address()
        {
            Street = "";
            HouseNumber = "";
            City = "";
            Postcode = "";
        }
    }

    public class BankAccount
    {
        [JsonProperty(PropertyName = "iban"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Iban { get; set; }

        [JsonProperty(PropertyName = "bic"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Bic { get; set; }

        [JsonProperty(PropertyName = "bankName"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string BankName { get; set; }

        public BankAccount()
        {
            Iban = "";
            Bic = "";
            BankName = "";
        }
    }

    public class Flat
    {
        [JsonProperty(PropertyName = "id"), JsonRequired]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "size"), JsonRequired]
        public int Size { get; set; }

        public Flat()
        {
            Id = Guid.NewGuid();
            Name = "";
        }

        private bool Equals(Flat other)
        {
            return Id.Equals(other.Id) && Name == other.Name && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Flat) obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return Id.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }

    public class Tenant
    {
        [JsonProperty(PropertyName = "salutation"), JsonRequired, JsonConverter(typeof(StringEnumConverter))]
        public Salutation Salutation { get; set; }

        [JsonProperty(PropertyName = "name"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "personCount"), JsonRequired]
        public int PersonCount { get; set; }

        [JsonProperty(PropertyName = "bankAccount"), JsonRequired]
        public BankAccount BankAccount { get; set; }

        [JsonProperty(PropertyName = "entryDate")]
        public DateTime? EntryDate { get; set; }

        [JsonProperty(PropertyName = "departureDate")]
        public DateTime? DepartureDate { get; set; }

        [JsonProperty(PropertyName = "rentedFlats"), JsonRequired, JsonConverter(typeof(FlatSerializationConverter))]
        public HashSet<Flat> RentedFlats { get; set; }

        [JsonProperty(PropertyName = "paidRent"), JsonRequired]
        public decimal PaidRent { get; set; }

        [JsonProperty(PropertyName = "customMessage1"), JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string CustomMessage1 { get; set; }

        [JsonProperty(PropertyName = "customMessage2"), JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string CustomMessage2 { get; set; }

        public Tenant()
        {
            Name = "";
            BankAccount = new BankAccount();
            RentedFlats = new HashSet<Flat>();
            CustomMessage1 = "";
            CustomMessage2 = "";
        }
    }

    public class Landlord
    {
        [JsonProperty(PropertyName = "salutation"), JsonRequired, JsonConverter(typeof(StringEnumConverter))]
        public Salutation Salutation { get; set; }

        [JsonProperty(PropertyName = "name"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "mailAddress"), JsonRequired,
         JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string MailAddress { get; set; }

        [JsonProperty(PropertyName = "phone"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "address"), JsonRequired]
        public Address Address { get; set; }

        [JsonProperty(PropertyName = "bankAccount"), JsonRequired]
        public BankAccount BankAccount { get; set; }

        public Landlord()
        {
            Name = "";
            MailAddress = "";
            Phone = "";
            Address = new Address();
            BankAccount = new BankAccount();
        }
    }

    public class House
    {
        [JsonProperty(PropertyName = "address"), JsonRequired]
        public Address Address { get; set; }

        [JsonProperty(PropertyName = "flats"), JsonRequired]
        public List<Flat> Flats { get; set; }

        public House()
        {
            Address = new Address();
            Flats = new List<Flat>();
        }
    }

    public class CostEntryDetails
    {
        [JsonProperty(PropertyName = "totalAmount"), JsonRequired]
        public decimal TotalAmount { get; set; }

        [JsonProperty(PropertyName = "unitCount"), JsonRequired]
        public decimal UnitCount { get; set; }

        [JsonProperty(PropertyName = "discountsInUnits"), JsonRequired]
        public List<decimal> DiscountsInUnits { get; set; }

        public CostEntryDetails()
        {
            DiscountsInUnits = new List<decimal>();
        }
    }

    public class CostEntry
    {
        [JsonProperty(PropertyName = "startDate"), JsonRequired]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate"), JsonRequired]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "amount"), JsonRequired]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "details"), JsonRequired]
        public CostEntryDetails Details { get; set; }

        public CostEntry()
        {
            Details = new CostEntryDetails();
        }
    }

    public enum CostDivision
    {
        Person,
        Flat,
        Size
    }

    public static class CostDivisions
    {
        public static string AsString(this CostDivision division)
        {
            switch (division)
            {
                case CostDivision.Person:
                    return "Pro Person";
                case CostDivision.Flat:
                    return "Pro Wohnung";
                case CostDivision.Size:
                    return "Pro m²";
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public class Cost
    {
        [JsonProperty(PropertyName = "name"), JsonRequired, JsonConverter(typeof(JsonNullToEmptyStringConverter))]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "division"), JsonRequired, JsonConverter(typeof(StringEnumConverter))]
        public CostDivision Division { get; set; }

        [JsonProperty(PropertyName = "affectsAll"), JsonRequired]
        public bool AffectsAll { get; set; }

        [JsonProperty(PropertyName = "shiftUnrented"), JsonRequired]
        public bool ShiftUnrented { get; set; }

        [JsonProperty(PropertyName = "affectedFlats"), JsonRequired, JsonConverter(typeof(FlatSerializationConverter))]
        public HashSet<Flat> AffectedFlats { get; set; }

        [JsonProperty(PropertyName = "entries"), JsonRequired]
        public List<CostEntry> Entries { get; set; }

        [JsonProperty(PropertyName = "displayInBill"), JsonRequired]
        public bool DisplayInBill { get; set; }

        public Cost()
        {
            Name = "";
            AffectedFlats = new HashSet<Flat>();
            Entries = new List<CostEntry>();
        }
    }

    public class Billing
    {
        [JsonProperty(PropertyName = "startDate"), JsonRequired]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate"), JsonRequired]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "landlord"), JsonRequired]
        public Landlord Landlord { get; set; }

        [JsonProperty(PropertyName = "house"), JsonRequired]
        public House House { get; set; }

        [JsonProperty(PropertyName = "tenants"), JsonRequired]
        public List<Tenant> Tenants { get; set; }

        [JsonProperty(PropertyName = "costs"), JsonRequired]
        public List<Cost> Costs { get; set; }

        public Billing()
        {
            Landlord = new Landlord();
            House = new House();
            Tenants = new List<Tenant>();
            Costs = new List<Cost>();
        }
    }
}