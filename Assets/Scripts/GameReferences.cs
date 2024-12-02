using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerUIReferences {
    [Header("Main UI")]
    public PlayerUI playerUI;
    public TextMeshProUGUI healthText;

    [Header("Card Containers")]
    public HandUI handUI;
    public BattlefieldUI battlefieldUI;

    [Header("Health")]
    public HealthUI healthUI;
}

public class GameReferences : Singleton<GameReferences> {
    [Header("Game Control")]
    [SerializeField] private Button resolveActionsButton;

    [Header("Player References")]
    [SerializeField] private PlayerUIReferences player1References;
    [SerializeField] private PlayerUIReferences player2References;

    [Header("Card Components")]
    [SerializeField] private Button cardPrefab;

    [Header("Card Style")]
    [SerializeField] private Color player1CardColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] private Color player2CardColor = new Color(1f, 0.8f, 0.8f);

    public override void Initialize() {
        if (IsInitialized) return;
        ValidateReferences();
        base.Initialize();
    }

    private void ValidateReferences() {
        // Core UI validation
        if (player1References.playerUI == null) Debug.LogError("Player1UI reference missing!");
        if (player2References.playerUI == null) Debug.LogError("Player2UI reference missing!");
        if (cardPrefab == null) Debug.LogError("CardPrefab reference missing!");

        // Validate hand and battlefield UIs
        ValidatePlayerReferences(player1References, "Player 1");
        ValidatePlayerReferences(player2References, "Player 2");
    }

    private void ValidatePlayerReferences(PlayerUIReferences refs, string playerName) {
        if (refs.handUI == null) Debug.LogError($"{playerName} HandUI reference missing!");
        if (refs.battlefieldUI == null) Debug.LogError($"{playerName} BattlefieldUI reference missing!");
        if (refs.healthUI == null) Debug.LogError($"{playerName} HealthUI reference missing!");
        if (refs.healthText == null) Debug.LogError($"{playerName} HealthText reference missing!");
    }

    // Player UI getters
    public PlayerUI GetPlayer1UI() => player1References.playerUI;
    public PlayerUI GetPlayer2UI() => player2References.playerUI;

    // Card Container getters
    public HandUI GetPlayer1HandUI() => player1References.handUI;
    public HandUI GetPlayer2HandUI() => player2References.handUI;
    public BattlefieldUI GetPlayer1BattlefieldUI() => player1References.battlefieldUI;
    public BattlefieldUI GetPlayer2BattlefieldUI() => player2References.battlefieldUI;

    // Health getters
    public HealthUI GetPlayer1HealthUI() => player1References.healthUI;
    public HealthUI GetPlayer2HealthUI() => player2References.healthUI;
    public TextMeshProUGUI GetPlayer1HealthText() => player1References.healthText;
    public TextMeshProUGUI GetPlayer2HealthText() => player2References.healthText;

    // Shared components getters
    public Button GetCardPrefab() => cardPrefab;
    public Button GetResolveActionsButton() => resolveActionsButton;
    public Color GetPlayer1CardColor() => player1CardColor;
    public Color GetPlayer2CardColor() => player2CardColor;
}