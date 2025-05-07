using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
	public UnityEvent goalReachedEvent;

	void OnTriggerEnter2D(Collider2D other)
	{
		goalReachedEvent.Invoke();
		Destroy(gameObject);
	}
}

