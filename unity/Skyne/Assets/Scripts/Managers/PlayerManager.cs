using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
	[System.Serializable]
	public class MoveSettings
	{
		[Tooltip ("Player's forward velocity")]
		public float forwardVel = 12;
		[Tooltip ("Player's side velocity")]
		public float strafeVel = 12;
		[Tooltip ("Player's jump velocity")]
		public float jumpVel;

		public float dashVel = 100;
		public float dashCooldown = 2;

		[Tooltip ("How close the player has to to the ground for them to stop falling")]
		public float distToGrounded = 0.5f;
		[Tooltip ("Determins which layers the player can jump off of.")]
		public LayerMask ground;
		[Tooltip ("Vector3 of the over-the-shoulder camera position")]
		public Vector3 focusOffset = new Vector3 (1.5f, 0f, -1f);
	}

	[System.Serializable]
	public class PlayerSettings
	{
		[Tooltip ("The UI object representing the image for the health")]
		public Transform healthbarFill;
		[Tooltip ("The UI text representing the text for health")]
		public Transform healthPercentage;

		[Tooltip ("The UI object representing the image for stamina")]
		public GameObject staminaBarFill;
		public GameObject staminaBarBackground;
		// The stamina UI bar.

		public float staminaDrain;
		public float staminaRecover;
	}

	[System.Serializable]
	public class PhysSettings
	{
		[Tooltip ("Player's downward acceleration when falling after a jump")]
		public float normDownAccel = 0.75f;

		public float downWallAccel = 0.35f;
	}

	[System.Serializable]
	public class InputSettings
	{
		[Tooltip ("(Read Only) Creates a small delay between when the player presses the button and when the character responds (Read Only)")]
		public float inputDelay = 0.1f;

		[Tooltip ("(Read Only) The string reference for vertical movement")]
		public string FORWARD_AXIS = "Vertical";
		[Tooltip ("(Read Only) The string reference for horizontal movement")]
		public string STRAFE_AXIS = "Horizontal";
		[Tooltip ("(Read Only) The string reference for jumping")]
		public string JUMP_AXIS = "Jump";
	}

	public MoveSettings moveSetting = new MoveSettings ();
	public PhysSettings physSetting = new PhysSettings ();
	public InputSettings inputSetting = new InputSettings ();
	public PlayerSettings playerSetting = new PlayerSettings ();

	public Camera playerCamera;

	[Tooltip ("Reference to the Camera controller script")]
	public MainCameraControl camCon;

	//MeshRenderer playerMesh;
	//Color playerColor;

	float curDownAccel;

	float lastZVel = 0;
	float lastXVel = 0;

	bool isSlowed = false;

	bool isOriented = true;

	bool isFalling = false;

	//bool jump = false;
	bool canDoubleJump = false;

	bool isDashing = false;

	bool isHuggingWall;
	RaycastHit wallHit;

	bool backToWall = false;
	bool faceToWall = false;
	bool rSideToWall = false;
	bool lSideToWall = false;

	float dashCounter;
	bool startCooldown;

	bool isWallJumping = false;

	bool isDoubleJumping = false;

	bool isAlive = true;

	public Enemy[] enemies;
	bool isSeen;

	//bool damageFlash = false;

	Quaternion lastRot;

	Vector3 velocity = Vector3.zero;
	//Quaternion targetRotation;
	Rigidbody rBody;
	float forwardInput;
	float strafeInput;
	float jumpInput;
	float doubleJumpInput;

	public static float maxHealth = 100;
	// The highest the players health can go.
	static float currentHealth;
	// The players current health.
	static float healthSmoothing;
	// Used to smooth health.
	static float targetHealth;
	// Used to smooth player health up or down.
	bool isDead = false;
	// Prevents the code from executing the respawn sequence multiple times.

	[Range (0, 100)]
	float currentStamina;
	// The players current stamina.
	float maxStamina = 100;
	// The highest the players stamina can go. 1 represents 100%.
	float cooldownTimer = 0;
	// Timer that keeps track of cooldown delay.
	public float cooldownTime = 1;
	// Time until the stamina bar regenerates.

	public Animator anim;

	bool canWallJump;

	bool cameFromGround;

	AudioSource playerAudio;
	public AudioClip itemEquip;
	public AudioClip slowMoSound;

	/*public AudioClip footstep1;
	public AudioClip footstep2;
	public AudioClip footstep3;
	public AudioClip doubleJumpSound;
	public AudioClip airDashSound;
	public AudioClip jumpSound;
	public AudioClip WallJumpSound;
	public AudioClip ameliaGrunt1;
	public AudioClip ameliaGrunt2;
	public AudioClip ameliaGrunt3; */

	// Particle effects
	[Space (5)]
	[Header ("Particle Effects")]
	public ParticleSystem doubleJumpParticles;
	public ParticleSystem dashRingParticles;
	public ParticleSystem dashBootParticles;
	public ParticleSystem wallSlideParticles; 
	public ParticleSystem wallSlideSparkParticles; 
	public ParticleSystem wallJumpRing; 
	public ParticleSystem wallJumpBlast; 
	public ParticleSystem wallJumpFireParticles; 
	public ParticleSystem landingParticles; 

	/// <summary>
	/// Shoots a raycast downwards from the player, and checks the distance between the player and the ground. If that distance is greater than the distToGrounded variable, the player will fall down
	/// </summary>
	bool Grounded ()
	{
		return Physics.Raycast (transform.position, Vector3.down, moveSetting.distToGrounded, moveSetting.ground);
	}

	void Start ()
	{
		//targetRotation = transform.rotation;
		rBody = GetComponent<Rigidbody> ();

		//playerMesh = GetComponent<MeshRenderer> ();
		//playerColor = playerMesh.material.color;

		forwardInput = 0;
		strafeInput = 0;
		jumpInput = 0;

		curDownAccel = physSetting.normDownAccel;

		//currentHealth = maxHealth;
		//targetHealth = maxHealth;

		currentHealth = PlayerPrefsManager.inst.GetSavedHealth (); 
		targetHealth = currentHealth; 

		maxStamina = CalculateMaxStamina (); 
		currentStamina = maxStamina;

		if (playerSetting.healthbarFill == null)
		{
			Debug.LogError ("Player has no Health Bar Fill");
		}

		if (playerSetting.staminaBarFill == null)
		{
			Debug.LogError ("Player has no Stamina Bar Fill");
		}

		if (playerSetting.healthPercentage == null)
		{
			Debug.LogError ("Player has no Health Percentage Text");
		}

		dashCounter = moveSetting.dashCooldown;

		playerAudio = GetComponent<AudioSource> ();

		dashRingParticles.enableEmission = false; 
		dashBootParticles.enableEmission = false;
		wallSlideParticles.enableEmission = false; 
		wallSlideSparkParticles.enableEmission = false; 

		//playerAudio.Play ();
	}

	/// <summary>
	/// Assignes the inputs to their respective references
	/// </summary>
	void GetInput ()
	{
		forwardInput = Input.GetAxisRaw (inputSetting.FORWARD_AXIS); //interpolated
		strafeInput = Input.GetAxisRaw (inputSetting.STRAFE_AXIS); //interpolated
		jumpInput = Input.GetAxisRaw (inputSetting.JUMP_AXIS); //non-interpolated
	}

	void Update ()
	{

		if (currentHealth <= 0)
		{
			isAlive = false;
			//velocity = Vector3.zero;
		}

		if (isAlive)
		{
			GetInput ();
			Health ();
			Stamina ();
			SlowMo ();
		}

		transform.rotation = Quaternion.Euler (0, transform.rotation.y, 0);

		if (!isDead)
		{
			OrientPlayer (playerCamera);
		}

		if (velocity.y < 0)
		{
			anim.SetBool ("Jump", false);
			isFalling = true;
		}
		else
		{
			isFalling = false;
		}
			
		anim.SetBool ("isFalling", isFalling);

		if (Input.GetKeyDown (KeyCode.LeftShift))
		{
			if (dashCounter == 2 && GameState.inst.upgradesFound [2])
			{
				isDashing = true;
				//dashBootParticles.Stop(); 
				//dashBootParticles.Play(); 
				startCooldown = true;
			}
		}

		if (startCooldown)
		{
			if (dashCounter > 0)
			{
				dashCounter -= Time.deltaTime;
			}
			else
			{
				startCooldown = false;
				dashCounter = moveSetting.dashCooldown;
			}
		}

		if (currentHealth < 0)
		{
			currentHealth = 0;
		}

		if (currentHealth == 0)
		{
			anim.SetBool ("isDead", true);
			StartCoroutine ("Death");
		}

		Animations ();

		Debug.Log (isHuggingWall);

		if (isHuggingWall && isAlive)
		{
			wallSlideParticles.enableEmission = true; 
			wallSlideSparkParticles.enableEmission = true; 
		}
		else
		{
			wallSlideParticles.enableEmission = false; 
			wallSlideSparkParticles.enableEmission = false; 
		}
	}

	void FixedUpdate ()
	{
		if (isAlive)
		{
			Run ();
			Strafe ();
			Jump ();
		}
		else
		{
			velocity.x = 0;
			velocity.z = 0;

			if (!Grounded ())
			{
				velocity.y -= physSetting.normDownAccel;
			}
			else
			{
				velocity.y = 0;
			}
		}
			
		anim.SetFloat ("Velocity", Mathf.Abs (forwardInput) + Mathf.Abs (strafeInput));

		if (GameState.inst.upgradesFound [1])
		{
			WallJump ();
		}

		if (isDashing)
		{
			StartCoroutine ("AirDash");
		}

		rBody.velocity = transform.TransformDirection (velocity);
	}

	void Animations ()
	{
		anim.SetBool ("isDoubleJumping", isDoubleJumping);
		anim.SetFloat ("verticalVelocity", velocity.y);

		anim.SetBool ("isGrounded", Grounded ());

		anim.SetBool ("backToWall", backToWall);
		anim.SetBool ("faceToWall", faceToWall);
		anim.SetBool ("rSideToWall", rSideToWall);
		anim.SetBool ("lSideToWall", lSideToWall);

		anim.SetBool ("canDoubleJump", canDoubleJump);

		anim.SetBool ("wallJumped", isWallJumping);

		anim.SetBool ("Dash", isDashing);

		if (isDashing == true && velocity.x != 0 || isDashing == true && velocity.z != 0)
		{
			dashRingParticles.enableEmission = true; 
			dashBootParticles.enableEmission = true;
		}
		else
		{
			dashRingParticles.enableEmission = false; 
			dashBootParticles.enableEmission = false;
		}
	}

	/// <summary>
	/// Method that contains player's forward movememnt code
	/// </summary>
	void Run ()
	{
		if (Mathf.Abs (forwardInput) > inputSetting.inputDelay && !isSlowed)
		{
			//move
			velocity.z = moveSetting.forwardVel * forwardInput * Time.timeScale;
			lastZVel = velocity.z;
			anim.SetFloat ("zDir", velocity.z);
		}
		else if (forwardInput == 0 && isDashing == false && Grounded ())
		{
			//zero velocity
			velocity.z = 0;
			lastZVel = 0;
			anim.SetFloat ("zDir", velocity.z);
		}
	}

	/// <summary>
	/// Method that contains player's strafing code
	/// </summary>
	void Strafe ()
	{
		if (Mathf.Abs (strafeInput) > inputSetting.inputDelay && !isSlowed)
		{
			//move
			velocity.x = moveSetting.strafeVel * strafeInput * Time.timeScale;
			lastXVel = velocity.x;
			anim.SetFloat ("xDir", velocity.x);
		}
		else if (strafeInput == 0 && isDashing == false && Grounded ())
		{
			//zero velocity
			velocity.x = 0;
			lastXVel = 0;
			anim.SetFloat ("xDir", velocity.x);
		}
	}

	IEnumerator AirDash ()
	{
		if (Grounded ())
		{
			velocity.z = (moveSetting.dashVel * forwardInput) + (moveSetting.forwardVel * forwardInput * Time.timeScale);
			velocity.x = (moveSetting.dashVel * strafeInput) + (moveSetting.forwardVel * strafeInput * Time.timeScale);
		}
		else if (velocity.z > 0 && Grounded () == false)
		{
			velocity.z = (moveSetting.dashVel / 1.5f) + (moveSetting.forwardVel * forwardInput * Time.timeScale);
		}
		else if (velocity.x > 0 && Grounded () == false)
		{
			velocity.x = (moveSetting.dashVel / 1.5f) + (moveSetting.forwardVel * strafeInput * Time.timeScale);
		}
		else if (velocity.z < 0 && Grounded () == false)
		{
			velocity.z = (-moveSetting.dashVel / 1.5f) + (moveSetting.forwardVel * forwardInput * Time.timeScale);
		}
		else if (velocity.x < 0 && Grounded () == false)
		{
			velocity.x = (-moveSetting.dashVel / 1.5f) + (moveSetting.forwardVel * strafeInput * Time.timeScale);
		}
		yield return new WaitForSeconds (0.2f);
		isDashing = false;
	}

	/// <summary>
	/// Method that contains player's jumping code
	/// </summary>
	void Jump ()
	{
		if (jumpInput > 0 && Grounded () && !isHuggingWall)
		{
			//Jump
			velocity.y = moveSetting.jumpVel;
			anim.SetBool ("Jump", true);
			if (GameState.inst.upgradesFound [0])
			{
				canDoubleJump = true;
			}
		}
		else if (jumpInput == 0 && Grounded ())
		{
			//zero out our velocity.y
			velocity.y = 0;
			isDoubleJumping = false;
			isHuggingWall = false;
			canDoubleJump = false;
		}
		else
		{
			//decrease velocity.y
			velocity.y -= curDownAccel * Time.timeScale;

			if (canDoubleJump == true)
			{
				if (Input.GetKeyDown (KeyCode.Space))
				{
					isDoubleJumping = true;
					velocity.y = moveSetting.jumpVel;
					if (velocity.magnitude > 0)
					{
						velocity = velocity * 1.2f;
					}
					canDoubleJump = false;
					anim.SetTrigger ("DoubleJump");

					// Trigger double jump particles
					doubleJumpParticles.Stop (); 
					doubleJumpParticles.Play (); 
				}
			}
		}
	}

	void WallJump ()
	{

		if (Physics.Raycast (transform.position, -transform.forward, 1))
		{
			backToWall = true;
		}
		else
		{
			backToWall = false;
		}
			
		if (Physics.Raycast (transform.position, transform.forward, 1))
		{
			faceToWall = true;
		}
		else
		{
			faceToWall = false;
		}

		if (Physics.Raycast (transform.position, transform.right, 1))
		{
			rSideToWall = true;
		}
		else
		{
			rSideToWall = false;
		}

		if (Physics.Raycast (transform.position, -transform.right, 1))
		{
			lSideToWall = true;
		}
		else
		{
			lSideToWall = false;
		}

		if (jumpInput > 0 && isHuggingWall)
		{
			if (faceToWall)
			{
				velocity.z = moveSetting.forwardVel * -1.5f; //* -1.5f;
				velocity.y = moveSetting.jumpVel * 1.2f;
				isHuggingWall = false;
				wallJumpRing.Play();
				wallJumpRing.gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0); 
				wallJumpBlast.Play(); 
				wallJumpBlast.gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0); 
				wallJumpFireParticles.Play(); 
				wallJumpFireParticles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
			}
			else if (backToWall)
			{
				velocity.z = moveSetting.forwardVel * 1.5f;
				velocity.y = moveSetting.jumpVel * 1.2f;
				isHuggingWall = false;
				wallJumpRing.Play();
				wallJumpRing.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
				wallJumpBlast.Play(); 
				wallJumpBlast.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0); 
				wallJumpFireParticles.Play(); 
				wallJumpFireParticles.gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
			}
			else if (rSideToWall)
			{
				velocity.x = moveSetting.strafeVel * -1.5f; //* -1.5f;
				velocity.y = moveSetting.jumpVel * 1.2f;
				isHuggingWall = false;
				wallJumpRing.Play();
				wallJumpRing.gameObject.transform.localRotation = Quaternion.Euler(0, -90, 0);
				wallJumpBlast.Play(); 
				wallJumpBlast.gameObject.transform.localRotation = Quaternion.Euler(0, -90, 0);
				wallJumpFireParticles.Play(); 
				wallJumpFireParticles.gameObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
			}
			else if (lSideToWall)
			{
				velocity.x = moveSetting.strafeVel * 1.5f;
				velocity.y = moveSetting.jumpVel * 1.2f;
				isHuggingWall = false;
				wallJumpRing.Play();
				wallJumpRing.gameObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
				wallJumpBlast.Play(); 
				wallJumpBlast.gameObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
				wallJumpFireParticles.Play(); 
				wallJumpFireParticles.gameObject.transform.localRotation = Quaternion.Euler(0, -90, 0);
			}
		}
		else if (jumpInput == 0 && isHuggingWall)
		{
			velocity.y -= physSetting.downWallAccel;
			velocity.x = 0;
			velocity.z = 0;
		}
	}

	/// <summary>
	/// Transforms the player's rotation to that of the specified camera
	/// </summary>
	/// <param name="cam">Cam.</param>
	void OrientPlayer (Camera cam)
	{
		if (isOriented)
		{
			transform.rotation = Quaternion.Euler (transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
			lastRot = Quaternion.Euler (transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
		}
		else
		{
			transform.rotation = lastRot;
		}
	}

	IEnumerator Death ()
	{
		yield return new WaitForSeconds (3);
		if (GlobalManager.inst.globalState == GlobalManager.GlobalState.Gameplay)
		{
			GlobalManager.inst.LoadGameOver ();
		}
	}

	IEnumerator DamageFlash ()
	{
		Renderer[] renderer = this.GetComponentsInChildren<Renderer> ();
		Color color = renderer [0].material.color;

		color = Color.red;

		foreach (Renderer rend in renderer)
		{
			rend.material.color = color;
		}

		yield return new WaitForSeconds (0.1f);

		float time = 0.0f;

		while (color != Color.white)
		{
			color = Color.Lerp (Color.red, Color.white, time * 2);
			time += Time.deltaTime;
			foreach (Renderer rend in renderer)
			{
				rend.material.color = color;
			}
			yield return new WaitForEndOfFrame ();
		} 
	}

	/// <summary>
	/// Method containing Player's health code
	/// </summary>
	void Health ()
	{
		if (currentHealth <= 0.9f && !isDead)
		{
			currentHealth = 0; // This ensures that the health percentage is NEVER less than zero.

			// Respawn execution goes here
			isDead = true; // Prevents the code from executing the respawn sequence multiple times.
		} 

		Mathf.Round (targetHealth);

		if (currentHealth >= maxHealth)
		{
			currentHealth = maxHealth; // Caps the players current health to the designated max health.
		} 

		// Updates the health percentage.
		playerSetting.healthPercentage.GetComponent<Text> ().text = ((int)currentHealth).ToString () + "%";

		// Updates the health bars fill.
		playerSetting.healthbarFill.GetComponent<Image> ().fillAmount = currentHealth / 100;

		// Smooths the current player health value based on the target health variable.
		currentHealth = Mathf.SmoothDamp (currentHealth, targetHealth, ref healthSmoothing, 0.3f);
	}

	/// <summary>
	/// Heals the player by the specified amount
	/// </summary>
	/// <param name="healthRecieved">Health recieved.</param>
	public static void HealCalculator (float healthReceived)
	{
		// Only heals the player if player health is below the max. 
		if (currentHealth < maxHealth)
		{
			targetHealth += healthReceived; 

			if (targetHealth > maxHealth)
				targetHealth = maxHealth; 
		}
	}

	/// <summary>
	/// Applies the specified damage value to the player's current health
	/// </summary>
	/// <returns>The calculator.</returns>
	/// <param name="damageRecieved">Damage recieved.</param>
	public static void DamageCalculator (float damageRecieved)
	{
		if (currentHealth > 0)
		{
			targetHealth = currentHealth - damageRecieved;
		}
	}

	/// <summary>
	/// Restores to full health.
	/// </summary>
	public void RestoreToFullHealth ()
	{
		targetHealth = maxHealth + 1;
	}

	float CalculateMaxStamina ()
	{
		return 100 + (GameState.inst.GetNumStaminaPickupsFound () * 25);
	}

	/// <summary>
	/// Method containing player's stamina code.
	/// </summary>
	void Stamina ()
	{
		maxStamina = CalculateMaxStamina ();

		playerSetting.staminaBarBackground.GetComponent<RectTransform> ().sizeDelta = new Vector2 (154.5f + (GameState.inst.GetNumStaminaPickupsFound () * 25.75f), 40);

		if (isFalling)
		{
			if (Input.GetMouseButton (1) && currentStamina > 0)
			{
				cooldownTimer = 0;
				currentStamina -= playerSetting.staminaDrain * Time.unscaledDeltaTime;
			}
		}

		if (currentStamina != maxStamina)
		{
			if (cooldownTimer < cooldownTime)
			{
				cooldownTimer += Time.unscaledDeltaTime;
			}

			if (cooldownTimer >= cooldownTime && currentStamina < maxStamina)
			{
				currentStamina += playerSetting.staminaRecover * Time.unscaledDeltaTime;
			}
		}

		playerSetting.staminaBarFill.GetComponent<Image> ().fillAmount = currentStamina / (100 + (GameState.inst.GetNumStaminaPickupsFound () * 25));
	}

	// Called from AnimSounds.LandingSFX()
	public void OnLanding()
	{
		landingParticles.Play(); 
	}

	/// <summary>
	/// Slows the mo.
	/// </summary>
	void SlowMo ()
	{
		if (Input.GetMouseButtonDown (1) && isFalling && currentStamina > 0)
		{
			playerAudio.PlayOneShot (slowMoSound);
		}

		if (Input.GetMouseButton (1) && isFalling && currentStamina > 0)
		{
			Timescaler.inst.timeSlowed = true;
			isSlowed = true;
			velocity.z = lastZVel;
			velocity.x = lastXVel;
		}
		else
		{
			Timescaler.inst.timeSlowed = false;
			isSlowed = false;
		}
	}

	public float getHealth ()
	{
		return currentHealth;
	}

	public void setHealth (float h)
	{
		currentHealth = h;
	}

	public bool GetIsAlive ()
	{
		return isAlive; 
	}

	void OnCollisionEnter (Collision col)
	{
		//If the player comes in contact with an enemy, initiate invincibility coroutine
		if (col.gameObject.tag == "Enemy")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			DamageCalculator (5);

			StopCoroutine ("DamageFlash");
			StartCoroutine ("DamageFlash");
		}

		if (col.gameObject.tag == "Charger")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			DamageCalculator (10);

			StopCoroutine ("DamageFlash");
			StartCoroutine ("DamageFlash");
		}

		if (col.gameObject.tag == "Wall" && !Grounded ())
		{
			
			if (GameState.inst.upgradesFound [1])
			{

				isHuggingWall = true;

				velocity = Vector3.zero;

				curDownAccel = physSetting.downWallAccel;

				StopCoroutine ("AirDash");
			}
			else
			{

				velocity = Vector3.zero;

				StopCoroutine ("AirDash");
			}
		}

		if (col.gameObject.tag == "OrganicWall" && !Grounded ())
		{
			velocity = Vector3.zero;

			StopCoroutine ("AirDash");
		}

		if (col.gameObject.tag == "Spikes")
		{
			targetHealth = 0;
		}
	}

	void OnCollisionExit (Collision col)
	{
		if (GameState.inst.upgradesFound [1])
		{
			if (col.gameObject.tag == "Wall" && !Grounded ())
			{
				curDownAccel = physSetting.normDownAccel;
			}
		}
	}

	void OnCollisionStay (Collision col)
	{
		//If the player is still touching the enemy when invuln wears off, ininitate invincibility coroutine again
		if (col.gameObject.tag == "Enemy")
		{
			anim.SetTrigger ("isHit");
			DamageCalculator (3);
		}

		if (GameState.inst.upgradesFound [1])
		{
			if (col.gameObject.tag == "Wall" && !Grounded ())
			{
				curDownAccel = physSetting.downWallAccel;
			}
		} 

		if (col.gameObject.tag == "OrganicWall" && !Grounded ())
		{
			velocity.x = 0;
			velocity.z = 0;

			velocity.y -= (physSetting.normDownAccel / 4);
		}

		if (col.gameObject.tag == "Charger")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			anim.SetTrigger ("isHit");
			DamageCalculator (10);

			StopCoroutine ("DamageFlash");
			StartCoroutine ("DamageFlash");
		}
	}

	void OnTriggerEnter (Collider col)
	{
		//If the player comes in contact with an enemy, initiate invincibility coroutine
		if (col.gameObject.tag == "Enemy")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			anim.SetTrigger ("isHit");
			DamageCalculator (10);

			StopCoroutine ("DamageFlash");
			StartCoroutine ("DamageFlash");
		}

		if (col.gameObject.tag == "ItemPickup")
		{
			playerAudio.PlayOneShot (itemEquip);
		}
	}

	void OnTriggerStay (Collider col)
	{
		//If the player is still touching the enemy when invuln wears off, ininitate invincibility coroutine again
		if (col.gameObject.tag == "Enemy")
		{
			DamageCalculator (10);
			anim.SetTrigger ("isHit");
			StopCoroutine ("DamageFlash");
			StartCoroutine ("DamageFlash");
		}
	}

	/// <summary>
	/// Called from the bullet class when a bullet hits a collider with the Player tag
	/// </summary>
	/// <param name="col">Col.</param>
	public void OnShot (Collision col, Bullet bullet)
	{ 
		StopCoroutine ("DamageFlash");
		StartCoroutine ("DamageFlash");
		anim.SetTrigger ("isHit");
		DamageCalculator (bullet.damage); 
	}
}