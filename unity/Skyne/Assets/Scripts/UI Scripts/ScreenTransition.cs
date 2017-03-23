using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class ScreenTransition : Singleton<ScreenTransition> 
{
	public enum TransitionState {fadingIn, fadingOut, blackScreenRest, transparentScreenRest}; 
	public TransitionState curState; 
	public float fadeInSpeed; 
	public float fadeOutSpeed; 

	public bool transitionActive; 

	public Image fadeImage; 

	public bool useFadeOutAudio; 

	// Events
	public static System.Action OnFadeInComplete; 
	public static System.Action OnFadeOutComplete; 

	// Use this for initialization
	void Start () 
	{
		if (curState == TransitionState.fadingIn)
		{
			SetFadeIn(); 
		}
		else if (curState == TransitionState.fadingOut)
		{
			SetFadeOut(); 
		}
		else if (curState == TransitionState.blackScreenRest)
		{
			fadeImage.color = new Color (0, 0, 0, 1);
		}
		else if (curState == TransitionState.transparentScreenRest)
		{
			fadeImage.color = new Color (0, 0, 0, 1);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (curState == TransitionState.fadingIn)
		{
			fadeImage.color = new Color (0, 0, 0, fadeImage.color.a - Time.unscaledDeltaTime * fadeInSpeed);
			if (fadeImage.color.a <= 0)
			{
				fadeImage.color = new Color (0, 0, 0, 0);
				curState = TransitionState.transparentScreenRest; 
				OnFadeInCompletion(); 
			}
		}
		else if (curState == TransitionState.fadingOut)
		{
			fadeImage.color = new Color (0, 0, 0, fadeImage.color.a + Time.unscaledDeltaTime * fadeOutSpeed);
			if (fadeImage.color.a >= 1)
			{
				fadeImage.color = new Color (0, 0, 0, 1);
				curState = TransitionState.blackScreenRest; 
				OnFadeOutCompletion(); 
			}

			if (useFadeOutAudio)
			{
				// Fade out master audio here
			}
		}
	}

	void m_SetFadeIn()
	{
		curState = TransitionState.fadingIn; 
		fadeImage.color = new Color (0, 0, 0, 1); 
		transitionActive = true; 
	}

	void m_SetFadeOut()
	{
		curState = TransitionState.fadingOut; 
		fadeImage.color = new Color (0, 0, 0, 0);
		transitionActive = true; 

		if (useFadeOutAudio)
		{
			// Fade out master audio here
		}
	}

	public void SetFadeIn()
	{
		if (!transitionActive)
		{
			m_SetFadeIn(); 
		}
	}

	public void SetFadeIn(float speed)
	{
		fadeInSpeed = speed; 
		if (!transitionActive)
		{
			m_SetFadeIn(); 
		}
	}

	public void SetFadeOut()
	{
		if (!transitionActive)
		{
			m_SetFadeOut();
		}
	}

	public void SetFadeOut(float speed)
	{
		fadeOutSpeed = speed; 
		if (!transitionActive)
		{
			m_SetFadeOut();
		}
	}

	void OnFadeInCompletion()
	{
		transitionActive = false; 

		// Send out an event
		if (OnFadeInComplete != null)
		{
			OnFadeInComplete(); 
		}
	}

	void OnFadeOutCompletion()
	{
		transitionActive = false; 

		// Send out an event
		if (OnFadeOutComplete != null)
		{
			OnFadeOutComplete(); 
		}
	}

}
