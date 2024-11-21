using UnityEngine;
using System.Collections.Generic;
using static Enums;

public class TestSetup : MonoBehaviour {
    public List<CardData> CreateTestCards() {
        var cards = new List<CardData>();

        var ogre = ScriptableObject.CreateInstance<CreatureData>();
        ogre.cardName = "Ogre";
        ogre.description = "High attack power";
        ogre.attack = 5;
        ogre.health = 3;
        cards.Add(ogre);

        var thornCreature = ScriptableObject.CreateInstance<CreatureData>();
        thornCreature.cardName = "Thorned";
        thornCreature.description = "Deals 1 damage when hit";
        thornCreature.attack = 2;
        thornCreature.health = 6;

        var thornEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.OnDamage,
            actions = new List<EffectAction>
            {
                new EffectAction
                {
                    actionType = ActionType.Damage,
                    value = 1,
                    targetType = TargetType.Enemy
                }
            }
        };
        thornCreature.effects.Add(thornEffect);
        cards.Add(thornCreature);

        var dragon = ScriptableObject.CreateInstance<CreatureData>();
        dragon.cardName = "Dragon";
        dragon.description = "Deals 2 damage when played";
        dragon.attack = 4;
        dragon.health = 4;

        var dragonEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.OnPlay,
            actions = new List<EffectAction>
            {
                new EffectAction
                {
                    actionType = ActionType.Damage,
                    value = 2,
                    targetType = TargetType.Enemy
                }
            }
        };
        dragon.effects.Add(dragonEffect);
        cards.Add(dragon);

        return cards;
    }
}
