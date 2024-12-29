using static DebugLogger;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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
    public IWeatherSystem WeatherSystem { get; private set; }
    private BattlefieldCombatHandler combatHandler;

    private GameMediator gameMediator;
    private GameReferences gameReferences;
    private ICardDealingService cardDealingService;
    private System.Random random = new System.Random();

    private bool creaturesPlaced = false;

    // Public accessor for the combat handler
    public BattlefieldCombatHandler CombatHandler => combatHandler;

    private bool weatherSystemInitialized = false;

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

        // Initialize systems in the correct order
        InitializeWeatherSystem();
        InitializeCombatSystem();
        InitializeActionsQueue();
        InitializeGameSystem();

        base.Initialize();

        // After everything is initialized, set initial weather
        if (WeatherSystem != null) {
            WeatherSystem.SetWeather(WeatherType.Rainy);
        }
    }

    private void InitializeWeatherSystem() {
        if (!weatherSystemInitialized) {
            WeatherSystem = new WeatherSystem(gameMediator);
            weatherSystemInitialized = true;
            Log("Weather system initialized", LogTag.Initialization);
        }
    }

    private void InitializeCombatSystem() {
        combatHandler = new BattlefieldCombatHandler(this);
        Log("Combat system initialized", LogTag.Initialization);
    }

    private void InitializeActionsQueue() {
        ActionsQueue = new ActionsQueue(gameMediator, combatHandler);
        Log("Actions queue initialized", LogTag.Initialization);
    }

    private void InitializeGameSystem() {
        // Initialize core game components first
        InitializePlayers();
        InitializeCards();

        // Setup initial game state
        if (GameUI.Instance.IsInitialized) {
            CompleteGameInitialization();
        } else {
            GameUI.Instance.onInitialized.AddListener(CompleteGameInitialization);
        }
    }

    private void CompleteGameInitialization() {
        PlaceInitialCreatures();
        SetupInitialGameState();
        SetupResolveButton();

        // Important: Notify game is initialized after all setup is complete
        gameMediator.NotifyGameInitialized();
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
        if (!HasValidBattlefields()) {
            LogError("Cannot place creatures - battlefield not initialized", LogTag.Initialization);
            return;
        }

        var testSetup = gameObject.AddComponent<TestSetup>();
        var availableCreatures = testSetup.CreateTestCards()
            .Where(card => card is CreatureData)
            .Cast<CreatureData>()
            .ToList();

        PlaceCreaturesForPlayer(Player1, availableCreatures);
        PlaceCreaturesForPlayer(Player2, availableCreatures);
    }

    private bool HasValidBattlefields() {
        return Player1?.Battlefield != null && Player1.Battlefield.Any() &&
               Player2?.Battlefield != null && Player2.Battlefield.Any();
    }

    private void PlaceCreaturesForPlayer(IPlayer player, List<CreatureData> availableCreatures) {
        for (int i = 0; i < 2; i++) {
            int randomIndex = random.Next(availableCreatures.Count);
            var creatureData = availableCreatures[randomIndex];
            var creature = CardFactory.CreateCard(creatureData) as ICreature;

            if (creature == null) continue;

            // Find first empty slot
            var emptySlot = player.Battlefield.FirstOrDefault(s => !s.IsOccupied());
            if (emptySlot == null) {
                LogWarning($"No empty battlefield slots available for {(player.IsPlayer1() ? "Player 1" : "Player 2")}", LogTag.Creatures);
                continue;
            }

            creature.SetOwner(player);
            player.AddToBattlefield(creature, emptySlot);
            Log($"Added {creature.Name} to {(player.IsPlayer1() ? "Player 1" : "Player 2")}'s battlefield with {creature.Effects.Count} effects",
                LogTag.Creatures | LogTag.Initialization);
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
            state += $"Player 1 - Health: {Player1.Health}, Hand: {Player1.Hand.Count}\n";
        }

        Log(state, LogTag.Players | LogTag.Creatures | LogTag.Actions);
        Player1?.LogBattlefieldCreatures();

        if (Player2 != null) {
            state += $"Player 2 - Health: {Player2.Health}, Hand: {Player2.Hand.Count}\n";
        }

        Player2?.LogBattlefieldCreatures();

        if (ActionsQueue != null) {
            state += $"Pending Actions: {ActionsQueue.GetPendingActionsCount()}\n";
        }

        Log(state, LogTag.Players | LogTag.Creatures | LogTag.Actions);
    }

    protected override void OnDestroy() {
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

            if (ActionsQueue != null) {
                ActionsQueue.Cleanup();
            }

            instance = null;
        }
    }
}