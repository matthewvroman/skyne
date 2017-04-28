using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : Singleton<ExplosionManager> 
{
	public GameObject enemyExplosionPrefab;
	public GameObject fortExplosionPrefab; 
	public GameObject enemyHitParticlesPrefab; 
	public GameObject criticalHitExplosionPrefab;
	public GameObject healthPickupExplosionPrefab; 

	public void SpawnEnemyExplosion(Vector3 spawnPos)
	{
		GameObject newExplosion = GameObject.Instantiate(enemyExplosionPrefab, spawnPos, Quaternion.identity, transform); 
	}

	public void SpawnFortExplosion(Vector3 spawnPos)
	{
		GameObject newExplosion = GameObject.Instantiate(fortExplosionPrefab, spawnPos, Quaternion.identity, transform); 
	} 

	public void SpawnBulletExplosion(Vector3 spawnPos, Quaternion spawnRot, GameObject explosionPrefab)
	{
		GameObject newExplosion = GameObject.Instantiate(explosionPrefab, spawnPos, spawnRot, transform); 
	}

	public void SpawnEnemyHitParticles(Vector3 spawnPos, int numParticles)
	{
		GameObject newParticles = GameObject.Instantiate(enemyHitParticlesPrefab, spawnPos, Quaternion.identity, transform); 

		ParticleSystem particles = newParticles.GetComponent<ParticleSystem>(); 
		particles.Emit(numParticles); 
	}

	public void SpawnCriticalHitExplosion(Vector3 spawnPos, Quaternion spawnRot)
	{
		GameObject newExplosion = GameObject.Instantiate(criticalHitExplosionPrefab, spawnPos, spawnRot, transform); 
	}

	public void SpawnHealthPickupExplosion(Vector3 spawnPos)
	{
		GameObject newPickupExplosion = GameObject.Instantiate(healthPickupExplosionPrefab, spawnPos, Quaternion.identity, transform); 

		if (GameObject.FindObjectOfType<PlayerManager>().gameObject != null)
		{
			newPickupExplosion.transform.SetParent(GameObject.FindObjectOfType<PlayerManager>().transform); 
		}
	}
}
