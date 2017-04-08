using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : Singleton<ExplosionManager> 
{
	public GameObject enemyExplosionPrefab;
	public GameObject enemyHitParticlesPrefab; 
	public GameObject criticalHitExplosionPrefab; 

	public void SpawnEnemyExplosion(Vector3 spawnPos)
	{
		GameObject newExplosion = GameObject.Instantiate(enemyExplosionPrefab, spawnPos, Quaternion.identity, transform); 
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
}
