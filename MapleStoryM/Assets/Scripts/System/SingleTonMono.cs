using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var _newGameObject = new GameObject(typeof(T).ToString());
                _instance = _newGameObject.AddComponent<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        DontDestroyOnLoad(gameObject);
    }
}
