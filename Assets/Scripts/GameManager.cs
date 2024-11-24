using UnityEngine;
using System;

public class GameManager : Singleton<GameManager> {
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    private IGameMediator gameMediator;
    private GameEvents gameEvents;
    private bool isInitialized;

    public GameUI GameUI => GameUI.Instance;
    public DamageResolver DamageResolver => DamageResolver.Instance;

    protected override void Awake() {
        base.Awake();
        InitializeGameSystems();
    }

    private void Start() {
        if (!isInitialized) {
            InitializeGame();
        }
    }

    private void InitializeGameSystems() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;
        Debug.Log("Game systems initialized");
    }

    private void InitializeGame() {
        try {
            GameContext = new GameContext();
            Player1 = new Player();
            Player2 = new Player();
            Player1.Opponent = Player2;
            Player2.Opponent = Player1;

            gameMediator.RegisterPlayer(Player1);
            gameMediator.RegisterPlayer(Player2);

            if (DamageResolver != null) {
                gameMediator.RegisterDamageResolver(DamageResolver);
            }

            isInitialized = true;
            gameEvents.NotifyGameInitialized();
            gameEvents.NotifyGameStateChanged();

            Debug.Log("GameManager initialized successfully");
            SetupTestGame();
        } catch (Exception e) {
            Debug.LogError($"Error initializing GameManager: {e.Message}");
            isInitialized = false;
        }
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        Debug.Log($"Playing card: {cardData.cardName} for player {(player == Player1 ? "1" : "2")}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        gameEvents.NotifyGameStateChanged();
    }

    private void SetupTestGame() {
        if (!isInitialized) return;

        var testSetup = gameObject.GetComponent<TestSetup>() ?? gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();

        foreach (var card in testCards) {
            var cardInstance = CardFactory.CreateCard(card);
            Player1.AddToHand(cardInstance);
            Player2.AddToHand(cardInstance);
        }

        gameEvents.NotifyGameStateChanged();
    }

    public void ResetGame() {
        if (gameMediator != null) {
            if (Player1 != null) gameMediator.UnregisterPlayer(Player1);
            if (Player2 != null) gameMediator.UnregisterPlayer(Player2);
            if (DamageResolver != null) gameMediator.UnregisterDamageResolver(DamageResolver);
            if (GameUI != null) gameMediator.UnregisterUI(GameUI);
        }

        isInitialized = false;
        GameContext = null;
        Player1 = null;
        Player2 = null;

        InitializeGame();
    }

    protected override void OnDestroy() {
        if (instance == this) {
            ResetGame();
            if (gameMediator != null) {
                gameMediator.Cleanup();
            }
            instance = null;
        }
        base.OnDestroy();
    }
}