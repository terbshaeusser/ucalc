using Newtonsoft.Json.Linq;
using sinkien.IBAN4Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/** Class to store an address */
public class PropertyAddress
{
    /** Street of the address */
    private string street;
    /** Number of the house */
    private string houseNumber;
    /** City of the address */
    private string city;
    /** PLZ of the city or 0 */
    private int plz;
    /** Determines whether one of the values was modified */
    private bool modified;

    public PropertyAddress()
    {
        street = "";
        houseNumber = "";
        city = "";
        plz = 0;
        modified = false;
    }

    /** 
	 * Returns and sets the street of the address 
	 */
    public string Street
    {
        get
        {
            return street;
        }
        set
        {
            modified = modified || street != value;
            street = value;
        }
    }

    /** 
	 * Returns and sets the number of the house
	 */
    public string HouseNumber
    {
        get
        {
            return houseNumber;
        }
        set
        {
            modified = modified || houseNumber != value;
            houseNumber = value;
        }
    }

    /** 
	 * Returns and sets the city of the address
	 */
    public string City
    {
        get
        {
            return city;
        }
        set
        {
            modified = modified || city != value;
            city = value;
        }
    }

    /** 
	 * Returns and sets the PLZ of the city or 0
	 */
    public int PLZ
    {
        get
        {
            return plz;
        }
        set
        {
            modified = modified || plz != value;
            plz = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified;
        }
        set
        {
            modified = value;
        }
    }
}

/** Class to store a bank account */
public class BankAccount
{
    /** IBAN of the account */
    private string iban;
    /** BIC of the bank */
    private string bic;
    /** Name of the bank */
    private string bankName;
    /** Determines whether one of the values was modified */
    private bool modified;

    public BankAccount()
    {
        iban = "";
        bic = "";
        bankName = "";
        modified = false;
    }

    /** 
	 * Returns and sets the IBAN of the account 
	 */
    public string IBAN
    {
        get
        {
            return iban;
        }
        set
        {
            modified = modified || iban != value;
            iban = value;
        }
    }

    /** 
	 * Returns and sets the BIC of the bank 
	 */
    public string BIC
    {
        get
        {
            return bic;
        }
        set
        {
            modified = modified || bic != value;
            bic = value;
        }
    }

    /** 
	 * Returns and sets name of the bank 
	 */
    public string BankName
    {
        get
        {
            return bankName;
        }
        set
        {
            modified = modified || bankName != value;
            bankName = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified;
        }
        set
        {
            modified = value;
        }
    }
}

/** Enumeration of salutations */
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
                {
                    Debug.Assert(false);
                    return "";
                }
        }
    }
}

/** Class describing the owner of the property */
public class PropertyOwner
{
    /** Salutation of the owner */
    private Salutation salutation;
    /** Name of the owner */
    private string name;
    /** Phone number of the owner */
    private string phone;
    /** Mail address of the owner */
    private string mail;
    /** Address of the owner */
    private PropertyAddress address;
    /** Bank account of the owner */
    private BankAccount account;
    /** Determines whether one of the values was modified */
    private bool modified;

    public PropertyOwner()
    {
        salutation = Salutation.Sir;
        name = "";
        phone = "";
        address = new PropertyAddress();
        account = new BankAccount();
    }

    /** 
	 * Returns and sets the salutation of the owner 
	 */
    public Salutation Salutation_
    {
        get
        {
            return salutation;
        }
        set
        {
            modified = modified || salutation != value;
            salutation = value;
        }
    }

    /** 
	 * Returns and sets the name of the owner 
	 */
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            modified = modified || name != value;
            name = value;
        }
    }

    /** 
	 * Returns and sets the phone number of the owner 
	 */
    public string Phone
    {
        get
        {
            return phone;
        }
        set
        {
            modified = modified || phone != value;
            phone = value;
        }
    }

    /** 
	 * Returns and sets the mail address of the owner 
	 */
    public string Mail
    {
        get
        {
            return mail;
        }
        set
        {
            modified = modified || mail != value;
            mail = value;
        }
    }

    /** 
	 * Returns the address of the owner 
	 */
    public PropertyAddress Address { get { return address; } }

    /** 
	 * Returns the bank account of the owner 
	 */
    public BankAccount Account { get { return account; } }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified || address.Modified || account.Modified;
        }
        set
        {
            modified = value;
            address.Modified = value;
            account.Modified = value;
        }
    }
}

/** Class to describe a renter */
public class Renter
{
    /** Salutation of the renter */
    private Salutation salutation;
    /** Name of the renter */
    private string name;
    /** Number of persons */
    private int personCount;
    /** Bank account of the renter */
    private BankAccount account;
    /** Entry date or null */
    private Nullable<DateTime> entryDate;
    /** Departure date or null */
    private Nullable<DateTime> departureDate;
    /** Already paid rent */
    private Decimal paidRent;
    /** First private message */
    private string message1;
    /** Second private message */
    private string message2;
    /** Determines whether one of the values was modified */
    private bool modified;

    public Renter()
    {
        salutation = Salutation.Sir;
        name = "";
        personCount = 1;
        account = new BankAccount();
        entryDate = null;
        departureDate = null;
        paidRent = 0;
        message1 = "";
        message2 = "";
        modified = false;
    }

    /** 
	 * Returns and sets the salutation of the renter 
	 */
    public Salutation Salutation_
    {
        get
        {
            return salutation;
        }
        set
        {
            modified = modified || salutation != value;
            salutation = value;
        }
    }

    /** 
	 * Returns and sets the name of the renter 
	 */
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            modified = modified || name != value;
            name = value;
        }
    }

    /** 
	 * Returns and sets the number of persons 
	 */
    public int PersonCount
    {
        get
        {
            return personCount;
        }
        set
        {
            modified = modified || personCount != value;
            personCount = value;
        }
    }

    /** 
	 * Returns the bank account of the renter */
    public BankAccount Account
    {
        get
        {
            return account;
        }
    }

    /** 
	 * Returns and sets the entry date or null 
	 */
    public Nullable<DateTime> EntryDate
    {
        get
        {
            return entryDate;
        }
        set
        {
            modified = modified || entryDate != value;
            entryDate = value;
        }
    }

    /** 
	 * Returns and sets the departure date or null 
	 */
    public Nullable<DateTime> DepartureDate
    {
        get
        {
            return departureDate;
        }
        set
        {
            modified = modified || departureDate != value;
            departureDate = value;
        }
    }

    /** 
	 * Returns and sets the already paid rent 
	 */
    public Decimal PaidRent
    {
        get
        {
            return paidRent;
        }
        set
        {
            modified = modified || paidRent != value;
            paidRent = value;
        }
    }

    /** 
	 * Returns and sets the first private message 
	 */
    public string Message1
    {
        get
        {
            return message1;
        }
        set
        {
            modified = modified || message1 != value;
            message1 = value;
        }
    }

    /** 
	 * Returns and sets the second private message 
	 */
    public string Message2
    {
        get
        {
            return message2;
        }
        set
        {
            modified = modified || message2 != value;
            message2 = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified || account.Modified;
        }
        set
        {
            modified = value;
            account.Modified = value;
        }
    }
}

/** Enumeration of divisions of costs */
public enum CostDivision
{
    PerPerson,
    PerFlat,
    PerRenter
}

/** Class with additional information about cubic meters */
public class CubicInfo
{
    private decimal cubicMeterSum;
    private decimal discount1CubicMeter;
    private decimal discount2CubicMeter;
    private decimal discount3CubicMeter;
    private decimal discount4CubicMeter;
    private decimal cubicMeterSumPrice;
    /** Determines whether one of the values was modified */
    private bool modified;

    public CubicInfo Clone()
    {
        CubicInfo result = new CubicInfo();
        result.cubicMeterSum = cubicMeterSum;
        result.discount1CubicMeter = discount1CubicMeter;
        result.discount2CubicMeter = discount2CubicMeter;
        result.discount3CubicMeter = discount3CubicMeter;
        result.discount4CubicMeter = discount4CubicMeter;
        result.cubicMeterSumPrice = cubicMeterSumPrice;
        result.modified = modified;
        return result;
    }

    public decimal CubicMeterSum
    {
        get
        {
            return cubicMeterSum;
        }
        set
        {
            modified = modified || cubicMeterSum != value;
            cubicMeterSum = value;
        }
    }

    public decimal Discount1CubicMeter
    {
        get
        {
            return discount1CubicMeter;
        }
        set
        {
            modified = modified || discount1CubicMeter != value;
            discount1CubicMeter = value;
        }
    }

    public decimal Discount2CubicMeter
    {
        get
        {
            return discount2CubicMeter;
        }
        set
        {
            modified = modified || discount2CubicMeter != value;
            discount2CubicMeter = value;
        }
    }

    public decimal Discount3CubicMeter
    {
        get
        {
            return discount3CubicMeter;
        }
        set
        {
            modified = modified || discount3CubicMeter != value;
            discount3CubicMeter = value;
        }
    }

    public decimal Discount4CubicMeter
    {
        get
        {
            return discount4CubicMeter;
        }
        set
        {
            modified = modified || discount4CubicMeter != value;
            discount4CubicMeter = value;
        }
    }

    public decimal CubicMeterSumPrice
    {
        get
        {
            return cubicMeterSumPrice;
        }
        set
        {
            modified = modified || cubicMeterSumPrice != value;
            cubicMeterSumPrice = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified;
        }
        set
        {
            modified = value;
        }
    }
}

/** Class describing single cost entries */
public class CostEntry
{
    /** The start date */
    private DateTime start;
    /** The end date */
    private DateTime end;
    /** The price to pay */
    private decimal price;
    /** Additional information about cubic meters or null */
    private CubicInfo cubicInfo;
    /** Determines whether one of the values was modified */
    private bool modified;

    public CostEntry(DateTime start, DateTime end, decimal price, CubicInfo cubicInfo = null)
    {
        this.start = start;
        this.end = end;
        this.price = price;
        this.cubicInfo = cubicInfo;
        modified = false;
    }

    public CostEntry Clone()
    {
        CostEntry result = new CostEntry(start, end, price, cubicInfo == null ? null : cubicInfo.Clone());
        result.modified = modified;
        return result;
    }

    /** 
	 * Returns and sets the start date  
	 */
    public DateTime Start
    {
        get
        {
            return start;
        }
        set
        {
            modified = modified || start != value;
            start = value;
        }
    }

    /** 
	 * Returns and sets the end date  
	 */
    public DateTime End
    {
        get
        {
            return end;
        }
        set
        {
            modified = modified || end != value;
            end = value;
        }
    }

    /** 
	 * Returns and sets the price to pay
	 */
    public decimal Price
    {
        get
        {
            return price;
        }
        set
        {
            modified = modified || price != value;
            price = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            return modified;
        }
        set
        {
            modified = value;
        }
    }

    public CubicInfo CubicInfo
    {
        get
        {
            return cubicInfo;
        }
        set
        {
            cubicInfo = value;
        }
    }
}

/** Class describing costs */
public class Cost
{
    /** Name of the cost */
    private string name;
    /** Determines whether all renters are affected */
    private bool affectsAllRenters;
    /** List of affected renters. This is only valid if affectsAllRenters == false */
    private HashSet<Renter> affectedRenters;
    /** Divisions of cost */
    private CostDivision division;
    /** List with entries */
    private List<CostEntry> entries;
    /** Determines if the cost should be calculated on the bill in detail */
    private bool displayInBill;
    /** Determines whether one of the values was modified */
    private bool modified;

    public Cost(DateTime start, DateTime end)
    {
        name = "";
        affectsAllRenters = true;
        affectedRenters = new HashSet<Renter>();
        division = CostDivision.PerRenter;
        entries = new List<CostEntry>();
        entries.Add(new CostEntry(start, end, 0));
        modified = false;
    }

    /** 
	 * Returns and sets the name of the cost 
	 */
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            modified = modified || name != value;
            name = value;
        }
    }

    /** 
	 * Determines whether all renters are affected 
	 */
    public bool AffectsAllRenters
    {
        get
        {
            return affectsAllRenters;
        }
        set
        {
            modified = modified || affectsAllRenters != value;
            affectsAllRenters = value;
        }
    }

    /** 
	 * Returns the list of affected renters. This is only valid if affectsAllRenters
	 * == false 
	 */
    public HashSet<Renter> AffectedRenters
    {
        get
        {
            return affectedRenters;
        }
    }

    /** 
	 * Returns and sets the divisions of cost 
	 */
    public CostDivision Division
    {
        get
        {
            return division;
        }
        set
        {
            modified = modified || division != value;
            division = value;
        }
    }

    /** 
	 * Returns the list with entries
	 */
    public List<CostEntry> Entries
    {
        get
        {
            return entries;
        }
    }

    public bool DisplayInBill
    {
        get
        {
            return displayInBill;
        }
        set
        {
            modified = modified || displayInBill != value;
            displayInBill = value;
        }
    }

    /**
	 * Determines whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            if (modified)
                return true;

            foreach (CostEntry entry in entries)
            {
                if (entry.Modified)
                    return true;
            }

            return false;
        }
        set
        {
            modified = value;
            foreach (CostEntry entry in entries)
            {
                entry.Modified = value;
            }
        }
    }
}

/** Class describing a billing */
public class Billing
{
    /** Number of numbers behind , for printing */
    public const int PrintPrecision = 2;
    /** Number of numbers behind , for internal display */
    public const int InternalPrecision = 4;
    /** Number of numbers behind , for m³ */
    public const int CubicMeterPrecision = 4;

    /** Start date of the billing */
    private DateTime startDate;
    /** End date of the billing */
    private DateTime endDate;
    /** The owner of the property */
    private PropertyOwner owner;
    /** The address of the property */
    private PropertyAddress address;
    /** Number of flats in the property */
    private int flatCount;
    /** List with renters */
    private List<Renter> renters;
    /** List with costs */
    private List<Cost> costs;
    /** Determines whether one of the values was modified */
    private bool modified;

    public Billing() : this(DateTime.Now, DateTime.Now)
    {
        // Do nothing
    }

    public Billing(DateTime startDate, DateTime endDate)
    {
        this.startDate = startDate;
        this.endDate = endDate;
        owner = new PropertyOwner();
        address = new PropertyAddress();
        flatCount = 1;
        renters = new List<Renter>();
        costs = new List<Cost>();
        modified = false;
    }

    /** 
	 * Returns and sets the start date of the billing 
	 */
    public DateTime StartDate
    {
        get
        {
            return startDate;
        }
        set
        {
            modified = modified || startDate != value;
            startDate = value;
        }
    }

    /**
	 * Returns and sets the end date of the billing 
	 */
    public DateTime EndDate
    {
        get
        {
            return endDate;
        }
        set
        {
            modified = modified || endDate != value;
            endDate = value;
        }
    }

    /**
	 * Returns the owner of the property 
	 */
    public PropertyOwner Owner
    {
        get
        {
            return owner;
        }
    }

    /**
	 * Returns the address of the property 
	 */
    public PropertyAddress Address
    {
        get
        {
            return address;
        }
    }

    /**
	 * Returns and sets the number of flats in the property
	 */
    public int FlatCount
    {
        get
        {
            return flatCount;
        }
        set
        {
            modified = modified || flatCount != value;
            flatCount = value;
        }
    }

    /**
	 * Returns the list with renters 
	 */
    public List<Renter> Renters
    {
        get
        {
            return renters;
        }
    }

    /**
	 * Returns the list with costs 
	 */
    public List<Cost> Costs
    {
        get
        {
            return costs;
        }
    }

    private void LoadPropertyAddress(JObject addrObj, PropertyAddress address)
    {
        address.Street = (string)addrObj["street"];
        address.HouseNumber = (string)addrObj["house_number"];
        address.City = (string)addrObj["city"];
        address.PLZ = (int)addrObj["plz"];
    }

    private void LoadBankAccount(JObject accObj, BankAccount account)
    {
        account.IBAN = (string)accObj["iban"];
        account.BIC = (string)accObj["bic"];
        account.BankName = (string)accObj["bank_name"];
    }

    /**
	 * Loads the billing from a file
	 * @param fileName The name of the file
	 * @return false if an error occurred
	 */
    public bool LoadFromFile(string fileName)
    {
        string content = "";

        try
        {
            content = File.ReadAllText(fileName);
            JObject rootObj = JObject.Parse(content);
            startDate = DateTime.Parse((string)rootObj["start_date"]);
            endDate = DateTime.Parse((string)rootObj["end_date"]);

            JObject ownerObj = (JObject)rootObj["owner"];
            owner.Salutation_ = (Salutation)(int)ownerObj["salutation"];
            owner.Name = (string)ownerObj["name"];
            owner.Phone = (string)ownerObj["phone"];
            owner.Mail = (string)ownerObj["mail"];
            LoadPropertyAddress((JObject)ownerObj["address"], owner.Address);
            LoadBankAccount((JObject)ownerObj["account"], owner.Account);

            JObject houseObj = (JObject)rootObj["house"];
            LoadPropertyAddress((JObject)houseObj["address"], address);
            flatCount = (int)houseObj["flat_count"];

            if (rootObj["renters"].Type != JTokenType.Null)
            {
                JArray renterArr = (JArray)rootObj["renters"];

                foreach (JObject renterObj in renterArr)
                {
                    Renter renter = new Renter();
                    renters.Add(renter);
                    renter.Salutation_ = (Salutation)(int)renterObj["salutation"];
                    renter.Name = (string)renterObj["name"];
                    renter.PersonCount = (int)renterObj["person_count"];
                    LoadBankAccount((JObject)renterObj["account"], renter.Account);
                    if (renterObj["entry_date"].Type != JTokenType.Null)
                        renter.EntryDate = DateTime.Parse((string)renterObj["entry_date"]);
                    if (renterObj["departure_date"].Type != JTokenType.Null)
                        renter.DepartureDate = DateTime.Parse((string)renterObj["departure_date"]);
                    renter.PaidRent = decimal.Parse((string)renterObj["paid_rent"]);
                    renter.Message1 = (string)renterObj["message1"];
                    renter.Message2 = (string)renterObj["message2"];
                }
            }

            if (rootObj["costs"].Type != JTokenType.Null)
            {
                JArray costArr = (JArray)rootObj["costs"];

                foreach (JObject costObj in costArr)
                {
                    Cost cost = new Cost(DateTime.Now, DateTime.Now);
                    costs.Add(cost);
                    cost.Name = (string)costObj["name"];
                    cost.AffectsAllRenters = (bool)costObj["affects_all_renters"];
                    if (!cost.AffectsAllRenters)
                    {
                        JArray affectedArr = (JArray)costObj["affected_renters"];

                        foreach (JObject affectedObj in affectedArr)
                        {
                            cost.AffectedRenters.Add(renters[(int)affectedObj["id"]]);
                        }
                    }
                    cost.Division = (CostDivision)(int)costObj["division"];
                    // We need to be compatible to the previous format
                    if (costObj.TryGetValue("display", out JToken token))
                        cost.DisplayInBill = (bool)token;

                    JArray entryArr = (JArray)costObj["entries"];
                    cost.Entries.Clear();
                    foreach (JObject entryObj in entryArr)
                    {
                        CubicInfo cubicInfo = null;
                        if (entryObj.TryGetValue("cubic.sum", out token))
                        {
                            cubicInfo = new CubicInfo();
                            cubicInfo.CubicMeterSum = decimal.Parse((string)entryObj["cubic.sum"]);
                            cubicInfo.Discount1CubicMeter = decimal.Parse((string)entryObj["cubic.discount1"]);
                            cubicInfo.Discount2CubicMeter = decimal.Parse((string)entryObj["cubic.discount2"]);
                            cubicInfo.Discount3CubicMeter = decimal.Parse((string)entryObj["cubic.discount3"]);
                            cubicInfo.Discount4CubicMeter = decimal.Parse((string)entryObj["cubic.discount4"]);

                            // Recalculation for backwards compatibility
                            if (entryObj.TryGetValue("cubic.price", out token))
                                cubicInfo.CubicMeterSumPrice = cubicInfo.CubicMeterSum * decimal.Parse((string)entryObj["cubic.price"]);
                            else
                                cubicInfo.CubicMeterSumPrice = decimal.Parse((string)entryObj["cubic.sumprice"]);
                        }

                        cost.Entries.Add(new CostEntry(DateTime.Parse((string)entryObj["start_date"]), DateTime.Parse((string)entryObj["end_date"]), decimal.Parse((string)entryObj["price"]), cubicInfo));
                    }
                }
            }

            modified = false;
            owner.Modified = false;
            address.Modified = false;
            foreach (Renter renter in renters)
            {
                renter.Modified = false;
            }
            foreach (Cost cost in costs)
            {
                cost.Modified = false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private JObject SavePropertyAddress(PropertyAddress address)
    {
        JObject addrObj = new JObject();
        addrObj.Add("street", address.Street);
        addrObj.Add("house_number", address.HouseNumber);
        addrObj.Add("city", address.City);
        addrObj.Add("plz", address.PLZ);
        return addrObj;
    }

    private JObject SaveBankAccount(BankAccount account)
    {
        JObject accObj = new JObject();
        accObj.Add("iban", account.IBAN);
        accObj.Add("bic", account.BIC);
        accObj.Add("bank_name", account.BankName);
        return accObj;
    }

    /**
	 * Stores the billing in a file and removes the modified flag
	 * @param fileName The name of the file
	 * @return false if an error occurred
	 */
    public bool SaveToFile(string fileName)
    {
        JObject rootObj = new JObject();
        rootObj.Add("start_date", startDate.ToShortDateString());
        rootObj.Add("end_date", endDate.ToShortDateString());

        JObject ownerObj = new JObject();
        ownerObj.Add("salutation", (int)owner.Salutation_);
        ownerObj.Add("name", owner.Name);
        ownerObj.Add("phone", owner.Phone);
        ownerObj.Add("mail", owner.Mail);
        ownerObj.Add("address", SavePropertyAddress(owner.Address));
        ownerObj.Add("account", SaveBankAccount(owner.Account));
        rootObj.Add("owner", ownerObj);

        JObject houseObj = new JObject();
        houseObj.Add("address", SavePropertyAddress(address));
        houseObj.Add("flat_count", flatCount);
        rootObj.Add("house", houseObj);

        JArray renterArr = new JArray();
        Dictionary<Renter, int> renterToId = new Dictionary<Renter, int>();
        for (int i = 0; i < renters.Count; ++i)
        {
            Renter renter = renters[i];
            renterToId.Add(renter, i);

            JObject renterObj = new JObject();
            renterObj.Add("salutation", (int)renter.Salutation_);
            renterObj.Add("name", renter.Name);
            renterObj.Add("person_count", renter.PersonCount);
            renterObj.Add("account", SaveBankAccount(renter.Account));
            if (renter.EntryDate == null)
                renterObj.Add("entry_date", null);
            else
                renterObj.Add("entry_date", ((DateTime)renter.EntryDate).ToShortDateString());
            if (renter.DepartureDate == null)
                renterObj.Add("departure_date", null);
            else
                renterObj.Add("departure_date", ((DateTime)renter.DepartureDate).ToShortDateString());
            renterObj.Add("paid_rent", renter.PaidRent.ToString());
            renterObj.Add("message1", renter.Message1);
            renterObj.Add("message2", renter.Message2);

            renterArr.Add(renterObj);
        }
        rootObj.Add("renters", renterArr);

        JArray costArr = new JArray();
        foreach (Cost cost in costs)
        {
            JObject costObj = new JObject();
            costObj.Add("name", cost.Name);
            costObj.Add("affects_all_renters", cost.AffectsAllRenters);
            if (cost.AffectsAllRenters)
                costObj.Add("affected_renters", null);
            else
            {
                JArray affectedArr = new JArray();
                foreach (Renter renter in cost.AffectedRenters)
                {
                    JObject affectedObj = new JObject();
                    int id;
                    if (renterToId.TryGetValue(renter, out id))
                        affectedObj.Add("id", id);
                    affectedArr.Add(affectedObj);
                }
                costObj.Add("affected_renters", affectedArr);
            }
            costObj.Add("division", (int)cost.Division);
            costObj.Add("display", cost.DisplayInBill);

            JArray entryArr = new JArray();
            foreach (CostEntry entry in cost.Entries)
            {
                JObject entryObj = new JObject();
                entryObj.Add("start_date", entry.Start.ToShortDateString());
                entryObj.Add("end_date", entry.End.ToShortDateString());
                entryObj.Add("price", entry.Price.ToString());
                if (entry.CubicInfo != null)
                {
                    entryObj.Add("cubic.sum", entry.CubicInfo.CubicMeterSum.ToString());
                    entryObj.Add("cubic.discount1", entry.CubicInfo.Discount1CubicMeter.ToString());
                    entryObj.Add("cubic.discount2", entry.CubicInfo.Discount2CubicMeter.ToString());
                    entryObj.Add("cubic.discount3", entry.CubicInfo.Discount3CubicMeter.ToString());
                    entryObj.Add("cubic.discount4", entry.CubicInfo.Discount4CubicMeter.ToString());
                    entryObj.Add("cubic.sumprice", entry.CubicInfo.CubicMeterSumPrice.ToString());
                }
                entryArr.Add(entryObj);
            }
            costObj.Add("entries", entryArr);

            costArr.Add(costObj);
        }
        rootObj.Add("costs", costArr);

        try
        {
            File.WriteAllText(fileName, rootObj.ToString());
            modified = false;
            owner.Modified = false;
            address.Modified = false;
            foreach (Renter renter in renters)
            {
                renter.Modified = false;
            }
            foreach (Cost cost in costs)
            {
                cost.Modified = false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /**
	 * Counts the number of affected persons between two dates
	 */
    private int CountAffectedPersonsInTimeSpan(List<Renter> affectedRenters, DateTime start, DateTime end)
    {
        int count = 0;

        foreach (Renter renter in affectedRenters)
        {
            if (renter.EntryDate != null && renter.EntryDate.Value > end)
                continue;
            if (renter.DepartureDate != null && renter.DepartureDate.Value < start)
                continue;

            count += renter.PersonCount;
        }

        return count;
    }

    /**
	 * Counts the number of affected renters between two dates
	 */
    private int CountAffectedRentersInTimeSpan(List<Renter> affectedRenters, DateTime start, DateTime end)
    {
        int count = 0;

        foreach (Renter renter in affectedRenters)
        {
            if (renter.EntryDate != null && renter.EntryDate.Value > end)
                continue;
            if (renter.DepartureDate != null && renter.DepartureDate.Value < start)
                continue;

            ++count;
        }

        return count;
    }

    public static decimal Ceil(decimal d, int decimals)
    {
        Debug.Assert(decimals >= 0);

        decimal factor = 1;
        for (int i = 0; i < decimals; ++i)
            factor *= 10;

        return decimal.Ceiling(d * factor) / factor;
    }

    public static string CeilToString(decimal d, int decimals)
    {
        Debug.Assert(decimals >= 0);

        string format = "0";
        if (decimals != 0)
        {
            if (decimals > 2)
                format += ".00" + new string('#', decimals - 2);
            else
                format += "." + new string('0', decimals);
        }

        return Ceil(d, decimals).ToString(format);
    }

    /**
	 * Calculates the cost of a cost in a certain date range in days
	 * @param start The start date
	 * @param end The end date
	 * @param hint A hint why this range was chosen
	 * @param costPerDay The cost per day
	 * @param personCount Number of persons of the renter
	 * @param affectedPersonCount Number of affected persons
	 * @param affectedRenterCount Number of affected renters
	 * @param division The cost division
	 * @param msg Message where the description should be added
     * @param details Message where the details should be added
	 * @return The calculated cost
	 */
    private decimal CalculateCostForRenterCostDateRange(DateTime start, DateTime end, string hint, decimal costPerDay, int personCount, int affectedPersonCount, int affectedRenterCount, CostDivision division, ref string msg, out string details)
    {
        string str = "Von " + start.ToShortDateString() + " bis " + end.ToShortDateString();
        if (hint != "")
            str += " (" + hint + ")";
        str += ":\n";
        msg += str;
        details = str;

        switch (division)
        {
            case CostDivision.PerPerson:
                {
                    decimal result = costPerDay * ((end - start).Days + 1) / affectedPersonCount * personCount;
                    str = "Zwischensumme = " + CeilToString(costPerDay, PrintPrecision) + " € * " + ((end - start).Days + 1).ToString() + " Tage / " + affectedPersonCount.ToString() + " Personen * " + personCount.ToString() + " Personen ≈ " + CeilToString(result, PrintPrecision) + " €\n";
                    msg += str;
                    details += str;
                    return result;
                }
            case CostDivision.PerFlat:
                {
                    decimal result = costPerDay * ((end - start).Days + 1) / flatCount;
                    str = "Zwischensumme = " + CeilToString(costPerDay, PrintPrecision) + " € * " + ((end - start).Days + 1).ToString() + " Tage / " + flatCount.ToString() + " Wohnungen ≈ " + CeilToString(result, PrintPrecision) + " €\n";
                    msg += str;
                    details += str;
                    return result;
                }
            case CostDivision.PerRenter:
                {
                    decimal result = costPerDay * ((end - start).Days + 1) / affectedRenterCount;
                    str = "Zwischensumme = " + CeilToString(costPerDay, PrintPrecision) + " € * " + ((end - start).Days + 1).ToString() + " Tage / " + affectedRenterCount.ToString() + " belegte Wohnungen ≈ " + CeilToString(result, PrintPrecision) + " €\n";
                    msg += str;
                    details += str;
                    return result;
                }
            default:
                throw new NotImplementedException();
        }
    }

    private decimal CalculateCostForRenter(Renter renter, Cost cost, out bool affectsThisRenter, out string msg, out string details)
    {
        decimal result = 0;
        msg = "";
        details = "";
        affectsThisRenter = cost.AffectsAllRenters || cost.AffectedRenters.Contains(renter);
        if (!affectsThisRenter)
            return 0;

        msg = "Kostenpunkt: " + cost.Name + "\n";
        if (cost.DisplayInBill)
            details = cost.Name + ":\n";

        // Create list of affected renters
        List<Renter> affectedRenters = new List<Renter>();
        if (cost.AffectsAllRenters)
            affectedRenters.AddRange(renters);
        else
            affectedRenters.AddRange(cost.AffectedRenters);

        List<KeyValuePair<DateTime, string>> dates = new List<KeyValuePair<DateTime, string>>();
        foreach (Renter affectedRenter in affectedRenters)
        {
            if (affectedRenter.EntryDate != null)
                dates.Add(new KeyValuePair<DateTime, string>(affectedRenter.EntryDate.Value.AddDays(-1), "Einzug von \"" + affectedRenter.Name + "\""));
            if (affectedRenter.DepartureDate != null)
                dates.Add(new KeyValuePair<DateTime, string>(affectedRenter.DepartureDate.Value, "Auszug von \"" + affectedRenter.Name + "\""));
        }
        dates.Sort(delegate (KeyValuePair<DateTime, string> a, KeyValuePair<DateTime, string> b)
        {
            return a.Key.CompareTo(b.Key);
        });

        // Combine duplicate entries
        int i = 1;
        while (i < dates.Count)
        {
            if (dates[i - 1].Key == dates[i].Key)
            {
                dates[i - 1] = new KeyValuePair<DateTime, string>(dates[i - 1].Key, dates[i - 1].Value + " / " + dates[i].Value);
                dates.RemoveAt(i);
            }
            else
                ++i;
        }

        string str;
        List<CostEntry> pastCoveringEntries = new List<CostEntry>();
        List<CostEntry> futureCoveringEntries = new List<CostEntry>();
        foreach (CostEntry entry in cost.Entries)
        {
            DateTime entryStartDate = entry.Start;
            DateTime entryEndDate = entry.End;
            if (entryEndDate < startDate || entryStartDate > endDate)
                continue;
            if (entryStartDate < startDate)
            {
                pastCoveringEntries.Add(entry);
                entryStartDate = startDate;
            }
            if (entryEndDate > endDate)
            {
                futureCoveringEntries.Add(entry);
                entryEndDate = endDate;
            }

            if (renter.EntryDate != null)
            {
                if (renter.EntryDate > entryEndDate)
                    continue;
                if (renter.EntryDate > entryStartDate)
                    entryStartDate = (DateTime)renter.EntryDate;
            }
            if (renter.DepartureDate != null)
            {
                if (renter.DepartureDate < entryStartDate)
                    continue;
                if (renter.DepartureDate < entryEndDate)
                    entryEndDate = (DateTime)renter.DepartureDate;
            }

            // Jump over dates not affecting this entry
            i = 0;
            while (i < dates.Count && dates[i].Key <= entryStartDate)
            {
                ++i;
            }

            // Calculate the costs per day
            decimal price;
            if (entry.CubicInfo != null)
            {
                decimal perCubicMeter = entry.CubicInfo.CubicMeterSumPrice / entry.CubicInfo.CubicMeterSum;
                str = "Gesamtverbrauch = " + CeilToString(entry.CubicInfo.CubicMeterSum, CubicMeterPrecision) + " m³\n";
                str += "Preis pro m³ = " + CeilToString(entry.CubicInfo.CubicMeterSumPrice, PrintPrecision) + " € / " + CeilToString(entry.CubicInfo.CubicMeterSum, CubicMeterPrecision) + " m³ ≈ " + CeilToString(perCubicMeter, CubicMeterPrecision) + " €/m³\n";

                decimal t = entry.CubicInfo.CubicMeterSum;
                str += "Verbrauch mit Abzügen = " + CeilToString(entry.CubicInfo.CubicMeterSum, CubicMeterPrecision) + " m³";
                if (entry.CubicInfo.Discount1CubicMeter != 0)
                    str += " - " + CeilToString(entry.CubicInfo.Discount1CubicMeter, CubicMeterPrecision) + " m³";
                t -= entry.CubicInfo.Discount1CubicMeter;
                if (entry.CubicInfo.Discount2CubicMeter != 0)
                    str += " - " + CeilToString(entry.CubicInfo.Discount2CubicMeter, CubicMeterPrecision) + " m³";
                t -= entry.CubicInfo.Discount2CubicMeter;
                if (entry.CubicInfo.Discount3CubicMeter != 0)
                    str += " - " + CeilToString(entry.CubicInfo.Discount3CubicMeter, CubicMeterPrecision) + " m³";
                t -= entry.CubicInfo.Discount3CubicMeter;
                if (entry.CubicInfo.Discount4CubicMeter != 0)
                    str += " - " + CeilToString(entry.CubicInfo.Discount4CubicMeter, CubicMeterPrecision) + " m³";
                t -= entry.CubicInfo.Discount4CubicMeter;
                str += " ≈ " + CeilToString(t, CubicMeterPrecision) + " m³\n";

                price = t * perCubicMeter;
                str += "Kosten = " + CeilToString(t, CubicMeterPrecision) + " m³ * " + CeilToString(perCubicMeter, CubicMeterPrecision) + " €/m³ ≈ " + CeilToString(price, PrintPrecision) + " €\n";
                msg += str;
                if (cost.DisplayInBill)
                    details += str;
            }
            else
                price = entry.Price;
            decimal costPerDay = price / ((entry.End - entry.Start).Days + 1);
            str = "Kosten pro Tag = " + CeilToString(price, PrintPrecision) + " € / " + ((entry.End - entry.Start).Days + 1).ToString() + " Tage ≈ " + CeilToString(costPerDay, PrintPrecision) + " €\n";
            msg += str;
            if (cost.DisplayInBill)
                details += str;

            DateTime prevDate = entryStartDate;

            int affectedPersonCount;
            int affectedRenterCount;

            while (i < dates.Count && dates[i].Key <= entryEndDate)
            {
                DateTime newDate = dates[i].Key;

                if (prevDate != newDate)
                {
                    affectedPersonCount = CountAffectedPersonsInTimeSpan(affectedRenters, prevDate, newDate);
                    affectedRenterCount = CountAffectedRentersInTimeSpan(affectedRenters, prevDate, newDate);
                    result += CalculateCostForRenterCostDateRange(prevDate, newDate, dates[i].Value, costPerDay, renter.PersonCount, affectedPersonCount, affectedRenterCount, cost.Division, ref msg, out str);
                    if (cost.DisplayInBill)
                        details += str;
                    newDate = newDate.AddDays(1);
                }

                prevDate = newDate;
                ++i;
            }

            // Calculate the remaining time span
            if (prevDate <= entryEndDate)
            {
                affectedPersonCount = CountAffectedPersonsInTimeSpan(affectedRenters, prevDate, entryEndDate);
                affectedRenterCount = CountAffectedRentersInTimeSpan(affectedRenters, prevDate, entryEndDate);
                result += CalculateCostForRenterCostDateRange(prevDate, entryEndDate, "", costPerDay, renter.PersonCount, affectedPersonCount, affectedRenterCount, cost.Division, ref msg, out str);
                if (cost.DisplayInBill)
                    details += str;
            }
        }

        result = Ceil(result, 2);
        str = "Betrag = " + CeilToString(result, PrintPrecision) + " €";
        msg += str;
        if (cost.DisplayInBill)
            details += str + "\n";

        // Calculate additonal prices that exist because of entries covering the past or future
        msg += "\nIn vergangener Abrechnung bereits gezahlt = ";
        decimal alreadyPaid = 0;
        foreach (CostEntry entry in pastCoveringEntries)
        {
            DateTime entryStartDate = entry.Start;
            DateTime entryEndDate = startDate.AddDays(-1);

            if (renter.EntryDate != null)
            {
                if (renter.EntryDate > entryEndDate)
                    continue;
                if (renter.EntryDate > entryStartDate)
                    entryStartDate = renter.EntryDate.Value;
            }
            if (renter.DepartureDate != null)
            {
                if (renter.DepartureDate < entryStartDate)
                    continue;
                if (renter.DepartureDate < entryEndDate)
                    entryEndDate = renter.DepartureDate.Value;
            }

            // Calculate the costs per day
            decimal price;
            if (entry.CubicInfo != null)
            {
                decimal perCubicMeter = entry.CubicInfo.CubicMeterSumPrice / entry.CubicInfo.CubicMeterSum;
                decimal t = entry.CubicInfo.CubicMeterSum;
                t -= entry.CubicInfo.Discount1CubicMeter;
                t -= entry.CubicInfo.Discount2CubicMeter;
                t -= entry.CubicInfo.Discount3CubicMeter;
                t -= entry.CubicInfo.Discount4CubicMeter;
                price = t * perCubicMeter;
            }
            else
                price = entry.Price;
            decimal costPerDay = price / ((entry.End - entry.Start).Days + 1);

            int affectedPersonCount = CountAffectedPersonsInTimeSpan(affectedRenters, entryStartDate, entryEndDate);
            int affectedRenterCount = CountAffectedRentersInTimeSpan(affectedRenters, entryStartDate, entryEndDate);
            alreadyPaid += CalculateCostForRenterCostDateRange(entryStartDate, entryEndDate, "", costPerDay, renter.PersonCount, affectedPersonCount, affectedRenterCount, cost.Division, ref str, out str);
        }
        msg += CeilToString(alreadyPaid, PrintPrecision) + " €";

        msg += "\nIn nächster Abrechnung erwartet = ";
        decimal expectedPayment = 0;
        foreach (CostEntry entry in futureCoveringEntries)
        {
            DateTime entryStartDate = endDate.AddDays(1);
            DateTime entryEndDate = entry.End;

            if (renter.EntryDate != null)
            {
                if (renter.EntryDate > entryEndDate)
                    continue;
                if (renter.EntryDate > entryStartDate)
                    entryStartDate = renter.EntryDate.Value;
            }
            if (renter.DepartureDate != null)
            {
                if (renter.DepartureDate < entryStartDate)
                    continue;
                if (renter.DepartureDate < entryEndDate)
                    entryEndDate = renter.DepartureDate.Value;
            }

            // Calculate the costs per day
            decimal price;
            if (entry.CubicInfo != null)
            {
                decimal perCubicMeter = entry.CubicInfo.CubicMeterSumPrice / entry.CubicInfo.CubicMeterSum;
                decimal t = entry.CubicInfo.CubicMeterSum;
                t -= entry.CubicInfo.Discount1CubicMeter;
                t -= entry.CubicInfo.Discount2CubicMeter;
                t -= entry.CubicInfo.Discount3CubicMeter;
                t -= entry.CubicInfo.Discount4CubicMeter;
                price = t * perCubicMeter;
            }
            else
                price = entry.Price;
            decimal costPerDay = price / ((entry.End - entry.Start).Days + 1);

            int affectedPersonCount = CountAffectedPersonsInTimeSpan(affectedRenters, entryStartDate, entryEndDate);
            int affectedRenterCount = CountAffectedRentersInTimeSpan(affectedRenters, entryStartDate, entryEndDate);
            expectedPayment += CalculateCostForRenterCostDateRange(entryStartDate, entryEndDate, "", costPerDay, renter.PersonCount, affectedPersonCount, affectedRenterCount, cost.Division, ref str, out str);
        }
        msg += CeilToString(expectedPayment, PrintPrecision) + " €";

        return result;
    }

    /**
	 * Calculates the bill for a specified renter
	 * @param renter The renter
	 * @param costOverview List where cost entries should be added
     * @param details The calculation details
	 * @return The calculated sum. If negative a refund is necessary
	 */
    public decimal CalculateForRenter(Renter renter, List<KeyValuePair<string, decimal>> costOverview, out string details)
    {
        decimal result = 0;
        details = "";

        foreach (Cost cost in costs)
        {
            bool affectsThisRenter;
            string dummy;
            decimal value = CalculateCostForRenter(renter, cost, out affectsThisRenter, out dummy, out string tempDetails);
            result += value;
            if (tempDetails != "")
            {
                if (details != "")
                    details += "\n";
                details += tempDetails;
            }

            if (affectsThisRenter)
                costOverview.Add(new KeyValuePair<string, decimal>(cost.Name, value));
        }

        return result - renter.PaidRent;
    }

    /**
	 * Calculates the bill for a specified renter and returns a human readable form
	 * @param renter The renter
	 * @return The calculated formulas
	 */
    public string CalculateForRenterEx(Renter renter)
    {
        // Check if there are errors
        foreach (string msg in Analyze())
        {
            if (msg[0] == 'E')
                return "Berechnung kann nicht vorgenommen werden, da Fehler gefunden wurden.";
        }

        string result = "";
        decimal sum = 0;
        foreach (Cost cost in costs)
        {
            bool affectsThisRenter;
            string str;
            sum += CalculateCostForRenter(renter, cost, out affectsThisRenter, out str, out string details);

            if (str != "")
                result += str + "\n\n";
        }

        result += "Bereits bezahlt: " + CeilToString(renter.PaidRent, 2) + " €\n\n";
        sum -= renter.PaidRent;

        result += "Summe: " + CeilToString(sum, 2) + " €";

        return result;
    }

    /**
	 * Determines and sets whether the value was modified
	 */
    public bool Modified
    {
        get
        {
            if (modified || owner.Modified || address.Modified)
                return true;

            foreach (Renter renter in renters)
            {
                if (renter.Modified)
                    return true;
            }

            foreach (Cost cost in costs)
            {
                if (cost.Modified)
                    return true;
            }

            return false;
        }
    }

    /**
	 * Enforces that the data was modified
	 */
    public void ForceModified()
    {
        modified = true;
    }

    /**
	 * Analyzes the billing for warnings or errors
	 * @return List with strings describing warnings and errors. If an entry holds a
	 * warning it starts with a 'W'. If an entry holds an error it starts with a 'E'
	 */
    public List<string> Analyze()
    {
        List<string> result = new List<string>();

        if (owner.Name == "")
            result.Add("EDer Name des Eigentümers ist ungültig.");
        if (owner.Phone == "")
            result.Add("EDie Telefonnummer des Eigentümers ist ungültig.");
        if (owner.Address.Street == "")
            result.Add("EDie Straße der Adresse des Eigentümers ist ungültig.");
        if (owner.Address.HouseNumber == "")
            result.Add("EDie Hausnummer der Adresse des Eigentümers ist ungültig.");
        if (owner.Address.City == "")
            result.Add("EDie Stadt der Adresse des Eigentümers ist ungültig.");
        if (owner.Address.PLZ == 0)
            result.Add("EDie PLZ der Adresse des Eigentümers ist ungültig.");
        try
        {
            IbanUtils.Validate(owner.Account.IBAN);
        }
        catch (IbanFormatException)
        {
            result.Add("EDie IBAN des Eigentümers ist ungültig.");
        }
        try
        {
            BicUtils.ValidateBIC(owner.Account.BIC);
        }
        catch (BicFormatException)
        {
            result.Add("EDie BIC des Eigentümers ist ungültig.");
        }
        if (owner.Account.BankName == "")
            result.Add("EDer Name der Bank des Eigentümers ist ungültig.");
        if (flatCount < 1)
            result.Add("EDie Anzahl der Wohnungen im Haus ist ungültig.");
        if (address.Street == "")
            result.Add("EDie Straße der Adresse des Hauses ist ungültig.");
        if (address.HouseNumber == "")
            result.Add("EDie Hausnummer der Adresse des Hauses ist ungültig.");
        if (address.City == "")
            result.Add("EDie Stadt der Adresse des Hauses ist ungültig.");
        if (address.PLZ == 0)
            result.Add("EDie PLZ der Adresse des Hauses ist ungültig.");

        if (renters.Count == 0)
            result.Add("EEs sind keine Mieter vorhanden.");

        foreach (Cost cost in costs)
        {
            if (!cost.AffectsAllRenters && cost.AffectedRenters.Count == 0)
                result.Add("WDer Kostenpunkt \"" + cost.Name + "\" wird bei der Berechnung ignoriert, da kein Mieter betroffen ist.");

            int coveredDays = 0;
            foreach (CostEntry entry in cost.Entries)
            {
                if (entry.Start <= startDate && entry.End >= startDate)
                {
                    coveredDays += (entry.End - startDate).Days + 1;
                }
                else if (entry.Start <= endDate)
                {
                    DateTime tmp = endDate;
                    if (tmp > entry.End)
                        tmp = entry.End;

                    coveredDays += (tmp - entry.Start).Days + 1;
                }
            }

            if (coveredDays < (endDate - startDate).Days + 1)
                result.Add("EDie Zeiträume des Kostenpunkts \"" + cost.Name + "\" decken nicht alle Tage der Abrechnung ab.");
        }

        return result;
    }
}