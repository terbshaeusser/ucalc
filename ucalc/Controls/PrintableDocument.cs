using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace UCalc.Controls
{
    public class PrintableDocument : IDisposable
    {
        private static readonly Uri DocUri = new Uri($"pack://mietrechner{new Guid()}.xps");
        private readonly Package _package;
        private readonly XpsDocument _document;

        private int PageCount =>
            _document.GetFixedDocumentSequence()!.References.First().GetDocument(false)!.Pages.Count;

        public PrintableDocument(IDocumentPaginatorSource source)
        {
            _package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);

            PackageStore.AddPackage(DocUri, _package);

            _document = new XpsDocument(_package, CompressionOption.SuperFast, DocUri.AbsoluteUri);
            var manager = new XpsSerializationManager(new XpsPackagingPolicy(_document), false);

            var paginator = source.DocumentPaginator;
            manager.SaveAsXaml(paginator);
        }

        public void Dispose()
        {
            ((IDisposable) _document).Dispose();
            ((IDisposable) _package).Dispose();
            PackageStore.RemovePackage(DocUri);
        }

        public void PreviewIn(DocumentViewer viewer)
        {
            viewer.Document = _document.GetFixedDocumentSequence();
        }

        public void Print(string description)
        {
            var dialog = new PrintDialog {UserPageRangeEnabled = true, MinPage = 1, MaxPage = (uint) PageCount};

            if (dialog.ShowDialog() == true)
            {
                var documentSequence = _document.GetFixedDocumentSequence();
                Package convPackage = null;
                XpsDocument convDocument = null;

                try
                {
                    if (dialog.PageRangeSelection == PageRangeSelection.UserPages)
                    {
                        convDocument = ExtractPages(_document, dialog.PageRange.PageFrom - 1,
                            dialog.PageRange.PageTo - 1, out convPackage);
                        documentSequence = convDocument.GetFixedDocumentSequence();
                    }

                    dialog.PrintDocument(documentSequence!.DocumentPaginator, description);
                }
                finally
                {
                    convDocument?.Close();
                    convPackage?.Close();
                }
            }
        }

        private static XpsDocument ExtractPages(XpsDocument source, int fromPage, int toPage, out Package package)
        {
            package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);

            var docUri = new Uri("pack://mietrechnerTempTicket.xps");
            PackageStore.RemovePackage(docUri);
            PackageStore.AddPackage(docUri, package);

            var document = new XpsDocument(package, CompressionOption.SuperFast, docUri.AbsoluteUri);
            var pages = source.GetFixedDocumentSequence()!.References.First().GetDocument(false)!.Pages;

            var documentReference = new DocumentReference();
            var fixedDocument = new FixedDocument();
            documentReference.SetDocument(fixedDocument);

            for (var i = fromPage; i <= toPage; ++i)
            {
                var page = pages[i];
                var pageContentChild = new PageContent {Source = page.Source};
                ((IUriContext) pageContentChild).BaseUri = ((IUriContext) page).BaseUri;
                pageContentChild.GetPageRoot(false);
                fixedDocument.Pages.Add(pageContentChild);
            }

            var documentSequence = new FixedDocumentSequence();
            documentSequence.References.Add(documentReference);

            var writer = XpsDocument.CreateXpsDocumentWriter(document);
            writer.Write(documentSequence);

            return document;
        }
    }
}