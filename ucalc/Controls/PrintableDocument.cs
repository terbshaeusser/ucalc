using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using Microsoft.Win32;
using UCalc.Data;

namespace UCalc.Controls
{
    public class NoPrinterException : Exception
    {
        public NoPrinterException(string message) : base(message)
        {
        }
    }

    public class PrintableDocument : IDisposable
    {
        private static readonly Uri DocUri = new Uri($"pack://mietrechner{new Guid()}.xps");
        private readonly Package _package;
        private readonly FlowDocument _flowDocument;
        private readonly FixedDocumentSequence _previewDocument;
        private XpsDocument _fixedDocument;

        public PrintableDocument(Billing billing, IEnumerable<Tenant> tenants)
        {
            _package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
            PackageStore.AddPackage(DocUri, _package);

            _flowDocument = CreateFlowDocument(billing, tenants);
            _previewDocument = CreatePreview(_flowDocument);
        }

        public void Dispose()
        {
            ((IDisposable) _fixedDocument).Dispose();
            PackageStore.RemovePackage(DocUri);
            ((IDisposable) _package).Dispose();
        }

        private static Size? GetPrinterMediaSize()
        {
            var localPrintServer = new LocalPrintServer();

            var size = localPrintServer.GetPrintQueues()
                .Select(printQueue => printQueue.DefaultPrintTicket?.PageMediaSize)
                .FirstOrDefault(mediaSize => mediaSize is {PageMediaSizeName: PageMediaSizeName.ISOA4});
            if (size == null || !size.Width.HasValue || !size.Height.HasValue)
            {
                return null;
            }

            return new Size(size.Width!.Value, size.Height!.Value);
        }

        public void PreviewIn(DocumentViewer viewer)
        {
            viewer.Document = _previewDocument;
        }

        public void Print(string description)
        {
            var version = (int) Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
                "CurrentMajorVersionNumber", 8);

            if (version == 10)
            {
                PrintDocumentImageableArea imageableArea = null;
                var pageRangeSelection = PageRangeSelection.AllPages;
                PageRange pageRange;

                var writer = PrintQueue.CreateXpsDocumentWriter(description, ref imageableArea, ref pageRangeSelection,
                    ref pageRange);
                if (writer == null)
                {
                    return;
                }

                var clonedDocument = CloneFlowDocument(_flowDocument);
                clonedDocument.PagePadding = Constants.DinA4Padding;
                clonedDocument.ColumnWidth = double.PositiveInfinity;

                var paginator = ((IDocumentPaginatorSource) clonedDocument).DocumentPaginator;
                paginator.PageSize = new Size(imageableArea.MediaSizeWidth, imageableArea.MediaSizeHeight);

                paginator.ComputePageCount();

                if (pageRangeSelection == PageRangeSelection.UserPages)
                {
                    if (pageRange.PageFrom > paginator.PageCount || pageRange.PageTo > paginator.PageCount ||
                        pageRange.PageFrom > pageRange.PageTo)
                    {
                        MessageBox.Show("Die eingegebenen Seitenzahlen sind ungültig!", "Ungültig!",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    paginator = new LimitedPaginator(paginator, pageRange.PageFrom - 1, pageRange.PageTo - 1);
                }

                writer.Write(paginator);
            }
            else
            {
                var appDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/UCalc";
                var path = $"{appDataPath}\\print.xps";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var xpsDocument = new XpsDocument(path, FileAccess.ReadWrite);
                using var manager = new XpsSerializationManager(new XpsPackagingPolicy(xpsDocument), false);

                var paginator = ((IDocumentPaginatorSource) _flowDocument).DocumentPaginator;
                manager.SaveAsXaml(paginator);

                xpsDocument.Close();

                var process = new Process();
                process.StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                process.Start();
            }
        }

        private FixedDocumentSequence CreatePreview(IDocumentPaginatorSource document)
        {
            _fixedDocument = new XpsDocument(_package, CompressionOption.SuperFast, DocUri.AbsoluteUri);
            using var manager = new XpsSerializationManager(new XpsPackagingPolicy(_fixedDocument), false);

            var paginator = document.DocumentPaginator;
            manager.SaveAsXaml(paginator);
            return _fixedDocument.GetFixedDocumentSequence();
        }

        private static FlowDocument CloneFlowDocument(FlowDocument document)
        {
            var sourceRange = new TextRange(document.ContentStart, document.ContentEnd);

            using var stream = new MemoryStream();
            sourceRange.Save(stream, DataFormats.Xaml);

            var targetDocument = new FlowDocument();
            var targetRange = new TextRange(targetDocument.ContentStart, targetDocument.ContentEnd);
            targetRange.Load(stream, DataFormats.Xaml);

            return targetDocument;
        }

        private class LimitedPaginator : DocumentPaginator
        {
            private readonly DocumentPaginator _paginator;
            private readonly int _minPage;
            private readonly int _maxPage;

            public LimitedPaginator(DocumentPaginator paginator, int minPage, int maxPage)
            {
                _paginator = paginator;
                _minPage = minPage;
                _maxPage = maxPage;
            }

            public override DocumentPage GetPage(int pageNumber)
            {
                return _paginator.GetPage(_minPage + pageNumber);
            }

            public override bool IsPageCountValid => _minPage < _paginator.PageCount &&
                                                     _maxPage < _paginator.PageCount &&
                                                     _minPage <= _maxPage;

            public override int PageCount => _maxPage - _minPage + 1;

            public override Size PageSize
            {
                get => _paginator.PageSize;
                set => _paginator.PageSize = value;
            }

            public override IDocumentPaginatorSource Source => _paginator.Source;
        }

        private static FlowDocument CreateFlowDocument(Billing billing, IEnumerable<Tenant> tenants)
        {
            var size = GetPrinterMediaSize();
            if (!size.HasValue)
            {
                throw new NoPrinterException("Failed to query printer");
            }

            var document = new FlowDocument
            {
                PageWidth = size.Value.Width,
                PageHeight = size.Value.Height,
                PagePadding = Constants.DinA4Padding,
                ColumnWidth = double.PositiveInfinity
            };

            foreach (var tenant in tenants)
            {
                var result = BillingCalculator.CalculateForTenant(billing, tenant);
                AddPagesForTenant(document, billing, tenant, result);
            }

            return document;
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