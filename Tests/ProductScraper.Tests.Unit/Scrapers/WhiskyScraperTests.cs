using Microsoft.Extensions.Logging;
using Moq;
using ProductScraper.Helpers;
using ProductScraper.Scrapers;
using Repository.Data.Entities;
using Repository.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Xunit;
using WhiskyModel = ProductScraper.Models.Whisky;

namespace ProductScraper.Tests.Unit.Scrapers
{
    public class WhiskyScraperTests
    {
        public class GetScrapedWhiskies : WhiskyScraperTestsBase
        {
            private readonly string _urlToScrape = "url";
            private readonly HttpResponseMessage _responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("blaat")
            };
            private readonly string _pageContent = "<div>fake content</div>";

            [Fact]
            public void Bla()
            {
                HttpHandlerMock
                    .Setup(s => s.GetAsync(_urlToScrape))
                    .ReturnsAsync(_responseMessage)
                    .Verifiable();

                HttpHandlerMock
                    .Setup(s => s.ReadAsStringAsync(_responseMessage.Content))
                    .ReturnsAsync(_pageContent)
                    .Verifiable();

                UtilsMock
                    .Setup(s => s.Sleep(1))
                    .Verifiable();

                var sut = CreateWhiskyScraper();

                // Act
                // TODO: FIX (ConfigurationManager) var result = await sut.GetScrapedWhiskies(_random, _converter, "GG");

                // Assert

            }
        }

        public class ProcessScrapedWhiskies : WhiskyScraperTestsBase
        {
            private const string Input = "Something";
            private readonly Whisky _whiskyResponse = new Whisky
            {
                Name = "Name",
                Price = 35.00m
            };

            [Fact]
            public void AddWhisky_Something()
            {
                var whiskies = new List<WhiskyModel>
                {
                    new WhiskyModel(LoggerMock.Object, "Type", "35", "Name", true, null)
                    {
                        Name = "Name",
                        Price = 35.00m
                    }
                };

                WhiskyServiceMock
                    .Setup(s => s.GetWhisky(Input, Input, Input))
                    .Returns((Whisky)null);

                var sut = CreateWhiskyScraper();

                // Act
                var result = sut.ProcessScrapedWhiskies(whiskies);

                // Assert
                // TODO:
            }

            [Fact]
            public void EditWhisky_Something()
            {
                var whiskies = new List<WhiskyModel>
                {
                    new WhiskyModel(LoggerMock.Object, "Type", "35", "Name", true, null)
                    {
                        Name = "Name",
                        Price = 35.00m
                    }
                };

                WhiskyServiceMock
                    .Setup(s => s.GetWhisky("Name", "Type"))
                    .Returns(_whiskyResponse);

                var sut = CreateWhiskyScraper();

                // Act
                var result = sut.ProcessScrapedWhiskies(whiskies);

                // Assert
                // TODO:
            }
        }

        public abstract class WhiskyScraperTestsBase
        {
            protected readonly Mock<ILogger> LoggerMock = new Mock<ILogger>();
            protected readonly Mock<IWhiskyService> WhiskyServiceMock = new Mock<IWhiskyService>();
            protected readonly Mock<IHttpHandler> HttpHandlerMock = new Mock<IHttpHandler>();
            protected readonly Mock<IUtils> UtilsMock = new Mock<IUtils>();

            protected WhiskyScraper CreateWhiskyScraper()
            {
                return new WhiskyScraper(LoggerMock. Object, HttpHandlerMock.Object, WhiskyServiceMock.Object, UtilsMock.Object);
            }

            protected void VerifyMocks()
            {
                LoggerMock.Verify();
                WhiskyServiceMock.Verify();
                HttpHandlerMock.Verify();
                UtilsMock.Verify();
            }
        }
    }
}