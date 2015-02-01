﻿using BusinessLogic.Migrations;
using BusinessLogic.Models;
using BusinessLogic.Models.PlayedGames;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using UI.Models.PlayedGame;

namespace UI.Transformations
{
    public class PlayedGameDetailsViewModelBuilder : IPlayedGameDetailsViewModelBuilder
    {
        internal const string EXCEPTION_MESSAGE_GAME_DEFINITION_CANNOT_BE_NULL = "PlayedGame.GameDefinition cannot be null.";
        internal const string EXCEPTION_MESSAGE_PLAYER_GAME_RESULTS_CANNOT_BE_NULL = "PlayedGame.PlayerGameResults cannot be null.";
        internal const string EXCEPTION_MESSAGE_GAMING_GROUP_CANNOT_BE_NULL = "PlayedGame.GamingGroup cannnot be null.";
        internal const string NEWLINE_REPLACEMENT_FOR_HTML = "<br/>";

        private readonly IGameResultViewModelBuilder playerResultBuilder;

        public PlayedGameDetailsViewModelBuilder(IGameResultViewModelBuilder playerGameResultBuilder)
        {
            playerResultBuilder = playerGameResultBuilder;
        }

        public PlayedGameDetailsViewModel Build(PlayedGame playedGame, ApplicationUser currentUser)
        {
            ValidateArguments(playedGame);
            
            PlayedGameDetailsViewModel summary = new PlayedGameDetailsViewModel();
            summary.GameDefinitionName = playedGame.GameDefinition.Name;
            summary.GameDefinitionId = playedGame.GameDefinitionId;
            summary.PlayedGameId = playedGame.Id;
            summary.DatePlayed = playedGame.DatePlayed;
            summary.GamingGroupId = playedGame.GamingGroup.Id;
            summary.GamingGroupName = playedGame.GamingGroup.Name;
            if (playedGame.Notes != null)
            {
                summary.Notes = playedGame.Notes.Replace(Environment.NewLine, NEWLINE_REPLACEMENT_FOR_HTML);
            }
            
            summary.UserCanEdit = (currentUser != null && playedGame.GamingGroupId == currentUser.CurrentGamingGroupId);
            summary.PlayerResults = new List<GameResultViewModel>();
            
            foreach(PlayerGameResult playerGameResult in playedGame.PlayerGameResults)
            {
                summary.PlayerResults.Add(playerResultBuilder.Build(playerGameResult));
            }

            SetWinnerType(playedGame, summary);

            return summary;
        }

        private static void ValidateArguments(PlayedGame playedGame)
        {
            if (playedGame == null)
            {
                throw new ArgumentNullException("playedGame");
            }

            if (playedGame.GamingGroup == null)
            {
                throw new ArgumentException(EXCEPTION_MESSAGE_GAMING_GROUP_CANNOT_BE_NULL);
            }

            if (playedGame.GameDefinition == null)
            {
                throw new ArgumentException(EXCEPTION_MESSAGE_GAME_DEFINITION_CANNOT_BE_NULL);
            }

            if (playedGame.PlayerGameResults == null)
            {
                throw new ArgumentException(EXCEPTION_MESSAGE_PLAYER_GAME_RESULTS_CANNOT_BE_NULL);
            }
        }


        private static void SetWinnerType(PlayedGame playedGame, PlayedGameDetailsViewModel summary)
        {
            if (playedGame.PlayerGameResults.All(x => x.GameRank == 1))
            {
                summary.WinnerType = WinnerTypes.TeamWin;
            }
            else if (playedGame.PlayerGameResults.All(x => x.GameRank != 1))
            {
                summary.WinnerType = WinnerTypes.TeamLoss;
            }
            else
            {
                summary.WinnerType = WinnerTypes.PlayerWin;
            }
        }
    }
}