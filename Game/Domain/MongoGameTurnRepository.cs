using MongoDB.Driver;
using System.Collections.Generic;
using System;

namespace Game.Domain
{
    public class MongoGameTurnRepository : IGameTurnRepository
    {
        private const string CollectionName = "gameTurns";
        private readonly IMongoCollection<GameTurnEntity> gameTurnCollection;

        public MongoGameTurnRepository(IMongoDatabase db)
        {
            gameTurnCollection = db.GetCollection<GameTurnEntity>(CollectionName);

            var indexKeysDefinition = Builders<GameTurnEntity>.IndexKeys
                .Ascending(t => t.GameId)
                .Descending(t => t.TurnIndex);
            gameTurnCollection.Indexes.CreateOne(new CreateIndexModel<GameTurnEntity>(indexKeysDefinition));
        }

        public GameTurnEntity Insert(GameTurnEntity gameTurn)
        {
            gameTurnCollection.InsertOne(gameTurn);
            return gameTurn;
        }

        public List<GameTurnEntity> GetLastTurns(Guid gameId, int turnsCount)
        {
            return gameTurnCollection
                .Find(t => t.GameId == gameId)
                .SortByDescending(t => t.TurnIndex)
                .Limit(turnsCount)
                .ToList();
        }

        public List<GameTurnEntity> GetTurnsByGame(Guid gameId)
        {
            return gameTurnCollection
                .Find(t => t.GameId == gameId)
                .SortByDescending(t => t.TurnIndex)
                .ToList();
        }
    }
}