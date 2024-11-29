using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public enum ContainerLayout {
    Horizontal,
    Vertical,
    Grid
}

[System.Serializable]
public class ContainerSettings {
    [Header("Layout")]
    public ContainerLayout layoutType = ContainerLayout.Horizontal;
    public float spacing = 220f;
    public float offset = 50f;
    public Vector2 gridCellSize = new Vector2(220f, 320f);
    public int gridColumns = 3;

    [Header("Animation")]
    public float cardMoveDuration = 0.15f;
    public Ease cardMoveEase = Ease.OutBack;
    public float cardHoverOffset = 30f;
    public float cardDragScale = 1.1f;
}

public class CardContainer : UIComponent {
    public List<CardController> GetCards() => cards;

    [SerializeField] private ContainerSettings cardContainerSettings = new ContainerSettings();
    [SerializeField] private bool autoInitialize = true;

    private RectTransform containerRect;
    private List<CardController> cards = new List<CardController>();
    private CardController selectedCard;
    private CardController hoveredCard;
    private bool isSwapping = false;
    private IPlayer player;

    // Cache for layout calculations
    private Vector2[] cardPositions;
    private Vector2 containerSize;

    public void SetSettings(ContainerSettings newSettings) {
        if (containerRect == null) {
            Debug.LogError("ContainerRect is null in CardContainer.SetSettings");
            containerRect = GetComponent<RectTransform>();
            if (containerRect == null) {
                containerRect = gameObject.AddComponent<RectTransform>();
            }
        }
        cardContainerSettings = newSettings;
        UpdateLayout();
    }

    protected override void Awake() {
        containerRect = GetComponent<RectTransform>();
        if (containerRect == null) {
            containerRect = gameObject.AddComponent<RectTransform>();
        }
    }

    private void Start() {
        containerRect = GetComponent<RectTransform>();
        if (autoInitialize) {
            InitializeContainer();
        }
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddGameInitializedListener(() => InitializeContainer());
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveGameInitializedListener(() => InitializeContainer());
        }
    }

    public void InitializeContainer(IPlayer assignedPlayer = null) {
        if (containerRect == null) {
            containerRect = gameObject.AddComponent<RectTransform>();
        }

        if (assignedPlayer != null) {
            player = assignedPlayer;
        }

        ClearContainer();
        ConfigureDropZone();
        UpdateLayout();
    }

    private void ConfigureDropZone() {
        var existingDropZone = GetComponent<CardDropZone>();
        if (existingDropZone == null && player != null) {
            var dropZone = gameObject.AddComponent<CardDropZone>();
            var gameManager = GameManager.Instance;
            dropZone.acceptPlayer1Cards = player == gameManager.Player1;
            dropZone.acceptPlayer2Cards = player == gameManager.Player2;
        }
    }

    public void SetPlayer(IPlayer newPlayer) {
        player = newPlayer;
        ConfigureDropZone();
        UpdateUI();
    }

    public IPlayer GetPlayer() => player;

    public void AddCard(ICard card) {
        var references = GameReferences.Instance;
        if (references == null) return;

        Button cardPrefab = references.GetCardPrefab();
        if (cardPrefab == null) return;

        CreateCardButton(card, cardPrefab);
        UpdateLayout();
    }

    public void RemoveCard(CardController card) {
        if (card == null) return;

        cards.Remove(card);
        Destroy(card.gameObject);
        UpdateLayout();
    }

    private void CreateCardButton(ICard card, Button prefab) {
        if (player == null) return;

        var cardObj = Instantiate(prefab, transform);
        var controller = cardObj.GetComponent<CardController>();

        if (controller != null) {
            var cardData = CreateCardData(card);
            controller.Setup(cardData, player);
            SetupCardHandlers(controller);
            cards.Add(controller);
            PositionCard(controller.GetComponent<RectTransform>(), cards.Count - 1, false);
        }
    }

    private CardData CreateCardData(ICard card) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        if (card is ICreature creature) {
            cardData.cardName = creature.Name;
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        } else {
            cardData.cardName = card.Name;
        }
        return cardData;
    }

    private void SetupCardHandlers(CardController card) {
        card.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        card.OnEndDragEvent.AddListener(OnCardEndDrag);
        card.OnPointerEnterHandler += () => OnCardHoverEnter(card);
        card.OnPointerExitHandler += () => OnCardHoverExit(card);
    }

    private void UpdateLayout() {
        if (containerRect == null) {
            Debug.LogError("ContainerRect is null in CardContainer.UpdateLayout");
            return;
        }

        CalculateLayout();
        UpdateContainerSize();
        RepositionAllCards();
    }

    private void CalculateLayout() {
        cardPositions = new Vector2[cards.Count];

        switch (cardContainerSettings.layoutType) {
            case ContainerLayout.Horizontal:
                CalculateHorizontalLayout();
                break;
            case ContainerLayout.Vertical:
                CalculateVerticalLayout();
                break;
            case ContainerLayout.Grid:
                CalculateGridLayout();
                break;
        }
    }

    private void CalculateHorizontalLayout() {
        if (containerRect == null) {
            Debug.LogError("ContainerRect is null in CardContainer.CalculateHorizontalLayout");
            return;
        }

        containerSize = new Vector2(
            cardContainerSettings.offset + (cardContainerSettings.spacing * cards.Count),
            containerRect.sizeDelta.y
        );

        for (int i = 0; i < cards.Count; i++) {
            cardPositions[i] = new Vector2(
                cardContainerSettings.offset + (cardContainerSettings.spacing * i),
                0
            );
        }
    }

    private void CalculateVerticalLayout() {
        containerSize = new Vector2(
            containerRect.sizeDelta.x,
            cardContainerSettings.offset + (cardContainerSettings.spacing * cards.Count)
        );

        for (int i = 0; i < cards.Count; i++) {
            cardPositions[i] = new Vector2(
                0,
                -(cardContainerSettings.offset + (cardContainerSettings.spacing * i))
            );
        }
    }

    private void CalculateGridLayout() {
        int rows = Mathf.CeilToInt((float)cards.Count / cardContainerSettings.gridColumns);

        containerSize = new Vector2(
            cardContainerSettings.offset + (cardContainerSettings.gridCellSize.x * cardContainerSettings.gridColumns),
            cardContainerSettings.offset + (cardContainerSettings.gridCellSize.y * rows)
        );

        for (int i = 0; i < cards.Count; i++) {
            int row = i / cardContainerSettings.gridColumns;
            int col = i % cardContainerSettings.gridColumns;

            cardPositions[i] = new Vector2(
                cardContainerSettings.offset + (cardContainerSettings.gridCellSize.x * col),
                -(cardContainerSettings.offset + (cardContainerSettings.gridCellSize.y * row))
            );
        }
    }

    private void UpdateContainerSize() {
        if (containerRect != null) {
            containerRect.sizeDelta = containerSize;
        }
    }

    private void RepositionAllCards() {
        for (int i = 0; i < cards.Count; i++) {
            PositionCard(cards[i].GetComponent<RectTransform>(), i, true);
        }
    }

    private void PositionCard(RectTransform cardRect, int index, bool animate) {
        if (cardRect == null || index >= cardPositions.Length) return;

        Vector3 targetPosition = new Vector3(cardPositions[index].x, cardPositions[index].y, 0);

        if (animate) {
            cardRect.DOLocalMove(targetPosition, cardContainerSettings.cardMoveDuration).SetEase(cardContainerSettings.cardMoveEase);
        } else {
            cardRect.localPosition = targetPosition;
        }
    }

    private void OnCardHoverEnter(CardController card) {
        hoveredCard = card;
        if (selectedCard == null) {
            Vector2 hoverOffset = cardContainerSettings.layoutType == ContainerLayout.Vertical
                ? Vector2.right * cardContainerSettings.cardHoverOffset
                : Vector2.up * cardContainerSettings.cardHoverOffset;

            Vector3 targetPosition = new Vector3(
                cardPositions[cards.IndexOf(card)].x + hoverOffset.x,
                cardPositions[cards.IndexOf(card)].y + hoverOffset.y,
                0
            );
            card.GetComponent<RectTransform>().DOLocalMove(targetPosition, cardContainerSettings.cardMoveDuration)
                .SetEase(cardContainerSettings.cardMoveEase);
        }
    }

    private void OnCardHoverExit(CardController card) {
        if (hoveredCard == card) {
            hoveredCard = null;
            if (selectedCard == null) {
                Vector3 targetPosition = new Vector3(
                    cardPositions[cards.IndexOf(card)].x,
                    cardPositions[cards.IndexOf(card)].y,
                    0
                );
                card.GetComponent<RectTransform>().DOLocalMove(targetPosition, cardContainerSettings.cardMoveDuration)
                    .SetEase(cardContainerSettings.cardMoveEase);
            }
        }
    }

    private void OnCardBeginDrag(CardController card) {
        selectedCard = card;
        card.transform.SetAsLastSibling();
        card.transform.DOScale(cardContainerSettings.cardDragScale, cardContainerSettings.cardMoveDuration);
    }

    private void OnCardEndDrag(CardController card) {
        if (selectedCard == null) return;

        card.transform.DOScale(1f, cardContainerSettings.cardMoveDuration);
        PositionCard(card.GetComponent<RectTransform>(), cards.IndexOf(card), true);
        selectedCard = null;
    }

    private void Update() {
        if (selectedCard == null || isSwapping) return;
        CheckAndHandleSwap();
    }

    private void CheckAndHandleSwap() {
        for (int i = 0; i < cards.Count; i++) {
            var currentCard = cards[i];
            if (currentCard == selectedCard) continue;

            if (ShouldSwap(selectedCard, currentCard)) {
                SwapCards(cards.IndexOf(selectedCard), i);
                break;
            }
        }
    }

    private bool ShouldSwap(CardController card1, CardController card2) {
        int card1Index = cards.IndexOf(card1);
        int card2Index = cards.IndexOf(card2);

        switch (cardContainerSettings.layoutType) {
            case ContainerLayout.Horizontal:
                return ShouldSwapHorizontal(card1, card2, card1Index, card2Index);
            case ContainerLayout.Vertical:
                return ShouldSwapVertical(card1, card2, card1Index, card2Index);
            case ContainerLayout.Grid:
                return ShouldSwapGrid(card1, card2, card1Index, card2Index);
            default:
                return false;
        }
    }

    private bool ShouldSwapHorizontal(CardController card1, CardController card2, int index1, int index2) {
        float card1X = card1.transform.position.x;
        float card2X = card2.transform.position.x;
        return (card1X > card2X && index1 < index2) ||
               (card1X < card2X && index1 > index2);
    }

    private bool ShouldSwapVertical(CardController card1, CardController card2, int index1, int index2) {
        float card1Y = card1.transform.position.y;
        float card2Y = card2.transform.position.y;
        return (card1Y > card2Y && index1 < index2) ||
               (card1Y < card2Y && index1 > index2);
    }

    private bool ShouldSwapGrid(CardController card1, CardController card2, int index1, int index2) {
        Vector2 card1Pos = card1.transform.position;
        Vector2 card2Pos = card2.transform.position;
        float distanceSqr = Vector2.SqrMagnitude(card1Pos - card2Pos);
        return distanceSqr < (cardContainerSettings.gridCellSize.x * cardContainerSettings.gridCellSize.x);
    }

    private void SwapCards(int index1, int index2) {
        isSwapping = true;

        // Swap in list
        var temp = cards[index1];
        cards[index1] = cards[index2];
        cards[index2] = temp;

        // Update positions
        PositionCard(cards[index1].GetComponent<RectTransform>(), index1, true);
        PositionCard(cards[index2].GetComponent<RectTransform>(), index2, true);

        // Notify game state changed using gameMediator
        gameMediator?.NotifyGameStateChanged();

        isSwapping = false;
    }

    private void ClearContainer() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        cards.Clear();
    }

    public override void UpdateUI() {
        if (player == null) return;

        foreach (var card in cards) {
            card?.UpdateUI();
        }
    }

    private void OnDestroy() {
        // Clean up event listeners and card references
        foreach (var card in cards) {
            if (card != null) {
                card.OnBeginDragEvent.RemoveListener(OnCardBeginDrag);
                card.OnEndDragEvent.RemoveListener(OnCardEndDrag);
                card.OnPointerEnterHandler = null;
                card.OnPointerExitHandler = null;
            }
        }
    }


}