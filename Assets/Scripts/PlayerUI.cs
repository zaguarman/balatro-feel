using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private GameEvents gameEvents;
    private bool isInitialized;

    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private List<CardButtonController> handCards = new List<CardButtonController>();

    private void Start() {
        InitializeDependencies();
    }

    private void InitializeDependencies() {
        gameMediator = GameMediator.Instance;
        gameEvents = GameEvents.Instance;

        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnPlayerDamaged.AddListener(HandlePlayerDamaged);
        }

        if (gameEvents != null) {
            gameEvents.OnGameInitialized.AddListener(InitializePlayer);
        }
    }

    public void SetReferences(PlayerUIReferences refs) {
        references = refs;
        ValidateReferences();
    }

    private void ValidateReferences() {
        if (references.healthText == null)
            Debug.LogError($"Health text reference is missing for Player{(isPlayer1 ? "1" : "2")}");
        if (references.handContainer == null)
            Debug.LogError($"Hand container reference is missing for Player{(isPlayer1 ? "1" : "2")}");
        if (references.battlefieldContainer == null)
            Debug.LogError($"Battlefield container reference is missing for Player{(isPlayer1 ? "1" : "2")}");
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        InitializePlayer();
    }

    private void InitializePlayer() {
        var gameManager = GameManager.Instance;
        if (gameManager != null) {
            player = isPlayer1 ? gameManager.Player1 : gameManager.Player2;
            if (player != null) {
                isInitialized = true;
                UpdateUI();
                Debug.Log($"Player{(isPlayer1 ? "1" : "2")} UI initialized successfully");
            }
        }
    }

    public void UpdateUI() {
        if (!isInitialized || player == null) return;
        UpdateHealth();
        UpdateHand();
    }

    private void UpdateHealth() {
        if (references.healthText != null) {
            references.healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateHand() {
        ClearContainer(references.handContainer);
        handCards.Clear();

        if (references.handContainer == null) return;

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
        var references = GameReferences.Instance;
        Button buttonObj = Instantiate(references.GetCardButtonPrefab(), parent);
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);

        return buttonObj;
    }

    private void HandlePlayerDamaged(IPlayer damagedPlayer, int damage) {
        if (damagedPlayer == player) {
            UpdateHealth();
        }
    }

    private void ClearContainer(RectTransform container) {
        if (container == null) return;
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }
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

    private void OnDestroy() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnPlayerDamaged.RemoveListener(HandlePlayerDamaged);
        }

        if (gameEvents != null) {
            gameEvents.OnGameInitialized.RemoveListener(InitializePlayer);
        }
    }
}