public class BattlefieldDropZone : CardDropZone {
    private BattlefieldUI battlefield;

    private void Start() {
        battlefield = GetComponent<BattlefieldUI>();
    }

    public override void OnCardDropped(CardController card) {
        if (!CanAcceptCard(card)) return;

        var gameManager = GameManager.Instance;
        if (gameManager != null && card.GetCardData() is CreatureData creatureData) {
            // Add the card to the battlefield through the game manager
            gameManager.PlayCard(creatureData, card.IsPlayer1Card() ? gameManager.Player1 : gameManager.Player2);
        }

        base.OnCardDropped(card);
    }
}