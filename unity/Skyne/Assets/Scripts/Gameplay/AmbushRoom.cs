using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushRoom : MonoBehaviour
{

	public List <GameObject> oneKey;
	public List <GameObject> twoKeys;
	public List <GameObject> threeKeys;
	public List <GameObject> noKeys;

	public GameObject oneKeyHold;
	public GameObject twoKeyHold;
	public GameObject threeKeyHold;
	public GameObject noKeyHold;

	public DoorControl[] doorCon;

	GameState gameState;
	int keysHeld;

	// Use this for initialization
	void Start ()
	{
		oneKey = new List<GameObject> ();

		foreach (Transform enemy in noKeyHold.transform)
		{
			noKeys.Add (enemy.gameObject);
		}

		foreach (Transform enemy in oneKeyHold.transform)
		{
			oneKey.Add (enemy.gameObject);
		}

		foreach (Transform enemy in twoKeyHold.transform)
		{
			twoKeys.Add (enemy.gameObject);
		}

		foreach (Transform enemy in threeKeyHold.transform)
		{
			threeKeys.Add (enemy.gameObject);
		}

		gameState = GameObject.Find ("GameState").GetComponent<GameState> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Debug.Log (noKeys.Count);

		if (this.noKeys.Count != 0 && this.oneKey.Count != 0 && this.twoKeys.Count != 0 && this.threeKeys.Count != 0)
		{
			
			keysHeld = gameState.GetNumKeysFound ();
			foreach (GameObject enemy in noKeys)
			{
				if (enemy == null)
				{
					noKeys.Remove (enemy);
				}
			}

			if (noKeys.Count <= 0)
			{
				//doorCon.setDoorState (true);
			}


			foreach (GameObject enemy in oneKey)
			{
				if (enemy == null)
				{
					oneKey.Remove (enemy);
				}
			}

			if (oneKey.Count <= 0)
			{
				//doorCon.setDoorState (true);
			}


			foreach (GameObject enemy in twoKeys)
			{
				if (enemy == null)
				{
					twoKeys.Remove (enemy);
				}
			}
			if (twoKeys.Count <= 0)
			{
				//doorCon.setDoorState (true);
			}


			foreach (GameObject enemy in threeKeys)
			{
				if (enemy == null)
				{
					threeKeys.Remove (enemy);
				}
			}
			if (threeKeys.Count <= 0)
			{
				//doorCon.setDoorState (true);
			}

			switch (keysHeld)
			{
			case 0:
				foreach (GameObject enemy in noKeys)
				{
					enemy.SetActive (true);
				}
				foreach (GameObject enemy in oneKey)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in twoKeys)
				{
					enemy.SetActive (false);

				}
				foreach (GameObject enemy in threeKeys)
				{
					enemy.SetActive (false);
				}
				break;

			case 1:
				foreach (GameObject enemy in noKeys)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in oneKey)
				{
					enemy.SetActive (true);
				}
				foreach (GameObject enemy in twoKeys)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in threeKeys)
				{
					enemy.SetActive (false);
				}
				break;

			case 2:
				foreach (GameObject enemy in noKeys)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in oneKey)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in twoKeys)
				{
					enemy.SetActive (true);
				}
				foreach (GameObject enemy in threeKeys)
				{
					enemy.SetActive (false);
				}
				break;

			case 3:
				foreach (GameObject enemy in noKeys)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in oneKey)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in twoKeys)
				{
					enemy.SetActive (false);
				}
				foreach (GameObject enemy in threeKeys)
				{
					enemy.SetActive (true);
				}
				break;
			}
		}
		else
		{
			//doorCon.setDoorState (true);
			foreach (DoorControl door in doorCon)
			{
				door.setDoorState (true);
			}
		}
	}
}
