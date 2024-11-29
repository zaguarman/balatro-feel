using UnityEngine;

public abstract class Singleton<T> : InitializableComponent where T : InitializableComponent {
    protected static T instance;
    private static bool isQuitting = false;

    public static T Instance {
        get {
            if (isQuitting) {
                //Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            if (instance == null) {
                instance = FindObjectOfType<T>();

                if (instance == null) {
                    Debug.LogError($"{typeof(T).Name} not found in scene!");
                }
            }
            return instance;
        }
    }

    protected override void Awake() {
        base.Awake();

        // Reset the quitting flag when a new instance is created
        isQuitting = false;

        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
    }

    protected virtual void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }

    protected virtual void OnApplicationQuit() {
        isQuitting = true;
    }

    // Optional: Add this if you want to reset when entering play mode in editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        isQuitting = false;
        instance = null;
    }
}