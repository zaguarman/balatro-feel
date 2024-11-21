using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ReferenceManager {
    private static ReferenceManager instance;
    public static ReferenceManager Instance {
        get {
            if (instance == null) {
                instance = new ReferenceManager();
            }
            return instance;
        }
    }

    private Dictionary<Type, Dictionary<string, UnityEngine.Object>> referenceCache
        = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();

    public static void Initialize(GameReferences gameReferences) {
        Instance.CacheReferences(gameReferences);
    }

    private void CacheReferences(GameReferences references) {
        var fields = typeof(GameReferences).GetFields(BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var field in fields) {
            var value = field.GetValue(references);
            if (value is UnityEngine.Object reference) {
                CacheReference(field.Name, reference);
            }
        }
    }

    private void CacheReference(string name, UnityEngine.Object reference) {
        var type = reference.GetType();
        if (!referenceCache.ContainsKey(type)) {
            referenceCache[type] = new Dictionary<string, UnityEngine.Object>();
        }
        referenceCache[type][name] = reference;
    }

    public T GetReference<T>(string name) where T : UnityEngine.Object {
        if (referenceCache.TryGetValue(typeof(T), out var typeCache)) {
            if (typeCache.TryGetValue(name, out var reference)) {
                return reference as T;
            }
        }
        Debug.LogWarning($"Reference not found: {name} of type {typeof(T)}");
        return null;
    }

    public GameMediator GetGameMediator() => GameMediator.Instance;

    public void Clear() {
        referenceCache.Clear();
    }
}