public class AuraEffect {
    public string SourceId { get; private set; }
    public int AttackBonus { get; private set; }

    public AuraEffect(string sourceId, int attackBonus) {
        SourceId = sourceId;
        AttackBonus = attackBonus;
    }

    public bool IsValidTarget(Creature target) {
        return target.Id != SourceId; // Don't buff self
    }
}