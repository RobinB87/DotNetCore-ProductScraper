using System;
using System.Web;
using Microsoft.Extensions.Logging;

namespace ProductScraper.Models
{
    public class Whisky
    {
        public Whisky(ILogger logger, string type, string price, string name, bool inStock, string store)
        {
            Type = HttpUtility.HtmlDecode(type);
            Name = HttpUtility.HtmlDecode(name);
            InStock = inStock;
            Store = store;

            decimal.TryParse(price.Replace(".", ","), out var decimalPrice);
            Price = decimalPrice;
            if (Price == 0)
            {
                logger.LogError($"Something wrong with price of whisky: {Name}");
            }

            CreateDt = DateTime.Now;
        }

        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string Taste { get; set; }
        public string Content { get; set; }
        public string Country { get; set; }
        public bool InStock { get; set; }
        public string Store { get; set; }
        public DateTime CreateDt { get; set; }
    }
}