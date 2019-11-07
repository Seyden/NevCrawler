using CrawlerLib.Extensions;
using CrawlerLib.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlerLib
{
    public class Crawler
    {
        private ConcurrentQueue<Page> _pagesToProcess;
        private ConcurrentBag<Page> _processedPages;
        private ConcurrentDictionary<Uri, int> _visitedLinks;
        private Uri _startUri;

        private HttpClient _client;

        public delegate void AfterParse(Page page);
        public event AfterParse AfterParseEvent;

        public Func<Uri, bool> IgnoreUriFilter = null;

        public Crawler(Uri uri)
        {
            BaseInitialization(uri);
        }

        private void BaseInitialization(Uri uri)
        {
            _pagesToProcess = new ConcurrentQueue<Page>();
            _processedPages = new ConcurrentBag<Page>();
            _visitedLinks = new ConcurrentDictionary<Uri, int>();
            _client = new HttpClient();

            _startUri = uri;
            EnqueuePage(uri);
        }

        // Wrapping it for the case if extra things are needed
        private void EnqueuePage(Uri uri)
        {
            bool ignore = IgnoreUriFilter?.Invoke(uri) ?? false;
            if (ignore)
                return;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return;

            if (uri.Host != _startUri.Host)
                return;

            Page page = new Page(uri);

            if (_visitedLinks.TryAdd(page.Uri, 0))
                _pagesToProcess.Enqueue(page);
        }

        public void SetTimeout(double timeInSeconds)
        {
            _client.Timeout = TimeSpan.FromSeconds(timeInSeconds);
        }

        public async Task Crawl(int maxTaskCount = 10, CancellationToken cancellationToken = default)
        {
            var tasks = new HashSet<Task> { ProcessQueue(cancellationToken) };

            while (tasks.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var finishedTask = await Task.WhenAny(tasks);
                    tasks.Remove(finishedTask);
                }
                catch (TaskCanceledException)
                {
                    // Caller requested cancellation, so return now.
                    return;
                }

                if (_pagesToProcess.Count > 0)
                {
                    // Always process one, thats the minimum you can do.
                    AddProcessQueueTask();
                    if (_pagesToProcess.Count > maxTaskCount && tasks.Count < maxTaskCount)
                        AddProcessQueueTask();
                }
            }

            void AddProcessQueueTask()
            {
                var task = ProcessQueue(cancellationToken);
                if (task != null)
                    tasks.Add(task);
            }
        }

        private async Task ProcessQueue(CancellationToken cancellationToken)
        {
            if (_pagesToProcess.TryDequeue(out var page))
                await CrawlPage(page, cancellationToken);
        }

        private async Task CrawlPage(Page page, CancellationToken cancellationToken)
        {
            if (page == null)
                return;

            HttpResponseMessage response;
            try
            {
                response = await _client.GetAsync(page.Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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

            var bytes = await response.Content.ReadAsByteArrayAsync();
            page.Html = Encoding.Default.GetString(bytes);

            _processedPages.Add(page);

            AfterParseEvent?.Invoke(page);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page.Html);

            ProcessLinks(document);
        }

        private void ProcessLinks(HtmlDocument document)
        {
            var links = document.GetLinks();
            if (links == null)
                return;

            foreach (var link in links)
            {
                var uri = ParseLink(link);
                if (uri == null)
                    continue;

                // Pass the URI to avoid unneccarry heap allocations when allocating a Page Object here
                EnqueuePage(uri);
            }
        }

        private Uri ParseLink(HtmlNode link)
        {
            var value = link.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(value))
                return null;

            var uri = default(Uri);

            // Not a valid URI Protocol (tel, skype, javascript, whatsapp, chrome:// etc.)
            if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                return null;

            // Relative Uris indicate that they're from the same domain, so we convert them for parsing them later.
            if (!uri.IsAbsoluteUri)
                uri = new Uri(new Uri(_startUri.OriginalString), uri);

            return uri;
        }
    }
}
