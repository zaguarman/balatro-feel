using System.Collections.Generic;

public class CardFactory {
    private readonly IActionStrategyFactory actionStrategyFactory;
    private readonly IEffectStrategyFactory effectStrategyFactory;
    private readonly ITriggerFactory triggerFactory;

    public CardFactory(
        IActionStrategyFactory actionStrategyFactory,
        IEffectStrategyFactory effectStrategyFactory,
        ITriggerFactory triggerFactory) {
        this.actionStrategyFactory = actionStrategyFactory;
        this.effectStrategyFactory = effectStrategyFactory;
        this.triggerFactory = triggerFactory;
    }

    public ICreature CreateCard(CardData data) {
        var effects = new List<IEffect>();

        foreach (var effectData in data.Effects) {
            if (effectData is CardEffect cardEffect) {
                foreach (var action in cardEffect.actions) {
                    var actionStrategy = actionStrategyFactory.Create(action.ActionData);
                    var trigger = triggerFactory.Create(cardEffect.Trigger);
                    var effectStrategy = effectStrategyFactory.Create(cardEffect.effectType, trigger);

                    var gameAction = actionStrategy.CreateAction(action.ActionData.Value);
                    effects.Add(effectStrategy.CreateEffect(gameAction, data.Id));
                }
            }
        }

        return new Creature(
            data.Name,
            (data as CreatureData)?.BaseAttack ?? 0,
            (data as CreatureData)?.BaseHealth ?? 0,
            effects.ToArray()
        );
    }
}

public interface IActionStrategyFactory {
    IActionStrategy Create(ActionData data);
}

public interface IEffectStrategyFactory {
    IEffectStrategy Create(string effectType, ITrigger trigger);
}

public interface ITriggerFactory {
    ITrigger Create(string triggerType);
}