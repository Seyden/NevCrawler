using CrawlerLib;
using CrawlerLib.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NevCrawler
{
    class Program
    {
        /*
         * THIS CODE IN THE MAIN CLASS IS ONLY A EXAMPLE
         * 
         */

        /*
         * FEATURES:
         *  - Async
         *  - Works with Tasks
         *  - Retry 
         *  - URI Filter (tested with https://crawler-test.com for infinite pages and long page load times)
         *  - Concurrent
         *  - AfterParseEvent (for saving the Page or just use the .Pages enumerable to save them later)
         */

        static Task Main(string[] args)
        {
            string uri = args[0];
            string savePath = args[1];

            Crawler crawler = new Crawler(new Uri(uri))
            {
                IgnoreUriFilter = x => x.ToString().Contains("infinite") || x.ToString().Contains("page_load_time")
            };
            crawler.AfterParseEvent += (p) => AfterParseEvent(p, savePath);
            crawler.SetTimeout(5);
            crawler.SetRetryDelay(5);
            crawler.SetRetryCount(4);
            
            return crawler.Crawl();
        }

        public static void AfterParseEvent(Page page, string path)
        {
            if (string.IsNullOrEmpty(page.Title))
                return;

            string fileName = $"{path}/{string.Join("_", page.Title.Split(Path.GetInvalidFileNameChars()))}.html";
            if (!File.Exists(path))
            {
                File.WriteAllText(fileName, page.Html);
            }
        }
    }
}
