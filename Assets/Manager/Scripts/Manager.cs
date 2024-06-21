using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ManagerModule : MonoBehaviour
{
    private bool _quitting;

    protected virtual void Awake()
    {
        Debug.Log($"[Manager] Registering {GetType()} on GameObject {gameObject.name}");
        Manager.Instance.Register(this);
        OnAwake();
    }

    protected virtual void OnApplicationQuit()
    {
        _quitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_quitting)
            return;

        if (Manager.Instance != null)
        {
            Manager.Instance.Unregister(this);
        }
    }

    public virtual void OnAwake() { }
}

public abstract class StaticManagerModule : ManagerModule { }

public abstract class StaticManagerModule<T> : StaticManagerModule where T : StaticManagerModule<T>
{
}

public class ModuleAlreadyRegisteredException : Exception
{
    public ModuleAlreadyRegisteredException(string message) : base(message)
    {
    }
}

public class ModuleNotFoundException : Exception { }

public class Manager : MonoBehaviour
{
    public static Manager Instance;

    [SerializeField]
    private Dictionary<Type, ManagerModule> _modules;

    private void Awake()
    {
        if (Instance != null)
        {
            throw new ArgumentException();
        }
        _modules = new Dictionary<Type, ManagerModule>();
        Instance = this;
    }

    public void Register(ManagerModule managerModule)
    {
        if (ModuleRegistered(managerModule.GetType()))
        {
            throw new ModuleAlreadyRegisteredException($"{managerModule.GetType().Name} is already registered");
        }
        _modules.Add(managerModule.GetType(), managerModule);
    }

    public void Unregister(ManagerModule managerModule)
    {
        if (!ModuleRegistered(managerModule.GetType()))
        {
            throw new ModuleNotFoundException();
        }
        _modules.Remove(managerModule.GetType());
    }

    private bool ModuleRegistered(Type t)
    {
        return _modules.ContainsKey(t);
    }

    public bool ModuleRegistered<T>()
    {
        return ModuleRegistered(typeof(T));
    }

    public T Get<T>() where T : ManagerModule
    {
        if (!ModuleRegistered(typeof(T)))
        {
            if (typeof(T).IsSubclassOf(typeof(StaticManagerModule)))
            {
                GameObject go = new GameObject(typeof(T).Name);
                go.transform.parent = Instance.gameObject.transform;
                T module = (T)go.AddComponent(typeof(T));
                return module;
            }
            throw new ModuleNotFoundException();
        }
        return (T)_modules[typeof(T)];
    }

    public static T Use<T>() where T : ManagerModule
    {
        return Application.isPlaying ? Instance.Get<T>() : FindObjectOfType<T>();
    }
}