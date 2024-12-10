using System.Linq;
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
    public ActionsQueue ActionsQueue { get; private set; }

    private GameMediator gameMediator;
    private GameReferences gameReferences;
    private ICardDealingService cardDealingService;
    private System.Random random = new System.Random();

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
        ActionsQueue = new ActionsQueue();
        InitializePlayers();
        InitializeCards();
        PlaceInitialCreatures();
        SetupInitialGameState();
        SetupResolveButton();
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

    private void PlaceInitialCreatures() {
        var testSetup = gameObject.AddComponent<TestSetup>();
        var availableCreatures = testSetup.CreateTestCards()
            .Where(card => card is CreatureData)
            .Cast<CreatureData>()
            .ToList();

        // Place 2 random creatures for Player 1
        for (int i = 0; i < 2; i++) {
            int randomIndex = random.Next(availableCreatures.Count);
            var creatureData = availableCreatures[randomIndex];
            var creature = new Creature(creatureData.cardName, creatureData.attack, creatureData.health, Player1);
            creature.SetOwner(Player1);  // Explicitly set the owner
            Player1.AddToBattlefield(creature);
            Debug.Log($"Added {creature.Name} to Player 1's battlefield with proper owner reference");
        }

        // Place 2 random creatures for Player 2
        for (int i = 0; i < 2; i++) {
            int randomIndex = random.Next(availableCreatures.Count);
            var creatureData = availableCreatures[randomIndex];
            var creature = new Creature(creatureData.cardName, creatureData.attack, creatureData.health, Player2);
            creature.SetOwner(Player2);  // Explicitly set the owner
            Player2.AddToBattlefield(creature);
            Debug.Log($"Added {creature.Name} to Player 2's battlefield with proper owner reference");
        }
    }

    private void SetupInitialGameState() {
        cardDealingService.DealInitialHands(Player1, Player2);
        LogGameState("Game Initialized");
        gameMediator.NotifyGameInitialized();
    }

    private void SetupResolveButton() {
        var resolveButton = gameReferences.GetResolveActionsButton();
        if (resolveButton != null) {
            resolveButton.onClick.AddListener(OnResolveButtonClicked);
        }
    }

    private void OnResolveButtonClicked() {
        if (ActionsQueue != null) {
            ActionsQueue.ResolveActions();
            gameMediator?.NotifyGameStateChanged();
        }
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!IsInitialized) {
            Debug.LogError("Cannot play card - GameManager not initialized");
            return;
        }

        ICard card = CardFactory.CreateCard(cardData);
        card.Play(player, ActionsQueue);
        gameMediator.NotifyGameStateChanged();
    }

    public void ResolveActions() {
        if (!IsInitialized) {
            Debug.LogError("Cannot resolve actions - GameManager not initialized");
            return;
        }

        ActionsQueue.ResolveActions();
        gameMediator.NotifyGameStateChanged();
        LogGameState("Actions resolved");
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

        if (ActionsQueue != null) {
            state += $"Pending Actions: {ActionsQueue.GetPendingActionsCount()}\n";
        }

        Debug.Log(state);
    }

    private void OnDestroy() {
        if (instance == this) {
            var resolveButton = gameReferences?.GetResolveActionsButton();
            if (resolveButton != null) {
                resolveButton.onClick.RemoveListener(OnResolveButtonClicked);
            }

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