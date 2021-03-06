﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/PlayerData")]
public class SO_PlayerData : ScriptableObject {
	const int itemQuantity = 13;
	public int level;
	public bool[] collectedItems = new bool[itemQuantity];	
}
