using UnityEngine.Events;
using System;
using System.Collections.Generic;
using static DebugLogger;
using System.Linq;

[Serializable]
public class PlayerDamagedUnityEvent : UnityEvent<int> { }

public interface IPlayer : IEntity, IDamageable {
    bool IsPlayer1();
    IPlayer Opponent { get; set; }
    List<ICard> Hand { get; }
    List<BattlefieldSlot> Battlefield { get; }
    void AddToHand(ICard card);
    void AddToBattlefield(ICreature creature, ITarget slotId = null);
    void RemoveFromBattlefield(ICreature creature);
    PlayerDamagedUnityEvent OnDamaged { get; }
    bool HasEmptyBattlefieldSlot();
    void InitializeBattlefield(List<BattlefieldSlot> slots);
    void LogBattlefieldCreatures();
    ICreature GetCreatureInSlot(ITarget slotId);
    CardController GetCardByCreature(ICreature creature);
}

public class Player : Entity, IPlayer {
    public int Health { get; private set; } = 20;
    public IPlayer Opponent { get; set; }
    public List<ICard> Hand { get; private set; }

    public List<BattlefieldSlot> Battlefield { get; private set; }
    public PlayerDamagedUnityEvent OnDamaged { get; } = new PlayerDamagedUnityEvent();

    // is player 1 prop using the isplayer1 method
    public bool Is_Player1 => IsPlayer1();

    private readonly GameMediator gameMediator;

    public Player(string name = "Player") : base(name) {
        Hand = new List<ICard>();
        Battlefield = new List<BattlefieldSlot>();
        gameMediator = GameMediator.Instance;
    }

    // Initialize battlefield with existing UI slots
    public void InitializeBattlefield(List<BattlefieldSlot> slots) {
        if (slots == null || slots.Count == 0) {
            LogError("Cannot initialize battlefield with null or empty slots", LogTag.Initialization);
            return;
        }
        Battlefield.Clear();
        Battlefield.AddRange(slots);
        Log($"Initialized battlefield with {slots.Count} slots", LogTag.Initialization);
    }

    public void LogBattlefieldCreatures() {
        foreach (var slot in Battlefield) {
            if (slot.IsOccupied()) {
                Log($"Slot {slot.TargetId} is occupied by {slot.OccupyingCreature.Name}", LogTag.Creatures);
            }
        }
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
        gameMediator?.NotifyHandStateChanged(this);
    }

    public void AddToBattlefield(ICreature creature, ITarget slot = null) {
        if (creature == null) return;

        // If no slot specified, find first empty slot
        if (slot == null) {
            slot = Battlefield.FirstOrDefault(s => !s.IsOccupied());
            if (slot == null) {
                LogWarning("No empty battlefield slots available", LogTag.Creatures);
                return;
            }
        }

        var targetSlot = Battlefield.FirstOrDefault(s => s.TargetId == slot.TargetId);
        if (targetSlot == null) {
            LogWarning($"Invalid slot {slot.TargetId}", LogTag.Creatures);
            return;
        }

        if (targetSlot.IsOccupied()) {
            Log($"Slot {slot.TargetId} is already occupied, replacing creature...", LogTag.Creatures);
        }

        targetSlot.AssignCreature(creature);

        gameMediator?.NotifyCreatureSummoned(creature, this);
        gameMediator?.NotifyBattlefieldStateChanged(this);
    }

    public void RemoveFromBattlefield(ICreature creature) {
        if (creature == null) return;

        foreach (var slot in Battlefield) {
            if (slot.IsOccupied() && slot.OccupyingCreature == creature) {
                slot.ClearSlot();
                Log($"Removed {creature.Name} from battlefield", LogTag.Creatures);
                gameMediator?.NotifyBattlefieldStateChanged(this);
                return;
            }
        }

        Log($"Removed creature {creature.Name} from battlefield", LogTag.Creatures);
        gameMediator?.NotifyGameStateChanged();
    }

    public CardController GetCardByCreature(ICreature creature) {
        foreach (BattlefieldSlot slot in Battlefield) {
            if (slot.OccupyingCreature == creature) {
                return slot.OccupyingCard;
            }
        }

        return null;
    }

    public ICreature GetCreatureInSlot(ITarget slotId) {
        if (string.IsNullOrEmpty(slotId?.TargetId)) return null;
        return Battlefield.FirstOrDefault(s => s.TargetId == slotId.TargetId)?.OccupyingCreature;
    }

    public bool HasEmptyBattlefieldSlot() {
        return Battlefield.Any(s => !s.IsOccupied());
    }

    public override bool IsValidTarget() => true;

    public override string ToString() {
        return $"{Name} - Health: {Health}, Hand: {Hand.Count}, Battlefield: {Battlefield.Count(s => s.IsOccupied())}";
    }
}