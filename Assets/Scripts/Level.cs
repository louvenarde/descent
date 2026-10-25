using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour {

	public event Gate.OnGateTriggeredDelegate OnStartGateTriggered;
	public event Gate.OnGateTriggeredDelegate OnSplitGateTriggered;
	public event Gate.OnGateTriggeredDelegate OnEndGateTriggered;
	public event Gate.OnGateTriggeredDelegate OnGateTriggered;

	[SerializeField]
	private TerrainGenerator generator;

	[SerializeField]
	private Gate gatePrefab;

	[Header("Optional")]
	[SerializeField]
	private Gate startGate;

	[SerializeField]
	private Gate endGate;

	[SerializeField]
	private Gate[] splitSegmentGates;

	private List<Gate> activeGates = new List<Gate>();

	// Use this for initialization
	void Start () {
		InitializeGates ();

		Game.i.RegisterLevel (this);
	}

	private void InitializeGates()
	{
		if (!startGate) {
			var inst = Gate.Instantiate (gatePrefab, this.transform);
			startGate = inst as Gate;
			startGate.transform.position = generator.Pieces [1].transform.position;
			startGate.transform.forward = generator.Pieces [1].transform.forward;
		}

		startGate.name = "START GATE";
		activeGates.Add(startGate);

		if (!endGate) {
			endGate = Gate.Instantiate (gatePrefab, this.transform) as Gate;
			endGate.transform.position = generator.Pieces [generator.Pieces.Count-1].transform.position;
			endGate.transform.forward = generator.Pieces [generator.Pieces.Count-1].transform.forward;
		}

		startGate.name = "END GATE";
		activeGates.Add(endGate);

		if (splitSegmentGates == null || splitSegmentGates.Length <= 0) {
			var splitGatesList = new List<Gate> ();
			for (int i = 2; i < generator.Pieces.Count - 1; i++) {
				var gate = Gate.Instantiate (gatePrefab, this.transform) as Gate;
				gate.transform.position = generator.Pieces [i].transform.position;
				gate.transform.forward = generator.Pieces [i].transform.forward;
				splitGatesList.Add (gate);
			}

			splitSegmentGates = splitGatesList.ToArray ();
		}

		startGate.SetGateType (Gate.EGateType.Start);
		endGate.SetGateType (Gate.EGateType.End);

		for (int i = 0; i < splitSegmentGates.Length; i++) {
			splitSegmentGates [i].name = string.Format("SPLIT GATE {0}", i);
			splitSegmentGates [i].SetGateType (Gate.EGateType.Split);
		}

		activeGates.AddRange (splitSegmentGates);

		RegisterGates ();
	}

	private void RegisterGates()
	{
		for (int i = 0; i < activeGates.Count; i++) {
			activeGates[i].OnGateTriggered += OnGateTriggeredCallback;
		}
	}

	void OnGateTriggeredCallback (Gate gate, Player player)
	{
		if (OnGateTriggered != null) {
			OnGateTriggered.Invoke (gate, player);
		}


		if (gate == startGate) {
			if (OnStartGateTriggered != null) {
				OnStartGateTriggered.Invoke (gate, player);
			}
		} else if (gate == endGate) {
			if (OnEndGateTriggered != null) {
				OnEndGateTriggered.Invoke (gate, player);
			}
		} else if (gate.GateType == Gate.EGateType.Split){
			if (OnSplitGateTriggered != null) {
				OnSplitGateTriggered.Invoke (gate, player);
			}
		}
	}


	#if UNITY_EDITOR
	void OnDrawGizmos()
	{

	}
	#endif
}
