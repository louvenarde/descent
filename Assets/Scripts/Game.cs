using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    public const byte PLAYER_COUNT = 1;

    public static Game i;

    void Awake()
    {
        i = this;
    }
}
