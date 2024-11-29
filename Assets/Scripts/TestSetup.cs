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
            actions = new List<EffectAction> {
                new EffectAction {
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
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.Damage,
                    value = 2,
                    targetType = TargetType.Enemy
                }
            }
        };
        dragon.effects.Add(dragonEffect);
        cards.Add(dragon);

        // New Card 1: Guardian Angel
        var guardian = ScriptableObject.CreateInstance<CreatureData>();
        guardian.cardName = "Guardian Angel";
        guardian.description = "Heals friendly creatures at the start of your turn";
        guardian.attack = 2;
        guardian.health = 5;

        var guardianEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.StartOfTurn,
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.Heal,
                    value = 1,
                    targetType = TargetType.FriendlyCreatures
                }
            }
        };
        guardian.effects.Add(guardianEffect);
        cards.Add(guardian);

        // New Card 2: Berserker
        var berserker = ScriptableObject.CreateInstance<CreatureData>();
        berserker.cardName = "Berserker";
        berserker.description = "Deals 1 damage to all creatures at end of turn";
        berserker.attack = 3;
        berserker.health = 3;

        var berserkerEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.EndOfTurn,
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.Damage,
                    value = 1,
                    targetType = TargetType.AllCreatures
                }
            }
        };
        berserker.effects.Add(berserkerEffect);
        cards.Add(berserker);

        // New Card 3: Soul Collector
        var soulCollector = ScriptableObject.CreateInstance<CreatureData>();
        soulCollector.cardName = "Soul Collector";
        soulCollector.description = "Draw a card when another creature dies";
        soulCollector.attack = 2;
        soulCollector.health = 4;

        var soulCollectorEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.OnDeath,
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.Draw,
                    value = 1,
                    targetType = TargetType.Player
                }
            }
        };
        soulCollector.effects.Add(soulCollectorEffect);
        cards.Add(soulCollector);

        // New Card 4: War Chief
        var warChief = ScriptableObject.CreateInstance<CreatureData>();
        warChief.cardName = "War Chief";
        warChief.description = "Damages enemy creatures when played";
        warChief.attack = 3;
        warChief.health = 5;

        var warChiefEffect = new CardEffect {
            effectType = EffectType.Triggered,
            trigger = EffectTrigger.OnPlay,
            actions = new List<EffectAction> {
                new EffectAction {
                    actionType = ActionType.Damage,
                    value = 1,
                    targetType = TargetType.EnemyCreatures
                }
            }
        };
        warChief.effects.Add(warChiefEffect);
        cards.Add(warChief);

        return cards;
    }
}
