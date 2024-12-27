using System.Collections.Generic;
using System.Linq;
using static Enums;

public interface ITarget {
    string TargetId { get; }
    bool IsValidTarget();
}

public static class TargetingSystem {
    public static List<ITarget> GetValidTargets(IPlayer controller, TargetType targetType) {
        var validTargets = new List<ITarget>();

        switch (targetType) {
            case TargetType.Enemy:
                validTargets.Add(controller.Opponent);
                break;
            case TargetType.Player:
                validTargets.Add(controller);
                break;
            case TargetType.AllCreatures:
                validTargets.AddRange(controller.Battlefield.Where(s => s.IsValidTarget()).Cast<ITarget>());
                validTargets.AddRange(controller.Opponent.Battlefield.Where(s => s.IsValidTarget()).Cast<ITarget>());
                break;
            case TargetType.FriendlyCreatures:
                validTargets.AddRange(controller.Battlefield.Where(s => s.IsValidTarget()).Cast<ITarget>());
                break;
            case TargetType.EnemyCreatures:
                validTargets.AddRange(controller.Opponent.Battlefield.Where(s => s.IsValidTarget()).Cast<ITarget>());
                break;
        }

        return validTargets;
    }
}