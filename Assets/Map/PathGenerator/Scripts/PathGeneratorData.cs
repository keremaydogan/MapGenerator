using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct PathInfo
{
    public int azimuthSlotCount;
    public PathNode[] nodes;

}


[Serializable]
public struct PathNode
{
    int prev;
    public int Prev { get { return prev; } set { prev = value; } }
    int azimuthPos;
    public int AzimuthPos { get { return azimuthPos; } set { azimuthPos = value; } }
    int radialPos;
    public int RadialPos { get { return radialPos; } set { radialPos = value; } }

    [Header("Node")]
    public float nodeRadius;
    public float nodePadding;

    [Header("Road")]
    public float roadWidth;
    public float roadPadding;

    public GameObject construct;

    public void SetPositionalInfo(int prev, int azimuthPos, int radialPos)
    {
        this.prev = prev;
        this.azimuthPos = azimuthPos;
        this.radialPos = radialPos;
    }

}

[CreateAssetMenu(fileName = "PathGeneratorData", menuName = "Map/PathGenerator/Data", order = 1)]
public class PathGeneratorData : ScriptableObject
{

    public int azimuthSlotCount;

    public int radialSlotCount;

    public int radialOffset;

    public int pathMinHeight;

    [Header("Center Node")]
    public int centerRadius;
    public int centerPadding;

    public PathInfo[] pathInfos;

    [Header("Road Construct")]
    [Range(0.0001f, float.MaxValue)]
    public float roadSectionStepSize;
    [Range(0.0001f, float.MaxValue)]
    public float roadPartMinLen;
    public float roadIndentHeightOffset;
    public float roadIndentWidthLength;

}
