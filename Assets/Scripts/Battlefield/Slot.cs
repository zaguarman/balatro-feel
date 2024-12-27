public class Slot : Entity {
    public CardController OccupyingCard { get; private set; }
    public ICreature OccupyingCreature { get; private set; }

    public Slot() : base("Slot") { }

    public virtual void OccupySlot(CardController card) {
        OccupyingCard = card;
        if (card != null) {
            OccupyingCreature = card.GetLinkedCreature();
        }
    }

    public virtual void ClearSlot() {
        OccupyingCard = null;
        OccupyingCreature = null;
    }

    public bool IsOccupied() {
        return OccupyingCard != null || OccupyingCreature != null;
    }

    public override bool IsValidTarget() {
        if (!IsOccupied()) return true;

        if (OccupyingCreature != null) {
            return true;
        }

        return false;
    }
}