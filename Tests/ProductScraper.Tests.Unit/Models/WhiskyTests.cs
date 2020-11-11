using Microsoft.Extensions.Logging;
using Moq;
using ProductScraper.Models;
using Xunit;

namespace ProductScraper.Tests.Unit.Models
{
    public class WhiskyTests
    {
        protected readonly Mock<ILogger> LoggerMock = new Mock<ILogger>();
        private const string JustSomeString = "Name";
        private const bool JustSomeBool = false;
        private const decimal ExpectedPrice = 0.57m;

        [Fact]
        public void Price_As_EmptyString_ConvertsTo_0()
        {
            var whisky = new Whisky(LoggerMock.Object, JustSomeString, string.Empty, JustSomeString, JustSomeBool, JustSomeString);
            Assert.Equal(0, whisky.Price);
        }

        [Fact]
        public void Price_As_WeirdString_ConvertsTo_0()
        {
            var whisky = new Whisky(LoggerMock.Object, JustSomeString, "adl.k,.wet346347ryhadffkjasd;flkj", JustSomeString, JustSomeBool, JustSomeString);
            Assert.Equal(0, whisky.Price);
        }

        [Fact]
        public void OneLeading0_CorrectlyProcessed()
        {
            var whisky = new Whisky(LoggerMock.Object, JustSomeString, "0.57", JustSomeString, JustSomeBool, JustSomeString);
            Assert.Equal(ExpectedPrice, whisky.Price);
        }

        [Fact]
        public void DoubleLeading0_Removed()
        {
            var whisky = new Whisky(LoggerMock.Object, JustSomeString, "00.57", JustSomeString, JustSomeBool, JustSomeString);
            Assert.Equal(ExpectedPrice, whisky.Price);
        }

        [Fact]
        public void TripleLeading0_Removed()
        {
            var whisky = new Whisky(LoggerMock.Object, JustSomeString, "000.57", JustSomeString, JustSomeBool, JustSomeString);
            Assert.Equal(ExpectedPrice, whisky.Price);
        }
    }
}
