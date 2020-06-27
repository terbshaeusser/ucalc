﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private bool Equals(Address other)
        {
            return Street == other.Street && HouseNumber == other.HouseNumber && City == other.City &&
                   Postcode == other.Postcode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Address) obj);
        }

        public Address Clone()
        {
            return new Address
            {
                Street = Street,
                HouseNumber = HouseNumber,
                City = City,
                Postcode = Postcode
            };
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

        private bool Equals(BankAccount other)
        {
            return Iban == other.Iban && Bic == other.Bic && BankName == other.BankName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BankAccount) obj);
        }

        public BankAccount Clone()
        {
            return new BankAccount
            {
                Iban = Iban,
                Bic = Bic,
                BankName = BankName
            };
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

        public Flat Clone()
        {
            return new Flat
            {
                Id = Id,
                Name = Name,
                Size = Size
            };
        }
    }

    public class Tenant
    {
        [JsonProperty(PropertyName = "id"), JsonRequired]
        public Guid Id { get; set; }

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
            Id = Guid.NewGuid();
            Name = "";
            BankAccount = new BankAccount();
            RentedFlats = new HashSet<Flat>();
            CustomMessage1 = "";
            CustomMessage2 = "";
        }

        private bool Equals(Tenant other)
        {
            return Id.Equals(other.Id) && Salutation == other.Salutation && Name == other.Name &&
                   PersonCount == other.PersonCount && Equals(BankAccount, other.BankAccount) &&
                   Nullable.Equals(EntryDate, other.EntryDate) && Nullable.Equals(DepartureDate, other.DepartureDate) &&
                   RentedFlats.SequenceEqual(other.RentedFlats) && PaidRent == other.PaidRent &&
                   CustomMessage1 == other.CustomMessage1 && CustomMessage2 == other.CustomMessage2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Tenant) obj);
        }

        public Tenant Clone(Dictionary<Flat, Flat> flatMapper)
        {
            return new Tenant
            {
                Id = Id,
                Salutation = Salutation,
                Name = Name,
                PersonCount = PersonCount,
                BankAccount = BankAccount.Clone(),
                EntryDate = EntryDate,
                DepartureDate = DepartureDate,
                RentedFlats = new HashSet<Flat>(RentedFlats.Select(flat => flatMapper[flat])),
                PaidRent = PaidRent,
                CustomMessage1 = CustomMessage1,
                CustomMessage2 = CustomMessage2
            };
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

        private bool Equals(Landlord other)
        {
            return Salutation == other.Salutation && Name == other.Name && MailAddress == other.MailAddress &&
                   Phone == other.Phone && Equals(Address, other.Address) && Equals(BankAccount, other.BankAccount);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Landlord) obj);
        }

        public Landlord Clone()
        {
            return new Landlord
            {
                Salutation = Salutation,
                Name = Name,
                MailAddress = MailAddress,
                Phone = Phone,
                Address = Address.Clone(),
                BankAccount = BankAccount.Clone()
            };
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

        private bool Equals(House other)
        {
            return Equals(Address, other.Address) && Flats.SequenceEqual(other.Flats);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((House) obj);
        }

        public House Clone()
        {
            return new House
            {
                Address = Address.Clone(),
                Flats = new List<Flat>(Flats.Select(flat => flat.Clone()))
            };
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

        private bool Equals(CostEntryDetails other)
        {
            return TotalAmount == other.TotalAmount && UnitCount == other.UnitCount &&
                   DiscountsInUnits.SequenceEqual(other.DiscountsInUnits);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CostEntryDetails) obj);
        }

        public CostEntryDetails Clone()
        {
            return new CostEntryDetails
            {
                TotalAmount = TotalAmount,
                UnitCount = UnitCount,
                DiscountsInUnits = new List<decimal>(DiscountsInUnits)
            };
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

        private bool Equals(CostEntry other)
        {
            return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate) && Amount == other.Amount &&
                   Details.Equals(other.Details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CostEntry) obj);
        }

        public CostEntry Clone()
        {
            return new CostEntry
            {
                StartDate = StartDate,
                EndDate = EndDate,
                Amount = Amount,
                Details = Details?.Clone()
            };
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

        private bool Equals(Cost other)
        {
            return Name == other.Name && Division == other.Division && AffectsAll == other.AffectsAll &&
                   ShiftUnrented == other.ShiftUnrented && AffectedFlats.SequenceEqual(other.AffectedFlats) &&
                   Entries.SequenceEqual(other.Entries) && DisplayInBill == other.DisplayInBill;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Cost) obj);
        }

        public Cost Clone(Dictionary<Flat, Flat> flatMapper)
        {
            return new Cost
            {
                Name = Name,
                Division = Division,
                AffectsAll = AffectsAll,
                ShiftUnrented = ShiftUnrented,
                AffectedFlats = new HashSet<Flat>(AffectedFlats.Select(flat => flatMapper[flat])),
                Entries = new List<CostEntry>(Entries.Select(entry => entry.Clone())),
                DisplayInBill = DisplayInBill
            };
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

        private bool Equals(Billing other)
        {
            return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate) &&
                   Landlord.Equals(other.Landlord) && House.Equals(other.House) &&
                   Tenants.SequenceEqual(other.Tenants) && Costs.SequenceEqual(other.Costs);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Billing) obj);
        }

        public Billing Clone()
        {
            var house = House.Clone();
            var flatMapper = new Dictionary<Flat, Flat>(new ObjectReferenceEqualityComparer<Flat>());

            for (var i = 0; i < House.Flats.Count; ++i)
            {
                flatMapper.Add(House.Flats[i], house.Flats[i]);
            }

            return new Billing
            {
                StartDate = StartDate,
                EndDate = EndDate,
                Landlord = Landlord.Clone(),
                House = House,
                Tenants = new List<Tenant>(Tenants.Select(tenant => tenant.Clone(flatMapper))),
                Costs = new List<Cost>(Costs.Select(cost => cost.Clone(flatMapper)))
            };
        }
    }

    public class ObjectReferenceEqualityComparer<T> : EqualityComparer<T>
        where T : class
    {
        public override bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}