using System.Text.Json;

namespace DynamicAuthorization
{
    public class PermissionsDatabase
    {
        private readonly string _dbPath;
        private JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true };
        public PermissionsDatabase(IWebHostEnvironment env)
        {
            _dbPath = Path.Combine(env.ContentRootPath, "permissions.json");
        }

        private Dictionary<string, HashSet<string>> Record => 
            File.Exists(_dbPath) ?
            JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(File.ReadAllText(_dbPath)) :
            new();

        public bool HasPermissions(string userId, string permission)
        {
            var db = Record;
            return db.ContainsKey(userId) && db[userId].Contains(permission);
        }

        public void AddPermissions(string userId, string permission)
        {
            var db = Record;
            if (db[userId] == null && !db[userId].Contains(permission)) return;

            db[userId].Add(permission);
            File.WriteAllText(_dbPath, JsonSerializer.Serialize(db, serializerOptions));
        }

        public void RemovePermissions(string userId, string permission)
        {
            var db = Record;
            if (db[userId] == null && !db[userId].Contains(permission)) return;

            db[userId].Remove(permission);
            File.WriteAllText(_dbPath, JsonSerializer.Serialize(db, serializerOptions));
        }
    }
}
