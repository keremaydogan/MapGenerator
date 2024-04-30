using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PremadeLandformData", menuName = "Map/TerrainGenerator/PremadeLandformData", order = 1)]
public class PremadeLandformData : ScriptableObject
{
    public GameObject[] landforms;

    public float paddingRatio;
}
