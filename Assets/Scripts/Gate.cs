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

	public delegate void OnGateTriggeredDelegate(Gate gate, PlayerController player);
	public event OnGateTriggeredDelegate OnGateTriggered;

	[SerializeField]
	private ColliderEventSource[] colliders;

	[SerializeField]
	private string segmentName;

	public string SegmentName { get { return segmentName; } }

	public EGateType GateType {get; private set;}

	private TerrainPiece.Connector connector;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < colliders.Length; i++) {
			colliders[i].OnTriggerEnterCallback += OnGateEnter;
		}
	}

	public void SetGateType(EGateType type)
	{
		GateType = type; 
		RefreshVisuals ();
	}

	public void SetGateConnector(TerrainPiece.Connector connector)
	{
		transform.position = connector.exitPoint;
		transform.forward = connector.exitDirection;
		this.connector = connector;
		RefreshVisuals ();
	}

	private void RefreshVisuals()
	{

	}

	void OnGateEnter (Collider arg1, ColliderEventSource arg2)
	{
		if (OnGateTriggered != null) {
			PlayerGateCollider playerCollider = arg1.gameObject.GetComponent<PlayerGateCollider> ();
			if (playerCollider) {
				if (playerCollider.GetController ()) {
					if (OnGateTriggered != null) {
						OnGateTriggered.Invoke (this, playerCollider.GetController ());	
					}
				}
			}
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

		if (connector.exitWidth > 0F) {
			UnityEditor.Handles.DrawWireDisc (transform.position, transform.forward, connector.exitWidth);
		} else {
			UnityEditor.Handles.DrawWireDisc (transform.position, transform.forward, 10f);
		}
	}
	#endif
}
