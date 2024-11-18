using UnityEngine;
using System.Collections.Generic;

public class TestSetup : MonoBehaviour {
    public List<CardData> CreateTestCards() {
        var cards = new List<CardData>();

        // 1. Basic Creature - Goblin
        var goblin = ScriptableObject.CreateInstance<CreatureData>();
        goblin.cardName = "Goblin";
        goblin.description = "A basic creature";
        goblin.attack = 2;
        goblin.health = 1;
        cards.Add(goblin);

        // 2. Tough Creature - Guard
        var guard = ScriptableObject.CreateInstance<CreatureData>();
        guard.cardName = "Guard";
        guard.description = "Defensive unit";
        guard.attack = 1;
        guard.health = 5;
        cards.Add(guard);

        // 3. Strong Creature - Ogre
        var ogre = ScriptableObject.CreateInstance<CreatureData>();
        ogre.cardName = "Ogre";
        ogre.description = "High attack power";
        ogre.attack = 5;
        ogre.health = 3;
        cards.Add(ogre);

        // 4. Creature with OnDamage effect - Thorned Creature
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

        // 5. Creature with OnPlay effect - Fire Dragon
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
