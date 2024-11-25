using System.Collections.Generic;
using UnityEngine;

public class HandUI : UIComponent {
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private List<CardButtonController> handCards = new List<CardButtonController>();
    private IPlayer player;
    private GameManager gameManager;
    private GameReferences gameReferences;
    private GameMediator gameMediator;
    private bool isPlayer1;
    private CardContainer handContainer;

    private void Start() {
        InitializeReferences();
    }

    private void InitializeReferences() {
        gameManager = GameManager.Instance;
        gameReferences = GameReferences.Instance;
        gameMediator = GameMediator.Instance;
        handContainer = isPlayer1
            ? gameReferences.GetPlayer1Hand()
            : gameReferences.GetPlayer2Hand();
    }

    public void SetIsPlayer1(bool value) {
        isPlayer1 = value;
        InitializeReferences();
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.AddListener(UpdateUI);
            gameMediator.OnGameInitialized.AddListener(InitializePlayer);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.OnGameStateChanged.RemoveListener(UpdateUI);
            gameMediator.OnGameInitialized.RemoveListener(InitializePlayer);
        }
    }

    private void InitializePlayer() {
        if (gameManager != null) {
            player = isPlayer1 ? gameManager.Player1 : gameManager.Player2;
            UpdateUI();
        }
    }

    public override void UpdateUI() {
        if (player == null || handContainer == null) return;

        ClearHand();
        UpdateHandLayout();
        CreateCards();
    }

    private void UpdateHandLayout() {
        float totalWidth = cardOffset + (cardSpacing * player.Hand.Count);
        handContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, 320f);
    }

    private void CreateCards() {
        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void CreateCardInHand(ICard card, int index) {
        var cardButtonPrefab = gameReferences.GetCardButtonPrefab();
        if (cardButtonPrefab == null) return;

        var buttonObj = Instantiate(cardButtonPrefab, handContainer.transform);
        var controller = buttonObj.GetComponent<CardButtonController>();
        if (controller == null) return;

        SetupCardTransform(buttonObj.GetComponent<RectTransform>(), index);
        var cardData = CreateCardData(card);
        controller.Setup(cardData, isPlayer1);

        // Setup drag and drop handlers
        SetupCardDragHandlers(controller);
        handCards.Add(controller);
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

    private void SetupCardDragHandlers(CardButtonController controller) {
        controller.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        controller.OnEndDragEvent.AddListener(OnCardEndDrag);
        controller.OnCardDropped.AddListener(OnCardDropped);
    }

    private void OnCardBeginDrag(CardButtonController card) {
        card.transform.SetAsLastSibling();
    }

    private void OnCardEndDrag(CardButtonController card) {
        // Handle card positioning if needed
    }

    private void OnCardDropped(CardButtonController card) {
        if (gameManager != null && IsValidDropLocation(card)) {
            gameManager.PlayCard(card.GetCardData(), player);
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private bool IsValidDropLocation(CardButtonController card) {
        return CardDropZone.IsOverDropZone(card.transform.position, out _);
    }

    private void ClearHand() {
        foreach (var card in handCards) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        handCards.Clear();
    }

    private void OnDestroy() {
        foreach (var card in handCards) {
            if (card != null) {
                card.OnBeginDragEvent.RemoveAllListeners();
                card.OnEndDragEvent.RemoveAllListeners();
                card.OnCardDropped.RemoveAllListeners();
            }
        }
    }
}