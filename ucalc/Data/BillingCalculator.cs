using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UCalc.Controls;

namespace UCalc.Data
{
    public class TenantCalculationResult
    {
        public Tenant Tenant { get; }
        public IReadOnlyDictionary<Cost, CostCalculationResult> Costs { get; }
        public decimal TotalAmount { get; }
        public string Details { get; }
        public string DetailsForLandlord { get; }

        public TenantCalculationResult(Tenant tenant, IReadOnlyDictionary<Cost, CostCalculationResult> costs,
            decimal totalAmount, string details, string detailsForLandlord)
        {
            Tenant = tenant;
            Costs = costs;
            TotalAmount = totalAmount;
            Details = details;
            DetailsForLandlord = detailsForLandlord;
        }
    }

    public class CostCalculationResult
    {
        public decimal TotalAmount { get; }
        public string Details { get; }
        public string DetailsForLandlord { get; }
        public bool AffectsTenant { get; }

        public CostCalculationResult(decimal totalAmount, string details, string detailsForLandlord, bool affectsTenant)
        {
            TotalAmount = totalAmount;
            Details = details;
            DetailsForLandlord = detailsForLandlord;
            AffectsTenant = affectsTenant;
        }
    }

    public static class BillingCalculator
    {
        private class EventCause
        {
            public Tenant Tenant { get; }
            public bool Entry { get; }

            public EventCause(Tenant tenant, bool entry)
            {
                Tenant = tenant;
                Entry = entry;
            }
        }

        private class Event
        {
            public DateTime Date { get; }
            public List<EventCause> Causes { get; }

            public string Cause
            {
                get
                {
                    if (Causes.Count == 0)
                    {
                        return null;
                    }

                    return string.Join(" / ",
                        Causes.Select(cause =>
                            cause.Entry
                                ? $"Einzug von \"{cause.Tenant.Name}\""
                                : $"Auszug von \"{cause.Tenant.Name}\""));
                }
            }

            public Event(DateTime date, List<EventCause> causes)
            {
                Date = date;
                Causes = causes;
            }
        }

        private static bool AffectsTenant(Tenant tenant, Cost cost)
        {
            return cost.AffectsAll || cost.AffectedFlats.Intersect(tenant.RentedFlats).Any();
        }

        private static IEnumerable<Tenant> AffectedTenants(Billing billing, Cost cost)
        {
            if (cost.AffectsAll)
            {
                return billing.Tenants;
            }

            return billing.Tenants.Where(tenant => tenant.RentedFlats.Any(flat => cost.AffectedFlats.Contains(flat)));
        }

        private static List<Event> CalculateEvents(Billing billing, IEnumerable<Tenant> affectedTenants)
        {
            var events = new Dictionary<DateTime, Event>();

            void AddEvent(DateTime date, Tenant tenant, bool entry)
            {
                if (events.TryGetValue(date, out var @event))
                {
                    @event.Causes.Add(new EventCause(tenant, entry));
                }
                else
                {
                    events.Add(date, new Event(date, new List<EventCause> {new EventCause(tenant, entry)}));
                }
            }

            foreach (var tenant in affectedTenants)
            {
                if (tenant.EntryDate != null && tenant.EntryDate.Value.IsBetween(billing.StartDate, billing.EndDate))
                {
                    AddEvent(tenant.EntryDate.Value, tenant, true);
                }

                if (tenant.DepartureDate != null &&
                    tenant.DepartureDate.Value.IsBetween(billing.StartDate, billing.EndDate))
                {
                    AddEvent(tenant.DepartureDate.Value, tenant, false);
                }
            }

            return events.Values.OrderBy(@event => @event.Date).ToList();
        }

        private static int GetFirstAffectedEvent(DateTime date, IReadOnlyList<Event> events)
        {
            for (var i = 0; i < events.Count; ++i)
            {
                if (events[i].Date > date)
                {
                    return i;
                }
            }

            return events.Count;
        }

        private static Tuple<DateTime, DateTime> GetEffectiveCostEntryStartAndEnd(Billing billing, Tenant tenant,
            CostEntry entry, out bool coversPast, out bool coversFuture)
        {
            coversPast = false;
            coversFuture = false;

            var entryStartDate = entry.StartDate;
            var entryEndDate = entry.EndDate;

            if (entryEndDate < billing.StartDate || entryStartDate > billing.EndDate)
            {
                return null;
            }

            if (entryStartDate < billing.StartDate)
            {
                coversPast = true;
                entryStartDate = billing.StartDate;
            }

            if (entryEndDate > billing.EndDate)
            {
                coversFuture = true;
                entryEndDate = billing.EndDate;
            }

            if (tenant.EntryDate != null)
            {
                if (tenant.EntryDate > entryEndDate)
                {
                    return null;
                }

                if (tenant.EntryDate > entryStartDate)
                {
                    entryStartDate = tenant.EntryDate.Value;
                }
            }

            if (tenant.DepartureDate != null)
            {
                if (tenant.DepartureDate < entryStartDate)
                {
                    return null;
                }

                if (tenant.DepartureDate < entryEndDate)
                {
                    entryEndDate = tenant.DepartureDate.Value;
                }
            }

            return new Tuple<DateTime, DateTime>(entryStartDate, entryEndDate);
        }

        private static decimal CalculateCostPerDay(CostEntry entry, StringBuilder details)
        {
            decimal totalAmount;

            if (entry.Details.TotalAmount != 0)
            {
                var amountPerUnit = entry.Details.TotalAmount / entry.Details.UnitCount;
                details.Append("Gesamtverbrauch = ");
                details.Append(entry.Details.UnitCount.CeilUnitCountToString(Constants.InternalPrecision));
                details.Append(" m³\n");
                details.Append("Preis pro m³ = ");
                details.Append(entry.Details.TotalAmount.CeilAmountToString(Constants.InternalPrecision));
                details.Append(" € ÷ ");
                details.Append(entry.Details.UnitCount.CeilUnitCountToString(Constants.InternalPrecision));
                details.Append(" m³ ≈ ");
                details.Append(amountPerUnit.CeilAmountToString(Constants.InternalPrecision));
                details.Append(" €/m³\n");

                var t = entry.Details.UnitCount;
                details.Append("Verbrauch mit Abzügen = ");
                details.Append(entry.Details.UnitCount.CeilUnitCountToString(Constants.InternalPrecision));
                details.Append(" m³");

                foreach (var discountInUnits in entry.Details.DiscountsInUnits)
                {
                    if (discountInUnits != 0)
                    {
                        details.Append(" - ");
                        details.Append(discountInUnits.CeilUnitCountToString(Constants.InternalPrecision));
                        details.Append(" m³");
                    }

                    t -= discountInUnits;
                }

                details.Append(" ≈ ");
                details.Append(t.CeilUnitCountToString(Constants.InternalPrecision));
                details.Append(" m³\n");

                totalAmount = t * amountPerUnit;
                details.Append("Kosten = ");
                details.Append(t.CeilUnitCountToString(Constants.InternalPrecision));
                details.Append(" m³ · ");
                details.Append(amountPerUnit.CeilAmountToString(Constants.InternalPrecision));
                details.Append(" €/m³ ≈ ");
                details.Append(totalAmount.CeilAmountToString(Constants.InternalPrecision));
                details.Append(" €\n");
            }
            else
            {
                totalAmount = entry.Amount;
            }

            var costPerDay = totalAmount / ((entry.EndDate - entry.StartDate).Days + 1);
            details.Append("Kosten pro Tag = ");
            details.Append(totalAmount.CeilAmountToString(Constants.InternalPrecision));
            details.Append(" € ÷ ");
            details.Append(((entry.EndDate - entry.StartDate).Days + 1).ToString());
            details.Append(" Tage ≈ ");
            details.Append(costPerDay.CeilAmountToString(Constants.InternalPrecision));
            details.Append(" €\n\n");

            return costPerDay;
        }

        private static IEnumerable<Flat> AffectedFlats(this Cost cost, Billing billing)
        {
            return cost.AffectsAll ? (IEnumerable<Flat>) billing.House.Flats : cost.AffectedFlats;
        }

        private static IEnumerable<Flat> AffectedFlats(this Cost cost, Billing billing, DateTime spanStartDate,
            DateTime spanEndDate)
        {
            var flats = cost.AffectedFlats(billing);

            if (cost.ShiftUnrented)
            {
                flats = flats.Where(flat => flat.IsRented(billing, spanStartDate, spanEndDate));
            }

            return flats;
        }

        private static bool IsRentedBy(this Flat flat, Tenant tenant, DateTime spanStartDate, DateTime spanEndDate)
        {
            return tenant.RentedFlats.Contains(flat) &&
                   (tenant.EntryDate == null || tenant.EntryDate.Value <= spanEndDate) &&
                   (tenant.DepartureDate == null || tenant.DepartureDate.Value >= spanStartDate);
        }

        private static bool IsRented(this Flat flat, Billing billing, DateTime spanStartDate, DateTime spanEndDate)
        {
            return billing.Tenants.Any(tenant => flat.IsRentedBy(tenant, spanStartDate, spanEndDate));
        }

        private static int AffectedPersonCount(this Cost cost, Billing billing, DateTime spanStartDate,
            DateTime spanEndDate)
        {
            return cost.AffectedFlats(billing).Select(flat =>
                billing.Tenants.Select(tenant =>
                    flat.IsRentedBy(tenant, spanStartDate, spanEndDate) ? tenant.PersonCount : 0).Sum()).Sum();
        }

        private static decimal AffectedSize(this Cost cost, Billing billing, DateTime spanStartDate,
            DateTime spanEndDate)
        {
            return cost.AffectedFlats(billing, spanStartDate, spanEndDate)
                .Aggregate((decimal) 0, (sum, flat) => sum + flat.Size);
        }

        private static int AffectedFlatCount(this Cost cost, Billing billing, DateTime spanStartDate,
            DateTime spanEndDate)
        {
            return cost.AffectedFlats(billing, spanStartDate, spanEndDate).Count();
        }

        private static decimal CalculateCostEntryForTimeSpan(Billing billing, Tenant tenant, Cost cost,
            StringBuilder details, DateTime spanStartDate, DateTime spanEndDate, string eventCause, decimal costPerDay)
        {
            details.Append("Von ");
            details.Append(spanStartDate.ToString(Constants.DateFormat));
            details.Append(" bis ");
            details.Append(spanEndDate.ToString(Constants.DateFormat));

            if (!string.IsNullOrEmpty(eventCause))
            {
                details.Append(" (");
                details.Append(eventCause);
                details.Append(")");
            }

            details.Append(":\n");

            decimal amount;
            switch (cost.Division)
            {
                case CostDivision.Person:
                    var totalPersonCount = cost.AffectedPersonCount(billing, spanStartDate, spanEndDate);
                    var personCount = tenant.PersonCount;
                    amount = costPerDay * ((spanEndDate - spanStartDate).Days + 1) / totalPersonCount * personCount;

                    details.Append("Zwischensumme = ");
                    details.Append(costPerDay.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" € · ");
                    details.Append(((spanEndDate - spanStartDate).Days + 1).ToString());
                    details.Append(" Tage ÷ ");
                    details.Append(totalPersonCount.ToString());
                    details.Append(" Personen · ");
                    details.Append(personCount.ToString());
                    details.Append(" Personen ≈ ");
                    details.Append(amount.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" €\n\n");

                    return amount;
                case CostDivision.Flat:
                    var totalFlatCount = cost.AffectedFlatCount(billing, spanStartDate, spanEndDate);
                    var flatCount = tenant.RentedFlats.Count;
                    amount = costPerDay * ((spanEndDate - spanStartDate).Days + 1) / totalFlatCount * flatCount;

                    details.Append("Zwischensumme = ");
                    details.Append(costPerDay.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" € · ");
                    details.Append(((spanEndDate - spanStartDate).Days + 1).ToString());
                    details.Append(" Tage ÷ ");
                    details.Append(totalFlatCount.ToString());
                    if (cost.ShiftUnrented)
                    {
                        details.Append(" bewohnte");
                    }

                    details.Append(" Wohnungen · ");
                    details.Append(flatCount.ToString());
                    details.Append(" Wohnungen ≈ ");
                    details.Append(amount.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" €\n\n");

                    return amount;
                case CostDivision.Size:
                    var totalSize = cost.AffectedSize(billing, spanStartDate, spanEndDate);
                    var size = tenant.RentedFlats.Aggregate((decimal) 0, (sum, flat) => sum + flat.Size);

                    amount = costPerDay * ((spanEndDate - spanStartDate).Days + 1) / totalSize * size;
                    details.Append("Zwischensumme = ");
                    details.Append(costPerDay.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" € · ");
                    details.Append(((spanEndDate - spanStartDate).Days + 1).ToString());
                    details.Append(" Tage ÷ ");
                    details.Append(totalSize.CeilUnitCountToString(Constants.InternalPrecision));
                    details.Append(" m² · ");
                    details.Append(size.CeilUnitCountToString(Constants.InternalPrecision));
                    details.Append(" m² ≈ ");
                    details.Append(amount.CeilAmountToString(Constants.InternalPrecision));
                    details.Append(" €\n\n");

                    return amount;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static decimal CalculateCostEntry(Billing billing, Tenant tenant, Cost cost, CostEntry entry,
            IReadOnlyList<Event> events, StringBuilder details, ICollection<CostEntry> pastCoveringEntries,
            ICollection<CostEntry> futureCoveringEntries)
        {
            var entryDates =
                GetEffectiveCostEntryStartAndEnd(billing, tenant, entry, out var coversPast, out var coversFuture);

            if (coversPast)
            {
                pastCoveringEntries.Add(entry);
            }

            if (coversFuture)
            {
                futureCoveringEntries.Add(entry);
            }

            if (entryDates == null)
            {
                return 0;
            }

            var (entryStartDate, entryEndDate) = entryDates;
            var index = GetFirstAffectedEvent(entryStartDate, events);
            var costPerDay = CalculateCostPerDay(entry, details);

            decimal totalAmount = 0;
            var prevDate = entryStartDate;

            while (index < events.Count && events[index].Date <= entryEndDate)
            {
                var newDate = events[index].Date;

                if (prevDate != newDate)
                {
                    totalAmount += CalculateCostEntryForTimeSpan(billing, tenant, cost, details, prevDate,
                        newDate, events[index].Cause, costPerDay);
                    newDate = newDate.AddDays(1);
                }

                prevDate = newDate;
                ++index;
            }

            if (prevDate <= entryEndDate)
            {
                totalAmount += CalculateCostEntryForTimeSpan(billing, tenant, cost, details, prevDate,
                    entryEndDate, null, costPerDay);
            }

            return totalAmount;
        }

        private static void CalculateCostInPast(Billing billing, Tenant tenant, Cost cost, StringBuilder details,
            IEnumerable<CostEntry> pastCoveringEntries)
        {
            details.Append("In vergangener Abrechnung bereits gezahlt (geschätzt) = ");
            decimal totalAmount = 0;
            var dummy = new StringBuilder();

            foreach (var entry in pastCoveringEntries)
            {
                var entryStartDate = entry.StartDate;
                var entryEndDate = billing.StartDate.AddDays(-1);

                if (tenant.EntryDate != null)
                {
                    if (tenant.EntryDate.Value > entryEndDate)
                    {
                        continue;
                    }

                    if (tenant.EntryDate.Value > entryStartDate)
                    {
                        entryStartDate = tenant.EntryDate.Value;
                    }
                }

                if (tenant.DepartureDate != null)
                {
                    if (tenant.DepartureDate.Value < entryStartDate)
                    {
                        continue;
                    }

                    if (tenant.DepartureDate.Value < entryEndDate)
                    {
                        entryEndDate = tenant.DepartureDate.Value;
                    }
                }

                var costPerDay = CalculateCostPerDay(entry, dummy);
                totalAmount += CalculateCostEntryForTimeSpan(billing, tenant, cost, dummy, entryStartDate,
                    entryEndDate, null, costPerDay);
            }

            details.Append(totalAmount.CeilToString());
            details.Append(" €\n");
        }

        private static void CalculateCostInFuture(Billing billing, Tenant tenant, Cost cost, StringBuilder details,
            IEnumerable<CostEntry> futureCoveringEntries)
        {
            details.Append("In nächster Abrechnung erwartet (geschätzt) = ");
            decimal totalAmount = 0;
            var dummy = new StringBuilder();

            foreach (var entry in futureCoveringEntries)
            {
                var entryStartDate = billing.EndDate.AddDays(1);
                var entryEndDate = entry.EndDate;

                if (tenant.EntryDate != null)
                {
                    if (tenant.EntryDate.Value > entryEndDate)
                    {
                        continue;
                    }

                    if (tenant.EntryDate.Value > entryStartDate)
                    {
                        entryStartDate = tenant.EntryDate.Value;
                    }
                }

                if (tenant.DepartureDate != null)
                {
                    if (tenant.DepartureDate.Value < entryStartDate)
                    {
                        continue;
                    }

                    if (tenant.DepartureDate.Value < entryEndDate)
                    {
                        entryEndDate = tenant.DepartureDate.Value;
                    }
                }

                var costPerDay = CalculateCostPerDay(entry, dummy);
                totalAmount += CalculateCostEntryForTimeSpan(billing, tenant, cost, dummy, entryStartDate,
                    entryEndDate, null, costPerDay);
            }

            details.Append(totalAmount.CeilToString());
            details.Append(" €\n");
        }

        private static CostCalculationResult CalculateCost(Billing billing, Tenant tenant, Cost cost)
        {
            if (!AffectsTenant(tenant, cost))
            {
                return new CostCalculationResult(0, "", "", false);
            }

            var details = new StringBuilder("Kostenpunkt: ");
            details.Append(cost.Name);
            details.Append("\n\n");

            var affectedTenants = AffectedTenants(billing, cost);
            var events = CalculateEvents(billing, affectedTenants);

            var pastCoveringEntries = new List<CostEntry>();
            var futureCoveringEntries = new List<CostEntry>();
            decimal totalAmount = 0;

            foreach (var entry in cost.Entries)
            {
                totalAmount += CalculateCostEntry(billing, tenant, cost, entry, events, details, pastCoveringEntries,
                    futureCoveringEntries);
            }

            totalAmount = totalAmount.Ceil2();
            details.Append("Betrag = ");
            details.Append(totalAmount.CeilToString());
            details.Append(" €\n");

            var detailsLandlord = new StringBuilder(details.ToString());
            CalculateCostInPast(billing, tenant, cost, detailsLandlord, pastCoveringEntries);
            CalculateCostInFuture(billing, tenant, cost, detailsLandlord, futureCoveringEntries);
            details.Append("\n");
            detailsLandlord.Append("\n");

            return new CostCalculationResult(totalAmount, details.ToString(), detailsLandlord.ToString(), true);
        }

        public static TenantCalculationResult CalculateForTenant(Billing billing, Tenant tenant)
        {
            var costResults = new Dictionary<Cost, CostCalculationResult>();
            var details = new StringBuilder();
            var detailsLandlord = new StringBuilder();

            decimal totalAmount = 0;

            foreach (var cost in billing.Costs)
            {
                var costResult = CalculateCost(billing, tenant, cost);
                if (!costResult.AffectsTenant)
                {
                    continue;
                }

                costResults.Add(cost, costResult);

                totalAmount += costResult.TotalAmount;

                details.Append(costResult.Details);
                details.Append("---\n\n");

                detailsLandlord.Append(costResult.DetailsForLandlord);
                detailsLandlord.Append("---\n\n");
            }


            var str = $"Zwischensumme: {totalAmount.CeilToString()} €\n";
            str += $"Bereits bezahlt: {tenant.PaidRent.CeilToString()} €\n\n";

            totalAmount -= tenant.PaidRent;
            totalAmount = totalAmount.Ceil2();

            str += $"Summe: {totalAmount.CeilToString()} €";

            details.Append(str);
            detailsLandlord.Append(str);

            return new TenantCalculationResult(tenant, costResults, totalAmount, details.ToString(),
                detailsLandlord.ToString());
        }
    }
}