﻿using BusinessLogic.DataAccess;
using BusinessLogic.Logic;
using BusinessLogic.Models;
using BusinessLogic.Models.Players;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace BusinessLogic.DataAccess.Repositories
{
    public class EntityFrameworkPlayerRepository : PlayerRepository
    {
        internal const string EXCEPTION_PLAYER_NOT_FOUND = "The specified player does not exist.";
        internal const string EXCEPTION_USER_DOES_NOT_HAVE_ACCESS_TO_THIS_PLAYER = 
            "User with user id '{0}' does not have access to player with player id '{1}'";
        public int MINIMUM_NUMBER_OF_GAMES_TO_BE_A_NEMESIS = 3;

        private static readonly string SQL_GET_WIN_LOSS_GAMES_COUNT =
            @"SELECT SUM(NumberOfGamesLost) AS NumberOfGamesLost, SUM(NumberOfGamesWon) AS NumberOfGamesWon, PlayerId as VersusPlayerId
            FROM
            (
	            SELECT COUNT(*) AS NumberOfGamesLost, 0 AS NumberOfGamesWon, PlayerId
	              FROM
	              (
		              SELECT OtherResults.PlayerId
		              ,OtherResults.PlayedGameId
		              ,OtherResults.GameRank
		              , PlayedGame.GameDefinitionId
		              FROM PlayerGameResult Inner Join PlayedGame ON PlayergameResult.PlayedGameId = PlayedGame.Id
		              INNER JOIN PlayerGameResult OtherResults ON PlayedGame.Id = OtherResults.PlayedGameId
		              WHERE PlayerGameResult.PlayerId = @PlayerId
		              AND OtherResults.GameRank < 
			            (
				            SELECT GameRank FROM PlayerGameResult PGR 
				            WHERE PGR.PlayedGameId = OtherResults.PlayedGameId 
				            AND PGR.PlayerId = @PlayerId
			            )
	               ) AS LostGames
	               GROUP BY PlayerId
	               UNION
	               SELECT 0 AS NumberOfgamesLost, COUNT(*) AS NumberOfGamesWon, PlayerId
	              FROM
	              (
		              SELECT OtherResults.PlayerId
		              ,OtherResults.PlayedGameId
		              ,OtherResults.GameRank
		              , PlayedGame.GameDefinitionId
		              FROM PlayerGameResult Inner Join PlayedGame ON PlayergameResult.PlayedGameId = PlayedGame.Id
		              INNER JOIN PlayerGameResult OtherResults ON PlayedGame.Id = OtherResults.PlayedGameId
		              WHERE PlayerGameResult.PlayerId = @PlayerId
		              AND OtherResults.GameRank > 
			            (
				            SELECT GameRank FROM PlayerGameResult PGR 
				            WHERE PGR.PlayedGameId = OtherResults.PlayedGameId 
				            AND PGR.PlayerId = @PlayerId
			            ) 
	               ) AS WonGames
	               GROUP BY PlayerId
            ) AS X
            GROUP BY PlayerId";

        private NemeStatsDbContext dbContext;

        public EntityFrameworkPlayerRepository(NemeStatsDbContext context)
        {
            dbContext = context;
        }

        internal virtual Player GetPlayer(int playerID, ApplicationUser currentUser)
        {
            Player returnPlayer = dbContext.Players
                .Where(player => player.Id == playerID)
                    .FirstOrDefault();

            ValidateAccessToPlayer(currentUser, returnPlayer);

            return returnPlayer;
        }

        private static void ValidateAccessToPlayer(ApplicationUser currentUser, Player desiredPlayer)
        {
            if (desiredPlayer != null)
            {
                if (desiredPlayer.GamingGroupId != currentUser.CurrentGamingGroupId)
                {
                    string message = string.Format(EXCEPTION_USER_DOES_NOT_HAVE_ACCESS_TO_THIS_PLAYER,
                        currentUser.Id,
                        desiredPlayer.Id);
                    throw new UnauthorizedAccessException(message);
                }
            }
        }

        public virtual PlayerDetails GetPlayerDetails(int playerID, int numberOfRecentGamesToRetrieve, ApplicationUser currentUser)
        {
            Player returnPlayer = GetPlayer(playerID, currentUser);

            ValidatePlayerWasFound(returnPlayer);

            PlayerStatistics playerStatistics = GetPlayerStatistics(playerID, currentUser);

            List<PlayerGameResult> playerGameResults = GetPlayerGameResultsWithPlayedGameAndGameDefinition(playerID, numberOfRecentGamesToRetrieve, currentUser);

            Nemesis nemesis = GetNemesis(playerID, currentUser);

            PlayerDetails playerDetails = new PlayerDetails()
            {
                Active = returnPlayer.Active,
                Id = returnPlayer.Id,
                Name = returnPlayer.Name,
                GamingGroupId = returnPlayer.GamingGroupId,
                PlayerGameResults = playerGameResults,
                PlayerStats = playerStatistics,
                Nemesis = nemesis
            };

            return playerDetails;
        }

        private static void ValidatePlayerWasFound(Player returnPlayer)
        {
            if (returnPlayer == null)
            {
                throw new ArgumentException(EXCEPTION_PLAYER_NOT_FOUND);
            }
        }

        internal virtual List<PlayerGameResult> GetPlayerGameResultsWithPlayedGameAndGameDefinition(
            int playerID, 
            int numberOfRecentGamesToRetrieve, 
            ApplicationUser currentUser)
        {
            List<PlayerGameResult> playerGameResults = dbContext.PlayerGameResults
                        .Where(result => result.PlayerId == playerID
                            && result.Player.GamingGroupId == currentUser.CurrentGamingGroupId)
                        .OrderByDescending(result => result.PlayedGame.DatePlayed)
                        .Take(numberOfRecentGamesToRetrieve)
                        .Include(result => result.PlayedGame)
                        .Include(result => result.PlayedGame.GameDefinition)
                        .ToList();
            return playerGameResults;
        }

        public List<Player> GetAllPlayers(bool active, ApplicationUser currentUser)
        {
            return dbContext.Players.Where(player => player.Active == active
                && player.GamingGroupId == currentUser.CurrentGamingGroupId).ToList();
        }

        public virtual PlayerStatistics GetPlayerStatistics(int playerId, ApplicationUser currentUser)
        {
            //TODO could hard code the below to get my integration test to pass. 
            //How do I do a 2nd test so that this would need to be fixed?
            //return new PlayerStatistics() { TotalGames = 3 };
            PlayerStatistics playerStatistics = new PlayerStatistics();
            playerStatistics.TotalGames = dbContext.PlayerGameResults.Count(playerGameResults => playerGameResults.PlayerId == playerId
                && playerGameResults.Player.GamingGroupId == currentUser.CurrentGamingGroupId);
            return playerStatistics;
        }

        //TODO refactor this. Might be tricky with anonymous data types. Should I create concrete types?
        public virtual Nemesis GetNemesis(int playerId, ApplicationUser currentUser)
        {
            //call GetPlayer just to ensure that the requesting user has access
            GetPlayer(playerId, currentUser);
            DbRawSqlQuery<WinLossStatistics> data = dbContext.Database.SqlQuery<WinLossStatistics>(SQL_GET_WIN_LOSS_GAMES_COUNT,
                new SqlParameter("PlayerId", playerId));

            List<WinLossStatistics> winLossStatistics = data.ToList<WinLossStatistics>();

            var result = (from x in winLossStatistics
                          where x.NumberOfGamesLost > x.NumberOfGamesWon
                          && x.NumberOfGamesLost >= MINIMUM_NUMBER_OF_GAMES_TO_BE_A_NEMESIS
                          select new
                          {
                              NumberOfGamesLost = x.NumberOfGamesLost,
                              LossPercentage = 100 * x.NumberOfGamesLost / (x.NumberOfGamesWon + x.NumberOfGamesLost),
                              NemesisPlayerId = x.VersusPlayerId
                          }).OrderByDescending(nemesisCandidates => nemesisCandidates.LossPercentage).FirstOrDefault();

            if (result == null)
            {
                return new NullNemesis();
            }

            Nemesis nemesis = new Nemesis();
            Player nemesisPlayer = dbContext.Players.Find(result.NemesisPlayerId);
            nemesis.NemesisPlayerId = nemesisPlayer.Id;
            nemesis.NemesisPlayerName = nemesisPlayer.Name;
            nemesis.GamesLostVersusNemesis = result.NumberOfGamesLost;
            nemesis.LossPercentageVersusNemesis = result.LossPercentage;

            return nemesis;
        }

        //TODO I did not write tests for this as its tested in GameDefinitionRepository and this needs to be refactored out as part
        //of a real repository base class
        public virtual Player Save(Player player, ApplicationUser currentUser)
        {
            if (player.AlreadyInDatabase())
            {
                ValidateAccessToPlayer(currentUser, player);
                dbContext.Entry(player).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                //TODO should throw some kind of exception if GamingGroupId is null
                player.GamingGroupId = currentUser.CurrentGamingGroupId.Value;
                dbContext.Players.Add(player);
            }

            dbContext.SaveChanges();

            return player;
        }

        //TODO I did not write tests for this as its tested in GameDefinitionRepository and this needs to be refactored out as part
        //of a real repository base class
        public virtual void Delete(int playerId, ApplicationUser currentUser)
        {
            Player player = GetPlayer(playerId, currentUser);

            dbContext.Players.Remove(player);
            dbContext.SaveChanges();
        }
    }
}