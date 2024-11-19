using System.Collections.Generic;

public abstract class CardDecorator : Card {
    protected Card card;

    public CardDecorator(Card card) {
        this.card = card;
        this.Name = card.Name;
    }

    public override void Play(GameContext context, IPlayer owner) {
        card.Play(context, owner);
    }
}

public class ContinuousEffectDecorator : CardDecorator {
    private List<EffectAction> actions;
    private bool applied;

    public ContinuousEffectDecorator(Card card, List<EffectAction> actions) : base(card) {
        this.actions = actions;
        this.applied = false;
    }

    public override void Play(GameContext context, IPlayer owner) {
        base.Play(context, owner);
        if (!applied) {
            foreach (var action in actions) {
                ApplyEffect(action, context, owner);
            }
            applied = true;
        }
    }

    private void ApplyEffect(EffectAction action, IGameContext context, IPlayer owner) {
        // Implement continuous effects like buffs, auras, etc.
    }
}