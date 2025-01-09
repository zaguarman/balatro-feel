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
    void AddToBattlefield(ICard creature, ITarget slotId = null);
    void RemoveFromBattlefield(ICard creature);
    PlayerDamagedUnityEvent OnDamaged { get; }
    void InitializeBattlefield(List<BattlefieldSlot> battlefieldSlots);
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

    public void InitializeBattlefield(List<BattlefieldSlot> slots) {
        if (slots == null || slots.Count == 0) {
            LogError("Cannot initialize battlefield with null or empty slots", LogTag.Initialization);
            return;
        }
        Battlefield.Clear();
        Battlefield.AddRange(slots);
        Log($"Initialized battlefield with {slots.Count} slots", LogTag.Initialization);
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

    public void AddToBattlefield(ICard card, ITarget slot = null) {
        if (card == null) return;

        BattlefieldSlot targetSlot = null;
        foreach (var battlefieldSlot in Battlefield) {
            if (slot.TargetId == battlefieldSlot.TargetId) {
                targetSlot = battlefieldSlot;
            }
        }

        var cardController = CardFactory.CreateCardController(card, this, targetSlot.transform);
        targetSlot.AssignCreature(cardController);
        gameMediator?.NotifyBattlefieldStateChanged(this); 
    }

    public void RemoveFromBattlefield(ICard creature) {
        //if (creature == null) return;

        //Battlefield.Remove(creature);

        //Log($"Removed creature {creature.Name} from battlefield", LogTag.Creatures);
        //gameMediator?.NotifyBattlefieldStateChanged(this);
        //gameMediator?.NotifyGameStateChanged();
    }

    public override bool IsValidTarget() => true;

    public override string ToString() {
        return $"{Name} - Health: {Health}, Hand: {Hand.Count}, Battlefield: {Battlefield.Count()}";
    }
}