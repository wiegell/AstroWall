using System;
using AppKit;
using Foundation;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Threading.Tasks;
using System.Text;

namespace AstroWall
{
    public struct urlResponseWrap
    {
        public string url { get; set; }
        public HttpStatusCode status { get; set; }
    }

    public class HTMLHelpers
    {
        public HTMLHelpers()
        {


        }

        public const string NASADateFormat = "yyMMdd";

        public static async Task<urlResponseWrap> getImgOnlineUrl(string pageUrl)
        {
            HtmlWeb parser = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = await parser.LoadFromWebAsync(pageUrl);
            if (parser.StatusCode == HttpStatusCode.NotFound) return new urlResponseWrap() { status = parser.StatusCode };
            HtmlNode node = new List<HtmlNode>(doc.DocumentNode.Descendants("img")).First().ParentNode;
            HtmlAttribute attrib = node.Attributes.Where((HtmlAttribute attr) => attr.Name == "href").First();
            Console.WriteLine(attrib.Value);
            //string task = await Task.Run(() => ();
            return new urlResponseWrap() { url = "https://apod.nasa.gov/apod/" + attrib.Value };
        }

        public static string genPublishDateUrl(DateTime date)
        {
            return "https://apod.nasa.gov/apod/ap" + date.ToString(NASADateFormat, System.Globalization.CultureInfo.InvariantCulture) + ".html";
        }

        public static async Task<string[]> getDescTitleAndCreditFromOnlineUrl(string pageUrl)
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
            string creditsUrl = "";
            while (sibling != null)
            {

                sb.Append(sibling.InnerText);
                if (sibling.Name == "a" && creditsUrl == "")
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

