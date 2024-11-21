using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
public interface ITarget {
    string TargetId { get; }
    bool IsValidTarget(IPlayer controller);
}

public class TargetingSystem {
    public List<ITarget> GetValidTargets(IPlayer controller, TargetType targetType) {
        var validTargets = new List<ITarget>();

        switch (targetType) {
            case TargetType.Enemy:
                validTargets.Add(controller.Opponent);
                Debug.Log($"Added opponent as valid target");
                break;
            case TargetType.Player:
                validTargets.Add(controller);
                Debug.Log($"Added controller as valid target");
                break;
            case TargetType.AllCreatures:
                validTargets.AddRange(controller.Battlefield.Where(c => c.IsValidTarget(controller)));
                validTargets.AddRange(controller.Opponent.Battlefield.Where(c => c.IsValidTarget(controller)));
                Debug.Log($"Added {validTargets.Count} creatures as valid targets");
                break;
            case TargetType.FriendlyCreatures:
                validTargets.AddRange(controller.Battlefield.Where(c => c.IsValidTarget(controller)));
                Debug.Log($"Added {validTargets.Count} friendly creatures as valid targets");
                break;
            case TargetType.EnemyCreatures:
                validTargets.AddRange(controller.Opponent.Battlefield.Where(c => c.IsValidTarget(controller)));
                Debug.Log($"Added {validTargets.Count} enemy creatures as valid targets");
                break;
        }

        return validTargets;
    }
}