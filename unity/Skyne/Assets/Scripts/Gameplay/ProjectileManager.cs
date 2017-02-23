using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileManager : Singleton<ProjectileManager> 
{
	public GameObject bulletPrefab; 

	[Space(5)]
	[Header("Player: Normal Bullet")]
	public float pNormalSpeed; 
	public float pNormalDamage; 
	public float pNormalLifetime; 

	[Space(5)]
	[Header("Player: Charge Bullet")]
	public float pChargeSpeed; 
	public float pChargeDamage; 
	public float pChargeLifetime; 

	[Space(5)]
	[Header("Player: Wide Bullet")]
	public float pWideSpeed; 
	public float pWideDamage; 
	public float pWideLifetime; 
	public float pWideHorizSpread; 

	[Space(5)]
	[Header("Player: Rapid Bullet")]
	public float pRapidSpeed; 
	public float pRapidDamage; 
	public float pRapidLifetime; 

	// Player normal shot
	public void Shoot_P_Normal(GameObject spawner)
	{
		float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
		Vector3 rotOffset = new Vector3(-vertRot, 0, 0); 
		Vector3 bulletRot = spawner.transform.rotation.eulerAngles + rotOffset; 

		GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, Quaternion.Euler(bulletRot), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		bullet.playerBullet = true; 
		bullet.speed = pNormalSpeed; //25
		bullet.damage = pNormalDamage; //4
		bullet.lifetime = pNormalLifetime; //0.9
		bullet.deltaTimePerc = 0.5f; 
	}

	public void Shoot_P_Charge(GameObject spawner)
	{
		float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
		Vector3 rotOffset = new Vector3(-vertRot, 0, 0); 
		Vector3 bulletRot = spawner.transform.rotation.eulerAngles + rotOffset; 

		GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, Quaternion.Euler(bulletRot), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		bullet.playerBullet = true; 
		bullet.speed = pChargeSpeed; //60
		bullet.damage = pChargeDamage; //15
		bullet.lifetime = pChargeLifetime; //0.9
		bullet.deltaTimePerc = 0.5f; 
		bullet.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f); 
	}

	public void Shoot_P_Wide(GameObject spawner)
	{
		for (int i = -5; i < 5; i++)
		{
			Debug.Log("Shoot wide " + i);

			float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
			Vector3 rotOffset = new Vector3 (-vertRot, 0, 0); 
			Vector3 bulletRot = spawner.transform.rotation.eulerAngles + rotOffset; 

			bulletRot += new Vector3 (0, i * pWideHorizSpread, 0); 

			GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, Quaternion.Euler(bulletRot), transform); 
			Bullet bullet = newBullet.GetComponent<Bullet>(); 

			bullet.playerBullet = true; 
			bullet.speed = pWideSpeed; //15
			bullet.damage = pWideDamage; //0.5
			bullet.lifetime = pWideLifetime; //0.3
			bullet.deltaTimePerc = 0.5f; 
			bullet.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f); 
		}
	}

	public void Shoot_P_Rapid(GameObject spawner)
	{
		float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
		Vector3 rotOffset = new Vector3(-vertRot, 0, 0); 
		Vector3 bulletRot = spawner.transform.rotation.eulerAngles + rotOffset; 

		GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, Quaternion.Euler(bulletRot), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		bullet.playerBullet = true; 
		bullet.speed = pRapidSpeed; //25
		bullet.damage = pRapidDamage; //0.5
		bullet.lifetime = pRapidLifetime; //0.9
		bullet.deltaTimePerc = 0.5f; 

		bullet.transform.localScale = new Vector3 (0.2f, 0.2f, 0.2f); 
	}

	/*
	 * Add functions here for enemy shooting. Any enemies that need to shoot should call a function here (specifying a bullet type) and passing in the spawner
	 */

	public void Shoot_E_Normal(GameObject spawner)
	{

	}
}
