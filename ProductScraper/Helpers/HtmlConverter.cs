using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ProductScraper.Models;

namespace ProductScraper.Helpers
{
    public class HtmlConverter
    {
        private readonly ILogger _logger;

        public HtmlConverter(ILogger logger)
        {
            _logger = logger;
        }

        public List<Whisky> ReadHtmlDocument(string pageContent, string xPathMain, 
            string divPrice, string divName, string divInStock, string divInStockMatch, string scraper,
            string xPathType = null, string priceDecimals = null, 
            string discountPriceBase = null, string discountPriceDecimals = null)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);
            var nodes = htmlDoc.DocumentNode.SelectNodes(xPathMain);

            var whiskeys = new List<Whisky>();
            if (nodes == null)
            {
                return whiskeys;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var type = string.Empty;
                if (!string.IsNullOrWhiteSpace(xPathType))
                {
                    type = node.SelectNodes(xPathType)?.FirstOrDefault()?.InnerText?.Trim();
                }

                var name = node.SelectNodes(divName)?.FirstOrDefault()?.InnerText?.Trim();
                var price = GetPrice(node, i, divPrice, priceDecimals, discountPriceBase, discountPriceDecimals);
                if (string.IsNullOrWhiteSpace(price))
                {
                    _logger.LogError($"Something wrong with price of whisky: {price}");
                    continue;
                }

                var inStock = node.SelectNodes(divInStock)?.FirstOrDefault()?.InnerText == divInStockMatch;
                Console.WriteLine($"Processing {name}: {price}");

                whiskeys.Add(new Whisky(_logger, type, price, name, inStock, scraper));
            }

            return whiskeys;
        }

        private string GetPrice(HtmlNode node, int i, string divPrice, string priceDecimals,
            string discountPriceBase, string discountPriceDecimals)
        {
            var elementNumber = i + 1;
            var price = string.Empty;

            // First check if discount price xpath is filled, try it
            if (!string.IsNullOrWhiteSpace(discountPriceBase))
            {
                price = ChangeXPathAndGetValue(discountPriceBase, elementNumber, node);
                if (!string.IsNullOrWhiteSpace(price))
                {
                    price += ChangeXPathAndGetValue(discountPriceDecimals, elementNumber, node);
                }
            }

            // If price is still empty, try regular price xpath
            if (string.IsNullOrWhiteSpace(price))
            {
                price = ChangeXPathAndGetValue(divPrice, elementNumber, node);
                if (!string.IsNullOrWhiteSpace(price) && !string.IsNullOrWhiteSpace(priceDecimals))
                {
                    price += ChangeXPathAndGetValue(priceDecimals, elementNumber, node);
                }
            }

            if (string.IsNullOrWhiteSpace(price))
            {
                return price;
            }

            // Check if price matches digits, dot or comma, digits
            var match = Regex.Match(price, @"\d+[.,]?\d+", RegexOptions.RightToLeft);
            if (match.Success)
            {
                price = match.Value;
            }
            else
            {
                _logger.LogError($"Something wrong while trying regex on price: {price}");
            }

            return price;
        }

        private string ChangeXPathAndGetValue(string xPath, int elementNumber, HtmlNode node)
        {
            xPath = xPath.Replace("{{x}}", elementNumber.ToString());
            var price = node.SelectNodes(xPath)?.FirstOrDefault()?.InnerText;
            return string.IsNullOrWhiteSpace(price) ? string.Empty : price;
        }
    }
}
