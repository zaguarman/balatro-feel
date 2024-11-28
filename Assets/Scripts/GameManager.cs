using UnityEngine;

public class GameManager : InitializableComponent {
    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                var go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    private GameMediator gameMediator;
    private GameReferences gameReferences;
    private ICardDealingService cardDealingService;

    protected override void Awake() {
        base.Awake();
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Initialize() {
        if (IsInitialized) return;

        var initManager = InitializationManager.Instance;
        if (!initManager.IsComponentInitialized<GameReferences>() ||
            !initManager.IsComponentInitialized<GameMediator>()) {
            throw new System.InvalidOperationException("Required dependencies not initialized");
        }

        gameMediator = GameMediator.Instance;
        gameReferences = GameReferences.Instance;
        cardDealingService = new CardDealingService(gameMediator);

        InitializeGameSystem();

        base.Initialize();  // This will set IsInitialized to true
    }

    private void InitializeGameSystem() {
        GameContext = new GameContext();
        InitializePlayers();
        InitializeCards();
        SetupInitialGameState();
    }

    private void InitializePlayers() {
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;

        gameMediator.RegisterPlayer(Player1);
        gameMediator.RegisterPlayer(Player2);
    }

    private void InitializeCards() {
        var testSetup = gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();
        cardDealingService.InitializeDecks(testCards, testCards);
    }

    private void SetupInitialGameState() {
        cardDealingService.DealInitialHands(Player1, Player2);
        LogGameState("Game Initialized");
        gameMediator.NotifyGameInitialized();
    }

    public bool CanDrawCard(IPlayer player) {
        return IsInitialized && cardDealingService.CanDrawCard(player);
    }

    public void DrawCardForPlayer(IPlayer player) {
        if (!IsInitialized) {
            Debug.LogError("Cannot draw card - GameManager not initialized");
            return;
        }

        cardDealingService.DrawCardForPlayer(player);
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
        gameMediator.NotifyGameStateChanged();

        LogGameState($"Player {playerNumber} played {cardData.cardName}");
    }

    private void LogGameState(string action = "") {
        if (!IsInitialized) return;

        var state = $"=== Game State {(string.IsNullOrEmpty(action) ? "" : $"- {action}")} ===\n";

        if (Player1 != null) {
            state += $"Player 1 - Health: {Player1.Health}, Hand: {Player1.Hand.Count}, Battlefield: {Player1.Battlefield.Count}\n";
        }

        if (Player2 != null) {
            state += $"Player 2 - Health: {Player2.Health}, Hand: {Player2.Hand.Count}, Battlefield: {Player2.Battlefield.Count}\n";
        }

        if (GameContext != null) {
            state += $"Pending Actions: {GameContext.GetPendingActionsCount()}\n";
        }

        Debug.Log(state);
    }

    private void OnDestroy() {
        if (instance == this) {
            if (Player1 != null) {
                gameMediator?.UnregisterPlayer(Player1);
            }

            if (Player2 != null) {
                gameMediator?.UnregisterPlayer(Player2);
            }

            instance = null;
        }
    }
}