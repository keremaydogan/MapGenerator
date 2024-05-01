using System;
using System.Collections.Generic;
using UnityEngine;

using GeneralHelper;

public struct ChunkGenerationInfo
{
    public string goName;
    public int groundSizeX;
    public int groundSizeZ;
    public Vector3 zeroCorner;
    public Vector2Int heightMapOffset;

    public ChunkGenerationInfo(String goName, int groundSizeX, int groundSizeZ, Vector3 zeroCorner, Vector2Int heightMapOffset)
    {
        this.goName = goName;
        this.groundSizeX = groundSizeX;
        this.groundSizeZ = groundSizeZ;
        this.zeroCorner = zeroCorner;
        this.heightMapOffset = heightMapOffset;
    }

}


[Serializable]
public class TerrainGenerator
{
    const float perlinMaxValue = 0.7071067812f;

    const int chunkMaxSize = 250;

    public TerrainGeneratorData data;

    //public PremadeLandformData premadeLandformData;

    int seed;

    Transform terrain;

    List<Vector2[,]> gradArrs;

    public float[,] heightMap;

    ChunkGenerationInfo[,] chunkGenerationInfos;

    [HideInInspector]
    public Vector3 terrainZeroCorner;

    public void SetParentTransform(Transform parent)
    {
        this.terrain = parent;
    }

    public void GenerateTerrainInfo(int seed = 0)
    {
        this.seed = seed;

        heightMap = new float[data.mapSize.x, data.mapSize.y];
        gradArrs = new List<Vector2[,]>();

        GenerateChunkInfos();

        GenerateGradArr();

        GenerateBasicHeights();

    }

    void GenerateChunkInfos()
    {

        bool isFitX = data.mapSize.x % chunkMaxSize == 0;
        bool isFitZ = data.mapSize.y % chunkMaxSize == 0;
        int chunkGridLenX = data.mapSize.x / chunkMaxSize;
        int chunkGridLenZ = data.mapSize.y / chunkMaxSize;
        if (!isFitX) { chunkGridLenX++; }
        if (!isFitZ) { chunkGridLenZ++; }

        chunkGenerationInfos = new ChunkGenerationInfo[chunkGridLenX, chunkGridLenZ];

        int groundSizeX = 0, groundSizeZ = 0;

        terrainZeroCorner = new Vector3(-1 * (data.mapSize.x - 3f) * data.unitSize / 2, 0, -1 * (data.mapSize.y - 3f) * data.unitSize / 2);

        Vector3 groundOffset = new Vector3();

        Vector2Int heightMapOffset = new Vector2Int(0, 0);

        String goName;

        for (int x = 0; x < chunkGridLenX; x++)
        {
            heightMapOffset.y = 0;
            for (int z = 0; z < chunkGridLenZ; z++)
            {
                goName = "Chunk_" + x + "-" + z;

                groundOffset.x = (heightMapOffset.x - 1) * data.unitSize;
                groundOffset.z = (heightMapOffset.y - 1) * data.unitSize;


                if (x == chunkGridLenX - 1 && !isFitX)
                {
                    groundSizeX = data.mapSize.x % chunkMaxSize;
                }
                else
                {
                    groundSizeX = chunkMaxSize;
                }

                if (x != 0)
                {
                    groundSizeX++;
                }


                if (z == chunkGridLenZ - 1 && !isFitZ)
                {
                    groundSizeZ = data.mapSize.y % chunkMaxSize;
                }
                else
                {
                    groundSizeZ = chunkMaxSize;
                }

                if (z != 0)
                {
                    groundSizeZ++;
                }

                chunkGenerationInfos[x, z] = new ChunkGenerationInfo(goName, groundSizeX, groundSizeZ, terrainZeroCorner + groundOffset, heightMapOffset);

                heightMapOffset.y += groundSizeZ - 1;
            }
            heightMapOffset.x += groundSizeX - 1;
        }


    }

    void GenerateGradArr()
    {

        Vector2[,] gradArr;
        int cellSize;
        for (int i = 0; i < data.perlinValues.Length; i++)
        {
            cellSize = data.perlinValues[i].cellSize;
            if (cellSize > data.mapSize.x)
            {
                cellSize = data.mapSize.x;
            }
            if (cellSize > data.mapSize.y)
            {
                cellSize = data.mapSize.y;
            }

            gradArr = new Vector2[data.mapSize.x / cellSize + 2, data.mapSize.y / cellSize + 2];

            for (int j = 0; j < gradArr.GetLength(0); j++)
            {
                UnityEngine.Random.InitState(seed - i * 10007 + j * 17);
                for (int k = 0; k < gradArr.GetLength(1); k++)
                {
                    gradArr[j, k] = UnityEngine.Random.insideUnitCircle;
                }
            }

            gradArrs.Add(gradArr);

        }

    }

    private void GenerateBasicHeights()
    {

        float perlin;
        int localX, localZ, cellIndX, cellIndZ;

        float[] dotProds = new float[4];

        Vector2 normalPos = new Vector2();

        PerlinValues perlinVals;

        for (int i = 0; i < data.perlinValues.Length; i++)
        {

            perlinVals = data.perlinValues[i];

            for (int x = 0; x < data.mapSize.x; x++)
            {
                for (int z = 0; z < data.mapSize.y; z++)
                {

                    cellIndX = Math.DivRem(x, perlinVals.cellSize, out localX);
                    cellIndZ = Math.DivRem(z, perlinVals.cellSize, out localZ);

                    normalPos.x = (float)localX / perlinVals.cellSize;
                    normalPos.y = (float)localZ / perlinVals.cellSize;

                    dotProds[0] = Vector2.Dot(gradArrs[i][cellIndX, cellIndZ], normalPos);
                    dotProds[1] = Vector2.Dot(gradArrs[i][cellIndX + 1, cellIndZ], normalPos - Vector2.right);
                    dotProds[2] = Vector2.Dot(gradArrs[i][cellIndX, cellIndZ + 1], normalPos - Vector2.up);
                    dotProds[3] = Vector2.Dot(gradArrs[i][cellIndX + 1, cellIndZ + 1], normalPos - Vector2.one);

                    normalPos.x = perlinVals.fade.Evaluate(normalPos.x);
                    normalPos.y = perlinVals.fade.Evaluate(normalPos.y);

                    perlin = Arith.Scale(
                        Arith.Lerp(normalPos.y,
                        Arith.Lerp(normalPos.x, dotProds[0], dotProds[1]),
                        Arith.Lerp(normalPos.x, dotProds[2], dotProds[3])),
                        -perlinMaxValue, perlinMaxValue, -0.5f, 0.5f);

                    float val = perlinVals.coeff * perlin;
                    heightMap[x, z] += val;
                }
            }
        }

        for (int x = 0; x < data.mapSize.x; x++)
        {
            for (int z = 0; z < data.mapSize.y; z++)
            {
                heightMap[x, z] += data.heightOffset;
            }
        }

    }


    public float[] GetRoadSection(Vector2Int startPos, Vector2Int endPos, float stepSize, int coeffThreshold = 0)
    {

        float perlin;
        int localX, localZ, cellIndX, cellIndZ;

        float step;

        float height;

        float[] dotProds = new float[4];

        Vector2 normalPos = new Vector2();

        PerlinValues perlinVals;

        Vector2 startToEnd = endPos - startPos;
        Vector2 pos;

        Ray2D startToEndRay = new Ray2D(startPos, startToEnd);

        List<float> heights = new List<float>() { heightMap[startPos.x, startPos.y] } ;

        for(step = stepSize; step < startToEnd.magnitude; step += stepSize)
        {

            pos = Vector2Int.RoundToInt(startToEndRay.GetPoint(step));

            height = 0;

            for (int j = 0; j < data.perlinValues.Length; j++)
            {
                perlinVals = data.perlinValues[j];

                if (perlinVals.coeff < coeffThreshold)
                {
                    continue;
                }

                cellIndX = Math.DivRem((int)pos.x, perlinVals.cellSize, out localX);
                cellIndZ = Math.DivRem((int)pos.y, perlinVals.cellSize, out localZ);

                normalPos.x = (float)localX / perlinVals.cellSize;
                normalPos.y = (float)localZ / perlinVals.cellSize;

                dotProds[0] = Vector2.Dot(gradArrs[j][cellIndX, cellIndZ], normalPos);
                dotProds[1] = Vector2.Dot(gradArrs[j][cellIndX + 1, cellIndZ], normalPos - Vector2.right);
                dotProds[2] = Vector2.Dot(gradArrs[j][cellIndX, cellIndZ + 1], normalPos - Vector2.up);
                dotProds[3] = Vector2.Dot(gradArrs[j][cellIndX + 1, cellIndZ + 1], normalPos - Vector2.one);

                normalPos.x = perlinVals.fade.Evaluate(normalPos.x);
                normalPos.y = perlinVals.fade.Evaluate(normalPos.y);

                perlin = Arith.Scale(
                   Arith.Lerp(normalPos.y,
                    Arith.Lerp(normalPos.x, dotProds[0], dotProds[1]),
                    Arith.Lerp(normalPos.x, dotProds[2], dotProds[3])),
                    -perlinMaxValue, perlinMaxValue, -0.5f, 0.5f);

                float val = perlinVals.coeff * perlin;
                height += val;

            }

            heights.Add(height + data.heightOffset);
        }

        heights.Add(heightMap[endPos.x, endPos.y]);

        return heights.ToArray();

    }


    void GenerateChunk(ChunkGenerationInfo chunkGenInfo)
    {
        GameObject ground = new GameObject(chunkGenInfo.goName);
        ground.transform.SetParent(terrain);
        ground.layer = LayerMask.NameToLayer("Ground");
        MeshFilter meshFilter = ground.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = ground.AddComponent<MeshRenderer>();
        MeshCollider meshCol = ground.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        int[] indices;

        Vector3 offset = new Vector3();

        List<Vector2> uvs = new List<Vector2>();


        for (int z = 0; z < chunkGenInfo.groundSizeZ; z++)
        {
            offset.z = z * data.unitSize;
            for (int x = 0; x < chunkGenInfo.groundSizeX; x++)
            {
                offset.x = x * data.unitSize;
                vertices.Add(chunkGenInfo.zeroCorner + offset + Vector3.up * heightMap[chunkGenInfo.heightMapOffset.x + x, chunkGenInfo.heightMapOffset.y + z]);
                uvs.Add(new Vector2((chunkGenInfo.heightMapOffset.x + (float)x) / data.mapSize.x, (chunkGenInfo.heightMapOffset.y + (float)z) / data.mapSize.y));
            }
        }

        indices = new int[(chunkGenInfo.groundSizeX - 1) * (chunkGenInfo.groundSizeZ - 1) * 6];
        int currLineOffset = 0;
        int upperLineOffset;
        int indOffset = 0;
        for (int z = 0; z < chunkGenInfo.groundSizeZ - 1; z++)
        {
            upperLineOffset = (z + 1) * chunkGenInfo.groundSizeX;
            for (int x = 0; x < chunkGenInfo.groundSizeX - 1; x++)
            {
                indices[indOffset] = currLineOffset + x;
                indices[indOffset + 1] = indices[indOffset + 4] = upperLineOffset + x;
                indices[indOffset + 2] = indices[indOffset + 3] = currLineOffset + x + 1;
                indices[indOffset + 5] = upperLineOffset + x + 1;

                indOffset = indOffset + 6;
            }
            currLineOffset = upperLineOffset;
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.SetUVs(0, uvs);

        meshRenderer.material = data.material;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        mesh.Optimize();

        meshFilter.sharedMesh = mesh;
        meshCol.sharedMesh = mesh;

    }

    public void GenerateAllChunks()
    {
        for (int x = 0; x < chunkGenerationInfos.GetLength(0); x++)
        {
            for (int z = 0; z < chunkGenerationInfos.GetLength(1); z++)
            {
                GenerateChunk(chunkGenerationInfos[x, z]);
            }
        }
    }

    /*
    #region LANDFORMS
    Vector2Int FindEmptyAreaForCircle(OBBArea area, int radius, ref List<OBBArea> occupiedAreas, int borderOffset = 0)
    {

        bool isCenterValid;

        int tryCount = 999;

        area.halfDiagonal = premadeLandformData.paddingRatio * area.halfDiagonal;

        Vector2Int center = new Vector2Int();

        Vector3 worldPos;

        do
        {
            
            center.x = UnityEngine.Random.Range(borderOffset + radius, heightMap.GetLength(0) - borderOffset - radius);
            center.y = UnityEngine.Random.Range(borderOffset + radius, heightMap.GetLength(1) - borderOffset - radius);

            isCenterValid = true;

            worldPos = WorldPosByHeightMapPos(center);

            area.center = new Vector2(worldPos.x, worldPos.z);

            foreach (OBBArea currArea in occupiedAreas)
            {
                if(OBBArea.CheckCollision(currArea, area))
                {
                    isCenterValid = false;
                    break;
                }
            }

            tryCount--;

        } while (!isCenterValid && tryCount > 0);

        if(!isCenterValid)
        {
            return -Vector2Int.one;
        }

        return center;

    }

    public void PlaceLandforms(ref List<OBBArea> occupiedAreas)
    {
        Vector3 pos;
        float radius;
        float padding;
        Vector2Int heightMapPos;
        Bounds totalBounds;

        foreach (var landform in premadeLandformData.landforms)
        {
            totalBounds = BoundsCalculator.CalculateTotalBounds(landform);

            radius = totalBounds.extents.x / data.unitSize;

            padding = radius * premadeLandformData.paddingRatio;

            OBBArea area = new OBBArea(Vector2.zero, totalBounds.extents.x * Vector2.one, 0);

            heightMapPos = FindEmptyAreaForCircle(area, Mathf.CeilToInt(radius + padding), ref occupiedAreas);

            pos = WorldPosByHeightMapPos(heightMapPos);

            area.center = new Vector2(pos.x, pos.z);

            if (heightMapPos.x == -1)
            {
                Debug.LogError("[NO SPACE FOR LANDFORM]");
                continue;
            }

            MakeFlatCircle(heightMapPos, radius, padding, heightMap[heightMapPos.x, heightMapPos.y], -10f, 10f);

            UnityEngine.Object.Instantiate(landform, pos, landform.transform.rotation, terrain);

            occupiedAreas.Add(area);

        }

    }
    #endregion
    */

    #region GENERAL
    void MakeFlatCircle(Vector2Int center, float radius, float padding, float flatHeight, float flatHeightOffset = 0, float innerPadding = 0)
    {

        int yLen;

        float dist;

        for (int x = -(int)(radius + padding); x < (radius + padding); x++)
        {

            yLen = (int)((radius + padding) * Mathf.Sin(Mathf.Acos(x / (radius + padding))));

            for (int y = -yLen; y < yLen; y++)
            {

                dist = Mathf.Sqrt(x * x + y * y);

                if (dist < radius)
                {
                    if (radius - dist < innerPadding)
                    {
                        heightMap[center.x + x, center.y + y] = Arith.Lerp(
                        EaseFuncs.EaseOutQubic(1 - ((radius - dist) / innerPadding)),
                        flatHeight + flatHeightOffset,
                        flatHeight);
                    }
                    else
                    {
                        heightMap[center.x + x, center.y + y] = flatHeight + flatHeightOffset;
                    }
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


    public Vector3 WorldPosByHeightMapPos(Vector2Int pos)
    {
        return terrainZeroCorner + new Vector3(pos.x * data.unitSize, heightMap[pos.x, pos.y], pos.y * data.unitSize);
    }

    public Vector2Int HeightMapPosByWorldPos(Vector3 pos)
    {
        pos = pos - terrainZeroCorner;
        return new Vector2Int((int)(pos.x / data.unitSize), (int)(pos.z / data.unitSize));
    }
    #endregion


}
