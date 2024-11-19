public interface IPhaseStrategy {
    void EnterPhase(IGameContext context);
    void ExitPhase(IGameContext context);
    bool CanTransitionTo(IPhaseStrategy nextPhase);
}

public class PlayPhase : IPhaseStrategy {
    public void EnterPhase(IGameContext context) {
        // Handle play phase start logic - e.g. draw cards, refresh resources
    }

    public void ExitPhase(IGameContext context) {
        // Cleanup play phase
    }

    public bool CanTransitionTo(IPhaseStrategy nextPhase) {
        return nextPhase is CombatPhase;
    }
}

public class CombatPhase : IPhaseStrategy {
    public void EnterPhase(IGameContext context) {
        // Initialize combat - e.g. declare attackers
    }

    public void ExitPhase(IGameContext context) {
        // Resolve combat, handle damage and deaths
    }

    public bool CanTransitionTo(IPhaseStrategy nextPhase) {
        return nextPhase is EndPhase;
    }
}

public class EndPhase : IPhaseStrategy {
    public void EnterPhase(IGameContext context) {
        // End turn cleanup - e.g. discard excess cards
    }

    public void ExitPhase(IGameContext context) {
        // Final cleanup before next turn
    }

    public bool CanTransitionTo(IPhaseStrategy nextPhase) {
        return nextPhase is PlayPhase;
    }
}