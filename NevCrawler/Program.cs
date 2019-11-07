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
            
            return crawler.Crawl();
        }

        public static void AfterParseEvent(Page page, string path)
        {
            string fileName = $"{path}/{string.Join("_", page.Title.Split(Path.GetInvalidFileNameChars()))}.html";
            if (!File.Exists(path))
            {
                File.WriteAllText(fileName, page.Html);
            }
        }
    }
}
