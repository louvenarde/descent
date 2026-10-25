using UnityEngine;
using System.Collections;
using System;

public class ColliderEventSource : MonoBehaviour
{


	public event Action<Collision, ColliderEventSource> OnCollisionEnterCallback;
	public event Action<Collision, ColliderEventSource> OnCollisionStayCallback;
	public event Action<Collision, ColliderEventSource> OnCollisionExitCallback;
	public event Action<Collider, ColliderEventSource> OnTriggerEnterCallback;
	public event Action<Collider, ColliderEventSource> OnTriggerExitCallback;

	private void OnTriggerEnter (Collider other)
	{
		if (OnTriggerEnterCallback != null)
			OnTriggerEnterCallback.Invoke (other, this);
	}

	private void OnTriggerExit (Collider other)
	{
		if (OnTriggerExitCallback != null)
			OnTriggerExitCallback.Invoke (other, this);
	}

	private void OnCollisionEnter (Collision collision)
	{
		if (OnCollisionEnterCallback != null)
			OnCollisionEnterCallback.Invoke (collision, this);
	}

	private void OnCollisionStay (Collision collision)
	{
		if (OnCollisionStayCallback != null)
			OnCollisionStayCallback.Invoke (collision, this);
	}

	private void OnCollisionExit (Collision collision)
	{
		if (OnCollisionExitCallback != null)
			OnCollisionExitCallback.Invoke (collision, this);
	}
}