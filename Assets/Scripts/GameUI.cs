public class GameUI : Singleton<GameUI> {
    private PlayerUI player1UI;
    private PlayerUI player2UI;
    private IGameMediator gameMediator;

    protected override void Awake() {
        base.Awake();
        InitializeDependencies();
        InitializePlayerUIs();
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;
        gameMediator?.RegisterUI(this);
    }

    private void InitializePlayerUIs() {
        var references = GameReferences.Instance;

        // Initialize Player 1 UI
        player1UI = references.GetPlayer1UI();
        if (player1UI != null) {
            player1UI.SetIsPlayer1(true);
            var player1References = new PlayerUIReferences {
                healthText = references.GetPlayer1HealthText(),
                handContainer = references.GetPlayer1Hand(),
                battlefieldContainer = references.GetPlayer1Battlefield()
            };
            player1UI.SetReferences(player1References);
        }

        // Initialize Player 2 UI
        player2UI = references.GetPlayer2UI();
        if (player2UI != null) {
            player2UI.SetIsPlayer1(false);
            var player2References = new PlayerUIReferences {
                healthText = references.GetPlayer2HealthText(),
                handContainer = references.GetPlayer2Hand(),
                battlefieldContainer = references.GetPlayer2Battlefield()
            };
            player2UI.SetReferences(player2References);
        }
    }

    public void UpdateUI() {
        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
    }

    protected override void OnDestroy() {
        if (instance == this) {
            if (gameMediator != null) {
                gameMediator.UnregisterUI(this);
            }
            instance = null;
        }
    }
}