using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic; 

public class SceneLoading : Singleton<SceneLoading> 
{
	// Keeps of list of scenes that are currently being loaded. Scenes are removed from the list once loaded.
	[SerializeField] private List<string> scenesBeingLoaded; 

	public bool startedLoadingLevels = false; 

	// Use this for initialization
	void Start () 
	{
	
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
	}

	/// <summary>
	/// Checks if level scenes are currently being loaded
	/// </summary>
	/// <returns><c>true</c>, if scenes are being loaded, <c>false</c> if no loading is happening (loading has finished)</returns>
	public bool LevelScenesBeingLoaded()
	{
		return scenesBeingLoaded.Count == 0 ? false : true; 
	}

	/// <summary>
	/// Loads a scene based on the provided name
	/// Additional scene loading logic can be placed here
	/// </summary>
	/// <param name="name">Name.</param>
	public void LoadLevelScene (string name)
	{
		SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

		//startedLoadingLevels = true; 

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
	public void UnloadLevelScene (string name)
	{
		SceneManager.UnloadSceneAsync(name);
	}
}
