using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoadGeneratorData", menuName = "Map/PathGenerator/RoadGeneratorData", order = 1)]
public class RoadGeneratorData : ScriptableObject
{

    public Material material;

    public float heightOffset;

    public Vector2 meshSampleSize = Vector2.one;

}
