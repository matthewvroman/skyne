using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : Singleton<PlayerShooting>
{
	[Tooltip("(Drag in) The GameObject from which player bullets will originate")]
	public GameObject bulletSpawner;

	// The current shoot mode is stored in GameState as pShootMode so that it is in the right place to be stored in PlayerPrefs
	public enum PlayerShootMode
	{
		Normal,
		Charge,
		Wide,
		Rapid
	};

	// Current shoot delay timer
	float shootDelay;

	AudioSource gunAudio;

	public AudioClip normalShotSound;
	public AudioClip wideShotSound;
	public AudioClip rapidShotSound;
	public AudioClip chargingSound;
	public AudioClip chargingHold;
	public AudioClip chargeShotSound;

	// Gun particles
	public ParticleSystem gunshotParticles; 
	public ParticleSystem chargingParticles;
	public float maxChargingSize; 
	public ParticleSystem chargedParticles; 

	[Space(5)]
	[Header("Player: Normal Bullet")]
	public GameObject pNormalBulletPrefab; 
	public float normalShootDelay;

	[Space(5)]
	[Header("Player: Charge Bullet")]
	public GameObject pChargeBulletPrefab; 
	float curCharge;
	[Tooltip("How much time does it take to get a full charge?")]
	public float fullCharge;

	[Space(5)]
	[Header("Player: Wide Bullet")]
	public GameObject pWideBulletPrefab; 
	public float pWideHorizSpread; 
	public float wideShootDelay;

	[Space(5)]
	[Header("Player: Rapid Bullet")]
	public GameObject pRapidBulletPrefab;  
	public float rapidShootDelay;

	[Space(5)]
	[Header("Other")]

	// Changing weapon variables
	// There needs to be a short delay between changing weapons
	// Also, don't allow shots if the player is holding the shoot key in between changing beam types
	public float changeDelay;
	float changeTimer;
	bool changeShootHeld;

	// Alpha UI
	public Image weaponIcon;
	public Sprite[] iconSprites;

	// Crosshairs
	[SerializeField] private GameObject normalCrosshair; 
	[SerializeField] private GameObject chargeCrosshair; 
	[SerializeField] private GameObject wideCrosshair; 
	[SerializeField] private GameObject rapidCrosshair; 

	// Player weapon models
	public GameObject normalGunModel; 
	public GameObject chargeGunModel; 
	public GameObject wideGunModel;
	public GameObject rapidGunModel; 

	PlayerManager playerManager; 

	void Start() 
	{
		gunAudio = GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update ()
	{
		// Test
		int x = Screen.width / 2;
		int y = Screen.height / 2;

		Ray ray = Camera.main.ScreenPointToRay(new Vector3 (x, y));
		Debug.DrawRay(ray.origin, ray.direction * 100, new Color (1f, 0.922f, 0.016f, 1f));

		// Decrement change timer until it reaches 0
		if (changeTimer > 0)
		{
			changeTimer -= Time.deltaTime;
			if (changeTimer < 0)
				changeTimer = 0; 
		}

		// Confirm that the shoot key is no longer being held when changing weapon types
		if (changeShootHeld && !Input.GetKey(KeyCode.Mouse0))
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
			

		if (GameState.inst.pShootMode == PlayerShootMode.Normal)
		{
			weaponIcon.sprite = iconSprites[0]; 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Charge)
		{
			weaponIcon.sprite = iconSprites[1]; 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Wide)
		{
			weaponIcon.sprite = iconSprites[2]; 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
		{
			weaponIcon.sprite = iconSprites[3]; 
		}

		// Charging particle effects
		if (GameState.inst.pShootMode == PlayerShootMode.Charge)
		{
			if (curCharge < fullCharge)
			{
				chargingParticles.enableEmission = true; 
				chargedParticles.enableEmission = false;

				// curCharge / fullCharge     curChargingSize / maxChargingSize
				chargingParticles.startSize = (maxChargingSize * curCharge) / fullCharge;
			}
			else
			{
				chargingParticles.enableEmission = false; 
				chargedParticles.enableEmission = true;
			}
		}
		else
		{
			chargingParticles.enableEmission = false; 
			chargedParticles.enableEmission = false; 
		}
			
		if (curCharge == fullCharge)
		{
			gunAudio.clip = chargingHold;
			if (!gunAudio.isPlaying)
			{
				gunAudio.Play ();
			}
		}
	}


	void CheckShootInput ()
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			if (playerManager == null)
			{
				playerManager = GameObject.FindObjectOfType<PlayerManager>(); 
			}

			// When shoot is pressed, check normal and wide shooting
			if (Input.GetKeyDown(KeyCode.Mouse0) && playerManager.GetIsAlive())
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
			if (Input.GetKey(KeyCode.Mouse0))
			{
				if (GameState.inst.pShootMode == PlayerShootMode.Charge)
				{
					HandleShootKey_Charge();
					gunAudio.clip = chargingSound;
					gunAudio.loop = true;
					gunAudio.Play ();
				}
				else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
				{
					HandleShootKey_Rapid(); 
				}
			}
		// When shoot is released, check if the charge is released and shot
		else if (Input.GetKeyUp(KeyCode.Mouse0))
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
				gunAudio.clip = null;

				if (curCharge < 0)
					curCharge = 0;
			}
		}
	}

	// Player normal shot
	void HandleShootKey_Normal ()
	{
		if (shootDelay == 0)
		{
			ProjectileManager.inst.Shoot_P_Normal(bulletSpawner); 
			gunAudio.clip = null;
			gunAudio.PlayOneShot(normalShotSound);
			shootDelay = normalShootDelay; 
			gunshotParticles.Play(); 
		}
	}

	// Player is holding the charge button
	void HandleShootKey_Charge ()
	{
		if (curCharge < fullCharge)
		{
			curCharge += Time.unscaledDeltaTime; 

			if (curCharge > fullCharge)
			{
				curCharge = fullCharge; 
			}
		}
	}

	// Player releases the charge button, potentially shoots charge shot if charge is complete
	void HandleShootKeyReleased_Charge ()
	{
		if (curCharge == fullCharge)
		{
			ProjectileManager.inst.Shoot_P_Charge(bulletSpawner); 
			gunAudio.clip = null;
			gunAudio.PlayOneShot (chargeShotSound);
			gunshotParticles.Play(); 
		}

		curCharge = 0; 
	}

	// Player wide shot
	void HandleShootKey_Wide ()
	{
		if (shootDelay == 0)
		{
			// TODO- change shoot function
			ProjectileManager.inst.Shoot_P_Wide(bulletSpawner); 
			gunAudio.clip = null;
			gunAudio.PlayOneShot(wideShotSound);
			shootDelay = wideShootDelay; 
			gunshotParticles.Play(); 
		}
	}

	// Player is holding to shoot rapid shot
	void HandleShootKey_Rapid ()
	{
		if (shootDelay == 0)
		{
			// TODO- change shoot function
			ProjectileManager.inst.Shoot_P_Rapid(bulletSpawner); 
			gunAudio.clip = null;
			gunAudio.PlayOneShot(rapidShotSound);
			shootDelay = rapidShootDelay; 
			gunshotParticles.Play(); 
		}
	}

	// Input for using numbered keys to change which weapon is active
	// This should probably be moved to a main PlayerInput class later
	void CheckWeaponSelectInput ()
	{
		if (GlobalManager.inst.GameplayIsActive())
		{
			// 'E' = Cycle through weapon type
			if (Input.GetKeyDown(KeyCode.E))
			{
				ChangeWeaponType(GetNextWeaponInCycle(GameState.inst.pShootMode)); 
			}
			// 1 = Normal
			else if (Input.GetKeyDown(KeyCode.Alpha1))
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
			if (GameState.inst != null)
			{
				bool invalidCharge = GameState.inst.pShootMode == PlayerShootMode.Charge && !GameState.inst.upgradesFound[3]; 
				bool invalidWide = GameState.inst.pShootMode == PlayerShootMode.Wide && !GameState.inst.upgradesFound[4]; 
				bool invalidRapid = GameState.inst.pShootMode == PlayerShootMode.Rapid && !GameState.inst.upgradesFound[5]; 

				// Set the shoot mode back to normal if the current beam hasn't been collected
				if (invalidCharge || invalidWide || invalidRapid)
				{
					ChangeWeaponType(PlayerShootMode.Normal); 
				}
			}
		}
	}

	/// <summary>
	/// Returns the next available shootingMode
	/// </summary>
	PlayerShootMode GetNextWeaponInCycle(PlayerShootMode curShootMode)
	{
		bool invalidChoice = true; 

		while (invalidChoice)
		{
			if (curShootMode == PlayerShootMode.Normal)
			{ 
				curShootMode = PlayerShootMode.Charge; 
			}
			else if (curShootMode == PlayerShootMode.Charge)
			{
				curShootMode = PlayerShootMode.Wide; 
			}
			else if (curShootMode == PlayerShootMode.Wide)
			{
				curShootMode = PlayerShootMode.Rapid; 
			}
			else if (curShootMode == PlayerShootMode.Rapid)
			{ 
				curShootMode = PlayerShootMode.Normal; 
			}

			// Test if the next shoot mode is a valid choice
			if (curShootMode == PlayerShootMode.Normal)
			{
				invalidChoice = false; 
			}
			else if (curShootMode == PlayerShootMode.Charge && GameState.inst.upgradesFound[3])
			{
				invalidChoice = false; 
			}
			else if (curShootMode == PlayerShootMode.Wide && GameState.inst.upgradesFound[4])
			{
				invalidChoice = false; 
			}
			else if (curShootMode == PlayerShootMode.Rapid && GameState.inst.upgradesFound[5])
			{
				invalidChoice = false; 
			}
		}
		return curShootMode; 
	}

	public void ChangeWeaponType (PlayerShootMode newType)
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

			ChangeWeaponTypeModels(); 
			ChangeWeaponCrosshair(); 
		}
	}

	public void ChangeWeaponTypeModels()
	{
		normalGunModel.SetActive(false); 
		chargeGunModel.SetActive(false); 
		wideGunModel.SetActive(false); 
		rapidGunModel.SetActive(false); 

		if (GameState.inst.pShootMode == PlayerShootMode.Normal)
		{
			normalGunModel.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Charge)
		{
			chargeGunModel.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Wide)
		{
			wideGunModel.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
		{
			rapidGunModel.SetActive(true); 
		}
	}

	public void ChangeWeaponCrosshair()
	{
		normalCrosshair.SetActive(false); 
		chargeCrosshair.SetActive(false); 
		wideCrosshair.SetActive(false); 
		rapidCrosshair.SetActive(false); 

		if (GameState.inst.pShootMode == PlayerShootMode.Normal)
		{
			normalCrosshair.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Charge)
		{
			chargeCrosshair.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Wide)
		{
			wideCrosshair.SetActive(true); 
		}
		else if (GameState.inst.pShootMode == PlayerShootMode.Rapid)
		{
			rapidCrosshair.SetActive(true); 
		}
	}
}
