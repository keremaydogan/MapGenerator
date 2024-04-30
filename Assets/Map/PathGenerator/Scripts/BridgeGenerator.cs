using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using GeneralHelper;

public class BridgeGenerator
{

    delegate void Instantiator(BridgeAsset[] bridgeAssetPool, float[] weightCumSum, Vector3 position, Quaternion rotation, Transform parent);

    Transform constructs;

    BridgeGeneratorData data;

    int bridgeCount;

    #region TERRAIN INFOS
    int unitSize;
    Vector3 terrainZeroCorner;
    #endregion

    public void ResetBridgeCount()
    {
        bridgeCount = 0;
    }

    public void SetParentTransform(Transform parent)
    {
        this.constructs = parent;
    }

    public void SetData(BridgeGeneratorData data, LayerMask groundLayer, int unitSize, Vector3 terrainZeroCorner)
    {
        this.data = data;
        this.unitSize = unitSize;
        this.terrainZeroCorner = terrainZeroCorner;
    }
    
    Instantiator SelectInstantiators(int arrLen)
    {
        switch (arrLen)
        {
            case 0:
                return InstantiatorEmpty;
            case 1:
                return InstantiatorMono;
            default:
                return InstantiatorMulti;
        }
    }


    public void BuiltAllBridges(List<(Vector3, Vector3)> positionPairs, ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {
        foreach (var posPair in positionPairs)
        {
            BuiltBridge(posPair.Item1, posPair.Item2, ref heightMap, ref occupiedAreas);
        }
    }


    void BuiltBridge(Vector3 from, Vector3 to, ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {

        GameObject bridgeGO = new GameObject();
        bridgeGO.name = "Bridge_" + bridgeCount++;

        Transform bridge = bridgeGO.transform;
        bridge.parent = constructs;

        OBBArea obbArea = new OBBArea();

        float[] fixedCumSum, jointCumSum;
        Instantiator instantiatorFixed, instantiatorJoint;
        
        Bounds roadPartBounds = BoundsCalculator.CalculateTotalBounds(data.roadJoint[0].asset);

        float roadPartLen = 2 * roadPartBounds.extents.z;

        Vector3 dif = to - from;

        float yRot = Vector3.SignedAngle(Vector3.forward, dif, Vector3.up);
        Quaternion fixedRot = Quaternion.Euler(0, yRot, 0);

        Vector3 fromRoadJunction = from + fixedRot * data.endFixedJunctionOffset;
        Vector3 toRoadJunction = to + Quaternion.Euler(0, yRot + 180, 0) * data.endFixedJunctionOffset;

        Vector3 difRoad = toRoadJunction - fromRoadJunction;

        float roadPartCount = difRoad.magnitude / roadPartLen;

        float remainder = 1f - (roadPartCount - Mathf.Floor(roadPartCount));

        from = from - (difRoad.normalized * 0.5f * remainder * roadPartLen);
        to = to + (difRoad.normalized * 0.5f * remainder * roadPartLen);

        roadPartCount = Mathf.Ceil(roadPartCount);

        float xRot = Vector3.SignedAngle(Vector3.forward, Quaternion.Euler(0, -yRot, 0) * difRoad, Vector3.right);
        Quaternion jointRot = Quaternion.Euler(xRot, yRot, 0);

        fromRoadJunction = from + fixedRot * data.endFixedJunctionOffset;
        toRoadJunction = to + Quaternion.Euler(0, yRot + 180, 0) * data.endFixedJunctionOffset;

        // SET INSTANTIATORS FOR ENDS
        fixedCumSum = CalculateCumSum(data.endFixed);
        jointCumSum = CalculateCumSum(data.endJoint);

        instantiatorFixed = SelectInstantiators((data.endFixed == null) ? 0 : data.endFixed.Length);
        instantiatorJoint = SelectInstantiators((data.endJoint == null) ? 0 : data.endJoint.Length);

        // PUT FROM END
        instantiatorFixed(data.endFixed, fixedCumSum, from, fixedRot, bridge);
        instantiatorJoint(data.endJoint, jointCumSum, from, jointRot, bridge);

        // PUT TO END
        instantiatorFixed(data.endFixed, fixedCumSum, to, Quaternion.Euler(0, yRot + 180, 0), bridge);
        instantiatorJoint(data.endJoint, jointCumSum, to, Quaternion.Euler(xRot + 180, yRot + 180, 0), bridge);

        // SET INSTANTIATORS FOR ROAD PARTS
        fixedCumSum = CalculateCumSum(data.roadFixed);
        jointCumSum = CalculateCumSum(data.roadJoint);

        instantiatorFixed = SelectInstantiators((data.roadFixed == null) ? 0 : data.roadFixed.Length);
        instantiatorJoint = SelectInstantiators((data.roadJoint == null) ? 0 : data.roadJoint.Length);

        // PUT ROAD PARTS
        for(int i = 0; i < roadPartCount; i++)
        {
            instantiatorFixed(data.roadFixed, fixedCumSum, fromRoadJunction + (i + 0.5f) * roadPartLen * difRoad.normalized, fixedRot, bridge);
            instantiatorJoint(data.roadJoint, jointCumSum, fromRoadJunction + (i + 0.5f) * roadPartLen * difRoad.normalized, jointRot, bridge);
        }

        // SET INSTANTIATORS FOR COLUMNS
        fixedCumSum = CalculateCumSum(data.columnFixed);
        jointCumSum = CalculateCumSum(data.columnJoint);

        instantiatorFixed = SelectInstantiators((data.roadFixed == null) ? 0 : data.columnFixed.Length);
        instantiatorJoint = SelectInstantiators((data.roadJoint == null) ? 0 : data.columnJoint.Length);

        float columnGapCount = Mathf.FloorToInt(Vector3.Distance(fromRoadJunction, toRoadJunction) / data.columnGapLength);

        float columnRealGapLength = Vector3.Distance(fromRoadJunction, toRoadJunction) / columnGapCount;

        Bounds columnPartBounds = BoundsCalculator.CalculateTotalBounds(data.columnFixed[0].asset);

        float columnPartLen = columnPartBounds.size.y;

        int columnVerticalCount;

        Vector3 columnOrigin;

        Vector2Int heightMapPos;

        // PUT COLUMNS
        for (int i = 1; i < columnGapCount; i++)
        {

            columnOrigin = fromRoadJunction + i * columnRealGapLength * difRoad.normalized + data.columnFixedJunctionOffset;

            heightMapPos = HeightMapPosByWorldPos(columnOrigin);

            columnVerticalCount = Mathf.CeilToInt((columnOrigin.y - heightMap[heightMapPos.x, heightMapPos.y]) / columnPartLen);

            instantiatorJoint(data.columnJoint, jointCumSum, columnOrigin, jointRot, bridge);

            for (int j = 0; j < columnVerticalCount; j++)
            {
                instantiatorFixed(data.columnFixed, fixedCumSum, columnOrigin + j * columnPartLen * Vector3.down, fixedRot, bridge);
            }

        }

        Vector3 centerV3 = (from + to) / 2f;

        Bounds endBounds = BoundsCalculator.CalculateTotalBounds(data.endFixed[0].asset);

        obbArea.center = new Vector2(centerV3.x, centerV3.z);
        obbArea.halfDiagonal = new Vector2(endBounds.extents.x, (to - from).magnitude / 2f);
        obbArea.angle = -fixedRot.eulerAngles.y;

        occupiedAreas.Add(obbArea);

        /*THERE IS STILL SOME ROAD PART PROBLEMS IN SEED -320670972,
         * -930852843, (DOUBLE BRIDGE)
         * -721963615, (DOUBLE BRIDGE)
         * -320103126 (NO BRIDGE),
         * -930852843 (ROAD NOT ALIGNED)
         * (GOOD TO EXPERIMENT FOR START)*/

    }

    void InstantiatorEmpty(BridgeAsset[] bridgeAssetPool, float[] weightCumSum, Vector3 position, Quaternion rotation, Transform parent)
    {
        return;
    }

    void InstantiatorMono(BridgeAsset[] bridgeAssetPool, float[] weightCumSum, Vector3 position, Quaternion rotation, Transform parent)
    {
        Object.Instantiate(bridgeAssetPool[0].asset, position, rotation, parent);

    }

    void InstantiatorMulti(BridgeAsset[] bridgeAssetPool, float[] weightCumSum, Vector3 position, Quaternion rotation, Transform parent)
    {
        float random = Random.Range(0f, weightCumSum[weightCumSum.Length - 1]);
        int ind = -1;

        for (int i = 0; i < weightCumSum.Length; i++)
        {
            if (random <= weightCumSum[i])
            {
                ind = i;
                break;
            }
        }

        Object.Instantiate(bridgeAssetPool[ind].asset, position, rotation, parent);

    }


    float[] CalculateCumSum(BridgeAsset[] bridgeAssetPool)
    {
        if(bridgeAssetPool == null)
        {
            return null;
        }

        float[] cumSum = new float[bridgeAssetPool.Length];

        if(cumSum.Length == 0)
        {
            return cumSum;
        }

        cumSum[0] = bridgeAssetPool[0].weight;

        for(int i = 1; i < cumSum.Length; i++)
        {
            cumSum[i] = cumSum[i-1] + bridgeAssetPool[i].weight;
        }

        return cumSum;

    }


    public Vector2Int HeightMapPosByWorldPos(Vector3 pos)
    {
        pos = pos - terrainZeroCorner;
        return new Vector2Int((int)(pos.x / unitSize), (int)(pos.z / unitSize));
    }

}
