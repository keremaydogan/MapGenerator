using GeneralHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;


[Serializable]
public class MapDecorator
{

    public MapDecoratorData data;

    Transform decorations;


    #region TERRAIN INFOS
    int unitSize;
    Vector2Int mapSize;
    Vector3 terrainZeroCorner;
    #endregion

    
    public void SetParentTransform(Transform parent)
    {
        this.decorations = parent;
    }


    public void SetTerrainInfos(int unitSize, Vector2Int mapSize, Vector3 terrainZeroCorner)
    {
        this.unitSize = unitSize;
        this.mapSize = mapSize;
        this.terrainZeroCorner = terrainZeroCorner;
    }


    public void PlaceDecorationsRandomOccupy(int seed, ref float[,] heightMap, ref List<OBBArea> occupiedAreas)
    {

        int tryCount, maxCount;
        float yRot;
        Vector2Int pos = new Vector2Int();
        Vector3 worldPos;

        int offset;

        Bounds bounds;

        OBBArea area = new OBBArea();

        bool isPositionValid;

        UnityEngine.Random.InitState(seed);

        foreach(var prefabInfo in data.prefabInfos)
        {

            Transform newTransform;

            tryCount = prefabInfo.tryCount;
            maxCount = prefabInfo.maxCount;

            bounds = BoundsCalculator.CalculateTotalBounds(prefabInfo.prefab);

            area.halfDiagonal = new Vector2(bounds.extents.x, bounds.extents.z);

            offset = Mathf.CeilToInt(area.halfDiagonal.magnitude / unitSize);

            for (; tryCount > 0 && maxCount > 0; tryCount--)
            {

                pos.x = UnityEngine.Random.Range(offset, mapSize.x - offset);
                pos.y = UnityEngine.Random.Range(offset, mapSize.y - offset);
                
                yRot = UnityEngine.Random.Range(0f, 360f);

                worldPos = RealPosByHeightMapPos(pos, ref heightMap);

                area.center = new Vector2(worldPos.x, worldPos.z);
                area.angle = -yRot;

                isPositionValid = false;

                for(int i = 0; i < data.regionCount; i++)
                {
                    if((byte)prefabInfo.regions == ((byte)prefabInfo.regions | (1 << i)) && worldPos.y < data.regionMaxHeights[i] && worldPos.y > data.regionMaxHeights[i + 1])
                    {
                        isPositionValid = true;
                        break;
                    }
                }

                if (!isPositionValid)
                {
                    continue;
                }

                for(int i = 0; i < occupiedAreas.Count; i++)
                {
                    if (OBBArea.CheckCollision(area, occupiedAreas[i]))
                    {
                        isPositionValid = false;
                        break;
                    }
                }

                if(!isPositionValid)
                {
                    continue;
                }

                newTransform = GameObject.Instantiate(prefabInfo.prefab, worldPos, Quaternion.Euler(0f, yRot, 0f), decorations).transform;
                newTransform.localScale = prefabInfo.prefab.transform.localScale;
                occupiedAreas.Add(area);
                maxCount--;
            }

        }


    }


    
    public Vector3 RealPosByHeightMapPos(Vector2Int pos, ref float[,] heightMap)
    {
        return terrainZeroCorner + new Vector3(pos.x * unitSize, heightMap[pos.x, pos.y], pos.y * unitSize);
    }


}
