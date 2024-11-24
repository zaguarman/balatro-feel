using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerUIReferences {
    public TextMeshProUGUI healthText;
    public RectTransform handContainer;
    public RectTransform battlefieldContainer;
}

public class PlayerUI : MonoBehaviour {
    private IPlayer player;
    private PlayerUIReferences references;
    private bool isPlayer1;
    private IGameMediator gameMediator;
    private bool isInitialized = false;

    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private List<CardButtonController> handCards = new List<CardButtonController>();

    private void Start() {
        gameMediator = GameMediator.Instance;
        RegisterEvents();
        InitializePlayer();
    }

    private void InitializePlayer() {
        if (GameManager.Instance == null) {
            Debug.LogError("GameManager.Instance is null during PlayerUI initialization");
            return;
        }

        player = isPlayer1 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;

        if (player == null) {
            Debug.LogError($"Player reference is null for Player{(isPlayer1 ? "1" : "2")} during initialization");
            return;
        }

        isInitialized = true;
        UpdateUI();
    }

    private void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnPlayerDamaged.AddListener(HandlePlayerDamaged);
        }
    }

    private void OnDestroy() {
        UnregisterEvents();
    }

    private void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        }
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        if (GameManager.Instance != null) {
            InitializePlayer();
        }
    }

    public void SetReferences(PlayerUIReferences refs) {
        references = refs;
        if (isInitialized) {
            UpdateUI();
        }
    }

    public void UpdateUI() {
        if (!isInitialized) {
            Debug.LogWarning("Attempting to update UI before initialization");
            return;
        }

        if (player == null) {
            Debug.LogError($"Player reference is null during UpdateUI for Player{(isPlayer1 ? "1" : "2")}");
            InitializePlayer();
            return;
        }

        UpdateHealth();
        UpdateHand();
    }

    private void UpdateHealth() {
        if (player != null && references.healthText != null) {
            references.healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateHand() {
        if (!isInitialized) {
            Debug.LogWarning("Attempting to update hand before initialization");
            return;
        }

        if (player == null) {
            Debug.LogError($"Player reference is null during UpdateHand for Player{(isPlayer1 ? "1" : "2")}");
            return;
        }

        if (references.handContainer == null) {
            Debug.LogError($"Hand container reference is null for Player{(isPlayer1 ? "1" : "2")}");
            return;
        }

        ClearContainer(references.handContainer);
        handCards.Clear();

        float totalWidth = cardOffset + (cardSpacing * player.Hand.Count);
        references.handContainer.sizeDelta = new Vector2(totalWidth, 320f);

        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void CreateCardInHand(ICard card, int index) {
        var buttonObj = CreateCardButton(references.handContainer, index);
        var controller = buttonObj.GetComponent<CardButtonController>();

        var cardData = CreateCardData(card);
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(cardData, player);
        });

        handCards.Add(controller);
    }

    private Button CreateCardButton(RectTransform parent, int index) {
        Button buttonObj = Instantiate(GameReferences.Instance.GetCardButtonPrefab(), parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);

        return buttonObj;
    }

    private CardData CreateCardData(ICard card) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = card.Name;

        if (card is ICreature creature) {
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        }

        return cardData;
    }

    private void ClearContainer(RectTransform container) {
        if (container == null) return;

        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }
    }

    private void HandlePlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            UpdateHealth();
        }
    }
}