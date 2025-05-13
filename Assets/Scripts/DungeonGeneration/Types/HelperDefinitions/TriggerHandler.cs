using UnityEngine;
using UnityEngine.Events;

public class TriggerHandler : MonoBehaviour
{
	public UnityEvent triggerEvent;

	void OnTriggerEnter2D(Collider2D other)
	{
		//Again, check if the player interacts, because otherwise anything else can activate it, causing a chain reaction
		if(other.transform.CompareTag("Player"))
			if (triggerEvent != null)
				triggerEvent.Invoke();
	}
}

