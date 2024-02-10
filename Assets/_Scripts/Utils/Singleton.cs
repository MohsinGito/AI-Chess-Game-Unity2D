using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{

    [SerializeField] private bool dontDestroyOnLoad = true;
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType<T>();

                if (objs.Length > 0)
                {
                    _instance = objs[0];
                }
                if (objs.Length > 1)
                {
                    Debug.LogError($"There is more than one {typeof(T).Name} in the scene.");
                    for (int i = 1; i < objs.Length; i++)
                    {
                        Destroy(objs[i].gameObject);
                    }
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = $"_{typeof(T).Name}";
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}