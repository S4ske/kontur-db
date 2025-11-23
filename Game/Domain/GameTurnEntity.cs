using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public class GameTurnEntity
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public int TurnIndex { get; set; }
        public DateTime FinishedAt { get; set; }

        public List<PlayerTurnResult> Players { get; init; } = new();
        public Guid? WinnerId { get; set; }

        public GameTurnEntity(Guid gameId, int turnIndex)
        {
            Id = Guid.NewGuid();
            GameId = gameId;
            TurnIndex = turnIndex;
            FinishedAt = DateTime.UtcNow;
        }
    }

    public class PlayerTurnResult
    {
        public Guid UserId { get; init; }
        public string UserName { get; set; }
        public PlayerDecision Decision { get; set; }
        public TurnResult Result { get; set; }
    }

    public enum TurnResult
    {
        Lost = 0,
        Won = 1,
        Draw = 2
    }
}