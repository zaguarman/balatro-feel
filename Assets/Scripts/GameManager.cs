using System;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    // Expose decks as read-only properties for debugging
    public IDeck Player1Deck => player1Deck;
    public IDeck Player2Deck => player2Deck;

    private IDeck player1Deck;
    private IDeck player2Deck;
    private IGameMediator gameMediator;
    private GameEvents gameEvents;
    private bool isInitialized;

    public GameUI GameUI => GameUI.Instance;
    public DamageResolver DamageResolver => DamageResolver.Instance;

    protected override void Awake() {
        base.Awake();
        InitializeGameSystems();
        StartGame();
    }

    private void InitializeGameSystems() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;
        Debug.Log("Game systems initialized");
    }

    private void InitializeGame() {
        Debug.Log("Starting game initialization");

        // Initialize core systems
        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;

        // Initialize UI references
        var references = GameReferences.Instance;
        references.ValidateAndCreateReferences();
        references.InitializeUI();

        // Initialize game systems
        InitializeDecks();

        // Register with mediator after UI is ready
        if (gameMediator != null) {
            gameMediator.RegisterPlayer(Player1);
            gameMediator.RegisterPlayer(Player2);

            if (DamageResolver != null) {
                gameMediator.RegisterDamageResolver(DamageResolver);
            }
        } else {
            Debug.LogError("GameMediator is null during initialization");
        }

        isInitialized = true;
        Debug.Log("Game initialization completed");

        // Notify systems of initialization
        gameEvents.NotifyGameInitialized();
        gameEvents.NotifyGameStateChanged();

        LogGameState("Game initialized");
        DealInitialHands();
    }

    private void InitializeDecks() {
        var testSetup = gameObject.GetComponent<TestSetup>() ?? gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();

        player1Deck = new Deck();
        player2Deck = new Deck();

        player1Deck.Initialize(testCards);
        player2Deck.Initialize(testCards);

        LogGameState("Decks initialized");
    }

    private void DealInitialHands(int initialHandSize = 3) {
        for (int i = 0; i < initialHandSize; i++) {
            DrawCardForPlayer(Player1);
            DrawCardForPlayer(Player2);
        }

        gameEvents.NotifyGameStateChanged();
        LogGameState($"Dealt initial hands of {initialHandSize} cards");
    }

    public bool CanDrawCard(IPlayer player) {
        if (!isInitialized) return false;

        var deck = GetPlayerDeck(player);
        return deck != null && deck.CardsRemaining > 0;
    }

    public void DrawCardForPlayer(IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        var deck = GetPlayerDeck(player);
        if (deck == null) {
            Debug.LogError("Could not find deck for player");
            return;
        }

        var card = deck.DrawCard();
        if (card != null) {
            player.AddToHand(card);
            gameEvents.NotifyGameStateChanged();
            LogGameState($"Player drew card: {card.Name}");
        }
    }

    private IDeck GetPlayerDeck(IPlayer player) {
        if (player == Player1) return player1Deck;
        if (player == Player2) return player2Deck;
        return null;
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        var playerNumber = player == Player1 ? "1" : "2";
        Debug.Log($"Playing card: {cardData.cardName} for player {playerNumber}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        gameEvents.NotifyGameStateChanged();

        LogGameState($"Player {playerNumber} played {cardData.cardName}");
    }

    // Debug information methods
    public void LogGameState(string action = "") {
        if (!isInitialized) {
            Debug.LogWarning("Cannot log game state - game not initialized");
            return;
        }

        var stateLog = new System.Text.StringBuilder();
        stateLog.AppendLine($"=== Game State {(string.IsNullOrEmpty(action) ? "" : $"- {action}")} ===");
        stateLog.AppendLine($"Player 1:");
        stateLog.AppendLine($"  Health: {Player1.Health}");
        stateLog.AppendLine($"  Cards in Hand: {Player1.Hand.Count}");
        stateLog.AppendLine($"  Cards in Deck: {Player1Deck.CardsRemaining}");
        stateLog.AppendLine($"  Battlefield Size: {Player1.Battlefield.Count}");

        stateLog.AppendLine($"Player 2:");
        stateLog.AppendLine($"  Health: {Player2.Health}");
        stateLog.AppendLine($"  Cards in Hand: {Player2.Hand.Count}");
        stateLog.AppendLine($"  Cards in Deck: {Player2Deck.CardsRemaining}");
        stateLog.AppendLine($"  Battlefield Size: {Player2.Battlefield.Count}");

        stateLog.AppendLine($"Pending Actions: {GameContext.GetPendingActionsCount()}");

        Debug.Log(stateLog.ToString());
    }

    public void StartGame() {
        if (!isInitialized) {
            InitializeGame();
        }
    }

    protected override void OnDestroy() {
        if (instance == this) {
            if (gameMediator != null) {
                gameMediator.Cleanup();
            }
            instance = null;
        }
        base.OnDestroy();
    }
}