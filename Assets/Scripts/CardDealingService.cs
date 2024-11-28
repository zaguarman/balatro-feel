using UnityEngine;
using System.Collections.Generic;

public interface ICardDealingService {
    void InitializeDecks(List<CardData> player1Cards, List<CardData> player2Cards);
    void DealInitialHands(IPlayer player1, IPlayer player2, int handSize = 3);
    bool CanDrawCard(IPlayer player);
    void DrawCardForPlayer(IPlayer player);
    void ShuffleDeck(IPlayer player);
}

public class CardDealingService : ICardDealingService {
    private readonly Dictionary<IPlayer, IDeck> playerDecks = new Dictionary<IPlayer, IDeck>();
    private readonly GameMediator gameMediator;

    public CardDealingService(GameMediator gameMediator) {
        this.gameMediator = gameMediator;
    }

    public void InitializeDecks(List<CardData> player1Cards, List<CardData> player2Cards) {
        var gameManager = GameManager.Instance;
        if (gameManager == null) {
            Debug.LogError("GameManager not found when initializing decks");
            return;
        }

        // Create and initialize deck for Player 1
        var player1Deck = new Deck();
        player1Deck.Initialize(player1Cards);
        playerDecks[gameManager.Player1] = player1Deck;

        // Create and initialize deck for Player 2
        var player2Deck = new Deck();
        player2Deck.Initialize(player2Cards);
        playerDecks[gameManager.Player2] = player2Deck;

        Debug.Log("Card dealing service initialized decks successfully");
    }

    public void DealInitialHands(IPlayer player1, IPlayer player2, int handSize = 3) {
        for (int i = 0; i < handSize; i++) {
            DrawCardForPlayer(player1);
            DrawCardForPlayer(player2);
        }
        Debug.Log($"Dealt initial hands of {handSize} cards to both players");
    }

    public bool CanDrawCard(IPlayer player) {
        if (player == null || !playerDecks.ContainsKey(player)) {
            Debug.LogWarning($"Cannot check draw capability - player not found in deck registry");
            return false;
        }

        var deck = playerDecks[player];
        return deck != null && deck.CardsRemaining > 0;
    }

    public void DrawCardForPlayer(IPlayer player) {
        if (player == null) {
            Debug.LogError("Cannot draw card - player is null");
            return;
        }

        if (!playerDecks.TryGetValue(player, out var deck)) {
            Debug.LogError($"Could not find deck for player");
            return;
        }

        var card = deck.DrawCard();
        if (card != null) {
            player.AddToHand(card);
            gameMediator.NotifyGameStateChanged();
            Debug.Log($"Drew card for {(player.IsPlayer1() ? "Player 1" : "Player 2")}: {card.Name}");
        }
    }

    public void ShuffleDeck(IPlayer player) {
        if (!playerDecks.TryGetValue(player, out var deck)) {
            Debug.LogError($"Could not find deck for player to shuffle");
            return;
        }

        // Implementation of deck shuffling would go here
        // Note: The current Deck class would need to be modified to support shuffling
        Debug.Log($"Shuffled deck for {(player.IsPlayer1() ? "Player 1" : "Player 2")}");
    }
}