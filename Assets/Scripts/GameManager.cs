using System.Linq;
using UnityEngine;
using static DebugLogger;

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
        ActionsQueue = new ActionsQueue(gameMediator);

        InitializeGameSystem();

        base.Initialize();
    }

    private void InitializeGameSystem() {
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

        // Place creatures for Player 1
        for (int i = 0; i < 2; i++) {
            int randomIndex = random.Next(availableCreatures.Count);
            var creatureData = availableCreatures[randomIndex];
            var creature = new Creature(creatureData.cardName, creatureData.attack, creatureData.health, Player1);
            creature.SetOwner(Player1);
            Player1.AddToBattlefield(creature);
            Log($"Added {creature.Name} to Player 1's battlefield", LogTag.Creatures | LogTag.Initialization);
        }

        // Place creatures for Player 2
        for (int i = 0; i < 2; i++) {
            int randomIndex = random.Next(availableCreatures.Count);
            var creatureData = availableCreatures[randomIndex];
            var creature = new Creature(creatureData.cardName, creatureData.attack, creatureData.health, Player2);
            creature.SetOwner(Player2);
            Player2.AddToBattlefield(creature);
            Log($"Added {creature.Name} to Player 2's battlefield", LogTag.Creatures | LogTag.Initialization);
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
        }
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!IsInitialized) {
            LogError("Cannot play card - GameManager not initialized", LogTag.Actions | LogTag.Initialization);
            return;
        }

        ICard card = CardFactory.CreateCard(cardData);
        card.Play(player, ActionsQueue);
        gameMediator.NotifyGameStateChanged();
    }

    public void ResolveActions() {
        if (!IsInitialized) {
            LogError("Cannot resolve actions - GameManager not initialized", LogTag.Actions | LogTag.Initialization);
            return;
        }

        ActionsQueue.ResolveActions();
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

        Log(state, LogTag.Players | LogTag.Creatures | LogTag.Actions);
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

            ActionsQueue?.Cleanup();
            instance = null;
        }
    }
}