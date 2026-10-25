using UnityEngine;
using System.Collections;

public class Gate : MonoBehaviour {

	public enum EGateType
	{
		Invisible,
		Split,
		Start,
		End,
	}

	public delegate void OnGateTriggeredDelegate(Gate gate, Player player);
	public event OnGateTriggeredDelegate OnGateTriggered;

	[SerializeField]
	private ColliderEventSource[] colliders;

	[SerializeField]
	private string segmentName;

	public string SegmentName { get { return segmentName; } }

	public EGateType GateType {get; private set;}

	// Use this for initialization
	void Start () {
		for (int i = 0; i < colliders.Length; i++) {
			colliders[i].OnTriggerEnterCallback += OnGateEnter;
		}
	}

	public void SetGateType(EGateType type)
	{
		GateType = type; 
		// TODO: Refresh visuals
	}

	void OnGateEnter (Collider arg1, ColliderEventSource arg2)
	{
		if (OnGateTriggered != null) {
			// For each collider in children of each player, check if it's the one?
//			OnGateTriggered?.Invoke ();	
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (GateType == EGateType.Start) {
			UnityEditor.Handles.color = Color.red;
		}

		if (GateType == EGateType.End) {
			UnityEditor.Handles.color = Color.green;
		}

		if (GateType == EGateType.Split) {
			UnityEditor.Handles.color = Color.blue;
		}

		UnityEditor.Handles.DrawWireDisc(transform.position, transform.forward, 10f);
	}
	#endif
}
