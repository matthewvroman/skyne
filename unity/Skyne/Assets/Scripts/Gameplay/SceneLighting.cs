using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLighting : MonoBehaviour 
{
	[Tooltip ("Drag in the player gameObject")]
	public GameObject player;

	public Light dirLight;

	[Tooltip ("(Read only) Whether the player is indoors and indoor lighting is being used")]
	public bool indoorLighting; 

	[Tooltip ("How quickly the ambient light color lerps between indoor/outdoor colors")]
	public float lerpSpeed; 

	[Tooltip ("The distance around the player that is raycast tested by shooting a raycast up. All raycasts around the player must have the same result before indoorLighting changes")]
	public float playerTestRadius; 

	[Space(5)]
	[Header("Ambient light settings")]
	public Color indoorAmbientColor; 
	public Color outdoorAmbientColor; 


	[Space(5)]
	[Header("Directional light settings")]
	public float indoorIntensity; 
	public float outdoorIntensity; 
	public Color indoorColor; 
	public Color outdoorColor; 

	float lerpTime; 
	 
	public GameObject[] playerModel;

	public LayerMask ceilingLayerMask; 

	// Use this for initialization
	void Start () 
	{
		StartCoroutine("CheckForCeiling");
//		playerModel = player.GetComponentInChildren<GameObject> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Temporary lighting state machine; replace with current color variable and lerping transitions
		if (indoorLighting)
		{
			//RenderSettings.ambientLight = indoorAmbientColor; 


			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, indoorAmbientColor, lerpSpeed);

//			dirLight.color = Color.Lerp(dirLight.color, indoorColor, lerpSpeed); 
//			dirLight.intensity = Mathf.Lerp(dirLight.intensity, indoorIntensity, lerpSpeed); 

			//dirLight.color = indoorColor; 
			//dirLight.intensity = indoorIntensity; 

		}
		else
		{
			//RenderSettings.ambientLight = outdoorAmbientColor;


			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, outdoorAmbientColor, lerpSpeed);

//			dirLight.color = Color.Lerp(dirLight.color, outdoorColor, lerpSpeed); 
//			dirLight.intensity = Mathf.Lerp(dirLight.intensity, outdoorIntensity, lerpSpeed); 

			//dirLight.color = outdoorColor;
			//dirLight.intensity = outdoorIntensity;
		}
	}

	public IEnumerator CheckForCeilingOldOld()
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
					if (hit.collider.tag == "Ceiling")
					{
						indoorLighting = true; 
					} else
					{
						indoorLighting = false;
					}
				}
			}

			yield return new WaitForSecondsRealtime(0.1f); 
		}
	}


	public IEnumerator CheckForCeilingOld()
	{
		RaycastHit hit;
		Vector3[] testPositions = new Vector3[5]; 
		bool isIndoor = false; 
		bool hitSomething; 
		bool validChange; 


		while (true)
		{
			if (GlobalManager.inst.GameplayIsActive())
			{
				// Test five positions relative to the player
				testPositions[0] = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z); 
				testPositions[1] = new Vector3 (player.transform.position.x + playerTestRadius, player.transform.position.y, player.transform.position.z); 
				testPositions[2] = new Vector3 (player.transform.position.x - playerTestRadius, player.transform.position.y, player.transform.position.z); 
				testPositions[3] = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z + playerTestRadius);
				testPositions[4] = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z - playerTestRadius); 

				validChange = true; 

				for (int i = 0; i < testPositions.Length && validChange; i++)
				{
					hitSomething = Physics.Raycast(testPositions[i], transform.up, out hit, 100); 

					// If the raycast hit something that's not a ceiling, don't test any more
					if (hitSomething && hit.collider.tag != "Ceiling")
					{
						validChange = false; 
					}
					// If the raycast hit a ceiling
					else if (hitSomething)
					{
						if (i == 0)
						{
							isIndoor = true; 
						}
						else if (!isIndoor)
						{
							validChange = false; 
						}
					}
					// If the raycast didn't hit a ceiling
					else
					{
						if (i == 0)
						{
							isIndoor = false; 
						}
						else if (isIndoor)
						{
							validChange = false; 
						}
					}
				}


				if (validChange)
				{
					indoorLighting = isIndoor; 
				}

			}

			yield return new WaitForSecondsRealtime(0.1f); 
		}
	}

	public IEnumerator CheckForCeiling()
	{
		RaycastHit hit;
		Ray ray = new Ray(); 
		ray.direction = transform.up; 

		while (true)
		{
			if (GlobalManager.inst.GameplayIsActive())
			{
				indoorLighting = false; 

				// Send a raycast up
				Debug.DrawRay(player.transform.position, transform.up * 100); 
		
				//if (Physics.Raycast(player.transform.position, transform.up, out hit, 100))
				ray.origin = player.transform.position; 

				if (Physics.Raycast(ray, out hit, 200, ceilingLayerMask))
				{
					// Only set indoor lighting if the raycast hits a collider on the "Ceiling" layer with the tag "Ceiling"
					if (hit.collider.tag == "Ceiling")
					{
						indoorLighting = true; 
					} 
				}
			}

			yield return new WaitForSecondsRealtime(0.1f); 
		}
	}

}
