using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductScraper.Helpers;
using ProductScraper.Scrapers;
using Repository.Data;
using Repository.Services;
using System;
using System.Threading.Tasks;

namespace ProductScraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddScoped<IHttpHandler, HttpClientHandler>()
                .AddScoped<IWhiskyService, WhiskyService>()
                .AddScoped<IUtils, Utils>()
                .AddScoped<WhiskyContext>()
                .BuildServiceProvider();

            // Configure logger
            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            try
            {
                MainAsync(logger, serviceProvider).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.LogError("Something went wrong in application...", e);
                throw;
            }
        }

        private static async Task MainAsync(ILogger logger, IServiceProvider serviceProvider)
        {
            logger.LogDebug("Starting application");

            // Use service
            var context = serviceProvider.GetService<WhiskyContext>();
            context.Database.Migrate();

            var httpHandler = serviceProvider.GetService<IHttpHandler>();
            var whiskyService = serviceProvider.GetService<IWhiskyService>();
            var utils = serviceProvider.GetService<IUtils>();

            // Start scraper
            logger.LogDebug("Starting scraper");
            var whiskyScraper = new WhiskyScraper(logger, httpHandler, whiskyService, utils);

            // Get scraped whiskies
            var whiskies = await whiskyScraper.Initialize();

            // Create audit trail
            var discounts = whiskyScraper.ProcessScrapedWhiskies(whiskies);
            if (discounts.Count > 0)
            {
                var emailHelper = new EmailHelper(logger);
                var finalBody = emailHelper.CreateEmailBody(discounts);
                emailHelper.SendEmail(finalBody, $"Whisky discounts found!");
            }

            logger.LogDebug("All done!");
        }
    }
}