using System.Collections.Generic;
using System.Linq;

public interface ITarget {
    void ReceiveAction(IAction action);
    string Id { get; }
}
public interface ITargetStrategy {
    List<ITarget> GetTargets(IGameContext context, IPlayer owner);
}

public class PlayerTargetStrategy : ITargetStrategy {
    private readonly bool targetOwner;

    public PlayerTargetStrategy(bool targetOwner) {
        this.targetOwner = targetOwner;
    }

    public List<ITarget> GetTargets(IGameContext context, IPlayer owner) {
        ITarget target = targetOwner ? (ITarget)owner : (ITarget)owner.Opponent;
        return new List<ITarget> { target };
    }
}

public class CreatureTargetStrategy : ITargetStrategy {
    private readonly bool friendlyOnly;
    private readonly bool enemyOnly;

    public CreatureTargetStrategy(bool friendlyOnly = false, bool enemyOnly = false) {
        this.friendlyOnly = friendlyOnly;
        this.enemyOnly = enemyOnly;
    }

    public List<ITarget> GetTargets(IGameContext context, IPlayer owner) {
        var targets = new List<ITarget>();
        if (!enemyOnly) {
            targets.AddRange(owner.Battlefield.Cast<ITarget>());
        }
        if (!friendlyOnly) {
            targets.AddRange(owner.Opponent.Battlefield.Cast<ITarget>());
        }
        return targets;
    }
}
