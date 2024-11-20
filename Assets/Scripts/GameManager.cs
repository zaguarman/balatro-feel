using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    public event Action OnGameStateChanged;
    public event Action<IPlayer> OnPlayerDamaged;
    public event Action<ICreature> OnCreatureDamaged;
    public event Action<IPlayer> OnGameOver;

    public void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializeGame();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeGame() {
        GameContext = new GameContext();
        Player1 = new Player();
        Player2 = new Player();
        Player1.Opponent = Player2;
        Player2.Opponent = Player1;
        NotifyGameStateChanged();
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        Debug.Log($"Playing card: {cardData.cardName}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    public void AttackWithCreature(ICreature attacker, IPlayer attackingPlayer, ICreature target = null) {
        if (target == null) {
            Debug.Log($"{attacker.Name} attacking opponent directly");
            GameContext.AddAction(new DamagePlayerAction(attackingPlayer.Opponent, attacker.Attack));
        } else {
            Debug.Log($"{attacker.Name} attacking {target.Name}");
            GameContext.AddAction(new DamageCreatureAction(target, attacker.Attack));
            GameContext.AddAction(new DamageCreatureAction(attacker, target.Attack));
        }

        GameContext.ResolveActions();
        CleanupDeadCreatures(Player1);
        CleanupDeadCreatures(Player2);
        NotifyGameStateChanged();
    }

    private void CleanupDeadCreatures(IPlayer player) {
        var deadCreatures = player.Battlefield.FindAll(creature => creature.Health <= 0);
        foreach (var creature in deadCreatures) {
            Debug.Log($"Removing dead creature: {creature.Name}");
            player.RemoveFromBattlefield(creature);
        }
    }

    public void NotifyGameStateChanged() {
        OnGameStateChanged?.Invoke();
    }
}