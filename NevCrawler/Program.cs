using CrawlerLib;
using CrawlerLib.Models;
using System;

namespace NevCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler crawler = new Crawler(new Uri("https://crawler-test.com/"));
            crawler.SetTimeout(30);
            crawler.AfterParseEvent += AfterParseEvent;
            crawler.Crawl();
        }

        public static void AfterParseEvent(Page page)
        {
            Console.WriteLine(page.Uri.AbsoluteUri);
        }
    }
}
