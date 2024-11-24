using TMPro;
using UnityEngine;
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

        // Add debug logging
        Debug.Log($"PlayerUI Dependencies initialized - Mediator: {gameMediator != null}, Events: {gameEvents != null}");
    }

    public void SetReferences(PlayerUIReferences refs) {
        references = refs;
        ValidateReferences();
        // Log reference setup
        Debug.Log($"PlayerUI References set - Health Text: {references.healthText != null}, Hand Container: {references.handContainer != null}");
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
        Debug.Log($"PlayerUI SetIsPlayer1 called with value: {value}");
    }

    private void InitializePlayer() {
        var gameManager = GameManager.Instance;
        if (gameManager != null) {
            player = isPlayer1 ? gameManager.Player1 : gameManager.Player2;
            if (player != null) {
                isInitialized = true;
                UpdateUI();
                Debug.Log($"Player{(isPlayer1 ? "1" : "2")} UI initialized successfully");
            } else {
                Debug.LogError($"Failed to get player reference from GameManager for Player{(isPlayer1 ? "1" : "2")}");
            }
        } else {
            Debug.LogError("GameManager instance not found during PlayerUI initialization");
        }
    }

    public void UpdateUI() {
        if (!isInitialized || player == null) {
            Debug.LogWarning($"UpdateUI called but not ready - Initialized: {isInitialized}, Player: {player != null}");
            return;
        }

        UpdateHealth();
        UpdateHand();
        Debug.Log($"UpdateUI called for Player{(isPlayer1 ? "1" : "2")} - Hand Count: {player.Hand.Count}");
    }

    private void UpdateHealth() {
        if (references.healthText != null) {
            references.healthText.text = $"Health: {player.Health}";
        }
    }

    private void UpdateHand() {
        if (references.handContainer == null) {
            Debug.LogError($"Hand container is null for Player{(isPlayer1 ? "1" : "2")}");
            return;
        }

        ClearContainer(references.handContainer);
        handCards.Clear();

        float totalWidth = cardOffset + (cardSpacing * player.Hand.Count);
        references.handContainer.sizeDelta = new Vector2(totalWidth, 320f);

        Debug.Log($"Updating hand for Player{(isPlayer1 ? "1" : "2")} with {player.Hand.Count} cards");

        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void CreateCardInHand(ICard card, int index) {
        // Check if we have valid hand container reference
        if (references.handContainer == null) {
            Debug.LogError($"Hand container is null for Player{(isPlayer1 ? "1" : "2")}");
            return;
        }

        // Get card button prefab from GameReferences
        var gameRefs = GameReferences.Instance;
        if (gameRefs == null) {
            Debug.LogError("GameReferences instance is null");
            return;
        }

        var cardButtonPrefab = gameRefs.GetCardButtonPrefab();
        if (cardButtonPrefab == null) {
            Debug.LogError("Card button prefab is null");
            return;
        }

        // Instantiate the card in our hand container
        var buttonObj = Instantiate(cardButtonPrefab, references.handContainer);
        if (buttonObj == null) {
            Debug.LogError("Failed to instantiate card button");
            return;
        }

        SetupCardTransform(buttonObj.GetComponent<RectTransform>(), index);

        var controller = buttonObj.GetComponent<CardButtonController>();
        if (controller == null) {
            Debug.LogError("CardButtonController component not found on instantiated button");
            return;
        }

        var cardData = CreateCardData(card);
        controller.Setup(cardData, isPlayer1);

        buttonObj.onClick.AddListener(() => {
            GameManager.Instance.PlayCard(cardData, player);
        });

        handCards.Add(controller);
        Debug.Log($"Created card in hand: {card.Name} at index {index} for Player{(isPlayer1 ? "1" : "2")}");
    }

    private void SetupCardTransform(RectTransform rect, int index) {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);
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