using System.Collections;
using UnityEngine;
using static DebugLogger;
public class GameBootstrap : MonoBehaviour {
    private void Start() {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame() {
        var initManager = InitializationManager.Instance;

        while (GameReferences.Instance == null ||
               GameMediator.Instance == null ||
               GameManager.Instance == null ||
               GameUI.Instance == null) {
            yield return null;
        }

        Log("Starting game initialization", LogTag.Initialization);

        initManager.RegisterComponent(GameReferences.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameReferences.Instance.IsInitialized);
        Log("GameReferences initialized", LogTag.Initialization);

        initManager.RegisterComponent(GameMediator.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameMediator.Instance.IsInitialized);
        Log("GameMediator initialized", LogTag.Initialization);

        if (GameManager.Instance != null) {
            initManager.RegisterComponent(GameManager.Instance);
            initManager.InitializeComponents();
            yield return new WaitUntil(() => GameManager.Instance.IsInitialized);
            Log("GameManager initialized", LogTag.Initialization);
        } else {
            LogError("GameManager instance is null!", LogTag.Initialization);
            yield break;
        }

        initManager.RegisterComponent(GameUI.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameUI.Instance.IsInitialized);
        Log("GameUI initialized", LogTag.Initialization);

        Log("All components initialized successfully", LogTag.Initialization);
    }
}