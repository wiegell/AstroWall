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

namespace AstroWall
{
    public class HTMLHelpers
    {
        public HTMLHelpers()
        {


        }

        public static string NASADateFormat = "yyMMdd";

        public static async Task<string> getImgOnlineUrl(string pageUrl)
        {
            HtmlWeb webparser = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = await webparser.LoadFromWebAsync(pageUrl);
            HtmlNode node = new List<HtmlNode>(doc.DocumentNode.Descendants("img")).First().ParentNode;
            HtmlAttribute attrib = node.Attributes.Where((HtmlAttribute attr) => attr.Name == "href").First();
            Console.WriteLine(attrib.Value);
            //string task = await Task.Run(() => ();
            return "https://apod.nasa.gov/apod/" + attrib.Value;
        }

        public static string genPublishDateUrl(DateTime date)
        {

            return "https://apod.nasa.gov/apod/ap" + date.ToString(NASADateFormat) + ".html";
        }

        public static async Task<string[]> getDescAndTitleFromOnlineUrl(string pageUrl)
        {
            HtmlWeb webparser = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = await webparser.LoadFromWebAsync(pageUrl);
            List<HtmlNode> par = new List<HtmlNode>((new List<HtmlNode>(doc.DocumentNode.Descendants("b"))).Where((HtmlNode node) => node.InnerHtml.Contains("Image Credit")).First().ParentNode.ChildNodes.Where((node) => (node.NodeType != HtmlNodeType.Text)));
            //foreach (HtmlNode no in par) Console.WriteLine(no.Name);
            HtmlNode titleNode = par[0];
            string title = titleNode.InnerText;

            List<HtmlNode> bodyNodes = new List<HtmlNode>(doc.DocumentNode.Descendants("body").First().ChildNodes.Where((node) => (node.NodeType != HtmlNodeType.Text)));
            string desc = bodyNodes[2].InnerText;
            return new string[] { title, desc };

        }

    }
}

