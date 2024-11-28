using System.Collections;
using UnityEngine;

public class GameBootstrap : MonoBehaviour {
    private void Start() {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame() {
        var initManager = InitializationManager.Instance;

        // Wait for all required components to be available
        while (GameReferences.Instance == null ||
               GameMediator.Instance == null ||
               GameManager.Instance == null ||
               GameUI.Instance == null) {
            yield return null;
        }

        // Step 1: Initialize GameReferences
        initManager.RegisterComponent(GameReferences.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameReferences.Instance.IsInitialized);

        // Step 2: Initialize GameMediator
        initManager.RegisterComponent(GameMediator.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameMediator.Instance.IsInitialized);

        // Step 3: Initialize GameManager
        if (GameManager.Instance != null) {
            initManager.RegisterComponent(GameManager.Instance);
            initManager.InitializeComponents();
            yield return new WaitUntil(() => GameManager.Instance.IsInitialized);
        } else {
            Debug.LogError("GameManager instance is null!");
            yield break;
        }

        // Step 4: Initialize GameUI
        initManager.RegisterComponent(GameUI.Instance);
        initManager.InitializeComponents();
        yield return new WaitUntil(() => GameUI.Instance.IsInitialized);

        Debug.Log("All components initialized successfully");
    }
}