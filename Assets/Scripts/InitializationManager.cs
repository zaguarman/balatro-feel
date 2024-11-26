using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInitializable {
    bool IsInitialized { get; }
    void Initialize();
}

public class InitializationManager : Singleton<InitializationManager> {
    private HashSet<IInitializable> initializedComponents = new HashSet<IInitializable>();
    private HashSet<IInitializable> pendingComponents = new HashSet<IInitializable>();
    private bool systemInitialized = false;

    public event Action OnSystemInitialized;

    protected override void Awake() {
        base.Awake();
        Debug.Log("InitializationManager Awake");
    }

    public void RegisterComponent(IInitializable component) {
        if (component == null) return;

        if (!pendingComponents.Contains(component) && !initializedComponents.Contains(component)) {
            Debug.Log($"Registering component for initialization: {component.GetType().Name}");
            pendingComponents.Add(component);
            CheckInitialization();
        }
    }

    public void MarkComponentInitialized(IInitializable component) {
        if (component == null) return;

        if (pendingComponents.Contains(component)) {
            Debug.Log($"Component initialized: {component.GetType().Name}");
            pendingComponents.Remove(component);
            initializedComponents.Add(component);
            CheckInitialization();
        }
    }

    public bool IsComponentInitialized(IInitializable component) {
        return initializedComponents.Contains(component);
    }

    private void CheckInitialization() {
        Debug.Log($"Checking initialization. Pending components: {pendingComponents.Count}");
        if (pendingComponents.Count == 0 && !systemInitialized) {
            systemInitialized = true;
            Debug.Log("All components initialized - system initialization complete");
            OnSystemInitialized?.Invoke();
        }
    }

    public void Reset() {
        initializedComponents.Clear();
        pendingComponents.Clear();
        systemInitialized = false;
    }
}