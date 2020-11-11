using Repository.Data;
using Repository.Data.Entities;
using System;
using System.Linq;

namespace Repository.Services
{
    public class WhiskyService : IWhiskyService
    {
        private readonly WhiskyContext _context;

        public WhiskyService(WhiskyContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public Whisky GetWhisky(string name, string store)
        {
            return _context.Whiskies.FirstOrDefault(
                w => w.Name == name && w.Store == store);
        }

        public Whisky GetWhisky(string name, string store, string type)
        {
            return _context.Whiskies.FirstOrDefault(
                w => w.Name == name && w.Store == store && w.Type == type);
        }

        public void AddWhisky(Whisky whisky, int? originalId = null)
        {
            whisky.CreateDt = DateTime.Now;
            whisky.ModifyDt = DateTime.Now;
            whisky.IsActive = true;
            _context.Whiskies.Add(whisky);
            
            // Save to get the new id
            _context.SaveChanges();

            // Set original id, based on existing, or just inserted entity
            whisky.OriginalId = originalId ?? whisky.Id;

            // Finally, update the record with the original id
            _context.Whiskies.Update(whisky);
        }

        public void EditWhisky(Whisky whisky)
        {
            whisky.ModifyDt = DateTime.Now;
            whisky.IsActive = false;
            _context.Whiskies.Update(whisky);
        }
    }
}