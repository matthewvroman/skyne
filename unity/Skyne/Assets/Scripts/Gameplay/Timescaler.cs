using UnityEngine;
using System.Collections;

/*
 * This script takes player input and smoothly slows down time while LEFT SHIFT is held
 * The code does a linear interpolation between normal speed and slow mo to make the change less jarring; alter the transition speed with timescaleChangeRate
 * Slowing time alters Time.timeScale and consequently Time.deltaTime
 * Any time-based code in the game that SHOULDN'T be affected by the slow-motion should replace Time.deltaTime with (Time.deltaTime / Time.timeScale)
 * TODO set property canSlowdown depending on whether the player is in the air, as well as if slow motion is allowed at a given time
 * TODO connect key input to a separate input class
 */
public class Timescaler : Singleton<Timescaler>
{
	//[Tooltip("Set to true if slow motion allowed (such as when jumping); false otherwise")]
	//public bool canSlowdown;

	[Tooltip("How slow should time get (1 = normal time)")]
	public float minTimescale;

	[Tooltip("How quickly does time interpolate (linear) between the normal and slowed states")]
	public float timescaleChangeRate;

	[Tooltip("True if slow motion (or slow-mo fade-in) in effect; false if normal speed or fading to normal speed")]
	public bool timeSlowed;

	public float pausedTimescale; 
	[SerializeField] private float curTimescale; 

	void OnEnable()
	{
		GlobalManager.OnGamePausedUpdated += HandleGamePausedUpdated; 
	}

	void OnDisable()
	{
		GlobalManager.OnGamePausedUpdated -= HandleGamePausedUpdated;
	}

	void HandleGamePausedUpdated(bool newState)
	{
		if (newState == true)
		{
			pausedTimescale = Time.timeScale; 
			Time.timeScale = 0; 
		}
		else
		{
			Time.timeScale = pausedTimescale; 
			//Debug.Log("Revert timeScale to paused timescale"); 
		}
	}

	void Update()
	{
		// Update the inspector display of the current timescale
		curTimescale = Time.timeScale; 

		// Change Time.fixedDeltaTime to make Physics smooth (removing this line makes choppy framerate)
		Time.fixedDeltaTime = 0.02F * Time.timeScale;
	}


	public void UpdateTimescale ()
	{
		//Debug.Log("Timescale: " + Time.timeScale); 
			
		// Set the current timeScale and fixedDeltaTime based on timeSlowed
		// Time is slowing down or remaining in slow-mo
		if (GlobalManager.inst.gamePaused)
		{
			//
		}
		else if (timeSlowed)
		{
			if (Time.timeScale > minTimescale)
			{
				// Time.deltaTime changes once we change the timeScale
				// To get Time.deltaTime independent of how we've just changed the timescale, we have to divide by the timeScale to undo the change
				// Use Time.deltaTime / Time.timeScale for timing that's independent of the slow motion's effect on time
				Time.timeScale -= timescaleChangeRate * (Time.deltaTime / Time.timeScale);
				if (Time.timeScale < minTimescale)
					Time.timeScale = minTimescale; 
			}
		}
		// Time is speeding up or remaining in normal speed
		else
		{
			if (Time.timeScale < 1 && Time.timeScale != 0)
			{
				Time.timeScale += timescaleChangeRate * (Time.deltaTime / Time.timeScale); 
				if (Time.timeScale > 1)
					Time.timeScale = 1; 
			}
		}

		// Change Time.fixedDeltaTime to make Physics smooth (removing this line makes choppy framerate)
		//Time.fixedDeltaTime = 0.02F * Time.timeScale;
	}

	// Returns a deltaTime value that scales differently than the normal slowdown
	// Argument percentChange: what percent does the timeScale differ from the normal slow-mo speed
	// 		0 = normal slow motion speed
	//		1 = same speed in both slow motion and normal speed
	//		0.5 = 50% increase from normal slow motion speed (still same normal speed)
	// Example: someObject.xPos += 3 * CalculateDeltaTime(0.25)
	// Increases the x position 25% faster than other objects while in slow motion; moves at normal speed when not in slow motion
	// Example: someObject.xPos += 3 * CalculateDeltaTime(1)
	// The x speed will stay the same in both slow motion and normal time
	public float CalculateDeltaTime (float percentChange)
	{
		// First, calculate the difference between absolute deltaTime and current deltaTime
		float deltaTimeDiff = (Time.deltaTime / Time.timeScale) - Time.deltaTime; 

		// Then, find the percent amount of deltaTimeDiff based on percentChange
		float deltaTimeChange = deltaTimeDiff * percentChange; 

		// Add this change to the final deltaTime result
		return Time.deltaTime + deltaTimeChange; 
	}

	// Returns the timeScale calculation based on the percentChange
	// Like CalculateOtherDeltaTime, this returns 1 when Time.timeScale = 1 but changes as the timeScale slows down
	// Not currently used
	public float CalculateTimescale (float percentChange)
	{
		// Example: slo-mo timescale is 0.2; diff is 1 - 0.2 = 0.8; percentChange is 0.5, so increase is 50% of 0.8, which is 0.4. So, newTimeScale = 0.2 + 0.4, which is 0.6
		float timeScaleDiff = 1 - Time.timeScale; 
		float timeScaleIncrease = timeScaleDiff * percentChange; 
		return Time.timeScale + timeScaleIncrease;  
	}
}
