using UnityEngine;

/// <summary>
/// Parent class for singleton GameObjects
/// Any class that should be marked as a singleton should inherit from this class using the declaration:
/// 	ClassName : Singleton<ClassName> 
/// Inheriting from this class prevents more than one instance of the class from being instantiated
/// Once a class inherits from Singleton, its instance can be accessed from any script using the statement:
/// 	ClassName.inst.STATEMENT
/// </summary>


public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T instance; 

	// The variable used for accessing the instance of the singleton
	public static T inst 
	{ 
		get 
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<T>(); 
			}
			return instance; 
		}
	}
}


/*
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	// The variable used for accessing the instance of the singleton
	public static T inst { get; private set; }

	// Called in Awake()
	// There could be errors if other scripts try to call these singletons before CheckSingleton() has been called
	// You should avoid calling any singleton instances until after Awake()
	public void CheckSingleton()
	{
		if (inst == null)
		{
			inst = (T) this;
		}
		else
		{
			Debug.LogError("Got a second instance of the class " + this.GetType());
		}
	}

	void Awake()
	{
		// This line must be included first
		CheckSingleton(); 
	}
}
*/