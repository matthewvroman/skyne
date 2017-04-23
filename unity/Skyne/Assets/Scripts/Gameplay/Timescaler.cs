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

	// Calculated in update; between 0 and 1
	public float percentSlowed; 

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

	float totalTimeRange; 
	float curTimeRange; 

	void Update()
	{
		// Update the inspector display of the current timescale
		curTimescale = Time.timeScale; 

		// Change Time.fixedDeltaTime to make Physics smooth (removing this line makes choppy framerate)
		Time.fixedDeltaTime = 0.02F * Time.timeScale;

		// Calculate percent
		totalTimeRange = 1 - minTimescale; // 0.2, totaltTimeRange = 0.8
		curTimeRange = 1 - curTimescale; // 0.8, curTimeRange = 0.4
		percentSlowed = curTimeRange / totalTimeRange; 
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
			if (Time.timeScale > minTimescale && Time.timeScale != 0)
			{
				// Time.deltaTime changes once we change the timeScale
				// To get Time.deltaTime independent of how we've just changed the timescale, we have to divide by the timeScale to undo the change
				// Use Time.deltaTime / Time.timeScale for timing that's independent of the slow motion's effect on time
				float newTimeScale = Time.timeScale - timescaleChangeRate * (Time.deltaTime / Time.timeScale);

				if (newTimeScale < minTimescale)
					newTimeScale = minTimescale; 

				Time.timeScale = newTimeScale; 
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
		if (Time.timeScale != 0)
		{
			float deltaTimeDiff = (Time.deltaTime / Time.timeScale) - Time.deltaTime; 

			// Then, find the percent amount of deltaTimeDiff based on percentChange
			float deltaTimeChange = deltaTimeDiff * percentChange; 

			// Add this change to the final deltaTime result
			return Time.deltaTime + deltaTimeChange; 
		}
		return 0; 
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

		// Test example: slo-mo timescale is 0.1; diff is 1 - 0.1 = 0.9; percentChange = 1, so increase is 100% of 0.9, which is 0.9. So, newTimeScale = 0.1 + 0.9. which is 1
	}

	// Returns the deltaTimePerc value needed for rigidbody FixedUpdate() calculations
	// At normal time, this will need to return 1, but during slowdown, the value needs to be calculated along a linear proportion
	// At full slow motion, the returned value will equal the deltaTimePerc specified in the parameter
	public float CalculateDeltaTimePerc(float deltaTimePerc)
	{
		float timeRange = 1 - minTimescale; 
		float curTimeFill = timeRange - (Time.timeScale - minTimescale); 

		float deltaTimeRange = 1 - deltaTimePerc; 

		float newDeltaTime = deltaTimeRange * curTimeFill / timeRange; 

		return 1 - newDeltaTime; 
	}

	// Provides the correct multiplier for handling speed in rigidbody functions in FixedUpdate()
	// Provide the normal speed value and the deltaTimePerc. 
	// Example: rb.velocity = transform.forward * Timescaler.inst.GetFixedUpdateSpeed(speed, deltaTimePerc); 

	/// <summary>
	/// Gets the fixed update speed.
	/// </summary>
	/// <returns>The fixed update speed.</returns>
	/// <param name="speed">Speed.</param>
	/// <param name="deltaTimePerc">Delta time perc.</param>
	public float GetFixedUpdateSpeed(float speed, float deltaTimePerc)
	{
		// Dividing speed by Time.timeScale reverses the effect of the slowdown, effectively restoring the original speed while everything else is in slow motion.
		// Then, that original speed is multiplied by the deltaTimePerc to figure out how much of the original speed is kept
		// HOWEVER, deltaTimePerc alone will always affect the bullet speed. Thus, deltaTimePerc must be scaled according to where the current timescale falls between 1 and minTimescale
		return (speed / Time.timeScale) * Timescaler.inst.CalculateDeltaTimePerc(deltaTimePerc);
	}
}
