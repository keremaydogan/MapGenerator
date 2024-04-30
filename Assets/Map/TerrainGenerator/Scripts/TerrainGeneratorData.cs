using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PerlinValues
{
    public int cellSize;
    public float coeff;
    public AnimationCurve fade;

}

[CreateAssetMenu(fileName = "TerrainGeneratorData", menuName = "Map/TerrainGenerator/Data", order = 1)]
public class TerrainGeneratorData : ScriptableObject
{
    public Vector2Int mapSize;

    public int unitSize;

    public float heightOffset;

    public float pathCellSize;

    public PerlinValues[] perlinValues;

    public Material material;

}
