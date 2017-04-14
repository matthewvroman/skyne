using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// This class controls all child manager classes
/// Its main purpose is to control the order of execution for start and update tasks in these managers
/// The child managers should NOT have Start() or Update() functions (exception for Update() if [ExecuteInEditMode] tasks are needed)
/// Instead, create functions in the managers and call them when appropriate in Start() and Update() here in MainManager
/// </summary>
public class MainGameplayManager : Singleton<MainGameplayManager> 
{
	[SerializeField] private GameObject player; 
	[SerializeField] private Camera mainCam; 

	// Must occur after Awake() to ensure that all manager Awake()s have been called to create the inst variable
	void Start()
	{
		// Any other Awake() commands go here
		//LoadGameplayScreen(); 
	}



	/// <summary>
	/// Call when the game needs to load gameplay
	/// This is essentially a Start method for gameplay 
	/// </summary>
	public void SetupGameplayScreen()
	{
		if (SceneMapping.inst == null)
			Debug.Log("null SceneMapping"); 

		// Generate the scene mapping
		SceneMapping.inst.GenerateSceneMapping(LevelData.inst.numLevels, LevelData.inst.numColumns, LevelData.inst.numRows); 

		// Check if a saved game exists
		// In GameStateManager, load the corresponding data from PlayerPrefs
		GameState.inst.LoadGame(); 

		LevelData.inst.UpdatePlayerGridPos ();

		// Set the player's active state to false until the game is done loading
		player.SetActive(false); 
	}
		
	// Once the global manager detects that everything has been loaded, this is called
	public void OnGameplayStart()
	{
		Debug.Log("OnGameplayStart()"); 
		GlobalManager.inst.initialLoadFinished = true; 

		player.GetComponent<PlayerManager>().setHealth(PlayerPrefsManager.inst.GetStoredPlayerHealth()); 
		player.SetActive(true); 

		mainCam.gameObject.SetActive(true);

		PlayerShooting.inst.ChangeWeaponTypeModels(); 
		PlayerShooting.inst.ChangeWeaponCrosshair(); 

		// Set the starting sensitivity
		MainCameraControl.inst.SetMouseSensitivity(PlayerPrefsManager.inst.GetSavedMouseSensitivity(GlobalManager.inst.defaultSensitivity)); 

		// Fade in here
	}

	
	// Manages updating for all managers. Managers should not have their own Update()
	void Update () 
	{
		// Manage the order that update events fire in manager scripts
		if (GlobalManager.inst.globalState == GlobalManager.GlobalState.Gameplay)
		{
			if (SceneManager.GetSceneByName("Loading").isLoaded)
				SceneManager.UnloadSceneAsync("Loading");

			// Update the player input
			PlayerInput.inst.UpdatePlayerInput(); 

			// Update the timescale
			Timescaler.inst.UpdateTimescale(); 

			// Update the player's position in the world grid
			LevelData.inst.UpdatePlayerGridPos(); 
		}
	}

	public void TriggerGameOver ()
	{
		if (GlobalManager.inst.globalState == GlobalManager.GlobalState.Gameplay)
		{
			GlobalManager.inst.LoadGameOver(); 
		}
	}
}
