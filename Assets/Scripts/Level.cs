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

	[SerializeField]
	private Transform[] playerSpawns;

	private List<Gate> activeGates = new List<Gate>();

	// Use this for initialization
	void Start () {
		InitializeGates ();

		Game.i.RegisterLevel (this);
	}

	public void RegisterPlayer(byte index)
	{
		var player = Game.i.GetPlayer (index);
		if (player) {
			Vector3 spawnPosition = Vector3.zero;
			Quaternion spawnRotation = Quaternion.identity;

			if (playerSpawns != null && playerSpawns.Length > 0) {
				Transform spawnPoint = playerSpawns [index % playerSpawns.Length];
				spawnPosition = spawnPoint.position;
				spawnRotation = spawnPoint.rotation;
			} else if (generator.Pieces.Count > 0){
				Vector3 direction = generator.Pieces [0].OutgoingConnector.exitPoint - generator.Pieces [0].transform.position;

				spawnPosition = generator.Pieces [0].transform.position + direction / 2f;
				spawnRotation = Quaternion.LookRotation (direction.normalized);
			}

			player.transform.position = spawnPosition;
			player.transform.rotation = spawnRotation;
		}
	}

	private void InitializeGates()
	{
		if (!startGate) {
			var inst = Gate.Instantiate (gatePrefab, this.transform);
			startGate = inst as Gate;
			startGate.SetGateConnector(generator.Pieces[0].OutgoingConnector);
		}

		startGate.name = "START GATE";
		activeGates.Add(startGate);

		if (!endGate) {
			endGate = Gate.Instantiate (gatePrefab, this.transform) as Gate;
			endGate.SetGateConnector(generator.Pieces[generator.Pieces.Count-2].OutgoingConnector);
		}

		startGate.name = "END GATE";
		activeGates.Add(endGate);

		if (splitSegmentGates == null || splitSegmentGates.Length <= 0) {
			var splitGatesList = new List<Gate> ();
			for (int i = 1; i < generator.Pieces.Count - 2; i++) {
				var gate = Gate.Instantiate (gatePrefab, this.transform) as Gate;
				gate.SetGateConnector(generator.Pieces[i].OutgoingConnector);
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

	void OnGateTriggeredCallback (Gate gate, PlayerController player)
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
