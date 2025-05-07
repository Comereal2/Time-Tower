using UnityEngine;
using UnityEngine.Events;

public class TriggerHandler : MonoBehaviour
{
	public UnityEvent triggerEvent;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (triggerEvent != null)
			triggerEvent.Invoke();
	}
}

