﻿using BusinessLogic.Models;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataAccess.Repositories
{
    public interface GameDefinitionRepository
    {
        List<GameDefinition> GetAllGameDefinitions(ApplicationUser currentUser);
        GameDefinition GetGameDefinition(int gameDefinitionId, ApplicationUser currentUser);
        GameDefinition Save(GameDefinition gameDefinition, ApplicationUser currentUser);
        void Delete(int gameDefinitionId, ApplicationUser currentUser);
    }
}