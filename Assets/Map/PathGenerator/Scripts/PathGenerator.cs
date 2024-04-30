using GeneralHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct TurningPointsInfo
{
    public Vector2[] turningPoints;
    public HashSet<int> bridgeInds;
    public TurningPointsInfo(Vector2[] turningPoints, List<int> bridgeInds)
    {
        this.turningPoints = turningPoints;
        this.bridgeInds = new HashSet<int>(bridgeInds);
    }
}


[Serializable]
public class PathGenerator
{

    Transform constructs;

    public PathGeneratorData data;

    public BridgeGeneratorData bridgeGenData;

    public RoadGeneratorData roadGenData;

    BridgeGenerator bridgeGen;

    RoadGenerator roadGen;

    int seed;

    List<PathNode> nodes;

    public List<PathNode> pathNodes => nodes;

    public float nodesMinHeight => data.pathMinHeight;

    HashSet<int> occupiedAzimuthSlots;

    float azimuthUnit, radialUnit;

    List<float[]> roadSections;

    LayerMask groundLayer;

    #region TERRAIN INFOS
    int unitSize;
    Vector2Int mapSize;
    Vector3 terrainZeroCorner;
    #endregion


    public PathGenerator()
    {
        nodes = new List<PathNode>();
        occupiedAzimuthSlots = new HashSet<int>();
    }

    public void SetParentTransform(Transform parent)
    {
        this.constructs = parent;
    }

    public void SetTerrainInfos(int unitSize, Vector2Int mapSize, Vector3 terrainZeroCorner, LayerMask groundLayer)
    {
        this.unitSize = unitSize;
        this.mapSize = mapSize;
        this.terrainZeroCorner = terrainZeroCorner;
        this.groundLayer = groundLayer;
    }

    public void GeneratePath(int seed)
    {

        this.seed = seed;
        UnityEngine.Random.InitState(seed);

        bridgeGen = new BridgeGenerator();
        roadGen = new RoadGenerator();

        bridgeGen.SetData(bridgeGenData, groundLayer, unitSize, terrainZeroCorner);
        roadGen.SetData(roadGenData);

        bridgeGen.SetParentTransform(constructs);
        roadGen.SetParentTransform(constructs);

        azimuthUnit = 360 / data.azimuthSlotCount;
        radialUnit = 1f / data.radialSlotCount;

        int firstAzimuthSlot;
        int quo, rem;
        int ind, pathStartInd;
        Dictionary<int, int> nodesPerRadial;

        nodes = new List<PathNode>();
        occupiedAzimuthSlots = new HashSet<int>();

        PathNode node = new PathNode();
        node.Prev = -1;
        node.AzimuthPos = -1;
        node.RadialPos = -1;
        node.nodeRadius = data.centerRadius;
        node.nodePadding = data.centerPadding;
        nodes.Add(node);

        for (int i = 0; i < data.pathInfos.Length; i++)
        {
            PathInfo pathInfo = data.pathInfos[i];

            pathStartInd = nodes.Count;

            nodes.AddRange(pathInfo.nodes);

            nodesPerRadial = new Dictionary<int, int>();

            do
            {
                firstAzimuthSlot = UnityEngine.Random.Range(0, data.azimuthSlotCount);
            }while(!isSlotsFree(firstAzimuthSlot, pathInfo.azimuthSlotCount));

            OccupySlots(firstAzimuthSlot, pathInfo.azimuthSlotCount);

            quo = Math.DivRem(pathInfo.nodes.Length, data.radialSlotCount - data.radialOffset, out rem);

            for (int radialPos = data.radialOffset + 1; radialPos < data.radialSlotCount; radialPos++)
            {
                nodesPerRadial.Add(radialPos, quo);
            }

            if(quo == 0)
            {
                nodesPerRadial[data.radialSlotCount - 1] = 1;
                rem--;
            }

            while (rem > 0)
            {
                do
                {
                    ind = UnityEngine.Random.Range(data.radialOffset + 1, data.radialSlotCount);
                } while (nodesPerRadial[ind] != quo);
                nodesPerRadial[ind] = nodesPerRadial[ind] + 1;
                rem--;
            }

            GeneretePositionsForPath(firstAzimuthSlot, pathInfo.azimuthSlotCount, nodesPerRadial, pathStartInd);

        }

    }

    bool isSlotsFree(int firstSlot, int count)
    {

        for(int i = 0; i < count; i++)
        {
            if (occupiedAzimuthSlots.Contains((firstSlot + i) % data.azimuthSlotCount))
            {
                return false;
            }
        }

        return true;
    }

    void OccupySlots(int firstSlot, int count)
    {

        for (int i = 0; i < count; i++)
        {
            occupiedAzimuthSlots.Add((firstSlot + i) % data.azimuthSlotCount);
        }
    }

    void GeneretePositionsForPath(int firstAzimuthSlot, int azimuthSlotCount, Dictionary<int, int> nodesPerRadial, int pathInd)
    {

        bool isToRight;

        int offset;
        PathNode node;

        int azimuthPos, prevAzimuthPos, prevNodeInd;

        prevNodeInd = 0;

        for (int radialPos = data.radialOffset + 1; radialPos < data.radialSlotCount; radialPos++)
        {
            isToRight = UnityEngine.Random.Range(0, 2) == 1;

            if(isToRight) { prevAzimuthPos = firstAzimuthSlot - 1; } else { prevAzimuthPos = firstAzimuthSlot + azimuthSlotCount; }

            while (nodesPerRadial[radialPos] > 0)
            {
                offset = nodesPerRadial[radialPos] - 1;

                if (isToRight)
                {
                    azimuthPos = UnityEngine.Random.Range(prevAzimuthPos + 1, firstAzimuthSlot + azimuthSlotCount - offset) % data.azimuthSlotCount;
                }
                else
                {
                    azimuthPos = UnityEngine.Random.Range(firstAzimuthSlot + offset, prevAzimuthPos) % data.azimuthSlotCount;
                }

                node = nodes[pathInd];
                node.SetPositionalInfo(prevNodeInd, azimuthPos, radialPos);
                nodes[pathInd] = node;
                
                prevNodeInd = pathInd;
                pathInd++;
    
                nodesPerRadial[radialPos]--;
            }

        }

    }

    public Vector2Int GetNodeHeightMapPos(int nodeInd) 
    {
        return GetNodeHeightMapPos(nodes[nodeInd]);

    }

    public Vector2Int GetNodeHeightMapPos(PathNode pathNode)
    {

        if(pathNode.RadialPos == -1)
        {
            return mapSize / 2;
        }

        Vector3 pos = radialUnit * (pathNode.RadialPos + 0.5f) * (Quaternion.AngleAxis(azimuthUnit * pathNode.AzimuthPos, Vector3.up) * Vector3.forward);

        return new Vector2Int((int)((mapSize.x / 2) + pos.x * (mapSize.x / 2)), (int)((mapSize.y / 2) + pos.z * (mapSize.y / 2)));

    }


    #region PATH

    void PlaceConstructOnPath(GameObject construct, Vector2Int nodeCenter, Vector2Int prevNodeCenter, ref float[,] heightMap)
    {

        if (construct == null) { return; }

        Vector3 pos = new Vector3((nodeCenter.x - mapSize.x / 2f) * unitSize,
            heightMap[nodeCenter.x, nodeCenter.y],
            (nodeCenter.y - mapSize.y / 2f) * unitSize);

        Vector3 posForPrev = new Vector3((prevNodeCenter.x - mapSize.x / 2f) * unitSize,
            heightMap[nodeCenter.x, nodeCenter.y],
            (prevNodeCenter.y - mapSize.y / 2f) * unitSize);

        Quaternion rotation = Quaternion.identity;

        rotation.SetLookRotation(posForPrev - pos, Vector3.up);

        UnityEngine.Object.Instantiate(construct, pos, rotation, constructs);

    }

    void MakeFlatCircle(Vector2Int center, float radius, float padding, ref float[,] heightMap)
    {
        float flatHeight = heightMap[center.x, center.y];

        if (flatHeight < nodesMinHeight)
        {
            flatHeight = nodesMinHeight;
        }

        int yLen;

        float dist;

        for (int x = -(int)(radius + padding); x < (radius + padding) + 1; x++)
        {

            yLen = (int)((radius + padding) * Mathf.Sin(Mathf.Acos(x / (radius + padding))));

            for (int y = -yLen; y < yLen; y++)
            {

                dist = Mathf.Sqrt(x * x + y * y);

                if (dist < radius)
                {
                    heightMap[center.x + x, center.y + y] = flatHeight;
                }
                else
                {
                    heightMap[center.x + x, center.y + y] = Arith.Lerp(
                        EaseFuncs.EaseInOutSine((dist - radius) / padding),
                        flatHeight,
                        heightMap[center.x + x, center.y + y]);
                }


            }
        }

    }

    float[] BalanceRoadHeights(float[] heights, Vector2[] positions, float maxDegree)
    {

        float dif, maxDif;

        for (int i = 1; i < heights.Length - 1; i++)
        {
            dif = heights[i] - heights[i - 1];

            maxDif = Mathf.Tan(Mathf.Deg2Rad * maxDegree) * Vector2.Distance(positions[i], positions[i - 1]);

            if (Mathf.Abs(dif) > maxDif)
            {
                heights[i] = heights[i - 1] + Mathf.Sign(dif) * maxDif;
            }

        }

        for (int i = heights.Length - 2; i > 0; i--)
        {
            dif = heights[i] - heights[i + 1];

            maxDif = Mathf.Tan(Mathf.Deg2Rad * maxDegree) * Vector2.Distance(positions[i], positions[i + 1]);

            if (Mathf.Abs(dif) > maxDif)
            {
                heights[i] = heights[i + 1] + Mathf.Sign(dif) * maxDif;
            }

            if (heights[i] < data.pathMinHeight)
            {
                heights[i] = data.pathMinHeight;
            }

        }

        return heights;

    }

    


    TurningPointsInfo GetTurningPointsForSection(Vector2Int startPos, Vector2Int endPos, int nodeInd, float stepSize)
    {

        List<Vector2> turningPoints = new List<Vector2>();

        List<int> bridgeInds = new List<int>();

        Vector2 startToEnd = endPos - startPos;

        Ray2D startToEndRay = new Ray2D(startPos, startToEnd);

        float[] sectionHeights = roadSections[nodeInd - 1];

        int bridgeFromInd = -1;

        Vector2Int currPos = startPos;

        turningPoints.Add(currPos);

        int headlandOffset = Mathf.CeilToInt(BoundsCalculator.CalculateTotalBounds(bridgeGenData.endFixed[0].asset).size.z / (stepSize * unitSize));

        float realRoadPartMinLen = Mathf.Min(data.roadPartMinLen, (endPos - startPos).magnitude);

        if (sectionHeights[1] < data.pathMinHeight)
        {
            turningPoints.Add(Vector2Int.FloorToInt(startToEndRay.GetPoint(headlandOffset * stepSize)));
            bridgeInds.Add(1);
            bridgeFromInd = 1;

        }

        for (int i = 1; i < sectionHeights.Length - 1; i++)
        {

            currPos = Vector2Int.FloorToInt(startToEndRay.GetPoint(i * stepSize));

            if (bridgeFromInd != -1)
            {
                if (sectionHeights[i] > data.pathMinHeight)
                {
                    bridgeFromInd = -1;
                    turningPoints.Add(Vector2Int.FloorToInt(startToEndRay.GetPoint((i - headlandOffset) * stepSize)));
                    turningPoints.Add(currPos);
                }
            }
            else
            {
                if (sectionHeights[i + 1] < data.pathMinHeight)
                {
                    turningPoints.Add(currPos);

                    bridgeInds.Add(turningPoints.Count);
                    bridgeFromInd = turningPoints.Count;
                    turningPoints.Add(Vector2Int.FloorToInt(startToEndRay.GetPoint((i + headlandOffset) * stepSize)));

                }
                else if (Vector2.Distance(turningPoints[turningPoints.Count - 1], currPos) >= realRoadPartMinLen &&
                    ((sectionHeights[i] > sectionHeights[i - 1] && sectionHeights[i] > sectionHeights[i + 1]) ||
                    (sectionHeights[i] < sectionHeights[i - 1] && sectionHeights[i] < sectionHeights[i + 1]))
                    )
                {
                    turningPoints.Add(currPos);
                }
            }

        }

        if (bridgeFromInd != -1)
        {
            turningPoints.Add(Vector2Int.FloorToInt(startToEndRay.GetPoint((sectionHeights.Length - 1 - headlandOffset) * stepSize)));
        }

        if (!turningPoints[turningPoints.Count - 1].Equals(endPos))
        {
            turningPoints.Add(endPos);
            //Debug.Log(turningPoints[turningPoints.Count - 1] + " == " + turningPoints[turningPoints.Count - 2]  + " : WTHHHHHHHHHHh");
        }

        return new TurningPointsInfo(turningPoints.ToArray(), bridgeInds);

    }


    public (Vector2Int, Vector2Int) GeneratePositionsForSection(Vector2Int startNodeCenter, Vector2Int endNodeCenter, float startNodeRadius = 0, float endNodeRadius = 0)
    {

        Vector2Int startPos, endPos;

        Vector2Int road = endNodeCenter - startNodeCenter;

        Ray2D roadRay = new Ray2D(startNodeCenter, road);

        startPos = new Vector2Int(startNodeCenter.x + (int)(roadRay.direction * startNodeRadius).x, startNodeCenter.y + (int)(roadRay.direction * startNodeRadius).y);
        endPos = new Vector2Int(endNodeCenter.x - (int)(roadRay.direction * endNodeRadius).x, endNodeCenter.y - (int)(roadRay.direction * endNodeRadius).y);

        return (startPos, endPos);

    }


    void BuildRoads(TurningPointsInfo turningPointsInfo, float[] heights, float width, ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {
        Vector3[] turningPointsWorldPositions = new Vector3[turningPointsInfo.turningPoints.Length];

        for(int i = 0; i < turningPointsWorldPositions.Length; i++)
        {
            turningPointsWorldPositions[i] = RealPosByHeightMapPos(Vector2Int.RoundToInt(turningPointsInfo.turningPoints[i]), ref heightMap);
            turningPointsWorldPositions[i].y = heights[i];
        }

        roadGen.BuildRoad(turningPointsWorldPositions, turningPointsInfo.bridgeInds, width, ref occupiedAreas);
    }


    void ApplyRoadGroundwork(Vector2Int startNodeCenter, Vector2Int endNodeCenter, float halfWidth, float padding, ref float[,] heightMap, ref List<OBBArea> occupiedAreas, int nodeInd, float startNodeRadius = 0, float endNodeRadius = 0)
    {
        Vector2Int startPos, endPos;

        Vector2Int road = endNodeCenter - startNodeCenter;

        Ray2D roadRay = new Ray2D(startNodeCenter, road);

        Vector2 roadNormal = (halfWidth + padding) * new Vector2(road.y, -road.x).normalized;
        Vector2Int roadNormalAbs = new Vector2Int((int)Mathf.Abs(roadNormal.x), (int)Mathf.Abs(roadNormal.y));

        Vector2 pos;

        float distanceToCenter, height;

        int xHigh, xLow, yHigh, yLow;

        HashSet<(Vector3, Vector3)> bridgeInfos = new HashSet<(Vector3, Vector3)>();
        HashSet<int> bridgeInfosAdded = new HashSet<int>();

        startPos = new Vector2Int(startNodeCenter.x + (int)(roadRay.direction * startNodeRadius).x, startNodeCenter.y + (int)(roadRay.direction * startNodeRadius).y);
        endPos = new Vector2Int(endNodeCenter.x - (int)(roadRay.direction * endNodeRadius).x, endNodeCenter.y - (int)(roadRay.direction * endNodeRadius).y);

        xHigh = Math.Max(startPos.x, endPos.x) + roadNormalAbs.x;
        xLow = Math.Min(startPos.x, endPos.x) - roadNormalAbs.x;

        yHigh = Math.Max(startPos.y, endPos.y) + roadNormalAbs.y;
        yLow = Math.Min(startPos.y, endPos.y) - roadNormalAbs.y;

        Vector3 startToPos;
        Vector3 startToEnd = new Vector3(endPos.x - startPos.x, endPos.y - startPos.y);
        float degree = Vector3.SignedAngle(startToEnd, Vector3.right, Vector3.forward);

        TurningPointsInfo turningPointsInfo = GetTurningPointsForSection(startPos, endPos, nodeInd, data.roadSectionStepSize);

        float[] heights = new float[turningPointsInfo.turningPoints.Length];

        for (int i = 0; i < turningPointsInfo.turningPoints.Length; i++)
        {
            heights[i] = heightMap[Mathf.RoundToInt(turningPointsInfo.turningPoints[i].x), Mathf.RoundToInt(turningPointsInfo.turningPoints[i].y)];
        }

        heights = BalanceRoadHeights(heights, turningPointsInfo.turningPoints, 40);

        int ind = 0;

        Vector3 startToPosSection;

        Vector3 bridgeStartPos, bridgeEndPos;

        for (int x = xLow; x < xHigh + 1; x++)
        {
            for (int y = yLow; y < yHigh + 1; y++)
            {

                pos = new Vector2(x, y);

                if (Vector2.Distance(startNodeCenter, pos) < startNodeRadius || Vector2.Distance(endNodeCenter, pos) < endNodeRadius)
                {
                    continue;
                }

                startToPos = pos - startPos;

                startToPos = Quaternion.AngleAxis(degree, Vector3.forward) * startToPos;

                distanceToCenter = Mathf.Abs(startToPos.y);

                if (startToPos.x < 0 || startToPos.x > startToEnd.magnitude)
                {
                    continue;
                }

                for (int i = 1; i < turningPointsInfo.turningPoints.Length; i++)
                {
                    if (startToPos.x < (turningPointsInfo.turningPoints[i] - startPos).magnitude)
                    {
                        ind = i - 1;
                        break;
                    }
                }

                if (bridgeInfosAdded.Contains(ind))
                {
                    continue;
                }

                if (turningPointsInfo.bridgeInds.Contains(ind))
                {
                    bridgeStartPos = RealPosByHeightMapPos(Vector2Int.RoundToInt(turningPointsInfo.turningPoints[ind]), ref heightMap);
                    bridgeStartPos.y = heights[ind];
                    bridgeEndPos = RealPosByHeightMapPos(Vector2Int.RoundToInt(turningPointsInfo.turningPoints[ind + 1]), ref heightMap);
                    bridgeEndPos.y = heights[ind + 1];

                    if (Vector3.Distance(bridgeStartPos, bridgeEndPos) < bridgeGenData.minLength)
                    {
                        turningPointsInfo.bridgeInds.Remove(ind);
                    }
                    else
                    {
                        bridgeInfos.Add((bridgeStartPos, bridgeEndPos));
                        bridgeInfosAdded.Add(ind);
                        continue;
                    }

                }

                startToPosSection = pos - turningPointsInfo.turningPoints[ind];

                startToPosSection = Quaternion.AngleAxis(degree, Vector3.forward) * startToPosSection;

                if (distanceToCenter < halfWidth + padding)
                {

                    height = Arith.Lerp(
                        EaseFuncs.EaseInOutSine(Mathf.Clamp01(startToPosSection.x / Vector2.Distance(turningPointsInfo.turningPoints[ind], turningPointsInfo.turningPoints[ind + 1]))),
                        heights[ind],
                        heights[ind + 1]
                        );

                    if (distanceToCenter < halfWidth)
                    {
                        
                        if(halfWidth - distanceToCenter < data.roadIndentWidthLength)
                        {
                            heightMap[x, y] = Arith.Lerp(
                                EaseFuncs.EaseInQubic((halfWidth - distanceToCenter) / data.roadIndentWidthLength),
                                height,
                                height + data.roadIndentHeightOffset
                                );
                        }
                        else
                        {
                            heightMap[x, y] = height + data.roadIndentHeightOffset;
                        }

                        continue;
                    }

                    heightMap[x, y] = Arith.Lerp(
                        EaseFuncs.EaseInOutSine(Mathf.Clamp01((distanceToCenter - halfWidth) / padding)),
                        height,
                        heightMap[x, y]
                        );

                }

            }

        }

        BuildRoads(turningPointsInfo, heights, halfWidth * 2f * unitSize, ref heightMap, ref occupiedAreas);

        bridgeGen.BuiltAllBridges(new List<(Vector3, Vector3)>(bridgeInfos), ref heightMap, ref occupiedAreas);

    }


    public void ApplyNodesToTerrain(ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {

        PathNode pathNode;

        Vector3 centerV3;

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            pathNode = nodes[i];

            MakeFlatCircle(GetNodeHeightMapPos(pathNode), pathNode.nodeRadius, pathNode.nodePadding, ref heightMap);
            if (i == 0) { break; }
            PlaceConstructOnPath(pathNode.construct, GetNodeHeightMapPos(pathNode), GetNodeHeightMapPos(pathNode.Prev), ref heightMap);

            centerV3 = RealPosByHeightMapPos(GetNodeHeightMapPos(pathNode), ref heightMap);

            occupiedAreas.Add(new OBBArea(new Vector3(centerV3.x, centerV3.z), Vector2.one * unitSize * pathNode.nodeRadius, 0));

        }

    }

    public List<(Vector2Int, Vector2Int)> GeneratePositionsForSections()
    {
        List<(Vector2Int, Vector2Int)> positionsForSections = new List<(Vector2Int, Vector2Int)>();


        PathNode pathNode;
        for (int i = 1; i < nodes.Count; i++)
        {
            pathNode = nodes[i];

            positionsForSections.Add(GeneratePositionsForSection(GetNodeHeightMapPos(pathNode), GetNodeHeightMapPos(pathNode.Prev), pathNode.nodeRadius, nodes[pathNode.Prev].nodeRadius));

        }

        return positionsForSections;
    }


    public void ApplyRoadsToTerrain(ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {
        PathNode pathNode;

        for (int i = nodes.Count - 1; i > 0; i--)
        {
            pathNode = nodes[i];
            ApplyRoadGroundwork(GetNodeHeightMapPos(pathNode), GetNodeHeightMapPos(pathNode.Prev), pathNode.roadWidth / 2, pathNode.roadPadding, ref heightMap, ref occupiedAreas, i, pathNode.nodeRadius, nodes[pathNode.Prev].nodeRadius);

        }
            
    }



    #endregion


    public Vector3 RealPosByHeightMapPos(Vector2Int pos, ref float[,] heightMap)
    {
        return terrainZeroCorner + new Vector3(pos.x * unitSize, heightMap[pos.x, pos.y], pos.y * unitSize);
    }

    public Vector2Int HeightMapPosByWorldPos(Vector3 pos)
    {
        pos = pos - terrainZeroCorner;
        return new Vector2Int((int)(pos.x / unitSize), (int)(pos.z / unitSize));
    }


    public void SetRoadSections(List<float[]> roadSections)
    {
        this.roadSections = roadSections;
    }


}
