using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlerLib.Models
{
    public class Page
    {
        public Uri Uri { get; set; }
        public string Html { get; set; }

        public Page(Uri uri)
        {
            Uri = uri;
        }
    }
}
