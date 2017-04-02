using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLighting : MonoBehaviour 
{
	public GameObject player;

	public Light dirLight;

	public bool indoorLighting; 

	public float lightLerpSpeed; 

	[Space(5)]
	[Header("Ambient light settings")]
	public Color indoorAmbientColor; 
	public Color outdoorAmbientColor; 

	/*
	[Space(5)]
	[Header("Directional light settings")]
	public float indoorIntensity; 
	public float outdoorIntensity; 
	public Color indoorColor; 
	public Color outdoorColor; 
	*/ 


	// Use this for initialization
	void Start () 
	{
		StartCoroutine("CheckForCeiling"); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Temporary lighting state machine; replace with current color variable and lerping transitions
		if (indoorLighting)
		{
			//RenderSettings.ambientLight = indoorAmbientColor; 

			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, indoorAmbientColor, lightLerpSpeed);

			//dirLight.color = indoorColor; 
			//dirLight.intensity = indoorIntensity; 

		}
		else
		{
			//RenderSettings.ambientLight = outdoorAmbientColor;

			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, outdoorAmbientColor, lightLerpSpeed);

			//dirLight.color = outdoorColor;
			//dirLight.intensity = outdoorIntensity;
		}
	}

	public IEnumerator CheckForCeiling()
	{
		while (true)
		{
			if (GlobalManager.inst.GameplayIsActive())
			{
				indoorLighting = false; 

				// Send a raycast up
				Debug.DrawRay(player.transform.position, transform.up * 100); 
				RaycastHit hit;
				if (Physics.Raycast(player.transform.position, transform.up, out hit, 100))
				{
					// Temporary way to deal with collision types. Should probably use a collision layer for the raycast
					if (hit.collider.tag != "Enemy" && hit.collider.tag != "Bullet")
					{
						indoorLighting = true; 
					}
				}
			}

			yield return new WaitForSecondsRealtime(0.1f); 
		}
	}
}
