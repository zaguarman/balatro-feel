using UnityEngine;
using System.Collections.Generic;

public enum CardType { Creature, Spell, Enchantment }
public enum EffectType { Immediate, Triggered, Continuous }
public enum EffectTrigger { OnPlay, OnDeath, OnDamage, StartOfTurn, EndOfTurn }
public enum ActionType { Damage, Heal, Draw, Summon }
public enum TargetType { Player, Enemy, AllCreatures, FriendlyCreatures, EnemyCreatures }

public abstract class CardData : ScriptableObject {
    public string cardName;
    public string description;
    public CardType cardType;
    public List<CardEffect> effects = new List<CardEffect>();
}

public class CreatureData : CardData {
    public int attack;
    public int health;

    public void OnEnable() {
        cardType = CardType.Creature;
    }
}

[System.Serializable]
public class CardEffect {
    public EffectType effectType;
    public EffectTrigger trigger;
    public List<EffectAction> actions = new List<EffectAction>();
}

[System.Serializable]
public class EffectAction {
    public ActionType actionType;
    public int value;
    public TargetType targetType;
}