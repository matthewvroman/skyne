﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

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

	// First playable UI
	public GameObject chargeText;
	public GameObject wideText;
	public GameObject rapidText; 
	public Text normalNum;
	public Text chargeNum; 
	public Text wideNum; 
	public Text rapidNum; 

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

		// Temporary- Update first playable UI display
		chargeText.SetActive(GameState.inst.upgradesFound[3]);
		wideText.SetActive(GameState.inst.upgradesFound[4]);
		rapidText.SetActive(GameState.inst.upgradesFound[5]);

		if (GameState.inst.pShootMode == PlayerShootMode.Normal)
		{
			normalNum.color = new Color32 (250, 50, 50, 255); 
			chargeNum.color = new Color32 (50, 50, 50, 255); 
			wideNum.color = new Color32 (50, 50, 50, 255); 
			rapidNum.color = new Color32 (50, 50, 50, 255); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Charge)
		{
			normalNum.color = new Color32 (50, 50, 50, 255); 
			chargeNum.color = new Color32 (250, 50, 50, 255); 
			wideNum.color = new Color32 (50, 50, 50, 255); 
			rapidNum.color = new Color32 (50, 50, 50, 255); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Wide)
		{
			normalNum.color = new Color32 (50, 50, 50, 255); 
			chargeNum.color = new Color32 (50, 50, 50, 255); 
			wideNum.color = new Color32 (250, 50, 50, 255); 
			rapidNum.color = new Color32 (50, 50, 50, 255); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
		{
			normalNum.color = new Color32 (50, 50, 50, 255); 
			chargeNum.color = new Color32 (50, 50, 50, 255); 
			wideNum.color = new Color32 (50, 50, 50, 255); 
			rapidNum.color = new Color32 (250, 50, 50, 255); 
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
		// 1 = Normal
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			ChangeWeaponType(PlayerShootMode.Normal); 
		}
		// 2 = Charge
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			ChangeWeaponType(PlayerShootMode.Charge); 
		}
		// 3 = Wide
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			ChangeWeaponType(PlayerShootMode.Wide); 
		}
		// 4 = Rapid
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			ChangeWeaponType(PlayerShootMode.Rapid); 
		}
			
		// Safeguard- check for a weapon type active that hasn't been collected
		bool invalidCharge = GameState.inst.pShootMode == PlayerShootMode.Charge && !GameState.inst.upgradesFound[3]; 
		bool invalidWide = GameState.inst.pShootMode == PlayerShootMode.Wide && !GameState.inst.upgradesFound[4]; 
		bool invalidRapid = GameState.inst.pShootMode == PlayerShootMode.Rapid && !GameState.inst.upgradesFound[5]; 

		// Set the shoot mode back to normal if the current beam hasn't been collected
		if (invalidCharge || invalidWide || invalidRapid)
		{
			ChangeWeaponType(PlayerShootMode.Normal); 
		}
	}

	public void ChangeWeaponType(PlayerShootMode newType)
	{
		bool changed = false; 

		// Normal
		if (newType == PlayerShootMode.Normal && GameState.inst.pShootMode != PlayerShootMode.Normal)
		{
			GameState.inst.pShootMode = PlayerShootMode.Normal; 
			changed = true; 
		}
		// Charge
		else if (newType == PlayerShootMode.Charge && GameState.inst.pShootMode != PlayerShootMode.Charge && GameState.inst.upgradesFound[3])
		{
			GameState.inst.pShootMode = PlayerShootMode.Charge; 
			changed = true; 
		}
		// Wide
		else if (newType == PlayerShootMode.Wide && GameState.inst.pShootMode != PlayerShootMode.Wide && GameState.inst.upgradesFound[4])
		{
			GameState.inst.pShootMode = PlayerShootMode.Wide; 
			changed = true; 
		}
		// Rapid
		else if (newType == PlayerShootMode.Rapid && GameState.inst.pShootMode != PlayerShootMode.Rapid && GameState.inst.upgradesFound[5])
		{
			GameState.inst.pShootMode = PlayerShootMode.Rapid; 
			changed = true; 
		}

		// Indicate that the weapon type has changed, set a delay between when you can shoot (for changing animation), and make sure the shoot button can't be held between shot types
		if (changed)
		{
			changeTimer = changeDelay; 
			changeShootHeld = true; 
		}
	}
}
