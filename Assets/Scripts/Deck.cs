using UnityEngine;
using System.Collections.Generic;

public interface IDeck {
    int CardsRemaining { get; }
    void Initialize(List<CardData> cardDataList);
    ICard DrawCard();
    void AddCardToTop(ICard card);
    void AddCardToBottom(ICard card);
}

public class Deck : IDeck {
    private Queue<ICard> cards;

    public int CardsRemaining => cards?.Count ?? 0;

    public Deck() {
        cards = new Queue<ICard>();
    }

    public void Initialize(List<CardData> cardDataList) {
        if (cardDataList == null || cardDataList.Count == 0) {
            Debug.LogError("Attempted to initialize deck with null or empty card list");
            return;
        }

        cards.Clear();
        foreach (var cardData in cardDataList) {
            var card = CardFactory.CreateCard(cardData);
            cards.Enqueue(card);
        }

        //Debug.Log($"Deck initialized with {cards.Count} cards");
    }

    public ICard DrawCard() {
        if (cards.Count == 0) {
            Debug.LogWarning("Attempted to draw from empty deck");
            return null;
        }

        var drawnCard = cards.Dequeue();
        //Debug.Log($"Drew card: {drawnCard.Name}");
        return drawnCard;
    }

    public void AddCardToTop(ICard card) {
        if (card == null) {
            Debug.LogError("Attempted to add null card to deck");
            return;
        }

        var tempList = new List<ICard>(cards);
        tempList.Add(card);
        cards = new Queue<ICard>(tempList);
        Debug.Log($"Added card to top: {card.Name}");
    }

    public void AddCardToBottom(ICard card) {
        if (card == null) {
            Debug.LogError("Attempted to add null card to deck");
            return;
        }

        cards.Enqueue(card);
        Debug.Log($"Added card to bottom: {card.Name}");
    }
}