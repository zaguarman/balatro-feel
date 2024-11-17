using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private TextMeshProUGUI player1BattlefieldText;
    [SerializeField] private TextMeshProUGUI player2BattlefieldText;

    private void Awake() {
        Instance = this;
        GameContext = new GameContext();

        // Initialize players
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;

        UpdateUI();
    }

    public void PlayCard(CardData cardData, Player player) {
        Card card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        UpdateUI();
    }

    private void UpdateUI() {
        player1HealthText.text = $"Player 1 HP: {Player1.Health}";
        player2HealthText.text = $"Player 2 HP: {Player2.Health}";

        // Update battlefield display
        player1BattlefieldText.text = "Battlefield: " + string.Join(", ",
            Player1.Battlefield.ConvertAll(c => $"{c.Name}({c.Attack}/{c.Health})"));
        player2BattlefieldText.text = "Battlefield: " + string.Join(", ",
            Player2.Battlefield.ConvertAll(c => $"{c.Name}({c.Attack}/{c.Health})"));
    }
}