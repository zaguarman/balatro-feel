using UnityEngine;
using System;

public class GameManager : Singleton<GameManager> {
    private bool isInitialized;
    private IGameMediator gameMediator;
    private GameEvents gameEvents;

    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    protected override void Awake() {
        base.Awake();
        InitializeGameSystems();
    }

    private void Start() {
        if (!isInitialized) {
            InitializeGame();
        }
        SetupTestGame();
    }

    private void InitializeGameSystems() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;
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

            isInitialized = true;
            NotifyGameStateChanged();
            Debug.Log("GameManager initialized successfully");
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
        NotifyGameStateChanged();
    }

    public void SetupTestGame() {
        if (!isInitialized) {
            InitializeGame();
        }

        var testSetup = gameObject.GetComponent<TestSetup>() ?? gameObject.AddComponent<TestSetup>();
        var testCards = testSetup.CreateTestCards();

        foreach (var card in testCards) {
            var cardInstance = CardFactory.CreateCard(card);
            Player1.AddToHand(cardInstance);
            Player2.AddToHand(cardInstance);
        }

        NotifyGameStateChanged();
    }

    public void NotifyGameStateChanged() => gameEvents.NotifyGameStateChanged();

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