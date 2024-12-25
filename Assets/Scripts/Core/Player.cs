using UnityEngine.Events;
using System;
using System.Collections.Generic;
using static DebugLogger;

[Serializable]
public class PlayerDamagedUnityEvent : UnityEvent<int> { }

public interface IPlayer : IEntity, IDamageable {
    bool IsPlayer1();
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<ICreature> Battlefield { get; }
    void AddToHand(ICard card);
    void AddToBattlefield(ICreature creature);
    void AddToBattlefield(ICreature creature, int slotIndex);
    void RemoveFromBattlefield(ICreature creature);
    PlayerDamagedUnityEvent OnDamaged { get; }
    int GetCreatureSlotIndex(ICreature creature);
    bool HasEmptyBattlefieldSlot();
    ICreature GetCreatureInSlot(int slotIndex);
    Dictionary<string, int> GetCreatureSlotMap();
}

public class Player : Entity, IPlayer {
    private const int MAX_BATTLEFIELD_SLOTS = 5;
    private readonly ICreature[] battlefieldSlots = new ICreature[MAX_BATTLEFIELD_SLOTS];
    private readonly Dictionary<string, int> creatureSlotMap = new Dictionary<string, int>();

    public int Health { get; private set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; }
    public List<ICreature> Battlefield { get; private set; }
    public PlayerDamagedUnityEvent OnDamaged { get; } = new PlayerDamagedUnityEvent();

    private readonly GameMediator gameMediator;

    public Player(string name = "Player") : base(name) {
        Hand = new List<ICard>();
        Battlefield = new List<ICreature>();
        gameMediator = GameMediator.Instance;
    }

    public bool IsPlayer1() {
        var gameManager = GameManager.Instance;
        return gameManager != null && gameManager.Player1 == this;
    }

    public void TakeDamage(int amount) {
        Health = Math.Max(0, Health - amount);
        OnDamaged.Invoke(amount);
    }

    public void AddToHand(ICard card) {
        if (card == null) return;
        Hand.Add(card);
        gameMediator?.NotifyGameStateChanged();
    }

    public void AddToBattlefield(ICreature creature) {
        // Find first empty slot when no specific slot is provided
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot != -1) {
            AddToBattlefield(creature, emptySlot);
        }
    }

    public void AddToBattlefield(ICreature creature, int slotIndex) {
        if (creature == null) return;

        // Validate slot index
        if (slotIndex < 0 || slotIndex >= MAX_BATTLEFIELD_SLOTS) {
            LogWarning($"Invalid slot index {slotIndex}", LogTag.Creatures);
            return;
        }

        // If creature is already in a slot, clear that slot
        if (creatureSlotMap.TryGetValue(creature.TargetId, out int currentSlot)) {
            battlefieldSlots[currentSlot] = null;
        }

        // If there's already a creature in the target slot, don't allow the placement
        if (battlefieldSlots[slotIndex] != null) {
            LogWarning($"Slot {slotIndex} is already occupied", LogTag.Creatures);
            return;
        }

        // Add to new slot
        battlefieldSlots[slotIndex] = creature;
        creatureSlotMap[creature.TargetId] = slotIndex;

        // Update the list representation
        if (!Battlefield.Contains(creature)) {
            Battlefield.Add(creature);
            gameMediator?.NotifyCreatureSummoned(creature, this);
        }

        gameMediator?.NotifyGameStateChanged();
    }

    public Dictionary<string, int> GetCreatureSlotMap() {
        return new Dictionary<string, int>(creatureSlotMap);
    }

    private int FindFirstEmptySlot() {
        for (int i = 0; i < MAX_BATTLEFIELD_SLOTS; i++) {
            if (battlefieldSlots[i] == null) {
                return i;
            }
        }
        return -1;
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;

        if (creatureSlotMap.TryGetValue(creature.TargetId, out int slot)) {
            battlefieldSlots[slot] = null;
            creatureSlotMap.Remove(creature.TargetId);
        }

        Battlefield.Remove(creature);
        Log($"Removed creature from battlefield: {creature.Name}", LogTag.Creatures);
        gameMediator?.NotifyGameStateChanged();
    }

    public int GetCreatureSlotIndex(ICreature creature) {
        return creatureSlotMap.TryGetValue(creature.TargetId, out int slot) ? slot : -1;
    }

    public bool HasEmptyBattlefieldSlot() {
        return FindFirstEmptySlot() != -1;
    }

    public ICreature GetCreatureInSlot(int slotIndex) {
        if (slotIndex >= 0 && slotIndex < MAX_BATTLEFIELD_SLOTS) {
            return battlefieldSlots[slotIndex];
        }
        return null;
    }

    public override string ToString() {
        return $"{Name} - Health: {Health}, Hand: {Hand.Count}, Battlefield: {Battlefield.Count}";
    }
}