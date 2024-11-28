public class HandDropZone : CardDropZone {
    private CardContainer container;

    private void Start() {
        container = GetComponent<CardContainer>();
    }

    public override void OnCardDropped(CardController card) {
        if (!CanAcceptCard(card)) return;

        // Handle adding card to hand
        // This might involve re-organizing the hand or updating the card's parent
        base.OnCardDropped(card);
    }
}