using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using DG.Tweening;
using System.Linq;

[Serializable]
public class CardUnityEvent : UnityEvent<CardController> { }

public class CardController : UIComponent, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardImage;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private bool isDragging;
    private CardData cardData;
    private IPlayer player;
    private ICreature linkedCreature;

    public CardUnityEvent OnBeginDragEvent = new CardUnityEvent();
    public CardUnityEvent OnEndDragEvent = new CardUnityEvent();
    public CardUnityEvent OnCardDropped = new CardUnityEvent();
    public Action OnPointerEnterHandler;
    public Action OnPointerExitHandler;

    protected override void Awake() {
        base.Awake();
        SetupComponents();
    }

    private void SetupComponents() {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("[CardController] Added CanvasGroup component");
        }
        cardImage = GetComponent<Image>();
    }

    public void Setup(CardData data, IPlayer owner) {
        Debug.Log($"[CardController] Setting up card: {data.cardName} for {(owner.IsPlayer1() ? "Player 1" : "Player 2")}");
        cardData = data;
        player = owner;

        if (cardData is CreatureData) {
            linkedCreature = FindLinkedCreature();
            if (linkedCreature != null) {
                Debug.Log($"[CardController] Linked to creature: {linkedCreature.Name} with health: {linkedCreature.Health}");
            }
        }

        UpdateUI();
    }

    private ICreature FindLinkedCreature() {
        if (cardData == null || player == null) return null;
        return player.Battlefield.FirstOrDefault(c => c.Name == cardData.cardName);
    }

    protected override void RegisterEvents() {
        if (gameMediator != null) {
            gameMediator.AddGameStateChangedListener(UpdateUI);
            gameMediator.AddCreatureDamagedListener(OnCreatureDamaged);
            gameMediator.AddCreatureDiedListener(OnCreatureDied);
        }
    }

    protected override void UnregisterEvents() {
        if (gameMediator != null) {
            gameMediator.RemoveGameStateChangedListener(UpdateUI);
            gameMediator.RemoveCreatureDamagedListener(OnCreatureDamaged);
            gameMediator.RemoveCreatureDiedListener(OnCreatureDied);
        }
    }

    private void OnCreatureDamaged(ICreature creature, int damage) {
        if (linkedCreature != null && creature.TargetId == linkedCreature.TargetId) {
            Debug.Log($"[CardController] Creature {creature.Name} took {damage} damage, updating UI");
            linkedCreature = creature; // Update reference to get new health
            UpdateUI();
        }
    }

    private void OnCreatureDied(ICreature creature) {
        if (linkedCreature != null && creature.TargetId == linkedCreature.TargetId) {
            Debug.Log($"[CardController] Creature {creature.Name} died, updating UI");
            linkedCreature = null; // Clear the reference since the creature is dead
            UpdateUI();
        }
    }

    public override void UpdateUI() {
        if (cardData == null) {
            Debug.LogWarning("[CardController] Attempted to update UI with null card data");
            return;
        }

        UpdateCardText();
        UpdateCardVisuals();
    }

    private void UpdateCardText() {
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description ?? string.Empty;

        if (cardData is CreatureData creatureData) {
            statsText.gameObject.SetActive(true);

            if (linkedCreature != null) {
                if (linkedCreature.Health <= 0) {
                    Debug.Log($"[CardController] Creature {linkedCreature.Name} is dead, should be removed");
                    statsText.text = $"{creatureData.attack}/{creatureData.health}";
                } else {
                    Debug.Log($"[CardController] Updating stats for {linkedCreature.Name}: {linkedCreature.Attack}/{linkedCreature.Health}");
                    statsText.text = $"{linkedCreature.Attack}/{linkedCreature.Health}";
                }
            } else {
                statsText.text = $"{creatureData.attack}/{creatureData.health}";
            }
        } else {
            statsText.gameObject.SetActive(false);
        }
    }

    private void UpdateCardVisuals() {
        if (cardImage != null && player != null) {
            cardImage.color = player.IsPlayer1()
                ? gameReferences.GetPlayer1CardColor()
                : gameReferences.GetPlayer2CardColor();
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (canvasGroup == null) {
            Debug.LogWarning("[CardController] CanvasGroup is missing, adding it now");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = transform.position;
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        OnBeginDragEvent.Invoke(this);
        Debug.Log($"[CardController] Started dragging {cardData.cardName}");
    }

    public void OnDrag(PointerEventData eventData) {
        if (!isDragging) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint)) {
            transform.position = parentCanvas.transform.TransformPoint(localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!isDragging) return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        OnEndDragEvent.Invoke(this);
        OnCardDropped.Invoke(this);

        Debug.Log($"[CardController] Ended dragging {cardData.cardName}");
        gameMediator?.NotifyGameStateChanged();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!isDragging) {
            OnPointerEnterHandler?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!isDragging) {
            OnPointerExitHandler?.Invoke();
        }
    }

    protected override void OnDestroy() {
        if (transform != null) {
            DOTween.Kill(transform);
        }

        CleanupEvents();
        base.OnDestroy();
    }

    private void CleanupEvents() {
        OnBeginDragEvent.RemoveAllListeners();
        OnEndDragEvent.RemoveAllListeners();
        OnCardDropped.RemoveAllListeners();
        OnPointerEnterHandler = null;
        OnPointerExitHandler = null;
    }

    public CardData GetCardData() => cardData;
    public bool IsPlayer1Card() => player?.IsPlayer1() ?? false;
    public ICreature GetLinkedCreature() => linkedCreature;
}