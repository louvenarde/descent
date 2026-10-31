using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    public const byte PLAYER_COUNT = 1;

    public static Game i;

	public Level Level { get; private set; }

	private event Action OnLevelLoaded;

	[SerializeField]
	private PlayerController playerPrefab;

	private PlayerController[] players = new PlayerController[PLAYER_COUNT];

    void Awake()
    {
        i = this;
		UnityEngine.Object.DontDestroyOnLoad (this);
    }

	public PlayerController GetPlayer(byte index)
	{
		return players [index];
	}

	public void OnceLevelLoaded(Action callback)
	{
		if (Level) {
			callback ();
		} else {
			OnLevelLoaded += callback;
		}
	}

	public void RegisterLevel(Level level)
	{
		Level = level;

		UnityEngine.Assertions.Assert.IsTrue (playerPrefab, "Missing player prefab in Game prefab");
		for (byte i = 0; i < players.Length; i++) {
			if (players [i] != null) {
				Destroy (players [i].gameObject);
			}

			if (FindExistingStrayPlayerController (out players [i])) {
				// Nothing to do
			} else {
				players [i] = UnityEngine.Object.Instantiate (playerPrefab, level.transform) as PlayerController;
			}

			players [i].SetPlayerIndex (i);
			Level.RegisterPlayer (i);
			UnityEngine.Assertions.Assert.IsTrue (players [i], "Could not instantiate player");
		}

		if (OnLevelLoaded != null) {
			OnLevelLoaded.Invoke ();
			OnLevelLoaded = null;
		}
	}

	private bool FindExistingStrayPlayerController(out PlayerController ctrl)
	{
		ctrl = null;
		var controllers = FindObjectsOfType<PlayerController> ();
		for (int i = 0; i < controllers.Length; i++) {
			bool controllerIsStray = true;
			for (int j = 0; j < players.Length; j++) {
				if (controllers [i] == players [j]) {
					controllerIsStray = false;
					break;
				}
			}

			if (controllerIsStray) {
				ctrl = controllers [i];
				return true;
			}
		}

		return false;
	}
}
