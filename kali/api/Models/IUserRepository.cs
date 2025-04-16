namespace api.Models
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetById(string id);
        User? GetByEmail(string email);
        void Insert(User user);
        bool Update(User user);
        bool Delete(string id);
    }
}