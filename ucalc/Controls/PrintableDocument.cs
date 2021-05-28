using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using Microsoft.Win32;

namespace UCalc.Controls
{
    public class NoPrinterException : Exception
    {
        public NoPrinterException(string message) : base(message)
        {
        }
    }

    public abstract class PrintableDocument<T> : IDisposable
    {
        private static readonly Uri DocUri = new Uri($"pack://mietrechner{new Guid()}.xps");
        private readonly Package _package;
        private readonly FlowDocument _flowDocument;
        private readonly FixedDocumentSequence _previewDocument;
        private XpsDocument _fixedDocument;

        protected PrintableDocument(T args)
        {
            _package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
            PackageStore.AddPackage(DocUri, _package);

            _flowDocument = CreateFlowDocument(args);
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

                var process = new Process {StartInfo = new ProcessStartInfo(path) {UseShellExecute = true}};
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

        private FlowDocument CreateFlowDocument(T args)
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

            FillFlowDocument(document, args);
            return document;
        }

        protected abstract void FillFlowDocument(FlowDocument flowDocument, T args);

        protected static void AddLineBreaks(TableRowGroup rowGroup, int count)
        {
            var row = new TableRow();
            rowGroup.Rows.Add(row);

            var cell = new TableCell {ColumnSpan = 2};
            row.Cells.Add(cell);
            cell.Blocks.Add(new Paragraph(new Run(new string('\n', count - 1))
                {FontSize = Constants.PrintNewlineFontSize}));
        }

        protected static void AddText(TableRowGroup rowGroup, string text, double? fontSize = null,
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
    }
}