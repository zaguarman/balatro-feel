using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "CardGame/Card Database")]
public class CardDatabase : ScriptableObject {
    public List<CardData> allCards = new List<CardData>();

    public CardData GetRandomCard() {
        if (allCards.Count == 0) return null;
        return allCards[Random.Range(0, allCards.Count)];
    }

    public CardData GetCardByName(string cardName) {
        return allCards.Find(card => card.cardName == cardName);
    }
}