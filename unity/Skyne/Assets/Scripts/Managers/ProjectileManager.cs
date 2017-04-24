using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileManager : Singleton<ProjectileManager> 
{ 
	public Vector3 testHitPoint; 

	//public GameObject bossHomingBulletPrefab; 
	//public GameObject bossBigBulletPrefab; 

	public LayerMask pShootingLayers; 

	// Player normal shot
	public void Shoot_P_Normal(GameObject spawner)
	{
		GameObject newBullet = GameObject.Instantiate(PlayerShooting.inst.pNormalBulletPrefab, spawner.transform.position, Quaternion.Euler(DefaultPBulletRot(spawner)), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>();

		RaycastTargetFound shotTarget = GetRaycastTarget(); 

		if (shotTarget.targetFound)
		{
			bullet.target = shotTarget.targetPos; 
			bullet.hasTarget = true; 
		}
	}

	public void Shoot_P_Charge(GameObject spawner)
	{
		GameObject newBullet = GameObject.Instantiate(PlayerShooting.inst.pChargeBulletPrefab, spawner.transform.position, Quaternion.Euler(DefaultPBulletRot(spawner)), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		RaycastTargetFound shotTarget = GetRaycastTarget(); 

		if (shotTarget.targetFound)
		{
			bullet.target = shotTarget.targetPos; 
			bullet.hasTarget = true; 
		}
	}

	public void Shoot_P_Wide(GameObject spawner)
	{
		for (int i = -5; i < 5; i++)
		{
			//Debug.Log("Shoot wide " + i);

			float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
			Vector3 rotOffset = new Vector3 (-vertRot + 2.5f, 0, 0); 
			Vector3 bulletRot = spawner.transform.rotation.eulerAngles + rotOffset; 

			bulletRot += new Vector3 (0, i * PlayerShooting.inst.pWideHorizSpread, 0); 

			GameObject newBullet = GameObject.Instantiate(PlayerShooting.inst.pWideBulletPrefab, spawner.transform.position, Quaternion.Euler(bulletRot), transform); 
			Bullet bullet = newBullet.GetComponent<Bullet>(); 
		}
	}

	public void Shoot_P_Rapid(GameObject spawner)
	{
		GameObject newBullet = GameObject.Instantiate(PlayerShooting.inst.pRapidBulletPrefab, spawner.transform.position, Quaternion.Euler(DefaultPBulletRot(spawner)), transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		RaycastTargetFound shotTarget = GetRaycastTarget(); 

		if (shotTarget.targetFound)
		{
			bullet.target = shotTarget.targetPos; 
			bullet.hasTarget = true; 
		}
	}

	/// <summary>
	/// Returns a default rotation for a player bullet based on the Main Camera angle
	/// </summary>
	Vector3 DefaultPBulletRot(GameObject spawner)
	{
		float vertRot = Camera.main.GetComponent<MainCameraControl>().GetVerticalAngle(); 
		Vector3 rotOffset = new Vector3(-vertRot, 0, 0); 
		return spawner.transform.rotation.eulerAngles + rotOffset; 
	}


	/*
	 * Add functions here for enemy shooting. Any enemies that need to shoot should call a function here and pass in the spawner
	 */

	public void EnemyShoot(GameObject spawner, GameObject bulletPrefab, bool lookAtTarget)
	{
		GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, spawner.transform.rotation, transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		if (lookAtTarget)
		{
			bullet.target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().position;
			bullet.targetObj = GameObject.FindGameObjectWithTag("Player").gameObject;
			bullet.hasTarget = true; 
		}
	}


	public void Shoot_BossHomingOrb(GameObject spawner, GameObject bulletPrefab)
	{
		GameObject newBullet = GameObject.Instantiate(bulletPrefab, spawner.transform.position, spawner.transform.rotation, transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		bullet.targetObj = GameObject.FindGameObjectWithTag("Player").gameObject; 
		bullet.target = bullet.targetObj.GetComponent<Transform>().position; 
		bullet.hasTarget = true; 
	}

	/*
	public void Shoot_BossBigOrb(GameObject spawner)
	{
		GameObject newBullet = GameObject.Instantiate(bossBigBulletPrefab, spawner.transform.position, spawner.transform.rotation, transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 
	}
	*/ 

	/*
	public void Shoot_Fort(GameObject spawner, bool lookAtTarget)
	{
		GameObject newBullet = GameObject.Instantiate(fortBulletPrefab, spawner.transform.position, spawner.transform.rotation, transform); 
		Bullet bullet = newBullet.GetComponent<Bullet>(); 

		if (lookAtTarget)
		{
			bullet.target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>().position; 
			bullet.hasTarget = true; 
		}
	}
	*/ 

	// Struct for getting the raycast target and determining if a target was actually found
	// This could be extended to include the actual RaycastHit object and passed into the Bullet, which would give the Bullet access to its target before hitting it
	public struct RaycastTargetFound
	{
		public bool targetFound; 
		public Vector3 targetPos; 
	}


	RaycastTargetFound GetRaycastTarget()
	{
		RaycastTargetFound result = new RaycastTargetFound();
		result.targetFound = false; 

		int crosshairX = Screen.width / 2;
		int crosshairY = Screen.height / 2;

		Ray ray = Camera.main.ScreenPointToRay(new Vector3(crosshairX, crosshairY));
		Debug.DrawRay(ray.origin, ray.direction * 100, new Color(1f,0.922f,0.016f,1f));

		RaycastHit hitInfo;

		//if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 200)) 
		if (Physics.Raycast(ray, out hitInfo, 200, pShootingLayers.value))
		{
			/*
			if(hitInfo.collider.gameObject.tag=="targetObject")
			{
				//Debug.Log('hit');
			}
			*/ 
			testHitPoint = new Vector3 (hitInfo.point.x, hitInfo.point.y, hitInfo.point.z); 

			result.targetFound  = true; 
			result.targetPos = hitInfo.point; 
		}

		return result;
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawSphere(testHitPoint, 0.2f); 
	}
}
