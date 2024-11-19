using System.Collections.Generic;
using System.Linq;

namespace CardGame.Core.Effects {
    public class EffectManager {
        private readonly List<(IEffect Effect, ITarget Target)> activeEffects = new();
        private readonly IGameContext context;

        public EffectManager(IGameContext context) {
            this.context = context;
        }

        public void AddEffect(IEffect effect, ITarget target) {
            activeEffects.Add((effect, target));
        }

        public void RemoveEffect(IEffect effect) {
            activeEffects.RemoveAll(x => x.Effect == effect);
        }

        public void UpdateEffects() {
            foreach (var (effect, target) in activeEffects.ToList()) {
                if (effect.IsActive) {
                    effect.Apply(context, target);
                } else {
                    RemoveEffect(effect);
                }
            }
        }
    }
}