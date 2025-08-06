using UnityEngine;
using Unity.Netcode;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    protected static T _instance;

    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                _instance = null;
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[NetworkSingleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T);

                        // 반드시 NetworkObject도 추가
                        if (singleton.GetComponent<NetworkObject>() == null)
                        {
                            singleton.AddComponent<NetworkObject>();
                        }

#if UNITY_EDITOR
                        if (Application.isPlaying == true)
                        {
                            DontDestroyOnLoad(singleton);
                        }
#else
                        DontDestroyOnLoad(singleton);
#endif
                        Debug.Log("[NetworkSingleton] An instance of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[NetworkSingleton] Using instance already created: " +
                            _instance.gameObject.name);
                    }
                }

                _instance.hideFlags = HideFlags.None;
                return _instance;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        _instance = null;
    }
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void Awake()
    {
        _applicationIsQuitting = false;
    }

    protected virtual void Start()
    {
    }
}
