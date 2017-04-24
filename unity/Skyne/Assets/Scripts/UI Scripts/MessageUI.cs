using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MessageUI : Singleton<MessageUI> 
{
	private string m_message; 
	public string message
	{
		get {
			return m_message;
		}
		set {
			m_message = value; 
			messageText.text = m_message; 
		}
	}

	private Sprite m_sprite; 
	public Sprite sprite
	{
		get {
			return m_sprite; 
		}
		set {
			m_sprite = value;
			messageImg.sprite = m_sprite; 
		}
	}

	[Space(5)]
	[Header("Disable functionality")]

	public KeyCode disableKey; 

	public bool useDisableTimer; 
	public float disableTimer; 

	public enum TransitionState {fadingIn, fadingOut, opaque, transparent}; 

	[Space(5)]
	[Header("Text alpha transition")]
	public TransitionState curState; 
	public float fadeInSpeed; 
	public float fadeOutSpeed; 
	public bool transitionActive; 

	[Space(5)]
	[Header("Object references")]
	public Text messageText; 
	public Image messageImg; 
	public CanvasGroup canvasGroup; 

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
		else if (curState == TransitionState.opaque)
		{
			canvasGroup.alpha = 1; 
		}
		else if (curState == TransitionState.transparent)
		{
			canvasGroup.alpha = 0; 
		}
	}

	// Update is called once per frame
	void Update () 
	{
		// Canvas group alpha fading
		if (curState == TransitionState.fadingIn)
		{
			canvasGroup.alpha = canvasGroup.alpha + Time.deltaTime * fadeInSpeed; 
			if (canvasGroup.alpha >= 1)
			{
				canvasGroup.alpha = 1;
				curState = TransitionState.opaque; 
				transitionActive = false;  
			}
		}
		else if (curState == TransitionState.fadingOut)
		{
			canvasGroup.alpha = canvasGroup.alpha - Time.deltaTime * fadeOutSpeed; 

			if (canvasGroup.alpha <= 0)
			{
				canvasGroup.alpha = 0; 
				curState = TransitionState.transparent; 
				transitionActive = false; 
			}
		}

		if (curState == TransitionState.opaque)
		{
			if (useDisableTimer && disableTimer > 0)
			{
				disableTimer -= Time.deltaTime; 
				if (disableTimer < 0)
					disableTimer = 0; 
			}

			if (Input.GetKeyDown(disableKey))
			{
				SetFadeOut(); 
			}
			else if (useDisableTimer && disableTimer == 0)
			{
				SetFadeOut(); 
			}
		}
	}
		

	public void SetMessage(string newMessage, KeyCode newDisableKey, Sprite newImg)
	{
		if (curState != TransitionState.transparent)
		{
			curState = TransitionState.transparent; 
			canvasGroup.alpha = 0; 

		}


		message = newMessage; 
		disableKey = newDisableKey;

		if (newImg != null)
		{
			sprite = newImg; 
			messageImg.enabled = true; 
		}
		else
		{
			messageImg.enabled = false; 
		}

		SetFadeIn(); 

	}
		

	void SetFadeIn()
	{
		curState = TransitionState.fadingIn; 
		canvasGroup.alpha = 0; 

		transitionActive = true; 
	}

	void SetFadeOut()
	{
		curState = TransitionState.fadingOut; 
		canvasGroup.alpha = 1; 
		transitionActive = true; 
	}
}
