using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using HtmlAgilityPack;
using static System.Net.WebRequestMethods;

namespace AstroWall
{
    /// <summary>
    /// HTML-related helper functions.
    /// </summary>
    internal class HTMLHelpers
    {
        /// <summary>
        /// Date format of NASA urls.
        /// </summary>
        internal const string NASADateFormat = "yyMMdd";

        /// <summary>
        /// Gets online url of image at pageUrl.
        /// </summary>
        /// <returns>UrlResponseWrap.</returns>
        internal static async Task<UrlResponseWrap> GetImgOnlineUrl(string pageUrl)
        {
            HtmlWeb parser = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = await parser.LoadFromWebAsync(pageUrl);
            if (parser.StatusCode == HttpStatusCode.NotFound)
            {
                return new UrlResponseWrap(parser.StatusCode);
            }

            HtmlNode node = new List<HtmlNode>(doc.DocumentNode.Descendants("img")).First().ParentNode;
            HtmlAttribute attrib = node.Attributes.Where((HtmlAttribute attr) => attr.Name == "href").First();
            Console.WriteLine(attrib.Value);

            return new UrlResponseWrap("https://apod.nasa.gov/apod/" + attrib.Value);
        }

        /// <summary>
        /// Generates NASA page url from date.
        /// </summary>
        /// <returns></returns>
        internal static string GenPublishDateUrl(DateTime date)
        {
            return "https://apod.nasa.gov/apod/ap" + date.ToString(NASADateFormat, System.Globalization.CultureInfo.InvariantCulture) + ".html";
        }

        /// <summary>
        /// Parse description, title and credit text from pageUrl.
        /// </summary>
        /// <returns>{ title, desc, credit, creditsUrl }.</returns>
        internal static async Task<string[]> ParseTitleDescriptiontAndCreditFromOnlineUrl(string pageUrl)
        {
            HtmlWeb webparser = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = await webparser.LoadFromWebAsync(pageUrl);
            HtmlNode imgCredNode = doc.DocumentNode.Descendants("b").ToList().Where((HtmlNode node) => node.InnerHtml.Contains("Image Credit")).First();
            List<HtmlNode> imgCredNodeSiblings = imgCredNode.ParentNode.ChildNodes.Where((node) => (node.NodeType != HtmlNodeType.Text)).ToList();

            // Get title
            HtmlNode titleNode = imgCredNodeSiblings[0];
            string title = titleNode.InnerText;

            // Get credit text
            StringBuilder sb = new StringBuilder();
            HtmlNode sibling = imgCredNode.NextSibling;
            string creditsUrl = string.Empty;
            while (sibling != null)
            {
                sb.Append(sibling.InnerText);
                if (sibling.Name == "a" && creditsUrl == string.Empty)
                {
                    try
                    {
                        creditsUrl = sibling.Attributes.Where(attrib => attrib.Name == "href").ToArray()[0].Value;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to parse credits url");
                    }
                }

                sibling = sibling.NextSibling;
            }

            string credit = sb.ToString();

            // Get description
            List<HtmlNode> bodyNodes = new List<HtmlNode>(doc.DocumentNode.Descendants("body").First().ChildNodes.Where((node) => (node.NodeType != HtmlNodeType.Text)));
            string desc = bodyNodes[2].InnerText;
            return new string[] { title, desc, credit, creditsUrl };
        }
    }
}