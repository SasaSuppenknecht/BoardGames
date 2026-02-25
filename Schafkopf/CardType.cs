using System.Collections.Generic;

namespace BoardGames.Schafkopf;

public enum CardType {
    Eichel6, Eichel7, Eichel8, Eichel9, Eichel10, EichelUnter, EichelOber, EichelKoenig, EichelAss, 
    Schelle6, Schelle7, Schelle8, Schelle9, Schelle10, SchelleUnter, SchelleOber, SchelleKoenig, SchelleAss,
    Herz6, Herz7, Herz8, Herz9, Herz10, HerzUnter, HerzOber, HerzKoenig, HerzAss,
    Blatt6, Blatt7, Blatt8, Blatt9, Blatt10, BlattUnter, BlattOber, BlattKoenig, BlattAss
    
}

public static class CardTypeInfo {
    
    public static readonly HashSet<CardType> NotSchafkopfCards =
        [CardType.Blatt6, CardType.Eichel6, CardType.Herz6, CardType.Schelle6];
    
}



