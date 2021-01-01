using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using UCalc.Annotations;
using UCalc.Controls;
using UCalc.Data;
using UCalc.Models;

namespace UCalc
{
    public class TenantMenuItem : INotifyPropertyChanged
    {
        public Tenant Tenant { get; }
        public bool None { get; }
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected)
                {
                    return;
                }

                _selected = value;
                OnPropertyChanged();
            }
        }

        public TenantMenuItem(Tenant tenant, bool none)
        {
            Tenant = tenant;
            None = none;
            Selected = tenant != null;
        }

        public string Name => Tenant != null ? Tenant.Name : None ? "Kein Mieter" : "Alle Mieter";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class PrintWindow
    {
        public Billing Billing { get; }
        public IReadOnlyList<TenantMenuItem> TenantMenuItems { get; }
        private PrintableDocument _document;


        public PrintWindow(Model model)
        {
            Billing = model.Dump();
            TenantMenuItems = new[] {new TenantMenuItem(null, false), new TenantMenuItem(null, true)}.Concat(
                Billing.Tenants.Select(tenant => new TenantMenuItem(tenant, false))).ToList();

            foreach (var item in TenantMenuItems)
            {
                item.PropertyChanged += OnSelectedTenantChanged;
            }

            InitializeComponent();

            Preview();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _document?.Dispose();
        }

        private void Preview()
        {
            Document.Blocks.Clear();

            foreach (var item in TenantMenuItems)
            {
                if (!item.Selected)
                {
                    continue;
                }

                var result = BillingCalculator.CalculateForTenant(Billing, item.Tenant);
                PreviewForSingleTenant(item.Tenant, result);
            }

            _document?.Dispose();
            _document = new PrintableDocument(Document);
            _document.PreviewIn(Viewer);
        }

        private static void AddLineBreaks(TableRowGroup rowGroup, int count)
        {
            var row = new TableRow();
            rowGroup.Rows.Add(row);

            var cell = new TableCell {ColumnSpan = 2};
            row.Cells.Add(cell);
            cell.Blocks.Add(new Paragraph(new Run(new string('\n', count - 1))
                {FontSize = Constants.PrintNewlineFontSize}));
        }

        private static void AddText(TableRowGroup rowGroup, string text, double? fontSize = null,
            bool alignRight = false)
        {
            var row = new TableRow();
            rowGroup.Rows.Add(row);

            var cell = new TableCell {ColumnSpan = 2};
            row.Cells.Add(cell);

            if (alignRight)
            {
                cell.TextAlignment = TextAlignment.Right;
            }

            var paragraph = new Paragraph(new Run(text));
            cell.Blocks.Add(paragraph);

            if (fontSize != null)
            {
                paragraph.FontSize = fontSize.Value;
            }
        }

        private void PreviewForSingleTenant(Tenant tenant, TenantCalculationResult result)
        {
            var section = new Section {BreakPageBefore = true, FontSize = Constants.PrintDefaultFontSize};
            Document.Blocks.Add(section);

            var table = new Table {CellSpacing = 0};
            section.Blocks.Add(table);
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            void AddLineBreaks(int count)
            {
                PrintWindow.AddLineBreaks(rowGroup, count);
            }

            void AddText(string text, double? fontSize = null, bool alignRight = false)
            {
                PrintWindow.AddText(rowGroup, text, fontSize, alignRight);
            }

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

            AddText(
                $"{Billing.Landlord.Salutation.AsString()} {Billing.Landlord.Name}\n{DateTime.Now.ToString(Constants.DateFormat)}\n\n{Billing.Landlord.Address.Street} {Billing.Landlord.Address.HouseNumber}\n{Billing.Landlord.Address.Postcode} {Billing.Landlord.Address.City}\nTelefon: {Billing.Landlord.Phone}");

            if (!string.IsNullOrEmpty(Billing.Landlord.MailAddress))
            {
                AddText($"Email: {Billing.Landlord.MailAddress}");
            }

            AddLineBreaks(2);

            AddText(
                $"{tenant.Salutation.AsString()} {tenant.Name}\n{Billing.House.Address.Street} {Billing.House.Address.HouseNumber}\n{Billing.House.Address.Postcode} {Billing.House.Address.City}",
                null, true);

            AddLineBreaks(4);

            AddText($"{tenant.Salutation.AsString()} {tenant.Name}");

            AddLineBreaks(1);

            AddText(
                $"Nebenkostenabrechnung vom {Billing.StartDate.ToString(Constants.DateFormat)} zum {Billing.EndDate.ToString(Constants.DateFormat)}",
                Constants.PrintSubjectFontSize);

            AddLineBreaks(1);

            if (!string.IsNullOrEmpty(tenant.CustomMessage1))
            {
                AddText(tenant.CustomMessage1);

                AddLineBreaks(1);
            }

            foreach (var (cost, costResult) in result.Costs)
            {
                AddCost(cost.Name, costResult.TotalAmount);
            }

            AddCost("Zwischensumme", result.SubTotalAmount);
            AddCost("Bereits gezahlt", tenant.PaidRent);
            AddCost(result.TotalAmount > 0 ? "Einmalige Nachzahlung" : "Einmalige Rückzahlung", result.TotalAmount,
                true);

            AddLineBreaks(1);

            if (result.TotalAmount > 0)
            {
                AddText(
                    $"Bitte überweisen Sie den einmaligen Betrag von {result.TotalAmount.CeilToString()} € auf das untenstehende Konto.");
            }
            else
            {
                AddText(
                    $"Der einmalige Betrag von {result.TotalAmount.CeilToString()} € wird in den nächsten Tagen auf Ihr Konto überwiesen.");
            }

            AddLineBreaks(1);

            if (!string.IsNullOrEmpty(tenant.CustomMessage2))
            {
                AddText(tenant.CustomMessage2);
                AddLineBreaks(1);
            }

            AddText("Kontoverbindung:");
            AddText($"IBAN: {Billing.Landlord.BankAccount.Iban}");
            AddText($"BIC: {Billing.Landlord.BankAccount.Bic}");
            AddText($"Name der Bank: {Billing.Landlord.BankAccount.BankName}");

            AddLineBreaks(2);

            AddText("Mit freundlichen Grüßen");

            if (result.Costs.Any(t => t.Key.DisplayInBill))
            {
                PreviewForSingleTenantDetails(result);
            }
        }

        private void PreviewForSingleTenantDetails(TenantCalculationResult result)
        {
            var section = new Section {BreakPageBefore = true, FontSize = Constants.PrintDefaultFontSize};
            Document.Blocks.Add(section);

            var table = new Table {CellSpacing = 0};
            section.Blocks.Add(table);
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});
            table.Columns.Add(new TableColumn {Width = new GridLength(1, GridUnitType.Star)});

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            void AddLineBreaks(int count)
            {
                PrintWindow.AddLineBreaks(rowGroup, count);
            }

            void AddText(string text, double? fontSize = null, bool alignRight = false)
            {
                PrintWindow.AddText(rowGroup, text, fontSize, alignRight);
            }

            AddText("Details zur Berechnung:");

            AddLineBreaks(1);

            foreach (var (cost, costResult) in result.Costs)
            {
                if (!cost.DisplayInBill)
                {
                    continue;
                }

                AddText(costResult.Details);
            }
        }

        private void OnPrintClick(object sender, RoutedEventArgs e)
        {
            _document.Print(
                $"MietRechner Abrechnung {Billing.StartDate.ToString(Constants.DateFormat)} - {Billing.EndDate.ToString(Constants.DateFormat)}");
        }

        private void OnSelectedTenantChanged(object sender, PropertyChangedEventArgs e)
        {
            Preview();
        }

        private void OnTenantSelectorClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var contextMenu = button.ContextMenu;
            // ReSharper disable once PossibleNullReferenceException
            contextMenu.PlacementTarget = button;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void OnTenantMenuItemClick(object sender, RoutedEventArgs e)
        {
            var item = (TenantMenuItem) ((MenuItem) sender).DataContext;

            if (item.Tenant == null)
            {
                foreach (var item2 in TenantMenuItems)
                {
                    if (item2.Tenant != null)
                    {
                        item2.Selected = !item.None;
                    }
                }
            }
            else
            {
                item.Selected = !item.Selected;
            }
        }
    }
}