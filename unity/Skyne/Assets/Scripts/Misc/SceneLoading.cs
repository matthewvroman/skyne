using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic; 

public class SceneLoading : Singleton<SceneLoading> 
{
	// Keeps of list of scenes that are currently being loaded. Scenes are removed from the list once loaded.
	[SerializeField] private List<string> scenesBeingLoaded; 
	[SerializeField] private List<string> scenesBeingUnloaded; 

	// List of scenes waiting to be unloaded
	[SerializeField] private List<string> scenesToUnload; 

	//public bool startedLoadingLevels = false; 
	public bool lockLevelSceneLoad; 

	public float loadingProgress; 

	Queue<string> sceneQ; 
	AsyncOperation sceneOp; 

	// Use this for initialization
	void Start () 
	{
		sceneQ = new Queue<string> (); 
	}

	// Update is called once per frame
	void Update () 
	{
		// Update the list of scenesBeingLoaded, removing scene names that have finished loading
		for (int i = 0; i < scenesBeingLoaded.Count; i++)
		{
			if (SceneManager.GetSceneByName(scenesBeingLoaded[i]).isLoaded)
			{
				scenesBeingLoaded.RemoveAt(i); 
			}
		}

		for (int i = 0; i < scenesBeingUnloaded.Count; i++)
		{
			if (!SceneManager.GetSceneByName(scenesBeingUnloaded[i]).isLoaded)
			{
				scenesBeingUnloaded.RemoveAt(i); 
			}
		}

		// Unload scenes waiting to be unloaded
		for (int i = 0; i < scenesToUnload.Count; i++)
		{
			if (!SceneManager.GetSceneByName(scenesToUnload[i]).IsValid())
			{
				scenesToUnload.RemoveAt(i); 
			}
			else
			{
				UnloadStart(scenesToUnload[i]); 
			}
		}

		// Update loading progress
		//loadingProgress = scenesBeingLoaded.Count

		// Update queue loading
		if (sceneQ.Count > 0 && sceneOp != null && sceneOp.isDone)
		{
			sceneQ.Dequeue(); 

			if (sceneQ.Count > 0)
			{
				SetAsynchLevelLoad(); 
			}
		}
			
	}

	/// <summary>
	/// Checks if level scenes are currently being loaded
	/// </summary>
	/// <returns><c>true</c>, if scenes are being loaded, <c>false</c> if no loading is happening (loading has finished)</returns>
	public bool LevelScenesBeingLoaded()
	{
		return scenesBeingLoaded.Count == 0 ? false : true; 
	}

	public bool LevelUnloadComplete()
	{
		if (scenesToUnload.Count == 0 && scenesBeingUnloaded.Count == 0)
			return true; 
		return false; 
	}

	/// <summary>
	/// Loads a scene based on the provided name
	/// Additional scene loading logic can be placed here
	/// </summary>
	/// <param name="name">Name.</param>
	public void LoadLevelScene (string name)
	{
		if (lockLevelSceneLoad)
		{
			Debug.Log("Stopped scene from loading because lockLevelSceneLoad == true"); 
			return; 
		}
		//Debug.Log("Set scene to load: " + name); 

		//SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		EnqueueScene(name);  

		//startedLoadingLevels = true; 

		// Load the scene if it's not already loaded and it is not currently being unloaded
		if (!scenesBeingLoaded.Contains(name))
		{
			scenesBeingLoaded.Add(name); 
		}
	}

	/// <summary>
	/// Unloads a scene based on the provided name
	/// Additional scene unloading logic can be placed here
	/// </summary>
	/// <param name="name">Name.</param>
	void UnloadStart (string name)
	{
		SceneManager.UnloadSceneAsync(name);
		scenesBeingUnloaded.Add(name); 
	}

	public void UnloadLevelScene (string name)
	{
		scenesToUnload.Add(name); 
	}

	/// <summary>
	/// Unloads all level scenes.
	/// </summary>
	/// <param name="sceneList">Scene list from SceneMapping singleton in MainLevel</param>
	public void UnloadAllLevelScenes (SceneMapping.sceneMap[] sceneList)
	{
		// This will only work if the main level is already loaded when the unload process starts
		if (SceneManager.GetSceneByName("MainLevel").isLoaded)
		{
			lockLevelSceneLoad = true; 

			for (int i = 0; i < sceneList.Length; i++)
			{
				scenesToUnload.Add(sceneList[i].sceneName); 
			}
		}
	}

	void EnqueueScene(string name)
	{
		sceneQ.Enqueue(name); 

		// If this is the first scene being added to the queue
		if (sceneQ.Count == 1)
		{
			SetAsynchLevelLoad(); 
		}
	}

	void SetAsynchLevelLoad()
	{
		// Error case- trying to load a duplicate level scene
		// TODO- ensure that this code is stable. This could have potential for bugs. 
		if (SceneManager.GetSceneByName(sceneQ.Peek()).isLoaded)
		{
			Debug.LogWarning("Tried to load a scene that's already loaded. Trying the next scene, if available"); 
			sceneQ.Dequeue(); 

			if (sceneQ.Count > 0)
			{
				SetAsynchLevelLoad(); 
			}

			return; 
		}

		sceneOp = SceneManager.LoadSceneAsync(sceneQ.Peek(), LoadSceneMode.Additive);
	}
}
