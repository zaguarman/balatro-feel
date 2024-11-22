using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    protected static T instance;
    public static T Instance {
        get {
            if (instance == null) {
                var go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
            }
            return instance;
        }
    }

    protected virtual void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }
} 