using UnityEngine;

/// <summary>
/// Simple MonoBehaviour singleton helper for managers.
/// Use: public class GameManager : Singleton<GameManager> { ... }
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnInit();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // override for initialization in derived classes
    protected virtual void OnInit() { }
}
