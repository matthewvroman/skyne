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

		public float startSlidingTimer = 5;

		public float knockbackForce;

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
		[Tooltip ("Speed at which color attempts to complete the lerp during invuln")]
		public float colorLerpSpeed;
		[Tooltip ("Number of seconds that invincibility frames last")]
		public float invulnTime;
		[Tooltip ("Speed at which player flashes during invuln")]
		public float flashSpeed;

		[Tooltip ("The UI object representing the image for the health")]
		public Transform healthbarFill;
		[Tooltip ("The UI text representing the text for health")]
		public Transform healthPercentage;

		[Tooltip ("The UI object representing the image for stamina")]
		public GameObject staminaBarFill;
		// The stamina UI bar.

		public float staminaDrain;
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

	MeshRenderer playerMesh;
	Color playerColor;

	float curDownAccel;

	bool isOriented = true;

	bool isFalling = false;

	bool isFocused = false;

	bool isInvincible = false;

	bool jump = false;
	bool canDoubleJump = false;

	bool isDashing = false;

	bool isHuggingWall = false;
	bool startSliding = false;
	float counter;
	RaycastHit wallHit;

	bool backToWall = false;
	bool faceToWall = false;
	bool rSideToWall = false;
	bool lSideToWall = false;

	float dashCounter;
	bool startCooldown;

	bool isWallJumping = false;

	bool isPushed = false;

	bool isDoubleJumping = false;

	Quaternion lastRot;

	Vector3 velocity = Vector3.zero;
	Quaternion targetRotation;
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
	public float cooldownTime = 3;
	// Time until the stamina bar regenerates.

	public Animator anim;

	AudioSource playerAudio;

	public AudioClip footstep1;
	public AudioClip footstep2;
	public AudioClip footstep3;
	public AudioClip doubleJumpSound;
	public AudioClip airDashSound;
	public AudioClip jumpSound;
	public AudioClip WallJumpSound;
	public AudioClip ameliaGrunt1;
	public AudioClip ameliaGrunt2;
	public AudioClip ameliaGrunt3;

	/// <summary>
	/// Shoots a raycast downwards from the player, and checks the distance between the player and the ground. If that distance is greater than the distToGrounded variable, the player will fall down
	/// </summary>
	bool Grounded ()
	{
		return Physics.Raycast (transform.position, Vector3.down, moveSetting.distToGrounded, moveSetting.ground);
	}

	void Start ()
	{
		targetRotation = transform.rotation;
		rBody = GetComponent<Rigidbody> ();

		playerMesh = GetComponent<MeshRenderer> ();
		playerColor = playerMesh.material.color;

		forwardInput = 0;
		strafeInput = 0;
		jumpInput = 0;

		curDownAccel = physSetting.normDownAccel;

		currentHealth = maxHealth;
		targetHealth = maxHealth;

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

		counter = moveSetting.startSlidingTimer;

		dashCounter = moveSetting.dashCooldown;

		playerAudio = GetComponent<AudioSource> ();
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
		GetInput ();
		//Focus ();
		Health ();
		Stamina ();
		SlowMo ();
		PlaySounds ();
		Footsteps ();

		transform.rotation = Quaternion.Euler (0, transform.rotation.y, 0);

		OrientPlayer (playerCamera);

		if (isInvincible)
		{
			DamageFlash ();
		}
		else
		{
			playerMesh.material.color = playerColor;
		}

		if (velocity.y < 0)
		{
			isFalling = true;
			rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
		else
		{
			isFalling = false;
			rBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
			
		anim.SetBool ("isFalling", isFalling);

		if (Grounded ())
		{
			canDoubleJump = false;
			startSliding = false;
			isWallJumping = false;
		}

		if (Input.GetKeyDown (KeyCode.LeftShift))
		{
			if (dashCounter == 2 && GameState.inst.upgradesFound [2])
			{
				isDashing = true;
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

		if (counter > 0)
		{
			startSliding = false;
		}

		if (currentHealth < 0)
		{
			currentHealth = 0;
		}

		if (currentHealth == 0)
		{
			anim.SetBool ("isDead", true);
			StartCoroutine ("Death");
			//GlobalManager.inst.Lo ();
		}

		Debug.Log ("Knockback:" + isPushed);

		Animations ();
	}

	void FixedUpdate ()
	{
		Run ();
		Strafe ();
		anim.SetFloat ("velocity", Mathf.Abs (forwardInput) + Mathf.Abs (strafeInput));

		Jump ();

		if (GameState.inst.upgradesFound [1])
		{
			WallJump ();
		}

		if (isDashing)
		{
			StartCoroutine (AirDash ());
		}

		/*if (startSliding)
		{
			velocity.y -= physSetting.downAccel * 2;
		} */

		rBody.velocity = transform.TransformDirection (velocity);
	}

	void Animations ()
	{
		anim.SetBool ("isGrounded", Grounded ());
		anim.SetFloat ("verticalVelocity", velocity.y);
		anim.SetBool ("isOnWall", backToWall);
		anim.SetBool ("wallJumped", isWallJumping);
		anim.SetBool ("canDoubleJump", canDoubleJump);
		anim.SetBool ("isDashing", isDashing);
		anim.SetBool ("isHit", isPushed);
		anim.SetBool ("isDoubleJumping", isDoubleJumping);
	}

	/// <summary>
	/// Method that contains player's forward movememnt code
	/// </summary>
	void Run ()
	{
		if (Mathf.Abs (forwardInput) > inputSetting.inputDelay && !isWallJumping && isPushed == false)
		{
			//move
			velocity.z = moveSetting.forwardVel * forwardInput * Time.timeScale;
		}
		else if (forwardInput == 0 && isDashing == false && !isWallJumping && Grounded ())
		{
			//zero velocity
			velocity.z = 0;
		}
	}

	/// <summary>
	/// Method that contains player's strafing code
	/// </summary>
	void Strafe ()
	{
		if (Mathf.Abs (strafeInput) > inputSetting.inputDelay && !isWallJumping && isPushed == false)
		{
			//move
			velocity.x = moveSetting.strafeVel * strafeInput * Time.timeScale;
			anim.SetFloat ("orientation", velocity.x);
		}
		else if (strafeInput == 0 && isDashing == false && !isWallJumping && Grounded ())
		{
			//zero velocity
			velocity.x = 0;
			anim.SetFloat ("orientation", velocity.x);
		}
	}

	IEnumerator AirDash ()
	{
		rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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
		rBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		isDashing = false;
	}

	/// <summary>
	/// Method that contains player's jumping code
	/// </summary>
	void Jump ()
	{
		if (jumpInput > 0 && Grounded ())
		{
			//Jump
			velocity.y = moveSetting.jumpVel;
			canDoubleJump = true;
		}
		else if (jumpInput == 0 && Grounded ())
		{
			//zero out our velocity.y
			velocity.y = 0;
			isDoubleJumping = false;
		}
		else
		{
			//decrease velocity.y
			velocity.y -= curDownAccel * Time.timeScale;

			if (GameState.inst.upgradesFound [0])
			{
				if (Input.GetKeyDown (KeyCode.Space) && canDoubleJump)
				{
					isDoubleJumping = true;
					velocity.y = moveSetting.jumpVel;
					canDoubleJump = false;

					playerAudio.clip = null;
					playerAudio.PlayOneShot (doubleJumpSound);
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

		if (isWallJumping)
		{
			if (isHuggingWall && !Grounded ())
			{
				curDownAccel = physSetting.downWallAccel;
				velocity.x = 0;
				velocity.z = 0;
				isOriented = false;
			}

			if (faceToWall)
			{
				if (Input.GetKeyDown (KeyCode.Space) && isHuggingWall)
				{
					isOriented = true;
					curDownAccel = physSetting.normDownAccel;
					velocity.y = moveSetting.jumpVel;
					velocity.z = moveSetting.forwardVel * -1;
					isHuggingWall = false;
				}
			}
			else if (backToWall)
			{
				if (Input.GetKeyDown (KeyCode.Space) && isHuggingWall)
				{
					isOriented = true;
					curDownAccel = physSetting.normDownAccel;
					velocity.y = moveSetting.jumpVel;
					velocity.z = moveSetting.forwardVel * 1;
					isHuggingWall = false;
				}
			}
			else if (rSideToWall)
			{
				if (Input.GetKeyDown (KeyCode.Space) && isHuggingWall)
				{
					isOriented = true;
					curDownAccel = physSetting.normDownAccel;
					velocity.y = moveSetting.jumpVel;
					velocity.x = moveSetting.strafeVel * -1;
					isHuggingWall = false;
				}
			}
			else if (lSideToWall)
			{
				if (Input.GetKeyDown (KeyCode.Space) && isHuggingWall)
				{
					isOriented = true;
					curDownAccel = physSetting.normDownAccel;
					velocity.y = moveSetting.jumpVel;
					velocity.x = moveSetting.strafeVel * 1;
					isHuggingWall = false;
				}
			}
		}
		else
		{
			curDownAccel = physSetting.normDownAccel;
			isOriented = true;
		}
			
		/*if (isHuggingWall)
		{
			if (Input.GetKeyDown(KeyCode.Space)) 
			{
				if (backToWall)
				{
					playerAudio.clip = null;
					playerAudio.PlayOneShot (jumpSound);
				}
			}

			if (jumpInput > 0 && forwardInput != 0)
			{
				if (backToWall)
				{
					velocity.y = moveSetting.jumpVel;
					velocity.z = moveSetting.forwardVel * forwardInput;
					isWallJumping = true;
				}
			}
			else if (jumpInput > 0 && forwardInput == 0)
			{
				isHuggingWall = false;
			}
			else
			{
				if (!Grounded ())
				{
					velocity.x = 0;
					velocity.z = 0;
					velocity.y = 0;
				}
			}
		}*/
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

		/*if (Input.GetKey (KeyCode.Q))
		{
			isOriented = false;
		}
		else
		{
			isOriented = true;
		} */
	}

	/// <summary>
	/// Moves the player camera into focus position when a button is held
	/// </summary>
	void Focus ()
	{
		if (Input.GetMouseButton (1))
		{
			camCon.SetTargetOffsets (camCon.pivotOffset, moveSetting.focusOffset);
			isFocused = true;
		}
		else
		{
			camCon.ResetTargetOffsets ();
			isFocused = false;
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

	/// <summary>
	/// Causes the player to become invincible for a specified number of seconds
	/// </summary>
	IEnumerator Invicibility ()
	{
		isInvincible = true;
		yield return new WaitForSeconds (playerSetting.invulnTime);
		isInvincible = false;
	}

	/// <summary>
	/// Causes player model to flash red during invuln
	/// </summary>
	void DamageFlash ()
	{
		playerMesh.material.color = Color.Lerp (playerColor, Color.red, Mathf.PingPong (Time.time * playerSetting.flashSpeed, playerSetting.colorLerpSpeed));
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
		playerSetting.healthbarFill.transform.localScale = new Vector3 (currentHealth / 100, playerSetting.healthbarFill.transform.localScale.y, playerSetting.healthbarFill.transform.localScale.z); 

		// Smooths the current player health value based on the target health variable.
		currentHealth = Mathf.SmoothDamp (currentHealth, targetHealth, ref healthSmoothing, 0.3f);
	}

	/// <summary>
	/// Heals the player by the specified amount
	/// </summary>
	/// <param name="healthRecieved">Health recieved.</param>
	public static void HealCalculator (float healthRecieved)
	{
		// Only heals the player if player health is below the max. 
		if (currentHealth < maxHealth)
		{
			targetHealth = currentHealth + healthRecieved;
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
		//currentHealth = maxHealth;
	}

	/// <summary>
	/// Method containing player's stamina code.
	/// </summary>
	void Stamina ()
	{
		if (isFalling)
		{
			if (Input.GetMouseButton(1) && currentStamina > 0)
			{
				cooldownTimer = 0;
				DecreaseStamina ();
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
				IncreaseStamina ();
			}
		}

		playerSetting.staminaBarFill.transform.localScale = new Vector3 (currentStamina / 100, playerSetting.staminaBarFill.transform.localScale.y, playerSetting.staminaBarFill.transform.localScale.z);
	}

	/// <summary>
	/// Decreases player stamina.
	/// </summary>
	void DecreaseStamina ()
	{
		currentStamina -= playerSetting.staminaDrain * Time.unscaledDeltaTime;
	}

	/// <summary>
	/// Increases player stamina.
	/// </summary>
	void IncreaseStamina ()
	{
		currentStamina += playerSetting.staminaDrain * Time.unscaledDeltaTime;
	}

	/// <summary>
	/// Slows the mo.
	/// </summary>
	void SlowMo ()
	{
		if (Input.GetMouseButton(1) && isFalling && currentStamina > 0)
		{
			Timescaler.inst.timeSlowed = true;
		}
		else
		{
			Timescaler.inst.timeSlowed = false;
		}
	}

	IEnumerator Knockback ()
	{
		isPushed = true;

		playerAudio.clip = null;
		playerAudio.PlayOneShot (ameliaGrunt2);

		rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		velocity.z = moveSetting.forwardVel * -forwardInput;
		velocity.x = moveSetting.strafeVel * -strafeInput;

		//velocity.z = 0;
		//velocity.x = 0;
		//velocity.y = 0;

		yield return new WaitForSeconds ((moveSetting.knockbackForce / 2) * 0.01f);

		isPushed = false;
		rBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
	}

	void PlaySounds ()
	{

		if (Input.GetKeyDown (KeyCode.LeftShift))
		{
			if (dashCounter == 2 && GameState.inst.upgradesFound [2])
			{
				playerAudio.clip = null;
				playerAudio.PlayOneShot (airDashSound);
			}
		}
		else if (Input.GetKeyDown (KeyCode.Space) && Grounded ())
		{
			playerAudio.clip = null;
			playerAudio.PlayOneShot (jumpSound);
		}
		else if (Grounded () && Mathf.Abs (velocity.magnitude) > 1) //&& playerAudio.isPlaying == false)
		{
			if (playerAudio.isPlaying == false)
			{
				playerAudio.clip = footstep1;
				playerAudio.loop = true;
				playerAudio.Play ();
			}
		}
		else if (!Input.anyKey && velocity.magnitude == 0)//if (Mathf.Abs(velocity.magnitude) == 0 || !Grounded())
		{
			playerAudio.Stop ();
		}
	}

	void Footsteps ()
	{
		if (playerAudio.clip == footstep1)
		{
			playerAudio.volume = Random.Range (0.8f, 1);
			playerAudio.pitch = Random.Range (0.8f, 1.1f); 
		}
		else
		{
			playerAudio.volume = 1;
			playerAudio.pitch = 1;
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

	void OnCollisionEnter (Collision col)
	{
		//If the player comes in contact with an enemy, initiate invincibility coroutine
		if (col.gameObject.tag == "Enemy")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			DamageCalculator (10);

			//StartCoroutine (Invicibility ());

			// Calculate Angle Between the collision point and the player
			Vector3 dir = -(col.contacts [0].point - transform.position).normalized;
			// We then get the opposite (-Vector3) and normalize it
			//dir = -dir.normalized;
			dir.y = 1;

			// And finally we add force in the direction of dir and multiply it by force. 
			// This will push back the player
			rBody.AddForce (dir * moveSetting.knockbackForce, ForceMode.VelocityChange);

			StartCoroutine (Knockback ());


		}

		if (col.gameObject.tag == "Wall" && !Grounded ())
		{
			/*if (isHuggingWall == false)
			{
				playerAudio.clip = null;
				playerAudio.PlayOneShot (WallJumpSound);
			} */

			isHuggingWall = true;
			isWallJumping = true;
			//counter = moveSetting.startSlidingTimer;
		}
		else
		{
			isHuggingWall = false;
		}
		/*else if (col.gameObject.tag == "Wall" && !Grounded () && isWallJumping)
		{
			if (isHuggingWall == false)
			{
				playerAudio.clip = null;
				playerAudio.PlayOneShot (WallJumpSound);
			}

			isHuggingWall = true;
			isWallJumping = true;
			counter = moveSetting.startSlidingTimer;
		} */

		if (col.gameObject.tag == "Spikes")
		{
			targetHealth = 0;
		}
	}

	void OnCollisionExit (Collision col)
	{
		if (col.gameObject.tag == "Wall")
		{
			isHuggingWall = false;
			//counter = moveSetting.startSlidingTimer;
		}
	}

	void OnCollisionStay (Collision col)
	{
		//If the player is still touching the enemy when invuln wears off, ininitate invincibility coroutine again
		if (col.gameObject.tag == "Enemy" && isInvincible == false)
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			if (isInvincible == false)
			{
				DamageCalculator (10);
				//StartCoroutine (DamageCalculator (10));
			}
			//StartCoroutine (Invicibility ());
		}

		/*if (col.gameObject.tag == "Wall" && isHuggingWall)
		{
			if (counter > 0)
			{
				startSliding = false;
				counter -= Time.fixedDeltaTime;
			}
			else
			{
				startSliding = true;
			}
		} */
	}

	void OnTriggerEnter (Collider col)
	{
		//If the player comes in contact with an enemy, initiate invincibility coroutine
		if (col.gameObject.tag == "Enemy")
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			if (isInvincible == false)
			{
				DamageCalculator (10);
				//StartCoroutine (DamageCalculator (10));
			}

			//StartCoroutine (Invicibility ());
		}
		else if (col.gameObject.tag == "Bullet")
		{
			Bullet bullet = col.GetComponent<Bullet> (); 
			if (!bullet.playerBullet)
			{
				bullet.shouldDestroy = true; 

				playerAudio.clip = null;
				playerAudio.PlayOneShot (ameliaGrunt2);

				DamageCalculator (bullet.damage); 
			}
		}
	}

	void OnTriggerStay (Collider col)
	{
		//If the player is still touching the enemy when invuln wears off, ininitate invincibility coroutine again
		if (col.gameObject.tag == "Enemy" && isInvincible == false)
		{
			//When the player collides with an enemy, it checks to see if the player is currently invincible or not. 
			//If not, then the player will take damage
			if (isInvincible == false)
			{
				DamageCalculator (10);
				//StartCoroutine (DamageCalculator (10));
			}
			//StartCoroutine (Invicibility ());
		}
	}
}