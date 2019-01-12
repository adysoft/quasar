using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Crawler
{
    public class ReseteraCrawler : BaseCrawler
    {
        public ReseteraCrawler(CrawlerSettings settings) : base(settings)
        {
        }

        protected override string GetPageUrlAtIndex(int index)
        {
            return $"{StartPage}/page-{index}";
        }

        protected override int GetNumberOfPagesOnline()
        {
            WebClient webClient = new WebClient();
            string src = webClient.DownloadString(StartPage);
            string pattern = "min=\"1\" max=\"";
            int patternStartIndex = src.IndexOf(pattern, StringComparison.Ordinal);
            if (patternStartIndex == -1)
            {
                throw new Exception("Failed to get page count.");
            }

            int patternEndIndex = src.IndexOf("\"", patternStartIndex + pattern.Length, StringComparison.Ordinal);
            string pageCountStr = src.Substring(patternStartIndex + pattern.Length,patternEndIndex - (patternStartIndex + pattern.Length));
            int pageCount = Convert.ToInt32(pageCountStr);
            return pageCount;
        }

        protected override string GetScreenshotAnchorUrl(string page, string source, string screenshotUrl)
        {
            string anchorPattern = "class=\"message-userContent lbContainer js-lbContainer  \" data-lb-id=\"";
            int screenshotUrlIndex = source.IndexOf(screenshotUrl, StringComparison.Ordinal);
            int anchorIndex = source.Substring(0, screenshotUrlIndex).LastIndexOf(anchorPattern, StringComparison.Ordinal);
            if (anchorIndex == -1)
            {
                throw new Exception("Failed to find anchor url.");
            }
            else
            {
                int endOffset = source.IndexOf("\"", anchorIndex + anchorPattern.Length, StringComparison.Ordinal);
                if (endOffset == -1)
                {
                    throw new Exception("Failed to find anchor url.");
                }

                string anchor = source.Substring(anchorIndex + anchorPattern.Length, endOffset - (anchorIndex + anchorPattern.Length));
                return $"{page.Substring(0, page.LastIndexOf("/"))}/{anchor}";
            }
        }
    }
}
