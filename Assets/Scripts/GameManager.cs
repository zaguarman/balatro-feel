using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>, IInitializable {
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }
    public bool IsInitialized { get; private set; }

    public IDeck Player1Deck => player1Deck;
    public IDeck Player2Deck => player2Deck;

    private IDeck player1Deck;
    private IDeck player2Deck;
    private GameMediator gameMediator;
    private Canvas mainCanvas;
    private GameReferences gameReferences;
    private InitializationManager initManager;

    protected override void Awake() {
        base.Awake();
        initManager = InitializationManager.Instance;
        gameReferences = GameReferences.Instance;
        gameMediator = GameMediator.Instance;

        // Register for initialization
        initManager.RegisterComponent(this);

        // Listen for system initialization
        initManager.OnSystemInitialized += OnSystemFullyInitialized;
    }

    public void Initialize() {
        if (IsInitialized) return;

        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;

        gameReferences.GetPlayer1UI().Initialize(Player1);
        gameReferences.GetPlayer2UI().Initialize(Player2);
        
        InitializeDecks();

        if (gameMediator != null && gameMediator.IsInitialized) {
            gameMediator.RegisterPlayer(Player1);
            gameMediator.RegisterPlayer(Player2);
        }

        IsInitialized = true;
        initManager.MarkComponentInitialized(this);
    }

    private void OnSystemFullyInitialized() {
        // Start the game only when all systems are ready
        StartGame();
    }

    private void InitializeDecks() {
        var testSetup = gameObject.GetComponent<TestSetup>() ?? gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();

        player1Deck = new Deck();
        player2Deck = new Deck();

        player1Deck.Initialize(testCards);
        player2Deck.Initialize(testCards);
    }

    public void StartGame() {
        if (!IsInitialized) {
            Initialize();
        }

        gameMediator?.NotifyGameInitialized();
        DealInitialHands();
        LogGameState("Game Started");
    }

    private void DealInitialHands(int initialHandSize = 3) {
        for (int i = 0; i < initialHandSize; i++) {
            DrawCardForPlayer(Player1);
            DrawCardForPlayer(Player2);
        }

        gameMediator?.NotifyGameStateChanged();
    }

    public bool CanDrawCard(IPlayer player) {
        if (!IsInitialized) return false;
        var deck = GetPlayerDeck(player);
        return deck != null && deck.CardsRemaining > 0;
    }

    public void DrawCardForPlayer(IPlayer player) {
        if (!IsInitialized) {
            Debug.LogError("Cannot draw card - GameManager not initialized");
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
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private IDeck GetPlayerDeck(IPlayer player) {
        if (player == Player1) return player1Deck;
        if (player == Player2) return player2Deck;
        return null;
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!IsInitialized) {
            Debug.LogError("Cannot play card - GameManager not initialized");
            return;
        }

        var playerNumber = player == Player1 ? "1" : "2";
        Debug.Log($"Playing card: {cardData.cardName} for player {playerNumber}");

        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        gameMediator?.NotifyGameStateChanged();

        LogGameState($"Player {playerNumber} played {cardData.cardName}");
    }

    public void LogGameState(string action = "") {
        if (!IsInitialized) return;

        var stateLog = new System.Text.StringBuilder();
        stateLog.AppendLine($"=== Game State {(string.IsNullOrEmpty(action) ? "" : $"- {action}")} ===");

        if (Player1 != null) {
            stateLog.AppendLine($"Player 1 - Health: {Player1.Health}, Hand: {Player1.Hand.Count}, Battlefield: {Player1.Battlefield.Count}");
        }

        if (Player2 != null) {
            stateLog.AppendLine($"Player 2 - Health: {Player2.Health}, Hand: {Player2.Hand.Count}, Battlefield: {Player2.Battlefield.Count}");
        }

        if (GameContext != null) {
            stateLog.AppendLine($"Pending Actions: {GameContext.GetPendingActionsCount()}");
        }

        Debug.Log(stateLog.ToString());
    }

    protected override void OnDestroy() {
        if (instance == this) {
            if (Player1 != null) {
                gameMediator?.UnregisterPlayer(Player1);
            }

            if (Player2 != null) {
                gameMediator?.UnregisterPlayer(Player2);
            }

            initManager.OnSystemInitialized -= OnSystemFullyInitialized;

            instance = null;
        }
        base.OnDestroy();
    }
}