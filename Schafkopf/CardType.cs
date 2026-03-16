using System.Collections.Generic;
using static BoardGames.Schafkopf.CardType;

namespace BoardGames.Schafkopf;

public enum CardType {
    // note: 6-cards are useful for easy usage of card images
    Eichel6, Eichel7, Eichel8, Eichel9, Eichel10, EichelUnter, EichelOber, EichelKoenig, EichelAss, 
    Schelle6, Schelle7, Schelle8, Schelle9, Schelle10, SchelleUnter, SchelleOber, SchelleKoenig, SchelleAss,
    Herz6, Herz7, Herz8, Herz9, Herz10, HerzUnter, HerzOber, HerzKoenig, HerzAss,
    Laub6, Laub7, Laub8, Laub9, Laub10, LaubUnter, LaubOber, LaubKoenig, LaubAss,
    
}

public static class CardTypeInfo {
    
    public static readonly HashSet<CardType> NotSchafkopfCards =
        [Laub6, Eichel6, Herz6, Schelle6];

    private static readonly List<CardType> Eicheln = [
        EichelAss, Eichel10, EichelKoenig, EichelOber, EichelUnter, Eichel9, Eichel8, Eichel7
    ];
    
    private static readonly List<CardType> Lauben = [
        LaubAss, Laub10, LaubKoenig, LaubOber, LaubUnter, Laub9, Laub8, Laub7
    ];
    
    private static readonly List<CardType> Herzen = [
        HerzAss, Herz10, HerzKoenig, HerzOber, HerzUnter, Herz9, Herz8, Herz7
    ];
    
    private static readonly List<CardType> Schellen = [
        SchelleAss, Schelle10, SchelleKoenig, SchelleOber, SchelleUnter, Schelle9, Schelle8, Schelle7
    ];
    
    public static int GetCardValue(this CardType cardType) {
        return cardType switch {
            EichelAss or LaubAss or HerzAss or SchelleAss => 11,
            Eichel10 or Laub10 or Herz10 or Schelle10 => 10,
            EichelKoenig or LaubKoenig or HerzKoenig or SchelleKoenig => 4,
            EichelOber or LaubOber or HerzOber or SchelleOber => 3,
            EichelUnter or LaubUnter or HerzUnter or SchelleUnter => 2,
            _ => 0,
        };
    }
    
    public static List<CardType> GetAllColorsOfCard(CardType cardType) {
        return cardType switch {
            EichelAss or Eichel10 or EichelKoenig or EichelOber or EichelUnter or Eichel9 or Eichel8
                or Eichel7 => Eicheln,
            LaubAss or Laub10 or LaubKoenig or LaubOber or LaubUnter or Laub9 or Laub8
                or Laub7 => Lauben,
            HerzAss or Herz10 or HerzKoenig or HerzOber or HerzUnter or Herz9 or Herz8
                or Herz7 => Herzen,
            SchelleAss or Schelle10 or SchelleKoenig or SchelleOber or SchelleUnter or Schelle9 or Schelle8
                or Schelle7 => Schellen,
            _ => [],
        };
    }
}