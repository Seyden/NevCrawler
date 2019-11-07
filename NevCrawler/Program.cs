using CrawlerLib;
using CrawlerLib.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NevCrawler
{
    class Program
    {
        static Task Main(string[] args)
        {
            string savePath = "";

            Crawler crawler = new Crawler(new Uri("https://crawler-test.com/"))
            {
                IgnoreUriFilter = x => x.ToString().Contains("infinite") || x.ToString().Contains("page_load_time")
            };
            crawler.AfterParseEvent += (p) => AfterParseEvent(p, savePath);
            crawler.SetTimeout(5);
            
            return crawler.Crawl();
        }

        public static void AfterParseEvent(Page page, string path)
        {
            //if (!File.Exists(path))
            //{
            //    File.WriteAllText(path, page.Html);
            //}
        }
    }
}
