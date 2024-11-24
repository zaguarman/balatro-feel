using UnityEngine;

public class GameUI : Singleton<GameUI> {
    private PlayerUI player1UI;
    private PlayerUI player2UI;
    private IGameMediator gameMediator;
    private GameEvents gameEvents;
    private bool initialized;

    protected override void Awake() {
        base.Awake();
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;

        if (gameMediator != null) {
            gameMediator.RegisterUI(this);
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
        }

        if (gameEvents != null) {
            gameEvents.OnGameInitialized.AddListener(InitializeUI);
            Debug.Log("GameUI registered for initialization events");
        }
    }

    private void InitializeUI() {
        if (!initialized) {
            SetupPlayerUIs();
            UpdateUI();
            initialized = true;
            Debug.Log("GameUI initialized");
        }
    }

    private void SetupPlayerUIs() {
        var references = GameReferences.Instance;
        if (references != null) {
            player1UI = references.GetPlayer1UI();
            player2UI = references.GetPlayer2UI();

            if (player1UI != null) {
                player1UI.SetIsPlayer1(true);
                Debug.Log("Player1 UI initialized");
            }

            if (player2UI != null) {
                player2UI.SetIsPlayer1(false);
                Debug.Log("Player2 UI initialized");
            }
        }
    }

    public void UpdateUI() {
        if (initialized) {
            player1UI?.UpdateUI();
            player2UI?.UpdateUI();
        }
    }

    protected override void OnDestroy() {
        if (gameEvents != null) {
            gameEvents.OnGameInitialized.RemoveListener(InitializeUI);
        }

        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.UnregisterUI(this);
        }

        if (instance == this) {
            instance = null;
        }
    }
}