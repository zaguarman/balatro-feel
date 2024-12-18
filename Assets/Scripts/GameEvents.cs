using UnityEngine.Events;

public class GameEvents {
    public UnityEvent GameStateChanged { get; } = new UnityEvent();
    public UnityEvent GameInitialized { get; } = new UnityEvent();
    public UnityEvent<IPlayer, int> PlayerDamaged { get; } = new UnityEvent<IPlayer, int>();
    public UnityEvent<ICreature, int> CreatureDamaged { get; } = new UnityEvent<ICreature, int>();
    public UnityEvent<ICreature> CreatureDied { get; } = new UnityEvent<ICreature>();
    public UnityEvent<IPlayer> GameOver { get; } = new UnityEvent<IPlayer>();
    public UnityEvent<ICreature> CreaturePreSummon { get; } = new UnityEvent<ICreature>();
    public UnityEvent<ICreature, IPlayer> CreatureSummoned { get; } = new UnityEvent<ICreature, IPlayer>();

    public void ClearAllListeners() {
        GameStateChanged.RemoveAllListeners();
        GameInitialized.RemoveAllListeners();
        PlayerDamaged.RemoveAllListeners();
        CreatureDamaged.RemoveAllListeners();
        CreatureDied.RemoveAllListeners();
        GameOver.RemoveAllListeners();
        CreaturePreSummon.RemoveAllListeners();
        CreatureSummoned.RemoveAllListeners();
    }
} 