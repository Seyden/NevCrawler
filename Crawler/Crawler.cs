using CrawlerLib.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerLib
{
    public class Crawler
    {
        private Queue<Page> _pagesToProcess;
        private List<Page> _processedPages;
        private ISet<Uri> _visitedLinks;
        private Uri _startUri;

        private HttpClient _client;

        public delegate void AfterParse(Page page);
        public event AfterParse AfterParseEvent;

        public bool AutoSave = false;
        public string SavePath = string.Empty;

        public Crawler(Uri uri)
        {
            BaseInitialization(uri);
        }

        public Crawler(Uri uri, bool autoSave, string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentException("SavePath can't be null or empty, please choose a path where your crawled pages should be saved.");

            AutoSave = true;
            SavePath = savePath;

            BaseInitialization(uri);
        }

        private void BaseInitialization(Uri uri)
        {
            _pagesToProcess = new Queue<Page>();
            _processedPages = new List<Page>();
            _visitedLinks = new HashSet<Uri>();
            _client = new HttpClient();

            _startUri = uri;
            EnqueuePage(uri);
        }

        // Wrapping it for the case if extra things are needed
        private void EnqueuePage(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return;

            if (uri.Host != _startUri.Host)
                return;

            Page page = new Page(uri);

            if (_visitedLinks.Add(page.Uri))
                _pagesToProcess.Enqueue(page);
        }

        public void SetTimeout(double timeInSeconds)
        {
            _client.Timeout = TimeSpan.FromSeconds(timeInSeconds);
        }

        public void Crawl()
        {
            while (_pagesToProcess.Count > 0)
            {
                ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (_pagesToProcess.TryDequeue(out var page))
                CrawlPage(page);
        }

        private void CrawlPage(Page page)
        {
            if (page == null)
                return;

            HttpResponseMessage response;
            try
            {
                response = _client.GetAsync(page.Uri, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode 
                    || response.Content.Headers.ContentLength == null
                    || response.Content.Headers.ContentLength == 0
                    || response.Content.Headers.ContentType.MediaType != "text/html")
                    return;
            }
            catch (OperationCanceledException)
            {
                // If the page gets a timeout, we just skip it
                return;
            }

            var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            page.Html = Encoding.Default.GetString(bytes);

            AfterParseEvent?.Invoke(page);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page.Html);

            var links = document.DocumentNode.SelectNodes("//a[@href]");
            if (links == null)
                return;

            foreach (var link in links)
            {
                var value = link.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(value))
                    continue;

                var uri = default(Uri);

                // Not a valid URI Protocol (tel, skype, javascript, whatsapp, chrome:// etc.)
                if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                    continue;

                // Relative Uris indicate that they're from the same domain, so we convert them for parsing them later.
                if (!uri.IsAbsoluteUri)
                    uri = new Uri(new Uri(_startUri.OriginalString), uri);

                // Pass the URI to avoid unneccarry heap allocations when allocating a Page Object here
                EnqueuePage(uri);
            }
        }
    }
}
