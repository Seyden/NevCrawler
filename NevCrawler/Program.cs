using CrawlerLib;
using CrawlerLib.Models;
using System;
using System.Threading.Tasks;

namespace NevCrawler
{
    class Program
    {
        static Task Main(string[] args)
        {
            Crawler crawler = new Crawler(new Uri("https://crawler-test.com/"));
            crawler.SetTimeout(30);
            crawler.AfterParseEvent += AfterParseEvent;
            return crawler.Crawl();
        }

        public static void AfterParseEvent(Page page)
        {
            Console.WriteLine(page.Uri.AbsoluteUri);
        }
    }
}
