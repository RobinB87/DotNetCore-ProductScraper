using Repository.Data.Entities;

namespace Repository.Services
{
    public interface IWhiskyService
    {
        void SaveChanges();
        Whisky GetWhisky(string name, string store);
        Whisky GetWhisky(string name, string store, string type);
        void AddWhisky(Whisky whisky, int? originalId = null);
        void EditWhisky(Whisky whisky);
    }
}
