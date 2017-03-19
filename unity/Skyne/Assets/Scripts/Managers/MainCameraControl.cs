using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraControl : MonoBehaviour 
{
	public Transform player;                                           // Player's reference.
	public Vector3 pivotOffset = new Vector3(0.0f, 1.0f,  0.0f);       // Offset to repoint the camera.
	public Vector3 cameraOffset   = new Vector3(0.0f, 0.7f, -3.0f);       // Offset to relocate the camera related to the player position.
	public float smooth = 10f;                                         // Speed of camera responsiveness.
	public float horizontalAimingSpeed = 400f;                         // Horizontal turn speed.
	public float verticalAimingSpeed = 400f;                           // Vertical turn speed.
	public float maxVerticalAngle = 30f;                               // Camera max clamp angle. 
	public float minVerticalAngle = -60f;                              // Camera min clamp angle.

	private float HorizontalAngle = 0;                                 // Float to store camera horizontal angle related to mouse movement.
	private float VerticalAngle = 0;                                   // Float to store camera vertical angle related to mouse movement.
	private Transform camera;                                          // This transform.
	private Vector3 relativePositionToPlayer;                          // Current camera position relative to the player.
	private float relativeDistanceToPlayer;                            // Current camera distance to the player.
	private Vector3 smoothPivotOffset;                                 // Camera current pivot offset on interpolation.
	private Vector3 smoothCamOffset;                                   // Camera current offset on interpolation.
	private Vector3 targetPivotOffset;                                 // Camera pivot offset target to iterpolate.
	private Vector3 targetCamOffset;                                   // Camera offset target to interpolate.
	private float defaultFieldOfView;                                  // Default camera Field of View.
	private float targetFieldOfView;                                   // Target camera FIeld of View.
	private float verticalAngleClamp;                              	   // Custom camera max vertical clamp angle. 

	void Awake()
	{
		// Reference to the camera transform.
		camera = transform;

		// Set camera default position.
		camera.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * cameraOffset; 
		camera.rotation = Quaternion.identity;

		// Get camera position relative to the player, used for collision test.
		relativePositionToPlayer = transform.position - player.position;
		relativeDistanceToPlayer = relativePositionToPlayer.magnitude - 0.5f;

		// Set up references and default values.
		smoothPivotOffset = pivotOffset;
		smoothCamOffset = cameraOffset;
		defaultFieldOfView = camera.GetComponent<Camera>().fieldOfView;

		ResetTargetOffsets ();
		ResetFieldOfView ();
		ResetMaxVerticalAngle();
	}

	void Start() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update() {
		
	}

	void FixedUpdate() 
	{
		//Cursor.lockState = CursorLockMode.Locked;
	}

	void LateUpdate()
	{
		// Get mouse movement to orbit the camera.
		HorizontalAngle += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed * Time.unscaledDeltaTime;//Time.deltaTime;
		VerticalAngle += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed * Time.unscaledDeltaTime;//Time.deltaTime;

		// Set vertical movement limit.
		VerticalAngle = Mathf.Clamp(VerticalAngle, minVerticalAngle, verticalAngleClamp);

		// Set camera orientation..
		Quaternion camYRotation = Quaternion.Euler(0, HorizontalAngle, 0);
		Quaternion aimRotation = Quaternion.Euler(-VerticalAngle, HorizontalAngle, 0);
		camera.rotation = aimRotation;

		// Set FOV.
		camera.GetComponent<Camera>().fieldOfView = Mathf.Lerp (camera.GetComponent<Camera>().fieldOfView, targetFieldOfView,  Time.unscaledDeltaTime);

		// Test for collision with the environment based on current camera position.
		Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
		Vector3 noCollisionOffset = targetCamOffset;
		for(float zOffset = targetCamOffset.z; zOffset <= 0; zOffset += 0.5f)
		{
			noCollisionOffset.z = zOffset;
			if (DoubleViewingPosCheck (baseTempPosition + aimRotation * noCollisionOffset) || zOffset == 0) 
			{
				break;
			} 
		}

		// Repostition the camera.
		smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.unscaledDeltaTime);
		smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset, smooth * Time.unscaledDeltaTime);

		camera.position =  player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
	}

	// Set camera offsets to custom values.
	public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
	{
		targetPivotOffset = newPivotOffset;
		targetCamOffset = newCamOffset;
	}

	// Reset camera offsets to default values.
	public void ResetTargetOffsets()
	{
		targetPivotOffset = pivotOffset;
		targetCamOffset = cameraOffset;
	}

	// Set camera vertical offset.
	public void SetYCamOffset(float y)
	{
		targetCamOffset.y = y;
	}

	// Set custom Field of View.
	public void SetFieldOfView(float customFOV)
	{
		this.targetFieldOfView = customFOV;
	}

	// Reset Field of View to default value.
	public void ResetFieldOfView()
	{
		this.targetFieldOfView = defaultFieldOfView;
	}

	// Set max vertical camera rotation angle.
	public void SetMaxVerticalAngle(float angle)
	{
		this.verticalAngleClamp = angle;
	}

	// Reset max vertical camera rotation angle to default value.
	public void ResetMaxVerticalAngle()
	{
		this.verticalAngleClamp = maxVerticalAngle;
	}

	// Double check for collisions: concave objects doesn't detect hit from outside, so cast in both directions.
	bool DoubleViewingPosCheck(Vector3 checkPos)
	{
		float playerFocusHeight = player.GetComponent<CapsuleCollider> ().height *0.5f;
		return ViewingPosCheck (checkPos, playerFocusHeight) && ReverseViewingPosCheck (checkPos, playerFocusHeight);
	}

	// Check for collision from camera to player.
	bool ViewingPosCheck (Vector3 checkPos, float deltaPlayerHeight)
	{
		RaycastHit hit;

		// If a raycast from the check position to the player hits something...
		if(Physics.Raycast(checkPos, player.position+(Vector3.up* deltaPlayerHeight) - checkPos, out hit, relativeDistanceToPlayer))
		{
			// ... if it is not the player...
			if(hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				// This position isn't appropriate.
				return false;
			}
		}
		// If we haven't hit anything or we've hit the player, this is an appropriate position.
		return true;
	}

	// Check for collision from player to camera.
	bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
	{
		RaycastHit hit;

		if(Physics.Raycast(player.position+(Vector3.up* deltaPlayerHeight), checkPos - player.position, out hit, relativeDistanceToPlayer))
		{
			if(hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				return false;
			}
		}
		return true;
	}

	// Get camera magnitude.
	public float getCurrentPivotMagnitude(Vector3 finalPivotOffset)
	{
		return Mathf.Abs ((finalPivotOffset - smoothPivotOffset).magnitude);
	}

	public float GetVerticalAngle()
	{
		return VerticalAngle; 
	}
}
