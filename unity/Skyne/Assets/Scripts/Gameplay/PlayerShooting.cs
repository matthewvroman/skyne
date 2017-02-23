﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : Singleton<PlayerShooting> 
{
	[Tooltip("(Drag in) The GameObject from which player bullets will originate")]
	public GameObject bulletSpawner; 

	// The current shoot mode is stored in GameState as pShootMode so that it is in the right place to be stored in PlayerPrefs
	public enum PlayerShootMode {Normal, Charge, Wide, Rapid}; 

	// Current shoot delay timer
	float shootDelay;

	public float normalShootDelay; //0.4
	public float wideShootDelay; //1.2
	public float rapidShootDelay; //0.15

	// Charge only 
	float curCharge; 
	[Tooltip("How much time does it take to get a full charge?")]
	public float fullCharge; 

	// Changing weapon variables
	// There needs to be a short delay between changing weapons
	// Also, don't allow shots if the player is holding the shoot key in between changing beam types
	public float changeDelay; 
	float changeTimer; 
	bool changeShootHeld; 



	// Update is called once per frame
	void Update () 
	{
		// Decrement change timer until it reaches 0
		if (changeTimer > 0)
		{
			changeTimer -= Time.deltaTime;
			if (changeTimer < 0)
				changeTimer = 0; 
		}

		// Confirm that the shoot key is no longer being held when changing weapon types
		if (changeShootHeld && !Input.GetKey(KeyCode.F))
		{
			changeShootHeld = false; 
		}

		// Check weapon changing
		CheckWeaponSelectInput(); 


		// Check shooting input if allowed
		if (changeTimer == 0 && !changeShootHeld)
		{
			CheckShootInput(); 
		}
			
		// Update the shooting delay timer (using unscaled time)
		if (shootDelay > 0)
		{
			shootDelay -= Time.unscaledDeltaTime; 
			if (shootDelay < 0)
				shootDelay = 0; 
		}
	}



	void CheckShootInput()
	{
		// TODO- replace 'F' key with the actual input

		// When shoot is pressed, check normal and wide shooting
		if (Input.GetKeyDown(KeyCode.F))
		{
			if (GameState.inst.pShootMode == PlayerShootMode.Normal)
			{
				HandleShootKey_Normal(); 
			}
			else if (GameState.inst.pShootMode == PlayerShootMode.Wide)
			{ 
				HandleShootKey_Wide(); 
			}
		}

		// When shoot is held down, check charge and rapid shooting
		if (Input.GetKey(KeyCode.F))
		{
			if (GameState.inst.pShootMode == PlayerShootMode.Charge)
			{
				HandleShootKey_Charge(); 
			}
			else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
			{
				HandleShootKey_Rapid(); 
			}
		}
		// When shoot is released, check if the charge is released and shot
		else if (Input.GetKeyUp(KeyCode.F))
		{
			if (GameState.inst.pShootMode == PlayerShootMode.Charge)
			{
				HandleShootKeyReleased_Charge(); 
			}
		}
		// If no keys are being held, update other values
		// Also, set changeShootHeld to false if it's true
		else
		{
			// Reduce the charge value
			curCharge -= Time.unscaledDeltaTime; 

			if (curCharge < 0)
				curCharge = 0;
		}
	}

	// Player normal shot
	void HandleShootKey_Normal()
	{
		if (shootDelay == 0)
		{
			ProjectileManager.inst.Shoot_P_Normal(bulletSpawner); 
			shootDelay = normalShootDelay; 
		}
	}

	// Player is holding the charge button
	void HandleShootKey_Charge()
	{
		if (curCharge < fullCharge)
		{
			curCharge += Time.unscaledDeltaTime; 

			if (curCharge > fullCharge)
				curCharge = fullCharge; 
		}
	}

	// Player releases the charge button, potentially shoots charge shot if charge is complete
	void HandleShootKeyReleased_Charge()
	{
		if (curCharge == fullCharge)
		{
			ProjectileManager.inst.Shoot_P_Charge(bulletSpawner); 
			curCharge = 0; 
		}
	}

	// Player wide shot
	void HandleShootKey_Wide()
	{
		if (shootDelay == 0)
		{
			// TODO- change shoot function
			ProjectileManager.inst.Shoot_P_Wide(bulletSpawner); 
			shootDelay = wideShootDelay; 
		}
	}

	// Player is holding to shoot rapid shot
	void HandleShootKey_Rapid()
	{
		if (shootDelay == 0)
		{
			// TODO- change shoot function
			ProjectileManager.inst.Shoot_P_Rapid(bulletSpawner); 
			shootDelay = rapidShootDelay; 
		}
	}

	// Input for using numbered keys to change which weapon is active
	// This should probably be moved to a main PlayerInput class later
	void CheckWeaponSelectInput()
	{
		bool changed = false; 

		// Normal
		if (Input.GetKeyDown(KeyCode.Alpha1) && GameState.inst.pShootMode != PlayerShootMode.Normal)
		{
			GameState.inst.pShootMode = PlayerShootMode.Normal; 
			changed = true; 
		}
		// Charge
		else if (Input.GetKeyDown(KeyCode.Alpha2) && GameState.inst.pShootMode != PlayerShootMode.Charge)
		{
			GameState.inst.pShootMode = PlayerShootMode.Charge; 
			changed = true; 
		}
		// Wide
		else if (Input.GetKeyDown(KeyCode.Alpha3) && GameState.inst.pShootMode != PlayerShootMode.Wide)
		{
			GameState.inst.pShootMode = PlayerShootMode.Wide; 
			changed = true; 
		}
		// Rapid
		else if (Input.GetKeyDown(KeyCode.Alpha4) && GameState.inst.pShootMode != PlayerShootMode.Rapid)
		{
			GameState.inst.pShootMode = PlayerShootMode.Rapid; 
			changed = true; 
		}

		if (changed)
		{
			changeTimer = changeDelay; 
			changeShootHeld = true; 
		}
	}
}
