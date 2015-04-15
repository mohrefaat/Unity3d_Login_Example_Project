using System.Collections;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	//the variable is declared to be volatile to ensure that
    //assignment to the instance variable completes before the
    //instance variable can be accessed.
    private static volatile T _Instance;

    private static object _lock = new object();

    public static T Instance {
        get {
            if (applicationIsQuitting) {
                /*Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");*/
                return null;
            }

            lock (_lock) {
                if (_Instance == null) {
                    _Instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1) {
                        /*Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");*/
                        return _Instance;
                    }

                    if (_Instance == null) {
                        GameObject singleton = new GameObject();
                        _Instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);
                        /*
                        Debug.Log("[Singleton] An Instance of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");*/
                    }
                    else {
                        /*Debug.Log("[Singleton] Using Instance already created: " +
                            _Instance.gameObject.name);*/
                    }
                }

                return _Instance;
            }
        }
    }

    private static bool applicationIsQuitting = false;

    public void OnDestroy() {
        applicationIsQuitting = true;
    }
}