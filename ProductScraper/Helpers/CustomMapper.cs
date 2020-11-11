using ProductScraper.Models;

namespace ProductScraper.Helpers
{
    public static class CustomMapper
    {
        public static Repository.Data.Entities.Whisky MapWhisky(Whisky whisky)
        {
            return new Repository.Data.Entities.Whisky
            {
                Name = whisky.Name,
                Price = whisky.Price,
                InStock = whisky.InStock,
                Type = whisky.Type,
                Content = whisky.Content,
                Store = whisky.Store,
                Taste = whisky.Taste,
                Country = whisky.Country
            };
        }
    }
}
