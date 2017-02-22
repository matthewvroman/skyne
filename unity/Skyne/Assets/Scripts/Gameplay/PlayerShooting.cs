using System.Collections;
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



	// Update is called once per frame
	void Update () 
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
		else
		{
			// Reduce the charge value
			curCharge -= Time.unscaledDeltaTime; 

			if (curCharge < 0)
				curCharge = 0; 
		}

		// Update the shooting delay timer (using unscaled time)
		if (shootDelay > 0)
		{
			shootDelay -= Time.unscaledDeltaTime; 
			if (shootDelay < 0)
				shootDelay = 0; 
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
}
