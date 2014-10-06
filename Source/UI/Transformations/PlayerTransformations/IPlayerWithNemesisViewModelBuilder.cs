﻿using BusinessLogic.Models;
using System;
using UI.Models.Players;
namespace UI.Transformations.PlayerTransformations
{
    public interface IPlayerWithNemesisViewModelBuilder
    {
        PlayerWithNemesisViewModel Build(Player player);
    }
}