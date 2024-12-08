using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public abstract class BaseCardContainer : UIComponent {
    [SerializeField] protected ContainerSettings settings = new ContainerSettings();
    protected List<CardController> cards = new List<CardController>();
    protected IPlayer player;
    protected RectTransform containerRect;

    public virtual void Initialize(IPlayer player) {
        this.player = player;
        containerRect = GetComponent<RectTransform>();
        if (containerRect == null) {
            containerRect = gameObject.AddComponent<RectTransform>();
        }

        UpdateLayout();
        IsInitialized = true;
        UpdateUI();
    }

    protected void UpdateLayout() {
        if (containerRect == null) return;

        Vector2[] positions = CalculateCardPositions();
        RepositionCards(positions);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
    }

    protected Vector2[] CalculateCardPositions() {
        Vector2[] positions = new Vector2[cards.Count];
        if (cards.Count == 0) return positions;

        switch (settings.layoutType) {
            case ContainerLayout.Horizontal:
                CalculatePositions(positions, true);
                break;
            case ContainerLayout.Vertical:
                CalculatePositions(positions, false);
                break;
            case ContainerLayout.Grid:
                CalculateGridPositions(positions);
                break;
        }
        return positions;
    }

    private void CalculatePositions(Vector2[] positions, bool isHorizontal) {
        for (int i = 0; i < cards.Count; i++) {
            float position = settings.offset + (settings.spacing * i);
            positions[i] = isHorizontal ?
                new Vector2(position, 0) :
                new Vector2(0, -position);
        }
    }

    protected virtual void SetupCardTransform(CardController card, Vector2 position) {
        if (card == null) return;

        var cardRect = card.GetComponent<RectTransform>();
        if (cardRect != null) {
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = position;
        }
    }

    protected void RepositionCards(Vector2[] positions) {
        for (int i = 0; i < cards.Count; i++) {
            if (cards[i] != null) {
                SetupCardTransform(cards[i], positions[i]);
            }
        }
    }


    protected virtual Vector2 CalculateContainerSize() {
        return settings.layoutType switch {
            ContainerLayout.Horizontal => new Vector2(settings.offset + (settings.spacing * cards.Count), containerRect.sizeDelta.y),
            ContainerLayout.Vertical => new Vector2(containerRect.sizeDelta.x, settings.offset + (settings.spacing * cards.Count)),
            ContainerLayout.Grid => CalculateGridSize(),
            _ => containerRect.sizeDelta
        };
    }

    protected virtual void CalculateHorizontalPositions(Vector2[] positions) {
        for (int i = 0; i < cards.Count; i++) {
            positions[i] = new Vector2(
                settings.offset + (settings.spacing * i),
                0
            );
        }
    }

    protected virtual void CalculateVerticalPositions(Vector2[] positions) {
        for (int i = 0; i < cards.Count; i++) {
            positions[i] = new Vector2(
                0,
                -(settings.offset + (settings.spacing * i))
            );
        }
    }

    protected virtual void CalculateGridPositions(Vector2[] positions) {
        for (int i = 0; i < cards.Count; i++) {
            int row = i / settings.gridColumns;
            int col = i % settings.gridColumns;
            positions[i] = new Vector2(
                settings.offset + (settings.gridCellSize.x * col),
                -(settings.offset + (settings.gridCellSize.y * row))
            );
        }
    }

    protected virtual Vector2 CalculateGridSize() {
        int rows = Mathf.CeilToInt((float)cards.Count / settings.gridColumns);
        return new Vector2(
            settings.offset + (settings.gridCellSize.x * settings.gridColumns),
            settings.offset + (settings.gridCellSize.y * rows)
        );
    }

    protected virtual void SetupCardEventHandlers(CardController card) {
        if (card == null) return;

        card.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        card.OnEndDragEvent.AddListener(OnCardEndDrag);
        card.OnCardDropped.AddListener(OnCardDropped);
        card.OnPointerEnterHandler += () => OnCardHoverEnter(card);
        card.OnPointerExitHandler += () => OnCardHoverExit(card);
    }

    protected virtual void CleanupCardEventHandlers(CardController card) {
        if (card == null) return;

        card.OnBeginDragEvent.RemoveListener(OnCardBeginDrag);
        card.OnEndDragEvent.RemoveListener(OnCardEndDrag);
        card.OnCardDropped.RemoveListener(OnCardDropped);
        card.OnPointerEnterHandler = null;
        card.OnPointerExitHandler = null;
    }

    protected virtual void OnCardBeginDrag(CardController card) {
        card.transform.SetAsLastSibling();
        card.transform.localScale = Vector3.one;
    }

    protected virtual void OnCardEndDrag(CardController card) {
        card.transform.localScale = Vector3.one;
        UpdateLayout();
    }

    protected virtual void OnCardDropped(CardController card) {
        // Override in derived classes
    }

    protected virtual void OnCardHoverEnter(CardController card) {
        var rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform != null) {
            var currentPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = settings.layoutType == ContainerLayout.Vertical
                ? new Vector2(currentPos.x + settings.cardHoverOffset, currentPos.y)
                : new Vector2(currentPos.x, currentPos.y + settings.cardHoverOffset);
        }
    }

    protected virtual void OnCardHoverExit(CardController card) {
        UpdateLayout();
    }

    protected virtual CardController CreateCard(ICard cardData) {
        return CardFactory.CreateCardController(cardData, player, transform, gameReferences);
    }

    public virtual void AddCard(CardController card) {
        if (card == null) return;
        cards.Add(card);
        SetupCardEventHandlers(card);
        UpdateLayout();
    }

    public virtual void RemoveCard(CardController card) {
        if (card == null) return;
        CleanupCardEventHandlers(card);
        cards.Remove(card);
        UpdateLayout();
    }

    protected override void OnDestroy() {
        foreach (var card in cards.ToList()) {
            if (card != null) {
                CleanupCardEventHandlers(card);
                Destroy(card.gameObject);
            }
        }
        cards.Clear();
        base.OnDestroy();
    }
}