using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformManager : MonoBehaviour
{

	GameObject platform;

	Transform botPos;
	Transform topPos;

	bool setPos;

	public float speed;
	public int timer;

	float countdown;

	// Use this for initialization
	void Start ()
	{
		platform = this.transform.FindChild ("Platform").gameObject;
		botPos = gameObject.transform.Find ("BottomPosition");
		topPos = gameObject.transform.Find ("TopPosition");

		setPos = true;

		countdown = timer;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Debug.Log (countdown);

		if (setPos == true)
		{
			countdown = timer;
			platform.transform.position = Vector3.Lerp (platform.transform.position, botPos.position, speed * Time.smoothDeltaTime);
		}

		if (setPos == false)
		{
			if (countdown > 0)
			{
				countdown -= Time.deltaTime;
			}
			platform.transform.position = Vector3.Lerp (platform.transform.position, topPos.position, speed * Time.smoothDeltaTime);
		}

		if (countdown <= 0)
		{
			setPos = true;
			countdown = timer;
		}
	}

	/// <summary>
	/// True = bottom position,
	/// False = top position.
	/// </summary>
	/// <param name="b">If set to <c>true</c> b.</param>
	public void SetPos (bool b)
	{
		setPos = b;
	}

	/// <summary>
	/// Returns it's target location
	/// True = bottom position,
	/// False = top position.
	/// </summary>
	/// <param name="b">If set to <c>true</c> b.</param>
	public bool GetSetPos ()
	{
		return setPos;
	}
}
