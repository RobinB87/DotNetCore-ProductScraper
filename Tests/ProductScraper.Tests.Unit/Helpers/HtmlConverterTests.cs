using Microsoft.Extensions.Logging;
using Moq;
using ProductScraper.Helpers;
using ProductScraper.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace ProductScraper.Tests.Unit.Helpers
{
    public class HtmlConverterTests : ScraperTestsBase
    {
        [Fact]
        public void RegularPage_WhiskyListNotNull()
        {
            var whiskies = GetWhiskiesGg();
            Assert.NotNull(whiskies);
        }

        [Fact]
        public void RegularPage_PricesAreNot0()
        {
            var whiskies = GetWhiskiesGg();
            foreach (var whisky in whiskies)
            {
                Assert.NotEqual(0, whisky.Price);
            }
        }

        [Fact]
        public void RegularPage_NamesAreNotNull()
        {
            var whiskies = GetWhiskiesGg();
            foreach (var whisky in whiskies)
            {
                Assert.NotNull(whisky.Name);
            }
        }

        [Fact]
        public void WhiskyListEmpty_WithFaultyXpathsForDiscount()
        {
            // This test only works correct if the unit tested page does NOT contain any discounts
            var whiskies = GetWhiskies_WhenDiscountXpathsAreUsed_InSteadOf_RegularXpaths();
            Assert.Empty(whiskies);
        }

        [Fact]
        public void Discount_HtmlPageContains3Whiskies()
        {
            // This test only works correct if the unit tested page contains exactly 3 discounts
            var whiskies = GetWhiskiesGgDiscount();
            Assert.Equal(3, whiskies.Count);
        }

        [Fact]
        public void Discount_PricesAreNot0()
        {
            var whiskies = GetWhiskiesGgDiscount();
            foreach (var whisky in whiskies)
            {
                Assert.NotEqual(0, whisky.Price);
            }
        }

        [Fact]
        public void MatchWorks()
        {
            var priceList = new List<string> { "0.0", "0,0", "0.0235235", "0,0235235", "1241240.0", "1241240,0", "0124124.1241240" };
            foreach (var price in priceList)
            {
                var output = string.Empty;
                var match = Regex.Match(price, @"\d+[.,]?\d+", RegexOptions.RightToLeft);
                if (match.Success)
                {
                    output = match.Value.Replace(".", ",");
                }

                Assert.NotNull(output);
                Assert.Contains(",", output);
            }
        }

        [Fact]
        public void RegularPage_DG_WhiskieListNotNull()
        {
            var whiskies = GetWhiskiesDg();
            foreach (var whisky in whiskies)
            {
                Assert.NotEqual(0, whisky.Price);
            }
        }

        [Fact]
        public void RegularPage_DG_PricesAreNot0()
        {
            var whiskies = GetWhiskiesDg();
            Assert.NotEmpty(whiskies);
        }

        private List<Whisky> GetWhiskiesGg()
        {
            var sut = CreateHtmlConverter();
            var content = GetRegularGgContent();
            return sut.ReadHtmlDocument(content, GGxPathMain, GGxPathPriceBase, GGxPathName, 
                GGxPathInStock, GGxPathInStockMatch, "GG", GGxPathType, GGxPathPriceDecimals);
        }

        private List<Whisky> GetWhiskies_WhenDiscountXpathsAreUsed_InSteadOf_RegularXpaths()
        {
            var sut = CreateHtmlConverter();
            var content = GetRegularGgContent();
            return sut.ReadHtmlDocument(content, GGxPathMain, GGxPathPriceDiscountBase, GGxPathName, 
                GGxPathInStock, GGxPathInStockMatch, "GG", GGxPathType, GGxPathPriceDiscountDecimals);
        }

        private List<Whisky> GetWhiskiesGgDiscount()
        {
            var sut = CreateHtmlConverter();
            var content = File.ReadAllText("Sources\\DiscountPage20201110.html");
            return sut.ReadHtmlDocument(content, GGxPathMain, GGxPathPriceBase, GGxPathName, 
                GGxPathInStock, GGxPathInStockMatch, "GG", GGxPathType, 
                GGxPathPriceDecimals, GGxPathPriceDiscountBase, GGxPathPriceDiscountDecimals);
        }

        private List<Whisky> GetWhiskiesDg()
        {
            var sut = CreateHtmlConverter();
            var content = File.ReadAllText("Sources\\PageContentDG20201111.html");
            return sut.ReadHtmlDocument(content, DGxPathMain, DGxPathPriceBase, DGxPathName, 
                DGxPathInStock, DGxPathInStockMatch, "DG");
        }

        private string GetRegularGgContent()
        {
            return File.ReadAllText("Sources\\PageContent20201109.html");
        }
    }

    public abstract class ScraperTestsBase
    {
        protected const string GGxPathMain = "//*[@class='o-col-6 o-col-4--lg product-grid__product']";
        protected const string GGxPathType = ".//span[@class='a-category-label']";

        protected const string GGxPathPriceBase =
            "//*[@id=\"content\"]/div[2]/div[4]/div[2]/div[4]/div/div[{{x}}]/div/div/a/div[3]/div[1]/div/div/div/span[1]";

        protected const string GGxPathPriceDiscountBase =
            "//*[@id=\"content\"]/div[2]/div[4]/div[2]/div[4]/div/div[{{x}}]/div/div/a/div[3]/div[1]/div/div/div[2]/span[1]";

        protected const string GGxPathPriceDiscountDecimals =
            "//*[@id=\"content\"]/div[2]/div[4]/div[2]/div[4]/div/div[{{x}}]/div/div/a/div[3]/div[1]/div/div/div[2]/span[2]";

        protected const string GGxPathPriceDecimals =
            "//*[@id=\"content\"]/div[2]/div[4]/div[2]/div[4]/div/div[{{x}}]/div/div/a/div[3]/div[1]/div/div/div/span[2]";

        protected const string GGxPathName = ".//a[@class='product-tile__title-inner']";
        protected const string GGxPathInStock = ".//span[@class='button__label']";
        protected const string GGxPathInStockMatch = "Fles";

        protected const string DGxPathMain = "//*[@class='item product product-item']";
        protected const string DGxPathPriceBase = ".//span[@class='price']";
        protected const string DGxPathName = ".//a[@class='product-item-link']";
        protected const string DGxPathInStock = ".//p[@class='availability in-stock']";
        protected const string DGxPathInStockMatch = "Direct leverbaar !";

        protected readonly Mock<ILogger> LoggerMock = new Mock<ILogger>();

        protected HtmlConverter CreateHtmlConverter()
        {
            return new HtmlConverter(LoggerMock.Object);
        }

        protected void VerifyMocks()
        {
            LoggerMock.Verify();
        }
    }
}