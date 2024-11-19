using CardGame.Core.Effects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AuraManager : MonoBehaviour {
    private Dictionary<ICreature, BuffDecorator> decoratedCreatures = new Dictionary<ICreature, BuffDecorator>();
    private readonly IEffectStrategy auraStrategy;
    private GameEventMediator mediator;
    private EffectManager effectManager;

    private void Awake() {
        mediator = GameEventMediator.Instance;
        effectManager = new EffectManager(GameManager.Instance.GameContext);

        if (mediator != null) {
            mediator.OnCreatureSummoned += HandleCreatureSummoned;
            mediator.OnCreatureDied += HandleCreatureDied;
        }
    }

    private bool HasAuraEffect(ICreature creature) {
        return creature.Effects?.Any(effect => effect is ContinuousEffect) ?? false;
    }

    private void HandleCreatureSummoned(ICreature creature, IPlayer owner) {
        // Apply existing auras to new creature
        foreach (var existingCreature in owner.Battlefield) {
            if (existingCreature != creature && HasAuraEffect(existingCreature)) {
                ApplyAuraToCreature(creature, existingCreature);
            }
        }

        // Apply new creature's aura to existing creatures
        if (HasAuraEffect(creature)) {
            foreach (var existingCreature in owner.Battlefield) {
                if (existingCreature != creature) {
                    ApplyAuraToCreature(existingCreature, creature);
                }
            }
        }
    }

    private void ApplyAuraToCreature(ICreature target, ICreature source) {
        var auraEffects = source.Effects.OfType<ContinuousEffect>();

        foreach (var effect in auraEffects) {
            if (!decoratedCreatures.TryGetValue(target, out var decorator)) {
                decorator = new BuffDecorator(target);
                decoratedCreatures[target] = decorator;
            }

            // Create buff actions based on the effect
            if (effect is ContinuousEffect continuousEffect) {
                var buffAction = new BuffAction(decorator, continuousEffect);
                effectManager.AddEffect(effect, target);
            }
        }
    }

    private void HandleCreatureDied(ICreature creature, IPlayer owner) {
        // Remove all effects where this creature was the source
        foreach (var decorator in decoratedCreatures.Values) {
            decorator.RemoveBuffsFromSource(creature.Id);
        }

        // Remove the creature's decorator if it had one
        decoratedCreatures.Remove(creature);

        CleanupEmptyDecorators();
    }

    private void CleanupEmptyDecorators() {
        var emptyDecorators = decoratedCreatures
            .Where(kvp => !kvp.Value.HasActiveBuffs())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var creature in emptyDecorators) {
            decoratedCreatures.Remove(creature);
        }
    }

    public ICreature GetDecoratedCreature(ICreature creature) {
        return decoratedCreatures.TryGetValue(creature, out var decorator)
            ? decorator
            : creature;
    }

    private void OnDestroy() {
        if (mediator != null) {
            mediator.OnCreatureSummoned -= HandleCreatureSummoned;
            mediator.OnCreatureDied -= HandleCreatureDied;
        }
    }
}