using UnityEngine;
using System.Collections.Generic;

public class TestSetup : MonoBehaviour {
    public List<CardData> CreateTestCards() {
        var cards = new List<CardData>();

        var ogre = ScriptableObject.CreateInstance<CreatureData>();
        ogre.Name = "Ogre";
        ogre.Description = "High attack power";
        ogre.BaseAttack = 5;
        ogre.BaseHealth = 3;
        cards.Add(ogre);

        var thornCreature = ScriptableObject.CreateInstance<CreatureData>();
        thornCreature.Name = "Thorned";
        thornCreature.Description = "Deals 1 damage when hit";
        thornCreature.BaseAttack = 2;
        thornCreature.BaseHealth = 6;

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
        dragon.Name = "Dragon";
        dragon.Description = "Deals 2 damage when played";
        dragon.BaseAttack = 4;
        dragon.BaseHealth = 4;

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

        var warchief = ScriptableObject.CreateInstance<CreatureData>();
        warchief.Name = "Warchief";
        warchief.Description = "All other friendly creatures get +2 attack";
        warchief.BaseAttack = 3;
        warchief.BaseHealth = 3;

        var warchiefEffect = new CardEffect {
            effectType = EffectType.Continuous,
            trigger = EffectTrigger.None,
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.AttackBuff,
                    value = 2,
                    targetType = TargetType.FriendlyCreatures
                }
            }
        };

        warchief.effects.Add(warchiefEffect);
        cards.Add(warchief);


        return cards;
    }
}
