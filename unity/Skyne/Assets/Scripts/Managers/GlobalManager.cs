using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio; 

public class GlobalManager : Singleton<GlobalManager> 
{
	public enum GlobalState {Menu, LoadMainGameplayScene, SetupGameplay, Gameplay, GameOver, GameOverUnloadGameplayScenes, TitleUnloadGameplayScenes, OutroUnloadGameplayScenes, EditorOnlyLoadGameplay, TransitionWait, GameplayFadeOut, Outro};  

	[Tooltip("The state machine of the game. Set to Menu when starting from Menu; set to Gameplay when starting in the middle of the game.")]
	public GlobalState globalState; 

	[Tooltip("(Read only) True if the button UI is active and the mouse needs to be set to visible")]
	public bool buttonUIIsActive; 

	public bool initialLoadFinished; 
	[SerializeField] private bool m_gamePaused; 

	public float loadScreenFadeSpeed; 
	public float levelFadeInSpeed; 
	public float levelFadeOutSpeed; 

	public bool skipTitleIntro; 

	public static System.Action<bool> OnGamePausedUpdated; 

	[Tooltip("The default camera when the player character is not being used.")]
	public Camera globalCamera; 

	public AsyncOperation mainLevelLoadOp; 

	public AudioMixer mixer; 

	public bool gamePaused
	{
		get {
			return m_gamePaused; 
		}
	}

	public void SetGamePaused(bool newState)
	{
		// Only set the game pause state if the newState is the opposite of the current pause state
		if (m_gamePaused != newState)
		{
			m_gamePaused = newState; 

			// Send out a message indicating that the pause state of the game has changed
			if (OnGamePausedUpdated != null)
			{
				OnGamePausedUpdated(newState); 
			}
		}
	}

	/// <summary>
	/// Returns true if gameplay is active, meaning that the initial scene has loaded and the game isn't paused
	/// </summary>
	public bool GameplayIsActive ()
	{
		if (!gamePaused && initialLoadFinished)
		{
			return true; 
		}
		return false; 
	}


	// Use this for initialization
	void Start () 
	{

		// Used for directly loading the game from within gameplay while in the editor
		#if UNITY_EDITOR
		if (globalState == GlobalState.Gameplay || globalState == GlobalState.SetupGameplay || globalState == GlobalState.EditorOnlyLoadGameplay)
		{
			globalState = GlobalState.EditorOnlyLoadGameplay; 

			UnloadSceneIfLoaded("Title"); 
			UnloadSceneIfLoaded("GameOver"); 
			LoadSceneIfUnloaded("Loading"); 
			SceneLoading.inst.UnloadAllLevelScenes(SceneMapping.inst.sceneList);
		}
		#endif

		if (globalState == GlobalState.Menu)
		{
			LoadTitleAtStart();  
		}

		// Set default audio levels
		SetMusicVolume(0); 
		SetSFXVolume(0); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (globalState == GlobalState.LoadMainGameplayScene)
		{
			/*
			// Once the main (global) level scene is loaded, call it to setup the rest of the game and load the remaining level scenes
			if (SceneManager.GetSceneByName("MainLevel").isLoaded)
			{
				// Close scenes that might be open when the main level is being loaded
				UnloadSceneIfLoaded("Title"); 
				UnloadSceneIfLoaded("GameOver"); 

				globalState = GlobalState.SetupGameplay; 
				SceneLoading.inst.lockLevelSceneLoad = false; 
				MainGameplayManager.inst.SetupGameplayScreen(); 
			}
			*/ 
		}
		else if (globalState == GlobalState.SetupGameplay)
		{
			// Once everything has been loaded
			//if (!SceneLoading.inst.LevelScenesBeingLoaded() && SceneLoading.inst.startedLoadingLevels)
			if (!SceneLoading.inst.LevelScenesBeingLoaded() && ScreenTransition.inst.curState == ScreenTransition.TransitionState.transparentScreenRest)
			{
				//Debug.Log("Test finished loading"); 
				ScreenTransition.inst.SetFadeOut(loadScreenFadeSpeed); 
				globalState = GlobalState.TransitionWait; 
				StartCoroutine("GameplayStartFadeOut"); 
			}

		}
		else if (globalState == GlobalState.GameOverUnloadGameplayScenes)
		{
			if (SceneLoading.inst.LevelUnloadComplete())
			{
				ScreenTransition.inst.SetFadeIn(loadScreenFadeSpeed); 
				ChangeToGameOver();
				SetGamePaused(false); 
			}
		}
		else if (globalState == GlobalState.TitleUnloadGameplayScenes)
		{
			if (SceneLoading.inst.LevelUnloadComplete())
			{
				ChangeToTitle(); 
				SetGamePaused(false); 
			}
		}
		else if (globalState == GlobalState.OutroUnloadGameplayScenes)
		{
			if (SceneLoading.inst.LevelUnloadComplete())
			{
				ChangeToOutro(); 
				SetGamePaused(false); 
			}
		}
		else if (globalState == GlobalState.EditorOnlyLoadGameplay)
		{
			if (SceneLoading.inst.LevelUnloadComplete())
			{
				LoadGameplayScreen(); 
			}
		}

		// Update Main Camera
		if (globalState == GlobalState.Gameplay || globalState == GlobalState.GameplayFadeOut)
		{
			globalCamera.gameObject.SetActive(false); 
		}
		else
		{
			globalCamera.gameObject.SetActive(true); 
		}

		// Update cursor lock state
		if (buttonUIIsActive && !ScreenTransition.inst.transitionActive)
		{
			Cursor.visible = true; 
			Cursor.lockState = CursorLockMode.Confined;
		}
		else
		{
			Cursor.visible = false; 
			Cursor.lockState = CursorLockMode.Locked; 
		}
	}

	public float GetSFXVolume()
	{
		float curVolume; 
		mixer.GetFloat("SFXVolume", out curVolume); 
		return curVolume; 
	}

	public void SetSFXVolume(float newVolume)
	{
		mixer.SetFloat("SFXVolume", newVolume); 
	}

	public float GetMusicVolume()
	{
		float curVolume; 
		mixer.GetFloat("MusicVolume", out curVolume); 
		return curVolume; 
	}

	public void SetMusicVolume(float newVolume)
	{
		mixer.SetFloat("MusicVolume", newVolume); 
	}

	public void LoadTitleAtStart()
	{
		// If starting the game and loading the title for the first time
		if (globalState == GlobalState.Menu)
		{ 
			UnloadSceneIfLoaded("GameOver"); 
			UnloadSceneIfLoaded("MainLevel"); 

			// Unload all level scenes as well as the MainLevel
			if (SceneMapping.inst != null)
			{
				//LevelData.inst.UnloadAllLevelScenes(); 
				SceneLoading.inst.UnloadAllLevelScenes(SceneMapping.inst.sceneList); 
			}

			ChangeToTitle(); 
		}
	}

	public void LoadGameplayScreen()
	{
		globalState = GlobalState.LoadMainGameplayScene; 

		// Add the MainLevel screen
		LoadSceneIfUnloaded("MainLevel"); 
	}

	public void LoadGameOver()
	{
		globalState = GlobalState.GameplayFadeOut; 
		ScreenTransition.inst.SetFadeOut(levelFadeOutSpeed); 
		StartCoroutine("GameOverGameplayFadeOut"); 
	}

	public void LoadOutro()
	{
		globalState = GlobalState.GameplayFadeOut; 
		ScreenTransition.inst.SetFadeOut(levelFadeOutSpeed); 
		StartCoroutine("OutroGameplayFadeOut"); 
	}

	public void OutroToTitle()
	{
		//globalState//globalState
	}

	/// <summary>
	/// Called when unloading tasks started in LoadGameOver are finished
	/// </summary>
	void ChangeToGameOver()
	{
		globalState = GlobalState.GameOver; 
		LoadSceneIfUnloaded("GameOver"); 
		UnloadSceneIfLoaded("Loading"); 
		//Cursor.lockState = CursorLockMode.Confined;
		buttonUIIsActive = true; 
	}

	void ChangeToTitle()
	{
		globalState = GlobalState.Menu;
		ScreenTransition.inst.SetFadeIn(levelFadeInSpeed); 
		LoadSceneIfUnloaded("Title");
		UnloadSceneIfLoaded("Loading"); 
		//Cursor.lockState = CursorLockMode.Confined; 
		buttonUIIsActive = true; 
	}

	void ChangeToOutro()
	{
		globalState = GlobalState.Outro; 
		ScreenTransition.inst.SetFadeIn(1); 
		LoadSceneIfUnloaded("Outro"); 
		UnloadSceneIfLoaded("Loading"); 
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

	/// <summary>
	/// Called from TitleScreen to initiate the new/load game sequence
	/// </summary>
	public void TitleToLoadScreen()
	{
		SetGamePaused(true); 
		UnloadSceneIfLoaded("Title"); 
		LoadSceneIfUnloaded("Loading"); 
		StartCoroutine("StartGameUnloadTitleScreen"); 
	}

	/// <summary>
	/// Called from GameOverScreen to initiate the new/load game sequence
	/// </summary>
	public void GameOverToLoadScreen()
	{
		SetGamePaused(true); 
		UnloadSceneIfLoaded("GameOver"); 
		LoadSceneIfUnloaded("Loading"); 
		StartCoroutine("StartGameUnloadGameOver"); 
	}

	public void GameOverToTitleScreen()
	{
		UnloadSceneIfLoaded("GameOver");
		ChangeToTitle(); 
	}

	public void OutroToTitleScreen()
	{
		UnloadSceneIfLoaded("Outro");

		ChangeToTitle(); 
	}

	/// <summary>
	/// Called from MainLevel screen in UIManager to initiate the new/load game sequence
	/// </summary>
	public void GameplayToTitleScreen()
	{
		SetGamePaused(true); 
		globalState = GlobalState.GameplayFadeOut; 
		ScreenTransition.inst.SetFadeOut(levelFadeOutSpeed); 
		StartCoroutine("TitleGameplayFadeOut"); 
	}

	/// <summary>
	/// Called in GameOverToLoadScreen. Ensures that the game over screen is unloaded, the loaded screen is loaded, and the fade out is finished before fading into the loading screen
	/// </summary>
	/// <returns>The game unload game over.</returns>
	IEnumerator StartGameUnloadGameOver()
	{
		while (!SceneManager.GetSceneByName("Loading").isLoaded)
		{
			yield return null; 
		}

		// Load the main level as soon as the loading screen finishes fading in, but don't let the main level start yet
		mainLevelLoadOp = SceneManager.LoadSceneAsync("MainLevel", LoadSceneMode.Additive); 
		mainLevelLoadOp.allowSceneActivation = false; 

		while (SceneManager.GetSceneByName("GameOver").isLoaded || ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		StartCoroutine("StartGameFadeIntoLoadingScreen");
	}

	/// <summary>
	/// Called in TitleToLoadScreen. Ensures that the title screen is unloaded, the loaded screen is loaded, and the fade out is finished before fading in to the loading screen
	/// </summary>
	IEnumerator StartGameUnloadTitleScreen()
	{  
		while (!SceneManager.GetSceneByName("Loading").isLoaded)
		{
			yield return null; 
		}

		// Load the main level as soon as the loading screen finishes fading in, but don't let the main level start yet
		mainLevelLoadOp = SceneManager.LoadSceneAsync("MainLevel", LoadSceneMode.Additive); 
		mainLevelLoadOp.allowSceneActivation = false; 

		while (SceneManager.GetSceneByName("Title").isLoaded || ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}
			
		StartCoroutine("StartGameFadeIntoLoadingScreen");  
	}

	/// <summary>
	/// Fades into the loading screen after the previous fade out has finished. 
	/// Once the fade in is complete (ensuring the Skyne logo is fully visible before the current loading freezes), LoadGameplayScreen is called
	/// </summary>
	IEnumerator StartGameFadeIntoLoadingScreen()
	{
		// Set a fade in and wait until the fade in is complete
		ScreenTransition.inst.SetFadeIn(loadScreenFadeSpeed);
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.transparentScreenRest)
		{
			yield return null; 
		}

		while (mainLevelLoadOp.progress < 0.89f)
		{
			yield return null; 
		}

		mainLevelLoadOp.allowSceneActivation = true;

		while (!mainLevelLoadOp.isDone)
		{
			yield return null; 
		}

		// At this point, there will be a brief freezing hitch as the MainLevel starts
		// This currently is not going to be feasible to solve

		// Once the main (global) level scene is loaded, call it to setup the rest of the game and load the remaining level scenes
		globalState = GlobalState.SetupGameplay; 
		SceneLoading.inst.lockLevelSceneLoad = false; 
		MainGameplayManager.inst.SetupGameplayScreen(); 


			
		// Currently, Update() checks when all the scenes have been loaded. Once that happens, GameplayStartFadeOut() is called
		// This might need to be moved from Update() to here in the coroutine
	}


	// Coroutines for transitions when level is loaded
	// The first coroutine fades out on the loading screen
	IEnumerator GameplayStartFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}
		ScreenTransition.inst.SetFadeIn(levelFadeInSpeed); 
		UnloadSceneIfLoaded("Loading"); 
		StartCoroutine("GameplayStartUnloadLoadingScreen"); 
	}

	// The second coroutine gives the player control once the loading screen has finished unloading
	IEnumerator GameplayStartUnloadLoadingScreen()
	{
		while (SceneManager.GetSceneByName("Loading").isLoaded)
		{
			yield return null; 
		}

		SetGamePaused(false); 
		globalState = GlobalState.Gameplay; 
		MainGameplayManager.inst.OnGameplayStart(); 
	}


	// Coroutines for game over screen loading

	// Waits until the fade out screen transition is complete to unload all the level scenes
	IEnumerator GameOverGameplayFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		globalState = GlobalState.GameOverUnloadGameplayScenes; 

		// Unload all level scenes as well as the MainLevel
		SceneLoading.inst.UnloadAllLevelScenes(SceneMapping.inst.sceneList); 
		LoadSceneIfUnloaded("Loading"); 
		UnloadSceneIfLoaded("MainLevel"); 
	}

	// Coroutines for title screen loading from in-game

	IEnumerator TitleGameplayFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		globalState = GlobalState.TitleUnloadGameplayScenes; 

		// Unload all level scenes as well as the MainLevel
		SceneLoading.inst.UnloadAllLevelScenes(SceneMapping.inst.sceneList); 
		LoadSceneIfUnloaded("Loading"); 
		UnloadSceneIfLoaded("MainLevel");
	}

	// Coroutines for outro (ending sequence) loading

	IEnumerator OutroGameplayFadeOut()
	{
		while (ScreenTransition.inst.curState != ScreenTransition.TransitionState.blackScreenRest)
		{
			yield return null; 
		}

		globalState = GlobalState.OutroUnloadGameplayScenes; 

		// Unload all level scenes as well as the MainLevel
		SceneLoading.inst.UnloadAllLevelScenes(SceneMapping.inst.sceneList); 
		//LoadSceneIfUnloaded("Loading"); 
		UnloadSceneIfLoaded("MainLevel");
	}

}
