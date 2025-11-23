using System;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);

            CreateLoginIndex(userCollection);
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection.Find(Builders<UserEntity>.Filter.Eq(u => u.Id, id)).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.Login, login);
            var update = Builders<UserEntity>.Update
                .SetOnInsert(u => u.Id, Guid.NewGuid())
                .SetOnInsert(u => u.Login, login);
    
            var options = new FindOneAndUpdateOptions<UserEntity>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return userCollection.FindOneAndUpdate(filter, update, options);
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(
                Builders<UserEntity>.Filter.Eq(u => u.Id, user.Id),
                user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(Builders<UserEntity>.Filter.Eq(u => u.Id, id));
        }

        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var items = userCollection.Find(Builders<UserEntity>.Filter.Empty)    
                .SortBy(u => u.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
            var totalCount = userCollection.CountDocuments(Builders<UserEntity>.Filter.Empty);
            
            return new PageList<UserEntity>(
                items,
                totalCount,
                pageNumber,
                pageSize);
        }

        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }

        private static void CreateLoginIndex(IMongoCollection<UserEntity> collection)
        {
            var indexKeys = Builders<UserEntity>.IndexKeys.Ascending(x => x.Login);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<UserEntity>(indexKeys, indexOptions);
            collection.Indexes.CreateOne(indexModel);
        }
    }
}