using System;
using System.Collections.Generic;
using static BoardGames.Schafkopf.CardType;

namespace BoardGames.Schafkopf;

public class SchafkopfGameType {

    protected static readonly List<CardType> StandardTrumpCards = [
        EichelOber, LaubOber, HerzOber, SchelleOber, EichelUnter, LaubUnter, HerzUnter, SchelleUnter,
        HerzAss, Herz10, HerzKoenig, Herz9, Herz8, Herz7
    ];
    
    public static readonly SchafkopfGameType Ramsch = new ("Ramsch", StandardTrumpCards);

    public static readonly SchafkopfGameType Bettel = new("Bettel", StandardTrumpCards);
    public static readonly SchafkopfGameType Wenz = new("Wenz", [
        EichelUnter, LaubUnter, HerzUnter, SchelleUnter
    ]);

    public static readonly SchafkopfGameType EichelSolo = new("Eichel-Solo", [
        EichelOber, LaubOber, HerzOber, SchelleOber, EichelUnter, LaubUnter, HerzUnter, SchelleUnter,
        EichelAss, Eichel10, EichelKoenig, Eichel9, Eichel8, Eichel7
    ]);

    public static readonly SchafkopfGameType LaubSolo = new("Laub-Solo", [
        EichelOber, LaubOber, HerzOber, SchelleOber, EichelUnter, LaubUnter, HerzUnter, SchelleUnter,
        LaubAss, Laub10, LaubKoenig, Laub9, Laub8, Laub7
    ]);

    public static readonly SchafkopfGameType HerzSolo = new("Herz-Solo", StandardTrumpCards);

    public static readonly SchafkopfGameType SchelleSolo = new("Schellen-Solo", [
        EichelOber, LaubOber, HerzOber, SchelleOber, EichelUnter, LaubUnter, HerzUnter, SchelleUnter,
        SchelleAss, Schelle10, SchelleKoenig, Schelle9, Schelle8, Schelle7
    ]);
    
    public readonly string Name;
    /// <summary>
    /// The trump cards in <b>descending</b> order.
    /// </summary>
    private readonly List<CardType> TrumpCards;
    
    protected SchafkopfGameType(string name, List<CardType> trumpCards) {
        Name = name;
        TrumpCards = trumpCards;
    }

    public bool IsTrumpCard(CardType card) {
        return TrumpCards.Contains(card);
    }

    public int CompareTrumps(CardType card1, CardType card2) {
        if (!TrumpCards.Contains(card1) || !TrumpCards.Contains(card2)) {
            throw new ArgumentException("At least one of the provided cards is not a trump card");
        }
        return TrumpCards.IndexOf(card2) - TrumpCards.IndexOf(card1);
    }
}

public class SchafkopfSauGameType : SchafkopfGameType {

    public static readonly SchafkopfSauGameType EichelSau = new ("Mit der Eichel-Sau", StandardTrumpCards, EichelAss);
    public static readonly SchafkopfSauGameType LaubSau = new ("Mit der Laub-Sau", StandardTrumpCards, LaubAss);
    public static readonly SchafkopfSauGameType HerzSau = new ("Mit der Herz-Sau", StandardTrumpCards, HerzAss);
    public static readonly SchafkopfSauGameType SchelleSau = new ("Mit der Schellen-Sau", StandardTrumpCards, SchelleAss);
    
    public readonly CardType Ace;
    
    protected SchafkopfSauGameType(string name, List<CardType> trumpCards, CardType ace) : base(name, trumpCards) {
        Ace = ace;
    }
}

