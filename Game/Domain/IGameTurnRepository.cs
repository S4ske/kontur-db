using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public interface IGameTurnRepository
    {
        GameTurnEntity Insert(GameTurnEntity gameTurn);
        List<GameTurnEntity> GetLastTurns(Guid gameId, int turnsCount);
        List<GameTurnEntity> GetTurnsByGame(Guid gameId);
    }
}