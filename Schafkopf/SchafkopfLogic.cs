using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGames.Schafkopf;

public class SchafkopfLogic {

    public SchafkopfGameType GameType = SchafkopfSauGameType.EichelSau;// null;

    private const int TrickSize = 4;
    
    public int DetermineTrickWinner(List<CardType> playedCards) {
        if (GameType == null) {
            throw new InvalidOperationException("GameType is null");
        }
        if (playedCards == null || playedCards.Count != TrickSize) {
            throw new InvalidOperationException("playedCards does not contain 4 cards");
        }
        
        CardType highestCard = playedCards[0];
        if (GameType.IsTrumpCard(highestCard)) {
            for (int i = 1; i < TrickSize; i++) {
                CardType card = playedCards[i];
                if (GameType.IsTrumpCard(card) && GameType.CompareTrumps(card, highestCard) > 0) {
                    highestCard = card;
                }
            }
        } else {
            var relevantColor = CardTypeInfo.GetAllColorsOfCard(highestCard);
            for (int i = 1; i < TrickSize; i++) {
                CardType card = playedCards[i];
                if (GameType.IsTrumpCard(card)) {
                    if (!GameType.IsTrumpCard(highestCard) 
                        || (GameType.IsTrumpCard(highestCard) && GameType.CompareTrumps(card, highestCard) > 0)) {
                        highestCard = card;
                    }
                } else {
                    if (!GameType.IsTrumpCard(highestCard) && relevantColor.Contains(card) && relevantColor.IndexOf(highestCard) > relevantColor.IndexOf(card)) {
                        highestCard = card;
                    }
                }
            }
        }
        return playedCards.IndexOf(highestCard);
    }

    public List<int> DetermineAllowedCardsToPlay(List<CardType> availableCards, CardType? firstCardPlayed) {
        if (!firstCardPlayed.HasValue) {
            if (GameType is SchafkopfSauGameType sauGameType && availableCards.Contains(sauGameType.Ace)) {
                // if the player has the relevant ace, then he may not play out cards with the same color 
                // EXCEPT if he has three or more of them
                var relevantColor = CardTypeInfo.GetAllColorsOfCard(sauGameType.Ace);
                var countColors = relevantColor.Count(type => relevantColor.Contains(type) && !GameType.IsTrumpCard(type));
                if (countColors >= 4) {
                    return new List<int>(Enumerable.Range(0, availableCards.Count));
                } else {
                    var output = new List<int>();
                    for (int i = 0; i < availableCards.Count; i++) {
                        CardType card = availableCards[i];
                        if (!relevantColor.Contains(card) || GameType.IsTrumpCard(card) || card == sauGameType.Ace) {
                            output.Add(i);
                        }
                    }
                    return output;
                }
            } else {
                return new List<int>(Enumerable.Range(0, availableCards.Count));
            }
        }
         
        // first card has been played and != null
        var indexList = new List<int>();
        if (GameType.IsTrumpCard(firstCardPlayed.Value)) {
            if (availableCards.Count(type => GameType.IsTrumpCard(type)) == 0) {
                indexList.AddRange(Enumerable.Range(0, availableCards.Count));
            } else {
                for (int i = 0; i < availableCards.Count; i++) {
                    CardType card = availableCards[i];
                    if (GameType.IsTrumpCard(card)) {
                        indexList.Add(i);
                    }
                }
            }
            
        } else {
            var relevantColors = CardTypeInfo.GetAllColorsOfCard(firstCardPlayed.Value);
            
            // if this is a Sau game and the current player has the ace that is looked for, then he has to play it
            if (GameType is SchafkopfSauGameType sauGameType 
                && availableCards.Contains(sauGameType.Ace)
                && relevantColors.Contains(sauGameType.Ace)) {
                return new List<int>([availableCards.IndexOf(sauGameType.Ace)]);
            }

            if (availableCards.Count(type => relevantColors.Contains(type) && !GameType.IsTrumpCard(type)) == 0) {
                indexList.AddRange(Enumerable.Range(0, availableCards.Count));
            } else {
                for (int i = 0; i < availableCards.Count; i++) {
                    CardType card = availableCards[i];
                    if (relevantColors.Contains(card) && !GameType.IsTrumpCard(card)) {
                        indexList.Add(i);
                    }
                }
            }
        }

        // SauGame Ace may not just be played
        if (availableCards.Count > 1 && GameType is SchafkopfSauGameType sauGame && availableCards.Contains(sauGame.Ace)) {
            indexList.Remove(availableCards.IndexOf(sauGame.Ace));
        }
        return indexList;
    }
    
}