using UnityEngine;

namespace DungeonGeneration
{

[RequireComponent(typeof(TriggerHandler))]
public class EnemySpawner : MonoBehaviour
{
	public EnemyBehavior enemyPrefab;
	public EnemyList enemyList;
	// Weapon drops
	private CircleCollider2D trigger;
	
	void Start()
	{
		trigger = GetComponent<CircleCollider2D>();
		if (trigger == null)
		{
			Debug.LogError("trigger null");
		}
	}

	public void SwapSpawnerAndEnemy()
	{
		gameObject.SetActive(false);
		EnemyBehavior newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, gameObject.transform);
		newEnemy.enemyStats = enemyList.RandomEnemy();
		newEnemy.gameObject.transform.SetParent(gameObject.transform.parent); // This awakens the enemy
		Destroy(gameObject);
	}
};

} // namespace DungeonGeneration

