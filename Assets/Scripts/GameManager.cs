using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameManager>();
                if (instance == null) {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public IPlayer Player1 { get; private set; }
    public IPlayer Player2 { get; private set; }
    public GameContext GameContext { get; private set; }

    public event Action OnGameStateChanged;
    public event Action<IPlayer> OnPlayerDamaged;
    public event Action<ICreature> OnCreatureDamaged;
    public event Action<IPlayer> OnGameOver;

    private bool isInitialized = false;

    public void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (!isInitialized) {
            InitializeGame();
            isInitialized = true;
        }
    }

    private void InitializeGame() {
        try {
            GameContext = new GameContext();
            Player1 = new Player();
            Player2 = new Player();
            Player1.Opponent = Player2;
            Player2.Opponent = Player1;

            GameMediator.Instance.RegisterPlayer(Player1);
            GameMediator.Instance.RegisterPlayer(Player2);

            NotifyGameStateChanged();

            Debug.Log("GameManager initialized successfully");
        } catch (Exception e) {
            Debug.LogError($"Error initializing GameManager: {e.Message}");
            isInitialized = false;
        }
    }

    public void PlayCard(CardData cardData, IPlayer player) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

        Debug.Log($"Playing card: {cardData.cardName}");
        ICard card = CardFactory.CreateCard(cardData);
        card.Play(GameContext, player);
        GameContext.ResolveActions();
        NotifyGameStateChanged();
    }

    public void AttackWithCreature(ICreature attacker, IPlayer attackingPlayer, ICreature target = null) {
        if (!isInitialized) {
            Debug.LogError("GameManager not properly initialized!");
            return;
        }

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
        GameMediator.Instance?.NotifyGameStateChanged();
    }

    // Reset game state
    public void ResetGame() {
        isInitialized = false;
        InitializeGame();
    }

    // Scene management helper
    public void OnSceneLoaded() {
        if (!isInitialized) {
            InitializeGame();
        }
    }

    private void OnApplicationQuit() {
        isInitialized = false;
    }

    public void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
}