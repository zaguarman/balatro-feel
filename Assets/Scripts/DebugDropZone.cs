public class DebugDropZone : CardDropZone {
    protected override void HandleCardDrop(CardController card) {
        var cardData = card.GetCardData();
        if (cardData != null) {
            var gameManager = GameManager.Instance;
            if (gameManager != null) {
                gameManager.PlayCard(cardData, card.IsPlayer1Card() ?
                    gameManager.Player1 : gameManager.Player2);
                UpdateVisualFeedback(true);
            }
        }
    }

    private void ShowValidDropFeedback() {
        if (dropZoneImage != null) {
            dropZoneImage.color = validDropColor;
        }
    }

    private void ShowInvalidDropFeedback() {
        if (dropZoneImage != null) {
            dropZoneImage.color = invalidDropColor;
        }
    }
}