using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class NeogafCrawler : BaseCrawler
    {
        public NeogafCrawler(CrawlerSettings settings) : base(settings)
        {
            
        }

        protected override string GetPageUrlAtIndex(int index)
        {
            return index == 1? StartPage : $"{StartPage}/page-{index}";
        }

        protected override string GetScreenshotAnchorUrl(string page, string source, string screenshotUrl)
        {
            string anchorPattern = "class=\"u-anchorTarget\" id=\"post-";
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

                string anchor = source.Substring(anchorIndex + anchorPattern.Length,endOffset - (anchorIndex + anchorPattern.Length));
                return $"{page}#post-{anchor}";
            }
        }

        protected override int GetNumberOfPagesOnline()
        {
            WebClient webClient = new WebClient();
            string src = webClient.DownloadString(StartPage);
            string pattern1 = "menu menu--pageJump";
            string pattern2 = "max=\"";
            int pattern1StartIndex =-1;
            int pattern2StartIndex = -1;
            pattern1StartIndex = src.IndexOf(pattern1, StringComparison.Ordinal);
            if (pattern1StartIndex == -1)
            {
                return 1;
            }

            if (pattern2StartIndex == -1)
            {
                pattern2StartIndex = src.IndexOf(pattern2, pattern1StartIndex + pattern1.Length, StringComparison.Ordinal);
                if (pattern2StartIndex == -1)
                {
                    throw new Exception("Failed to get page count.");
                }
            }

            int patternEndIndex = src.IndexOf("\"", pattern2StartIndex + pattern2.Length, StringComparison.Ordinal);
            string pageCountStr = src.Substring(pattern2StartIndex + pattern2.Length, patternEndIndex - (pattern2StartIndex + pattern2.Length));
            int pageCount = Convert.ToInt32(pageCountStr);
            return pageCount;
        }
    }
}
