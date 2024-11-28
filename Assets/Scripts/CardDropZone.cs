using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public interface ICardDropZone {
    bool CanAcceptCard(CardController card);
    void OnCardDropped(CardController card);
    RectTransform GetRectTransform();
}

[RequireComponent(typeof(RectTransform))]
public class CardDropZone : UIComponent, IDropHandler, ICardDropZone {
    [SerializeField] public bool acceptPlayer1Cards = true;
    [SerializeField] public bool acceptPlayer2Cards = true;

    private RectTransform rectTransform;
    private GameMediator gameMediator;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        gameMediator = GameMediator.Instance;
    }

    protected override void RegisterEvents() {
        // No events to register for basic drop zone
    }

    protected override void UnregisterEvents() {
        // No events to unregister
    }

    public virtual bool CanAcceptCard(CardController card) {
        if (card == null) return false;
        return card.IsPlayer1Card() ? acceptPlayer1Cards : acceptPlayer2Cards;
    }

    public virtual void OnCardDropped(CardController card) {
        if (!CanAcceptCard(card)) return;
        gameMediator?.NotifyGameStateChanged();
    }

    public void OnDrop(PointerEventData eventData) {
        var card = eventData.pointerDrag?.GetComponent<CardController>();
        if (card != null && CanAcceptCard(card)) {
            OnCardDropped(card);
        }
    }

    public RectTransform GetRectTransform() => rectTransform;

    public override void UpdateUI() {
        // Base drop zone doesn't need UI updates
    }

    public static bool IsOverDropZone(Vector3 cardPosition, out ICardDropZone dropZone) {
        dropZone = null;
        var results = new List<RaycastResult>();
        var eventData = new PointerEventData(EventSystem.current) {
            position = Camera.main.WorldToScreenPoint(cardPosition)
        };

        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results) {
            dropZone = result.gameObject.GetComponent<ICardDropZone>();
            if (dropZone != null) {
                return true;
            }
        }

        return false;
    }
}