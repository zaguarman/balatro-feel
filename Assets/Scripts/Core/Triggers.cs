using System;

public interface ITrigger {
    bool IsTriggered(IGameContext context);
}

namespace CardGame.Core.Triggers {
    public class OnDamageTrigger : ITrigger {
        public bool IsTriggered(IGameContext context) => context.LastAction is AttackAction;
    }

    public class OnPlayTrigger : ITrigger {
        public bool IsTriggered(IGameContext context) => context.CurrentPhase is PlayPhase;
    }

    public class OnPhaseStartTrigger : ITrigger {
        private readonly Type phaseType;

        public OnPhaseStartTrigger(Type phaseType) {
            this.phaseType = phaseType;
        }

        public bool IsTriggered(IGameContext context) {
            return context.CurrentPhase.GetType() == phaseType;
        }
    }
}