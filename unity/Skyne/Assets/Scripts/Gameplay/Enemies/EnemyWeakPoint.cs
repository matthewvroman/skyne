using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeakPoint : MonoBehaviour 
{
	[Tooltip("The main enemy script associated with this weak point.")]
	public Enemy enemyParent; 

	[Tooltip("The bullet damage is modified by this value to calculate the final damage value. 0.5 means half damage; 1 = normal damage; 0 = no damage")]
	public float defenseModifier; 

	[Tooltip("If this is marked as a weak point, special effects can be triggered when this collider is hit.")]
	public bool isWeakPoint; 

	/// <summary>
	/// Called by Bullet when it hits a gameObject with the enemy tag
	/// </summary>
	/// <param name="collision">Collision.</param>
	public void OnShot(Collision collision, Bullet bullet)
	{
		if (enemyParent == null)
		{
			enemyParent = this.GetComponentInParent<Enemy>(); 

			if (enemyParent == null)
			{
				Debug.LogError("Enemy weak point " + gameObject.name + " has a null enemyParent."); 
				return; 
			}
		}


		//Debug.Log("Bullet hit enemy"); 
		enemyParent.OnShot(collision, bullet, defenseModifier, isWeakPoint); 

	}
}
