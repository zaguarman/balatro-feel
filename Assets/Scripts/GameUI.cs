public class GameUI : Singleton<GameUI> {
    private PlayerUI player1UI;
    private PlayerUI player2UI;
    private IGameMediator gameMediator;

    protected override void Awake() {
        base.Awake();
        InitializeDependencies();
        UpdateUI();
    }

    protected override void OnDestroy() {
        if (instance == this) {
            UnregisterFromMediator();
            instance = null;
        }
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;
        gameMediator?.RegisterUI(this);
    }

    public void SetPlayerUIs(PlayerUI player1, PlayerUI player2) {
        player1UI = player1;
        player2UI = player2;
    }

    public void UpdateUI() {
        player1UI?.UpdateUI();
        player2UI?.UpdateUI();
    }

    private void UnregisterFromMediator() {
        if (gameMediator != null) {
            gameMediator.UnregisterUI(this);
        }
    }
}