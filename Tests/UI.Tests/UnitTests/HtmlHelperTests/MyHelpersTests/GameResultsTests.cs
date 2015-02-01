﻿using BusinessLogic.Models.PlayedGames;
using NUnit.Framework;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Rhino.Mocks.Constraints;
using UI.HtmlHelpers;
using UI.Models.PlayedGame;

namespace UI.Tests.UnitTests.HtmlHelperTests.MyHelpersTests
{
    [TestFixture]
    public class GameResultsTests
    {
        [Test]
        public void ItRequiresPlayerGameResultDetails()
        {
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new ViewPage());
            
            var exception = Assert.Throws<ArgumentNullException>(() =>
                helper.GameResults(null, WinnerTypes.PlayerWin));

            Assert.AreEqual("playerGameResultDetails", exception.ParamName);
        }

        [Test]
        public void ItRendersTheGameRankInASpanWithTheGameRankClass()
        {
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new ViewPage());
            GameResultViewModel playerGameResultDetails = new GameResultViewModel()
            {
                GameRank = 1151
            };

            XElement result = helper.GameResults(playerGameResultDetails, WinnerTypes.PlayerWin).ToXElement();

            Assert.AreEqual("span", result.Name.ToString());
            Assert.True(result.FirstAttribute.ToString().Contains("class=\"" + PlayedGameHelper.CSS_CLASS_GAME_RANK));
        }

        private void TestRenderingForGivenRank(int gameRank, WinnerTypes winnerType, string expectedCssClass, string expectedRankText)
        {
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new ViewPage());
            GameResultViewModel playerGameResultDetails = new GameResultViewModel()
            {
                GameRank = gameRank
            };

            XElement result = helper.GameResults(playerGameResultDetails, winnerType).ToXElement();

            string firstAttribute = result.FirstAttribute.ToString();
            StringAssert.Contains("class=\"", firstAttribute);
            StringAssert.Contains(expectedCssClass, firstAttribute);
            string firstNodeText = result.FirstNode.ToString();
            StringAssert.StartsWith(expectedRankText, firstNodeText);
        }

        [Test]
        public void ItRendersFirstPlace()
        {
            TestRenderingForGivenRank(1, WinnerTypes.PlayerWin, PlayedGameHelper.CSS_CLASS_FIRST_PLACE, PlayedGameHelper.PLACE_FIRST);
        }

        [Test]
        public void ItRendersSecondPlace()
        {
            TestRenderingForGivenRank(2, WinnerTypes.PlayerWin, PlayedGameHelper.CSS_CLASS_SECOND_PLACE, PlayedGameHelper.PLACE_SECOND);
        }

        [Test]
        public void ItRendersThirdPlace()
        {
            TestRenderingForGivenRank(3, WinnerTypes.PlayerWin, PlayedGameHelper.CSS_CLASS_THIRD_PLACE, PlayedGameHelper.PLACE_THIRD);
        }

        [Test]
        public void ItRendersFourthPlace()
        {
            TestRenderingForGivenRank(4, WinnerTypes.PlayerWin, PlayedGameHelper.CSS_CLASS_FOURTH_PLACE, PlayedGameHelper.PLACE_FOURTH);
        }

        [Test]
        public void ItRendersTheBigLoser()
        {
            TestRenderingForGivenRank(5, WinnerTypes.PlayerWin, PlayedGameHelper.CSS_CLASS_LOSER_PLACE, PlayedGameHelper.PLACE_BIG_LOSER);
        }

        [Test]
        public void ItRendersATeamWinIfTheTeamWon()
        {
            TestRenderingForGivenRank(1, WinnerTypes.TeamWin, PlayedGameHelper.CSS_CLASS_TEAM_WIN, PlayedGameHelper.PLACE_TEAM_WIN);
        }

        [Test]
        public void ItRendersATeamLossIfTheTeamLost()
        {
            TestRenderingForGivenRank(2, WinnerTypes.TeamLoss, PlayedGameHelper.CSS_CLASS_TEAM_LOSS, PlayedGameHelper.PLACE_TEAM_LOSS);
        }

        [Test]
        public void ItRendersGordonPoints()
        {
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new ViewPage());
            GameResultViewModel playerGameResultDetails = new GameResultViewModel()
            {
                GordonPoints = 9
            };

            XElement result = helper.GameResults(playerGameResultDetails, WinnerTypes.PlayerWin).ToXElement();

            string firstNodeText = result.FirstNode.ToString();
            string gordonPointsComponent = string.Format(PlayedGameHelper.HTML_GORDON_POINTS_TEMPLATE, playerGameResultDetails.GordonPoints);
            Assert.True(firstNodeText.EndsWith(gordonPointsComponent));
        }
    }
}
