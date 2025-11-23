using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Game.Domain
{
    public class GameEntity
    {
        [BsonElement]
        private readonly List<Player> players;

        public GameEntity(int turnsCount)
            : this(Guid.Empty, GameStatus.WaitingToStart, turnsCount, 0, new List<Player>())
        {
        }

        [BsonConstructor]
        public GameEntity(Guid id, GameStatus status, int turnsCount, int currentTurnIndex, List<Player> players)
        {
            Id = id;
            Status = status;
            TurnsCount = turnsCount;
            CurrentTurnIndex = currentTurnIndex;
            this.players = players;
        }

        public Guid Id { get; private set; }

        public IReadOnlyList<Player> Players => players.AsReadOnly();

        public int TurnsCount { get; }

        public int CurrentTurnIndex { get; private set; }

        public GameStatus Status { get; private set; }

        public void AddPlayer(UserEntity user)
        {
            if (Status != GameStatus.WaitingToStart)
                throw new ArgumentException(Status.ToString());

            players.Add(new Player(user.Id, user.Login));

            if (players.Count == 2)
                Status = GameStatus.Playing;
        }

        public bool IsFinished()
        {
            return Status == GameStatus.Finished
                   || Status == GameStatus.Canceled
                   || CurrentTurnIndex >= TurnsCount;
        }

        public void Cancel()
        {
            if (!IsFinished())
                Status = GameStatus.Canceled;
        }

        public bool HaveDecisionOfEveryPlayer => players.All(p => p.Decision.HasValue);

        public void SetPlayerDecision(Guid userId, PlayerDecision decision)
        {
            if (Status != GameStatus.Playing)
                throw new InvalidOperationException(Status.ToString());

            foreach (var player in players.Where(p => p.UserId == userId))
            {
                if (player.Decision.HasValue)
                    throw new InvalidOperationException(player.Decision.ToString());

                player.Decision = decision;
            }
        }

        public GameTurnEntity FinishTurn()
        {
            if (!HaveDecisionOfEveryPlayer)
                throw new InvalidOperationException("Not all players made decisions");

            var turn = new GameTurnEntity(Id, CurrentTurnIndex)
            {
                Players = players
                    .Select(p => new PlayerTurnResult
                    {
                        UserId = p.UserId,
                        UserName = p.Name,
                        Decision = p.Decision.Value
                    })
                    .ToList()
            };

            var decisions = players.Select(p => p.Decision.Value).Distinct().ToList();

            if (decisions.Count == 2)
            {
                var winning = GetWinningDecision(decisions[0], decisions[1]);
                var winner = players.First(p => p.Decision == winning);

                winner.Score++;
                turn.WinnerId = winner.UserId;

                foreach (var result in turn.Players)
                    result.Result = result.UserId == winner.UserId ? TurnResult.Won : TurnResult.Lost;
            }
            else
            {
                foreach (var result in turn.Players)
                    result.Result = TurnResult.Draw;
            }

            foreach (var p in players)
                p.Decision = null;

            CurrentTurnIndex++;

            return turn;
        }

        private static PlayerDecision GetWinningDecision(PlayerDecision d1, PlayerDecision d2)
        {
            if ((d1 == PlayerDecision.Rock && d2 == PlayerDecision.Scissors) ||
                (d1 == PlayerDecision.Scissors && d2 == PlayerDecision.Paper) ||
                (d1 == PlayerDecision.Paper && d2 == PlayerDecision.Rock))
                return d1;

            return d2;
        }
    }
}
