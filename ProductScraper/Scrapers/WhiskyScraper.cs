using Microsoft.Extensions.Logging;
using ProductScraper.Helpers;
using ProductScraper.Models;
using Repository.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ProductScraper.Scrapers
{
    public class WhiskyScraper
    {
        private readonly ILogger _logger;
        private readonly IHttpHandler _httpHandler;
        private readonly IWhiskyService _whiskyService;
        private readonly IUtils _utils;

        public WhiskyScraper(ILogger logger, IHttpHandler httpHandler, IWhiskyService whiskyService, IUtils utils)
        {
            _logger = logger;
            _httpHandler = httpHandler;
            _whiskyService = whiskyService;
            _utils = utils;
        }

        public async Task<List<Whisky>> Initialize()
        {
            var whiskiesTotal = new List<Whisky>();
            var availableScrapers = ConfigurationManager.AppSettings["AvailableScrapers"].Split(";");
            var htmlConverter = new HtmlConverter(_logger);
            var random = new Random();

            foreach (var scraper in availableScrapers)
            {
                whiskiesTotal.AddRange(await GetScrapedWhiskies(random, htmlConverter, scraper));
            }

            return whiskiesTotal;
        }

        public async Task<List<Whisky>> GetScrapedWhiskies(Random random, HtmlConverter converter, string scraper)
        {
            var url = ConfigurationManager.AppSettings[scraper + "url"];
            var startNumber = Convert.ToInt32(ConfigurationManager.AppSettings[scraper + "startNumber"]);
            var endNumber = Convert.ToInt32(ConfigurationManager.AppSettings[scraper + "endNumber"]);
            var increaseNumber = Convert.ToInt32(ConfigurationManager.AppSettings[scraper + "increaseNumber"]);
            var xPathMain = ConfigurationManager.AppSettings[scraper + "xPathMain"];
            var xPathType = ConfigurationManager.AppSettings[scraper + "xPathType"];
            var xPathPriceBase = ConfigurationManager.AppSettings[scraper + "xPathPriceBase"];
            var xPathPriceDecimals = ConfigurationManager.AppSettings[scraper + "xPathPriceDecimals"];
            var xPathDiscountBase = ConfigurationManager.AppSettings[scraper + "xPathPriceDiscountBase"];
            var xPathDiscountDecimals = ConfigurationManager.AppSettings[scraper + "xPathPriceDiscountDecimals"];
            var xPathName = ConfigurationManager.AppSettings[scraper + "xPathName"];
            var xPathStock = ConfigurationManager.AppSettings[scraper + "xPathInStock"];
            var xPathStockMatch = ConfigurationManager.AppSettings[scraper + "xPathInStockMatch"];
            
            var whiskeysTotal = new List<Whisky>();
            for (var i = startNumber; i < endNumber; i += increaseNumber)
            {
                var urlToScrape = url.Replace("{i}", $"{i}");

                // Wait random time to not over request webpage
                if (i != 0)
                {
                    _utils.Sleep(random.Next(3000, 23000));
                }

                var response = await _httpHandler.GetAsync(urlToScrape);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"No 200 statuscode for: {urlToScrape}");
                }

                var pageContent = await _httpHandler.ReadAsStringAsync(response.Content);

                var whiskeys = converter.ReadHtmlDocument(pageContent, xPathMain, xPathPriceBase, xPathName, 
                        xPathStock, xPathStockMatch, scraper, xPathType, 
                        xPathPriceDecimals, xPathDiscountBase, xPathDiscountDecimals);

                whiskeysTotal.AddRange(whiskeys);
            }

            return whiskeysTotal;
        }

        public List<string> ProcessScrapedWhiskies(IEnumerable<Whisky> scrapedWhiskies)
        {
            var discountsForEmail = new List<string>();
            foreach (var whisky in scrapedWhiskies)
            {
                // Get whisky, preferably with it's type
                var whiskyEntity = string.IsNullOrWhiteSpace(whisky.Type)
                    ? _whiskyService.GetWhisky(whisky.Name, whisky.Store)
                    : _whiskyService.GetWhisky(whisky.Name, whisky.Store, whisky.Type);

                if (whiskyEntity == null)
                {
                    // Add to database
                    _whiskyService.AddWhisky(CustomMapper.MapWhisky(whisky));
                    continue;
                }

                // Whisky found, make this one inactive
                _whiskyService.EditWhisky(whiskyEntity);

                // Add new record with original id, which will now be active
                _whiskyService.AddWhisky(CustomMapper.MapWhisky(whisky), whiskyEntity.OriginalId);

                // Check if price has decreased since last scan
                if (whisky.Price >= whiskyEntity.Price) continue;
                var priceDifference = whiskyEntity.Price - whisky.Price;
                var body =
                    $"<p>{whisky.Name} | Price: {whisky.Price} vs {whiskyEntity.Price} (-{priceDifference})</p>";
                discountsForEmail.Add(body);
            }

            _whiskyService.SaveChanges();
            return discountsForEmail;
        }
    }
}