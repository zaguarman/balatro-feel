using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    public IDeck Player1Deck => player1Deck;
    public IDeck Player2Deck => player2Deck;

    private IDeck player1Deck;
    private IDeck player2Deck;
    private GameMediator gameMediator;
    private Canvas mainCanvas;
    private bool isInitialized;
    private GameReferences gameReferences;

    public GameUI GameUI => gameReferences?.GetGameUI();

    protected override void Awake() {
        base.Awake();
        gameReferences = GameReferences.Instance;
        gameMediator = GameMediator.Instance;
        EnsureCanvasExists();
        StartGame();
    }

    private void EnsureCanvasExists() {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null) {
            var canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }

    private void InitializeGame() {
        if (isInitialized) return;

        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;

        InitializeDecks();

        gameMediator.RegisterPlayer(Player1);
        gameMediator.RegisterPlayer(Player2);

        isInitialized = true;
        gameMediator.NotifyGameInitialized();
        gameMediator.NotifyGameStateChanged();

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
    }

    private void DealInitialHands(int initialHandSize = 3) {
        for (int i = 0; i < initialHandSize; i++) {
            DrawCardForPlayer(Player1);
            DrawCardForPlayer(Player2);
        }

        gameMediator.NotifyGameStateChanged();
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
            gameMediator.NotifyGameStateChanged();
            //Debug.Log($"Player {(player == Player1 ? 1 : 2)} drew card {card.Name}");
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
        gameMediator.NotifyGameStateChanged();

        LogGameState($"Player {playerNumber} played {cardData.cardName}");
    }

    public void LogGameState(string action = "") {
        if (!isInitialized) return;

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

    public void StartGame() {
        if (!isInitialized) {
            InitializeGame();
        }
    }

    protected override void OnDestroy() {
        if (instance == this) {
            if (Player1 != null) {
                gameMediator?.UnregisterPlayer(Player1);
            }

            if (Player2 != null) {
                gameMediator?.UnregisterPlayer(Player2);
            }

            if (GameUI != null) {
                gameMediator?.UnregisterUI(GameUI);
            }

            instance = null;
        }
        base.OnDestroy();
    }
}