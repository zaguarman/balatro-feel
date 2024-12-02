using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public abstract class BaseCardContainer : UIComponent {
    [SerializeField] protected float cardSpacing = 220f;
    [SerializeField] protected float cardOffset = 50f;
    [SerializeField] protected float cardMoveDuration = 0.15f;
    [SerializeField] protected Ease cardMoveEase = Ease.OutBack;
    [SerializeField] protected float cardHoverOffset = 30f;
    [SerializeField] protected float cardDragScale = 1.1f;

    protected List<CardController> cards = new List<CardController>();
    protected IPlayer player;
    protected CardContainer container;
    protected RectTransform containerRect;

    public virtual void Initialize(CardContainer container, IPlayer player) {
        this.container = container;
        this.player = player;
        containerRect = container.GetComponent<RectTransform>();

        if (containerRect == null) {
            containerRect = container.gameObject.AddComponent<RectTransform>();
        }

        UpdateContainerSettings();
        IsInitialized = true;
        UpdateUI();
    }

    protected virtual void UpdateContainerSettings() {
        if (container == null) return;

        var settings = new ContainerSettings {
            layoutType = ContainerLayout.Horizontal,
            spacing = cardSpacing,
            offset = cardOffset,
            cardMoveDuration = cardMoveDuration,
            cardMoveEase = cardMoveEase,
            cardHoverOffset = cardHoverOffset
        };

        container.SetSettings(settings);
        container.SetPlayer(player);
    }

    protected void UpdateLayout(int cardCount) {
        float totalWidth = cardOffset + (cardSpacing * cardCount);
        containerRect.sizeDelta = new Vector2(totalWidth, containerRect.sizeDelta.y);

        // Position each card
        for (int i = 0; i < cards.Count; i++) {
            SetupCardPosition(cards[i].GetComponent<RectTransform>(), i);
        }

        // Ensure proper layout update
        if (container != null) {
            container.UpdateUI();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
    }

    protected void SetupCardPosition(RectTransform rect, int index) {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(cardOffset + (cardSpacing * index), 0);
    }

    protected virtual void ClearCards() {
        foreach (var card in cards) {
            if (card != null) {
                // Kill any active tweens before destroying
                DOTween.Kill(card.transform);
                Destroy(card.gameObject);
            }
        }
        cards.Clear();
    }

    protected virtual CardController CreateCard(ICard cardData, Transform parent) {
        var cardPrefab = gameReferences.GetCardPrefab();
        if (cardPrefab == null) return null;

        var cardObj = Instantiate(cardPrefab, parent);
        var controller = cardObj.GetComponent<CardController>();

        if (controller != null) {
            var data = CreateCardData(cardData);
            controller.Setup(data, player);
            SetupCardEventHandlers(controller);
        }

        return controller;
    }

    protected virtual CardData CreateCardData(ICard card) {
        var cardData = ScriptableObject.CreateInstance<CreatureData>();
        cardData.cardName = card.Name;

        if (card is ICreature creature) {
            cardData.attack = creature.Attack;
            cardData.health = creature.Health;
        }

        return cardData;
    }

    protected virtual void SetupCardEventHandlers(CardController controller) {
        if (controller == null) return;

        controller.OnBeginDragEvent.AddListener(OnCardBeginDrag);
        controller.OnEndDragEvent.AddListener(OnCardEndDrag);
        controller.OnCardDropped.AddListener(OnCardDropped);
        controller.OnPointerEnterHandler += () => OnCardHoverEnter(controller);
        controller.OnPointerExitHandler += () => OnCardHoverExit(controller);
    }

    protected virtual void OnCardBeginDrag(CardController card) {
        if (card == null || card.transform == null) return;

        card.transform.SetAsLastSibling();
        DOTween.Kill(card.transform);
        card.transform.DOScale(cardDragScale, cardMoveDuration);
    }

    protected virtual void OnCardEndDrag(CardController card) {
        if (card == null || card.transform == null) return;

        DOTween.Kill(card.transform);
        card.transform.DOScale(1f, cardMoveDuration);
    }

    protected virtual void OnCardDropped(CardController card) {
        // Override in derived classes if needed
    }

    protected virtual void OnCardHoverEnter(CardController card) {
        // Override in derived classes if needed
    }

    protected virtual void OnCardHoverExit(CardController card) {
        // Override in derived classes if needed
    }

    protected override void OnDestroy() {
        ClearCards();
        base.OnDestroy();
    }
}