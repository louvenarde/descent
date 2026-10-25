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

    void Awake()
    {
        i = this;
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
		if (OnLevelLoaded != null) {
			OnLevelLoaded.Invoke ();
			OnLevelLoaded = null;
		}
	}
}
