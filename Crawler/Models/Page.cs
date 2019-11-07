using System;

namespace CrawlerLib.Models
{
    public class Page
    {
        public string Title { get; set;  }
        public Uri Uri { get; set;  }
        public string Html { get; set; }

        public Page(Uri uri)
        {
            Uri = uri;
        }
    }
}
