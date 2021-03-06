﻿using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Web.Mvc;
using Shouldly;
using UI.Controllers;
using UI.Models.GameDefinitionModels;

namespace UI.Tests.UnitTests.ControllerTests.GameDefinitionControllerTests
{
    [TestFixture]
    public class ShowTrendingGamesTests : GameDefinitionControllerTestBase
    {
        [Test]
        public void It_Returns_A_ViewResult_With_The_Specified_Number_Of_Trending_Games_Over_The_Specified_Number_Of_Days()
        {
            //--assert
            var expectedViewModels = new List<TrendingGameViewModel>();
            autoMocker.PartialMockTheClassUnderTest();
            autoMocker.ClassUnderTest.Expect(mock =>
                    mock.GetTrendingGamesViewModels(Arg<int>.Is.Anything, Arg<int>.Is.Anything))
                .Return(expectedViewModels);

            //--act
            var results = autoMocker.ClassUnderTest.ShowTrendingGames();

            //--assert
            var viewResult = results as ViewResult;
            viewResult.ShouldNotBeNull();
            autoMocker.ClassUnderTest.AssertWasCalled(mock =>
                mock.GetTrendingGamesViewModels(Arg<int>.Is.Equal(GameDefinitionController.NUMBER_OF_TRENDING_GAMES_TO_SHOW), Arg<int>.Is.Equal(GameDefinitionController.NUMBER_OF_DAYS_OF_TRENDING_GAMES)));

            viewResult.Model.ShouldBeSameAs(expectedViewModels);
        }
    }
}