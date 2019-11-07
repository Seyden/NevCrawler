using HtmlAgilityPack;

namespace CrawlerLib.Extensions
{
    public static class HtmlDocumentExtension
    {
        public static HtmlNodeCollection GetLinks(this HtmlDocument document)
        {
            return document.DocumentNode.SelectNodes("//a[@href]");
        }
    }
}
