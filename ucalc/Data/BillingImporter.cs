using System;
using System.Text;

namespace UCalc.Data
{
    public static class BillingImporter
    {
        private static void RemoveDepartedTenants(Billing billing, StringBuilder details)
        {
            for (var i = 0; i < billing.Tenants.Count;)
            {
                var tenant = billing.Tenants[i];

                if (tenant.DepartureDate != null && tenant.DepartureDate.Value < billing.StartDate)
                {
                    details.Append("- Mieter \"");
                    details.Append(tenant.Name);
                    details.Append("\" wurde entfernt, da er ausgezogen ist.\n");

                    billing.Tenants.RemoveAt(i);
                    continue;
                }

                if (tenant.EntryDate != null && tenant.EntryDate.Value <= billing.StartDate)
                {
                    details.Append("- Das Einzugsdatum von Mieter \"");
                    details.Append(tenant.Name);
                    details.Append("\" wurde entfernt.\n");

                    tenant.EntryDate = null;
                }

                ++i;
            }
        }

        public static Billing Import(string path, DateTime startDate, DateTime endDate, StringBuilder details)
        {
            var billing = BillingLoader.Load(path);

            billing.StartDate = startDate;
            billing.EndDate = endDate;

            RemoveDepartedTenants(billing, details);

            return billing;
        }
    }
}