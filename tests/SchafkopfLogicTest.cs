using System.Collections.Generic;
using System.Linq;
using BoardGames.Schafkopf;

namespace BoardGames.tests;

using GdUnit4;
using static GdUnit4.Assertions;
using static CardType;

[TestSuite]
public class SchafkopfLogicTest {

    private SchafkopfLogic _schafkopfLogic;
    
    [Before]
    public void Setup() {
        _schafkopfLogic = new SchafkopfLogic();
    }
    
    [After]
    public void Cleanup() {
        _schafkopfLogic = null;
    }


    [TestCase]
    [DataPoint(nameof(WinningCardsData))]
    public void TestWinningCards(CardType[] cards, CardType winner, SchafkopfGameType gameType) {
        _schafkopfLogic.GameType = gameType;
        int winnerIndex = _schafkopfLogic.DetermineTrickWinner(cards.ToList());
        AssertObject(cards[winnerIndex])
            .AppendFailureMessage($"Wrong winning card in [{string.Join(", ", cards)}]")
            .IsEqual(winner);
        _schafkopfLogic.GameType = null;
    }

    public static IEnumerable<object[]> WinningCardsData => [
        [ new[] {Eichel7, Eichel10, EichelAss, Eichel9}, EichelAss, SchafkopfSauGameType.EichelSau], // Basic color
        [ new[] {Herz10, EichelUnter, LaubOber, HerzAss}, LaubOber, SchafkopfSauGameType.EichelSau], // Basic trump
        [ new[] {Herz10, EichelUnter, Eichel8, HerzAss}, EichelUnter, SchafkopfSauGameType.EichelSau], // trump with one color
        [ new[] {Laub8, Laub10, Schelle10, Eichel7}, Laub10, SchafkopfSauGameType.EichelSau], // mixed colors
        [ new[] {Laub8, Laub10, Schelle10, Herz7}, Herz7, SchafkopfSauGameType.EichelSau], // mixed colors and trump
        [ new[] {EichelAss, Herz10, Eichel9, EichelKoenig}, Herz10, SchafkopfSauGameType.EichelSau], // color ace and trump
        [ new[] {Laub8, Laub10, LaubUnter, LaubKoenig}, LaubUnter, SchafkopfSauGameType.EichelSau], // same colored trump
        [ new[] {SchelleOber, SchelleUnter, SchelleKoenig, Herz7}, SchelleOber, SchafkopfSauGameType.EichelSau], // same color in trumps
        
        [ new[] {Schelle7, HerzAss, Herz10, Herz7}, Schelle7, SchafkopfGameType.SchelleSolo],
        [ new[] {HerzAss, Herz10, Herz7, Schelle7}, Schelle7, SchafkopfGameType.SchelleSolo],
        [ new[] {Schelle7, HerzAss, Herz10, LaubUnter}, LaubUnter, SchafkopfGameType.SchelleSolo],
        [ new[] {Schelle7, Schelle10, SchelleAss, HerzOber}, HerzOber, SchafkopfGameType.SchelleSolo],
        
        [ new[] {Schelle7, HerzAss, Herz10, Herz7}, Schelle7, SchafkopfGameType.Wenz],
        [ new[] {HerzAss, Herz10, Herz7, Schelle7}, HerzAss, SchafkopfGameType.Wenz],
        [ new[] {HerzAss, Herz10, HerzUnter, Schelle7}, HerzUnter, SchafkopfGameType.Wenz],
        [ new[] {SchelleUnter, HerzAss, EichelOber, SchelleKoenig}, SchelleUnter, SchafkopfGameType.Wenz],
    ];

    [TestCase]
    [DataPoint(nameof(AllowedCardsData))]
    public void TestAllowedCards(CardType[] cards, CardType? firstPlayed, int[] expectedIndices, SchafkopfGameType gameType) {
        _schafkopfLogic.GameType = gameType;
        List<int> indices = _schafkopfLogic.DetermineAllowedCardsToPlay(cards.ToList(), firstPlayed);
        string firstPlayedString = firstPlayed.HasValue ? firstPlayed.Value.ToString() : "None";
        AssertArray(indices)
            .AppendFailureMessage($"Wrong indices for cards [{string.Join(", ", cards)}] and played card {firstPlayedString}")
            .IsEqual(expectedIndices.ToList());
        _schafkopfLogic.GameType = null;
    }

    public static IEnumerable<object[]> AllowedCardsData => [
        [new[] {Eichel7}, null, new[] {0}, SchafkopfSauGameType.LaubSau], // one card
        [new[] {Laub8, LaubOber, LaubAss, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Laub7, new[] {2}, SchafkopfSauGameType.LaubSau], // ace is searched
        [new[] {Laub8, LaubOber, LaubAss, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Schelle10, new[] {0, 1, 3, 4, 5, 6, 7}, SchafkopfSauGameType.LaubSau], // free of played color
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Laub7, new[] {0, 2, 6}, SchafkopfSauGameType.LaubSau], // no game-ace, any color possible
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            null, new[] {0, 1, 2, 3, 4, 5, 6, 7}, SchafkopfSauGameType.LaubSau], // no initial card, any card possible
        [new[] {Laub8, LaubOber, Laub9, LaubAss, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            null, new[] {0, 1, 2, 3, 4, 5, 6, 7}, SchafkopfSauGameType.LaubSau], // game-ace + three of its color
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            EichelUnter, new[] {1, 7}, SchafkopfSauGameType.LaubSau], // trump played
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Eichel10, new[] {3, 4, 5}, SchafkopfSauGameType.LaubSau],
        
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Eichel10, new[] {1, 3, 4, 5}, SchafkopfGameType.EichelSolo],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            Herz8, new[] {7}, SchafkopfGameType.EichelSolo],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            null, new[] {0, 1, 2, 3, 4, 5, 6, 7}, SchafkopfGameType.EichelSolo],
        
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, Eichel9, LaubKoenig, Herz10}, 
            LaubUnter, new[] {0, 1, 2, 3, 4, 5, 6, 7}, SchafkopfGameType.Wenz],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, EichelUnter, LaubKoenig, Herz10}, 
            Eichel10, new[] {3, 4}, SchafkopfGameType.Wenz],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, EichelUnter, LaubKoenig, Herz10}, 
            EichelOber, new[] {3, 4}, SchafkopfGameType.Wenz],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, EichelUnter, LaubKoenig, Herz10}, 
            LaubUnter, new[] {5}, SchafkopfGameType.Wenz],
        [new[] {Laub8, LaubOber, Laub9, Eichel7, Eichel8, EichelUnter, LaubKoenig, Herz10}, 
            null, new[] {0, 1, 2, 3, 4, 5, 6, 7}, SchafkopfGameType.Wenz],
    ];
}