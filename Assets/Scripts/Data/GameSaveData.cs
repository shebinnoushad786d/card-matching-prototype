using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GameSaveData
{
    public int layoutType;

    public int score;
    public int moves;
    public float time;

    public int[] cardIds;
    public bool[] matchedStates;
    public bool[] faceUpStates;
}
