using UnityEngine;
using System.Collections;

public class PlayerGateCollider : MonoBehaviour {

	[SerializeField]
	private PlayerController controller;

	public PlayerController GetController(){
		return controller;
	}
}
