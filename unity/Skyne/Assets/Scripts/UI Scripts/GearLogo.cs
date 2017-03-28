using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearLogo : MonoBehaviour 
{
	public float spinSpeed; 
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(new Vector3 (0, 0, spinSpeed * Time.unscaledDeltaTime)); 
	}
}
