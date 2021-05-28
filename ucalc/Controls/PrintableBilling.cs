using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using UCalc.Data;

namespace UCalc.Controls
{
    public class PrintableBilling : PrintableDocument<Tuple<Billing, IEnumerable<Tenant>>>
    {
        public PrintableBilling(Billing billing, IEnumerable<Tenant> tenants) : base(
            new Tuple<Billing, IEnumerable<Tenant>>(billing, tenants))
        {
        }

        protected override void FillFlowDocument(FlowDocument document, Tuple<Billing, IEnumerable<Tenant>> args)
        {
            var (billing, tenants) = args;

            foreach (var tenant in tenants)
            {
                var result = BillingCalculator.CalculateForTenant(billing, tenant);
                AddPagesForTenant(document, billing, tenant, result);
            }
        }

        private static void AddPagesForTenant(FlowDocument document, Billing billing, Tenant tenant,
            TenantCalculationResult result)
        {
            var section = new Section {BreakPageBefore = true, FontSize = Constants.PrintDefaultFontSize};
            document.Blocks.Add(section);

            var table = new Table {CellSpacing = 0};
            section.Blocks.Add(table);
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            void AddCost(string name, decimal amount, bool isLast = false)
            {
                var row2 = new TableRow();
                rowGroup.Rows.Add(row2);

                if (isLast)
                {
                    row2.Background = Brushes.LightGray;
                }

                var cell2 = new TableCell
                    {BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1, 1, 0, isLast ? 1 : 0)};
                row2.Cells.Add(cell2);

                cell2.Blocks.Add(new Paragraph(new Run(name)) {Padding = new Thickness(6)});

                cell2 = new TableCell
                {
                    BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1, 1, 1, isLast ? 1 : 0),
                    TextAlignment = TextAlignment.Right
                };
                row2.Cells.Add(cell2);

                cell2.Blocks.Add(new Paragraph(new Run($"{amount.CeilToString()} €")) {Padding = new Thickness(6)});
            }

            void AddTextLeftRight(string leftText, string rightText)
            {
                var row2 = new TableRow();
                rowGroup.Rows.Add(row2);

                var cell2 = new TableCell();
                row2.Cells.Add(cell2);

                cell2.Blocks.Add(new Paragraph(new Run(leftText)));

                cell2 = new TableCell
                {
                    TextAlignment = TextAlignment.Right
                };
                row2.Cells.Add(cell2);

                cell2.Blocks.Add(new Paragraph(new Run(rightText)));
            }

            AddTextLeftRight(
                $"{billing.Landlord.Name}\n" +
                $"{billing.Landlord.Address.Street} {billing.Landlord.Address.HouseNumber}\n" +
                $"{billing.Landlord.Address.Postcode} {billing.Landlord.Address.City}\n" +
                $"Telefon: {billing.Landlord.Phone}" +
                (string.IsNullOrEmpty(billing.Landlord.MailAddress) ? "" : $"\nEmail: {billing.Landlord.MailAddress}"),
                DateTime.Now.ToString(Constants.DateFormat)
            );

            AddLineBreaks(rowGroup, 2);

            AddText(
                rowGroup,
                $"{tenant.Salutation.AsString()} {tenant.Name}\n" +
                $"{billing.House.Address.Street} {billing.House.Address.HouseNumber}\n" +
                $"{billing.House.Address.Postcode} {billing.House.Address.City}"
            );

            AddLineBreaks(rowGroup, 4);

            AddText(rowGroup, $"{tenant.Salutation.AsString()} {tenant.Name}");

            AddLineBreaks(rowGroup, 1);

            var startDate = billing.StartDate;
            if (tenant.EntryDate.HasValue && tenant.EntryDate.Value > startDate)
            {
                startDate = tenant.EntryDate.Value;
            }

            var endDate = billing.EndDate;
            if (tenant.DepartureDate.HasValue && tenant.DepartureDate.Value < endDate)
            {
                endDate = tenant.DepartureDate.Value;
            }

            AddText(
                rowGroup,
                $"Nebenkostenabrechnung vom {startDate.ToString(Constants.DateFormat)} zum {endDate.ToString(Constants.DateFormat)}",
                Constants.PrintSubjectFontSize
            );

            AddLineBreaks(rowGroup, 1);

            if (!string.IsNullOrEmpty(tenant.CustomMessage1))
            {
                AddText(rowGroup, tenant.CustomMessage1);

                AddLineBreaks(rowGroup, 1);
            }

            foreach (var (cost, costResult) in result.Costs)
            {
                AddCost(cost.Name, costResult.TotalAmount);
            }

            AddCost("Zwischensumme", result.SubTotalAmount);
            AddCost("Bereits gezahlt", tenant.PaidRent);
            AddCost(result.TotalAmount > 0 ? "Einmalige Nachzahlung" : "Einmalige Rückzahlung", result.TotalAmount,
                true);

            AddLineBreaks(rowGroup, 1);

            if (result.TotalAmount > 0)
            {
                AddText(
                    rowGroup,
                    $"Bitte überweisen Sie den einmaligen Betrag von {result.TotalAmount.CeilToString()} € auf das untenstehende Konto."
                );
            }
            else
            {
                AddText(
                    rowGroup,
                    $"Der einmalige Betrag von {(result.TotalAmount * -1).CeilToString()} € wird in den nächsten Tagen auf Ihr Konto überwiesen."
                );
            }

            AddLineBreaks(rowGroup, 1);

            if (!string.IsNullOrEmpty(tenant.CustomMessage2))
            {
                AddText(rowGroup, tenant.CustomMessage2);
                AddLineBreaks(rowGroup, 1);
            }

            AddText(rowGroup, "Kontoverbindung:");
            AddText(rowGroup, $"IBAN: {billing.Landlord.BankAccount.Iban}");
            AddText(rowGroup, $"BIC: {billing.Landlord.BankAccount.Bic}");
            AddText(rowGroup, $"Name der Bank: {billing.Landlord.BankAccount.BankName}");

            AddLineBreaks(rowGroup, 2);

            AddText(rowGroup, "Mit freundlichen Grüßen");

            if (result.Costs.Any(t => t.Key.DisplayInBill))
            {
                AddPagesForTenantDetails(document, result);
            }
        }

        private static void AddPagesForTenantDetails(FlowDocument document, TenantCalculationResult result)
        {
            var section = new Section {BreakPageBefore = true, FontSize = Constants.PrintDefaultFontSize};
            document.Blocks.Add(section);

            var table = new Table {CellSpacing = 0};
            section.Blocks.Add(table);
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            AddText(rowGroup, "Details zur Berechnung:");

            AddLineBreaks(rowGroup, 1);

            foreach (var (cost, costResult) in result.Costs)
            {
                if (!cost.DisplayInBill)
                {
                    continue;
                }

                AddText(rowGroup, costResult.Details);
            }
        }
    }
}