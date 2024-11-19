
public enum BuffType {
    Attack,
    Health,
    // Add more buff types as needed
}


public class Buff {
    public BuffType Type { get; private set; }
    public int Value { get; private set; }
    public string SourceId { get; private set; }

    public Buff(BuffType type, int value, string sourceId) {
        Type = type;
        Value = value;
        SourceId = sourceId;
    }
}