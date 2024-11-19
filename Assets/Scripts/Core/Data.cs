using System.Collections.Generic;
using UnityEngine;

public class CardEffect : Effect {
    public string effectType;
    public List<EffectAction> actions = new List<EffectAction>();
}

public class CardData : ScriptableObject {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Effect> Effects { get; set; } = new List<Effect>();
}

public class CreatureData : CardData {
    public int BaseAttack { get; set; }
    public int BaseHealth { get; set; }
}

public class Effect {
    public ITrigger Trigger { get; set; }
    public List<EffectAction> Actions { get; set; } = new List<EffectAction>();
}

public class EffectAction {
    public ActionData ActionData { get; set; }
    public string TargetType { get; set; } 
}

public class ActionData {
    public string ActionId { get; set; }
    public int Value { get; set; }
}
