using GeneralHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;


[Flags]
public enum Regions
{
    Region0 = 1 << 0,
    Region1 = 1 << 1,
    Region2 = 1 << 2,
    Region3 = 1 << 3,
    Region4 = 1 << 4,
    Region5 = 1 << 5,
    Region6 = 1 << 6,
    Region7 = 1 << 7
}

[Serializable]
public struct PrefabInfo
{

    public GameObject prefab;
    public int tryCount;
    public int maxCount;
    public Regions regions;

}

[CreateAssetMenu(fileName = "MapDecorator", menuName = "Map/MapDecorator/Data", order = 1)]
public class MapDecoratorData : ScriptableObject
{
    public int regionCount = 8;

    public float[] regionMaxHeights;
    public PrefabInfo[] prefabInfos;

    private void Reset()
    {
        regionMaxHeights = new float[9];
        regionMaxHeights[0] = float.MaxValue;
        regionMaxHeights[regionMaxHeights.Length - 1] = float.MinValue;
    }

}
