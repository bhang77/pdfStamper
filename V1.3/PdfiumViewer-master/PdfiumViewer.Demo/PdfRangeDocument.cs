using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;

namespace PdfiumViewer.Demo
{
    public class PdfRangeDocument : PdfView.IPdfDocument
    {
        public static PdfRangeDocument FromDocument(PdfView.IPdfDocument document, int startPage, int endPage)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (endPage < startPage)
                throw new ArgumentException("End page cannot be less than start page");
            if (startPage < 0)
                throw new ArgumentException("Start page cannot be less than zero");
            if (endPage >= document.PageCount)
                throw new ArgumentException("End page cannot be more than the number of pages in the document");

            return new PdfRangeDocument(
                document,
                startPage,
                endPage
            );
        }

        private readonly PdfView.IPdfDocument _document;
        private readonly int _startPage;
        private readonly int _endPage;
        private PdfView.PdfBookmarkCollection _bookmarks;
        private IList<SizeF> _sizes;


        private PdfRangeDocument(PdfView.IPdfDocument document, int startPage, int endPage)
        {
            _document = document;
            _startPage = startPage;
            _endPage = endPage;
        }

       
        public int PageCount
        {
            get { return _endPage - _startPage + 1; }
        }

        public PdfView.PdfBookmarkCollection Bookmarks
        {
            get
            {
                if (_bookmarks == null)
                    _bookmarks = TranslateBookmarks(_document.Bookmarks);
                return _bookmarks;
            }
        }

        private PdfView.PdfBookmarkCollection TranslateBookmarks(PdfView.PdfBookmarkCollection bookmarks)
        {
            var result = new PdfView.PdfBookmarkCollection();

            TranslateBookmarks(result, bookmarks);

            return result;
        }

        private void TranslateBookmarks(PdfView.PdfBookmarkCollection result, PdfView.PdfBookmarkCollection bookmarks)
        {
            foreach (var bookmark in bookmarks)
            {
                if (bookmark.PageIndex >= _startPage && bookmark.PageIndex <= _endPage)
                {
                    var resultBookmark = new PdfView.PdfBookmark
                    {
                        PageIndex = bookmark.PageIndex - _startPage,
                        Title = bookmark.Title
                    };

                    TranslateBookmarks(resultBookmark.Children, bookmark.Children);

                    result.Add(resultBookmark);
                }
            }
        }

        public IList<SizeF> PageSizes
        {
            get
            {
                if (_sizes == null)
                    _sizes = TranslateSizes(_document.PageSizes);
                return _sizes;
            }
        }

        private IList<SizeF> TranslateSizes(IList<SizeF> pageSizes)
        {
            var result = new List<SizeF>();

            for (int i = _startPage; i <= _endPage; i++)
            {
                result.Add(pageSizes[i]);
            }

            return result;
        }

        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, bool forPrinting)
        {
            _document.Render(TranslatePage(page), graphics, dpiX, dpiY, bounds, forPrinting);
        }

        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, PdfView.PdfRenderFlags flags)
        {
            _document.Render(TranslatePage(page), graphics, dpiX, dpiY, bounds, flags);
        }

        public Image Render(int page, float dpiX, float dpiY, bool forPrinting)
        {
            return _document.Render(TranslatePage(page), dpiX, dpiY, forPrinting);
        }

        public Image Render(int page, float dpiX, float dpiY, PdfView.PdfRenderFlags flags)
        {
            return _document.Render(TranslatePage(page), dpiX, dpiY, flags);
        }

        public Image Render(int page, int width, int height, float dpiX, float dpiY, bool forPrinting)
        {
            return _document.Render(TranslatePage(page), width, height, dpiX, dpiY, forPrinting);
        }

        public Image Render(int page, int width, int height, float dpiX, float dpiY, PdfView.PdfRenderFlags flags)
        {
            return _document.Render(TranslatePage(page), width, height, dpiX, dpiY, flags);
        }

        public Image Render(int page, int width, int height, float dpiX, float dpiY, PdfView.PdfRotation rotate, PdfView.PdfRenderFlags flags)
        {
            return _document.Render(page, width, height, dpiX, dpiY, rotate, flags);
        }

        public void Save(string path)
        {
            _document.Save(path);
        }

        public void Save(Stream stream)
        {
            _document.Save(stream);
        }

        public PdfView.PdfMatches Search(string text, bool matchCase, bool wholeWord)
        {
            return TranslateMatches(_document.Search(text, matchCase, wholeWord));
        }

        public PdfView.PdfMatches Search(string text, bool matchCase, bool wholeWord, int page)
        {
            return TranslateMatches(_document.Search(text, matchCase, wholeWord, page));
        }

        public PdfView.PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage)
        {
            return TranslateMatches(_document.Search(text, matchCase, wholeWord, startPage, endPage));
        }

        private PdfView.PdfMatches TranslateMatches(PdfView.PdfMatches search)
        {
            if (search == null)
                return null;

            var matches = new List<PdfView.PdfMatch>();

            foreach (var match in search.Items)
            {
                matches.Add(new PdfView.PdfMatch(
                    match.Text,
                    new PdfView.PdfTextSpan(match.TextSpan.Page + _startPage, match.TextSpan.Offset, match.TextSpan.Length),
                    match.Page + _startPage
                ));
            }

            return new PdfView.PdfMatches(
                search.StartPage + _startPage,
                search.EndPage + _startPage,
                matches
            );
        }

        public PrintDocument CreatePrintDocument()
        {
            return _document.CreatePrintDocument();
        }

        public PrintDocument CreatePrintDocument(PdfView.PdfPrintMode printMode)
        {
            return _document.CreatePrintDocument(printMode);
        }

        public PrintDocument CreatePrintDocument(PdfView.PdfPrintSettings settings)
        {
            return _document.CreatePrintDocument(settings);
        }

        public PdfView.PdfPageLinks GetPageLinks(int pageNumber, Size pageSize)
        {
            return TranslateLinks(_document.GetPageLinks(pageNumber + _startPage, pageSize));
        }

        private PdfView.PdfPageLinks TranslateLinks(PdfView.PdfPageLinks pageLinks)
        {
            if (pageLinks == null)
                return null;

            var links = new List<PdfView.PdfPageLink>();

            foreach (var link in pageLinks.Links)
            {
                links.Add(new PdfView.PdfPageLink(
                    link.Bounds,
                    link.TargetPage + _startPage,
                    link.Uri
                ));
            }

            return new PdfView.PdfPageLinks(links);
        }

        public void DeletePage(int pageNumber)
        {
            _document.DeletePage(TranslatePage(pageNumber));
        }

        public void RotatePage(int pageNumber, PdfView.PdfRotation rotation)
        {
            _document.RotatePage(TranslatePage(pageNumber), rotation);
        }

        public PdfView.PdfInformation GetInformation()
        {
            return _document.GetInformation();
        }

        public string GetPdfText(int page)
        {
            return _document.GetPdfText(TranslatePage(page));
        }

        public string GetPdfText(PdfView.PdfTextSpan textSpan)
        {
            return _document.GetPdfText(textSpan);
        }

        public IList<PdfView.PdfRectangle> GetTextBounds(PdfView.PdfTextSpan textSpan)
        {
            var result = new List<PdfView.PdfRectangle>();

            foreach (var rectangle in _document.GetTextBounds(textSpan))
            {
                result.Add(new PdfView.PdfRectangle(
                    rectangle.Page + _startPage,
                    rectangle.Bounds
                ));
            }

            return result;
        }

        public PointF PointToPdf(int page, Point point)
        {
            return _document.PointToPdf(TranslatePage(page), point);
        }

        public Point PointFromPdf(int page, PointF point)
        {
            return _document.PointFromPdf(TranslatePage(page), point);
        }

        public RectangleF RectangleToPdf(int page, Rectangle rect)
        {
            return _document.RectangleToPdf(TranslatePage(page), rect);
        }

        public Rectangle RectangleFromPdf(int page, RectangleF rect)
        {
            return _document.RectangleFromPdf(TranslatePage(page), rect);
        }

        private int TranslatePage(int page)
        {
            if (page < 0 || page >= PageCount)
                throw new ArgumentException("Page number out of range");
            return page + _startPage;
        }

        public void Dispose()
        {
            _document.Dispose();
        }
    }
}
