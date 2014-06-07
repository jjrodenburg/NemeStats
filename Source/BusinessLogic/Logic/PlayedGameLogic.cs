﻿using BusinessLogic.Models;
using System;
using System.Collections.Generic;
namespace BusinessLogic.Logic
{
    public interface PlayedGameLogic
    {
        PlayedGame GetPlayedGameDetails(int playedGameId);
        List<PlayedGame> GetRecentGames(int numberOfGames);
    }
}