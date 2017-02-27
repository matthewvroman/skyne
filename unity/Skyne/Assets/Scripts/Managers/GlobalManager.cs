using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class GlobalManager : Singleton<GlobalManager> 
{
	public enum GlobalState {Menu, LoadMainGameplayScene, SetupGameplay, Gameplay, GameOver};  

	[Tooltip("The state machine of the game. Set to Menu when starting from Menu; set to Gameplay when starting in the middle of the game.")]
	public GlobalState globalState; 

	// Use this for initialization
	void Start () 
	{
		// Used for directly loading the game from within gameplay while in the editor
		#if UNITY_EDITOR
		if (globalState == GlobalState.Gameplay || globalState == GlobalState.SetupGameplay)
		{
			LoadGameplayScreen(); 
			//globalState = GlobalState.LoadMainGameplayScene; 
			MainGameplayManager.inst.SetupGameplayScreen(); 
		}
		#endif

		if (globalState == GlobalState.Menu)
		{
			LoadTitle(); 
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (globalState == GlobalState.LoadMainGameplayScene)
		{
			// Once the main (global) level scene is loaded, call it to setup the rest of the game and load the remaining level scenes
			if (SceneManager.GetSceneByName("MainLevel").isLoaded)
			{
				// Close scenes that might be open when the main level is being loaded
				UnloadSceneIfLoaded("Title"); 
				UnloadSceneIfLoaded("GameOver"); 

				globalState = GlobalState.SetupGameplay; 
				MainGameplayManager.inst.SetupGameplayScreen(); 
			}
		}
		else if (globalState == GlobalState.SetupGameplay)
		{
			// Once everything has been loaded
			//if (!SceneLoading.inst.LevelScenesBeingLoaded() && SceneLoading.inst.startedLoadingLevels)
			if (!SceneLoading.inst.LevelScenesBeingLoaded())
			{
				//Debug.Log("Test finished loading"); 
				globalState = GlobalState.Gameplay; 
				MainGameplayManager.inst.OnGameplayStart(); 

			}
		}
	}

	public void LoadTitle()
	{ 
		globalState = GlobalState.Menu; 
		LoadSceneIfUnloaded("Title"); 
		UnloadSceneIfLoaded("GameOver"); 
		UnloadSceneIfLoaded("MainLevel"); 

		// Unload all level scenes as well as the MainLevel
		LevelData.inst.UnloadAllLevelScenes(); 
	}

	public void LoadGameplayScreen()
	{
		globalState = GlobalState.LoadMainGameplayScene; 

		// Add the loading screen
		LoadSceneIfUnloaded("Loading"); 

		// Add the MainLevel screen
		LoadSceneIfUnloaded("MainLevel"); 
	}

	public void LoadGameOver()
	{
		globalState = GlobalState.GameOver; 

		LoadSceneIfUnloaded("GameOver"); 
		Cursor.lockState = CursorLockMode.Confined;

		// Unload all level scenes as well as the MainLevel
		LevelData.inst.UnloadAllLevelScenes(); 

		UnloadSceneIfLoaded("MainLevel"); 
	}



	/// <summary>
	/// Helper for SceneManager that unloads a scene if it already is loaded.
	/// </summary>
	/// <returns><c>true</c>, if the scene can be unloaded, <c>false</c> otherwise.</returns>
	/// <param name="sceneName">Scene name.</param>
	public bool UnloadSceneIfLoaded(string sceneName)
	{
		if (SceneManager.GetSceneByName(sceneName).isLoaded)
		{
			SceneManager.UnloadSceneAsync(sceneName); 
			return true; 
		}
		return false; 
	}

	/// <summary>
	/// Helper for SceneManager that loads a scene if it is not already loaded
	/// </summary>
	/// <returns><c>true</c>, if the scene was loaded in this call, <c>false</c> otherwise.</returns>
	/// <param name="sceneName">Scene name.</param>
	public bool LoadSceneIfUnloaded(string sceneName)
	{
		if (!SceneManager.GetSceneByName(sceneName).isLoaded)
		{
			SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); 
			return true; 
		}
		return false; 
	}
}
