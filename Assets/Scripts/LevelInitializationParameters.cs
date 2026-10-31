using System;
using UnityEngine;

[System.Serializable]
public struct LevelInitializationParameters
{
	[System.Serializable]
	public struct Player
	{
		public RiderProfile rider;
		public BoardProfile board;
		public HumanoidProfile humanoid;
		public Texture2D characterTexture;
		public Texture2D boardTexture;
	}

	public Player[] players;

}
