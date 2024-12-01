using System.Collections.Generic;
using UnityEngine;

public class HandUI : UIComponent {
    [SerializeField] private float cardSpacing = 220f;
    [SerializeField] private float cardOffset = 50f;

    private List<CardController> handCards = new List<CardController>();
    private IPlayer player;
    private GameManager gameManager;
    private CardContainer handContainer;

    private void Start() {
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            InitializeReferences();
        } else {
            InitializationManager.Instance.OnSystemInitialized.AddListener(InitializeReferences);
        }
    }

    private void InitializeReferences() {
        if (!InitializationManager.Instance.IsComponentInitialized<GameManager>()) {
            Debug.LogWarning("GameManager not initialized yet, will retry later");
            return;
        }

        gameManager = GameManager.Instance;

        if (player != null) {
            handContainer = player.IsPlayer1()
                ? gameReferences.GetPlayer1Hand()
                : gameReferences.GetPlayer2Hand();
        }

        UpdateUI();
    }

    public void Initialize(IPlayer player) {
        this.player = player;
        if (InitializationManager.Instance.IsComponentInitialized<GameMediator>()) {
            InitializeReferences();
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
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
        handContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, 220f);
    }

    private void CreateCards() {
        if (player.Hand == null) return;

        for (int i = 0; i < player.Hand.Count; i++) {
            CreateCardInHand(player.Hand[i], i);
        }
    }

    private void ClearHand() {
        foreach (var card in handCards) {
            if (card != null) {
                Destroy(card.gameObject);
            }
        }
        handCards.Clear();
    }

    private void CreateCardInHand(ICard card, int index) {
        if (gameReferences == null) return;

        var cardButtonPrefab = gameReferences.GetCardPrefab();
        if (cardButtonPrefab == null) return;

        var buttonObj = Instantiate(cardButtonPrefab, handContainer.transform);
        var controller = buttonObj.GetComponent<CardController>();
        if (controller == null) return;

        SetupCardController(controller, card, index);
        handCards.Add(controller);
    }

    private void SetupCardController(CardController controller, ICard card, int index) {
        var cardData = CreateCardData(card);
        controller.Setup(cardData, player);
        SetupCardTransform(controller.GetComponent<RectTransform>(), index);
        SetupCardDragHandlers(controller);
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

    private void SetupCardTransform(RectTransform rect, int index) {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);
    }

    private void SetupCardDragHandlers(CardController controller) {
        controller.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        controller.OnEndDragEvent.AddListener(OnCardEndDrag);
        controller.OnCardDropped.AddListener(OnCardDropped);
    }

    private void OnCardBeginDrag(CardController card) {
        card.transform.SetAsLastSibling();
    }

    private void OnCardEndDrag(CardController card) {
        // Handle any end drag logic if needed
    }

    private void OnCardDropped(CardController card) {
        if (gameManager != null && IsValidDropLocation(card)) {
            gameManager.PlayCard(card.GetCardData(), player);
            gameMediator?.NotifyGameStateChanged();
        }
    }

    private bool IsValidDropLocation(CardController card) {
        return CardDropZone.IsOverDropZone(card.transform.position, out _);
    }
}