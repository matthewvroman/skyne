using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITestScript : MonoBehaviour
{

	public Image normalShot;
	public Image wideShot;
	public Image rapidShot;
	public Image chargeShot;

	public GameObject staminaBarFill;
	float currentStamina;
	float maxStamina = 1;
	float cooldownTimer = 0;
	float cooldownTime = 3;

	void Start ()
	{
//		normalShot.color = Color.yellow;

		currentStamina = maxStamina;
	}

	void Update ()
	{
		StaminaBar ();
		print (cooldownTimer);
	}

	void StaminaBar ()
	{
		if (Input.GetKey (KeyCode.R)) {
			if (currentStamina > 0) {
				cooldownTimer = 0;
				DecreaseStamina ();
			}
		}

		if (currentStamina != maxStamina) {
			if (cooldownTimer < cooldownTime) {
				cooldownTimer += Time.unscaledDeltaTime;
			}

			if (cooldownTimer >= cooldownTime && currentStamina < maxStamina) {
				IncreaseStamina ();
			}
		}

		staminaBarFill.transform.localScale = new Vector3 (currentStamina, staminaBarFill.transform.localScale.y, staminaBarFill.transform.localScale.z);
	}

	void DecreaseStamina ()
	{
		currentStamina -= Time.unscaledDeltaTime;
	}

	void IncreaseStamina ()
	{
		currentStamina += Time.unscaledDeltaTime;
	}
}
