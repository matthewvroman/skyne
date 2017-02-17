using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour 
{
	public float maxLength; 

	RectTransform rectTransform;

	// Use this for initialization
	void Start () 
	{
		rectTransform = GetComponent<RectTransform>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (GameState.inst != null)
		{
			//rectTransform.sizeDelta = new Vector2 (200, 10);
			rectTransform.localScale = new Vector3(GameState.inst.playerHealth / 100f, 1, 1); 
		}
	}
}
