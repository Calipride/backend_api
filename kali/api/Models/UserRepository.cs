using LiteDB;

namespace api.Models
{
    public class UserRepository : IUserRepository
    {
        private readonly ILiteDatabase _database;
        private readonly ILiteCollection<User> _users;

        public UserRepository(ILiteDatabase database)
        {
            _database = database;
            _users = database.GetCollection<User>("users");
            _users.EnsureIndex(x => x.Email, unique: true);
        }

        public IEnumerable<User> GetAll()
        {
            return _users.FindAll();
        }

        public User? GetById(string id)
        {
            return _users.FindById(id);
        }

        public User? GetByEmail(string email)
        {
            return _users.FindOne(x => x.Email == email);
        }

        public void Insert(User user)
        {
            _users.Insert(user);
        }

        public bool Update(User user)
        {
            return _users.Update(user);
        }

        public bool Delete(string id)
        {
            return _users.Delete(id);
        }
    }
}