using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BridgeAsset
{
    public GameObject asset;
    public float weight;
}

[CreateAssetMenu(fileName = "BridgeGeneratorData", menuName = "Map/PathGenerator/BridgeGeneratorData", order = 1)]
public class BridgeGeneratorData : ScriptableObject
{

    [Header("End Points")]
    public BridgeAsset[] endFixed;
    public BridgeAsset[] endJoint;
    public Vector3 endFixedJunctionOffset;

    [Header("Road")]
    public BridgeAsset[] roadFixed;
    public BridgeAsset[] roadJoint;

    [Header("Column")]
    public BridgeAsset[] columnFixed;
    public BridgeAsset[] columnJoint;
    public Vector3 columnFixedJunctionOffset;
    [Range(0.0001f, 100000f)]
    public float columnGapLength;

    [Header("Restrictions")]
    [Range(0f, 100000f)]
    public float minLength;

}
