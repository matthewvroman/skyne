using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MessageTrigger : MonoBehaviour 
{
	[Tooltip ("Set to true if this GameObject should use OnTriggerEnter() to trigger a message")]
	public bool useCollider; 

	public string message; 
	public KeyCode disableKey; 
	public Sprite sprite; 

	[Tooltip ("Set to true if the message should fade out after enough time passes. This can be used in conjunction with the disableKey.")]
	public bool useDisableTimer; 
	[Tooltip ("If useDisableTimer==true, this is how much time to wait before calling the fade out.")]
	public float disableTime; 

	[Tooltip ("Set to true if this collider trigger can be activated repeatedly. False if it only works once (this will not be preserved upon reload)")]
	public bool allowReactivation; 
	bool hasBeenActivated; 


	void OnTriggerEnter(Collider col)
	{
		if (!useCollider)
		{
			return; 
		}

		if (!hasBeenActivated || allowReactivation)
		{
			TriggerMessage();  
			hasBeenActivated = true; 
		}
	}

	public void TriggerMessage()
	{
		MessageUI.inst.SetMessage(message, disableKey, sprite);

		if (useDisableTimer)
		{
			MessageUI.inst.useDisableTimer = true; 
			MessageUI.inst.disableTimer = disableTime; 
		}
		else
		{
			MessageUI.inst.useDisableTimer = false; 
		}
	}
}
