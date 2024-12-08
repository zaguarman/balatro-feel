
using System;
using UnityEngine;

public interface ICreature : ICard {
    int Attack { get; }
    int Health { get; }
    void TakeDamage(int damage);
}

public class Creature : Card, ICreature {
    public int Attack { get; private set; }
    public int Health { get; private set; }
    private bool isDead = false;
    private IPlayer owner;

    public Creature(string name, int attack, int health) : base(name) {
        Attack = attack;
        Health = health;
    }

    public override void Play(IPlayer owner, ActionsQueue context) {
        this.owner = owner;
        context.AddAction(new SummonCreatureAction(this, owner));
    }

    public void TakeDamage(int damage) {
        if (isDead) return; // Prevent multiple death triggers

        Health = Math.Max(0, Health - damage);
        Debug.Log($"[Creature] {Name} took {damage} damage, health now: {Health}");

        var gameMediator = GameMediator.Instance;
        if (gameMediator != null) {
            gameMediator.NotifyCreatureDamaged(this, damage);

            if (Health <= 0 && !isDead) {
                isDead = true;
                Debug.Log($"[Creature] {Name} died, removing from battlefield");

                // Remove from owner's battlefield
                if (owner != null) {
                    owner.RemoveFromBattlefield(this);
                }

                gameMediator.NotifyCreatureDied(this);
            }
        }
    }
}