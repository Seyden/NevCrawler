using HtmlAgilityPack;
using System.Linq;

namespace CrawlerLib.Extensions
{
    public static class HtmlDocumentExtension
    {
        public static HtmlNodeCollection GetLinks(this HtmlDocument document)
        {
            return document.DocumentNode.SelectNodes("//a[@href]");
        }

        public static string GetHeaderTitle(this HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText;
        }
    }
}
