using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GeneralHelper;
using Unity.VisualScripting;
using static UnityEngine.InputManagerEntry;

public class RoadGenerator
{

    Transform constructs;

    RoadGeneratorData data;

    int roadCount = 0;

    public void SetData(RoadGeneratorData data)
    {
        this.data = data;
    }

    public void SetParentTransform(Transform parent)
    {
        this.constructs = parent;
        
    }

    public void ResetRoadCount()
    {
        roadCount = 0;
    }

    public void BuildRoad(Vector3[] turningPoints, HashSet<int> bridgeInds, float width, ref List<OBBArea> occupiedAreas)
    {
        int roadPartCount = 0;

        GameObject roadGO = new GameObject();
        roadGO.name = "Road_" + roadCount++;

        Transform road = roadGO.transform;
        road.parent = constructs;

        Vector3 startXZ = turningPoints[0];
        Vector3 endXZ = turningPoints[turningPoints.Length - 1];
        Vector3 currXZ, nextXZ;

        startXZ.y = 0;
        endXZ.y = 0;

        Vector3 roadPartXZ = endXZ - startXZ;
        roadPartXZ.y = 0;

        float yRot = Vector3.SignedAngle(Vector3.forward, roadPartXZ, Vector3.up);

        float totalLen = roadPartXZ.magnitude;

        OBBArea obbArea = new OBBArea();

        for (int i = 0; i < turningPoints.Length - 1; i++)
        {

            if (bridgeInds.Contains(i))
            {
                continue;
            }

            currXZ = turningPoints[i];
            nextXZ = turningPoints[i + 1];

            currXZ.y = 0;
            nextXZ.y = 0;

            BuildRoadPart(
                turningPoints[i],
                turningPoints[i + 1],
                width,
                yRot,
                Vector3.Distance(currXZ, startXZ) / totalLen,
                Vector3.Distance(nextXZ, startXZ) / totalLen,
                road,
                roadPartCount++
                );

        }

        Vector3 centerV3 = (startXZ + endXZ) / 2f;
        obbArea.center = new Vector2(centerV3.x, centerV3.z);
        obbArea.halfDiagonal = new Vector2(width / 2, (endXZ - startXZ).magnitude / 2);
        obbArea.angle = -Vector3.SignedAngle(Vector3.forward, endXZ - startXZ, Vector3.up);

        occupiedAreas.Add(obbArea);

    }


    void BuildRoadPart(Vector3 start, Vector3 end, float width, float yRot, float uvZStart, float uvZEnd, Transform parent, int roadPartId = 0)
    {

        GameObject roadPart = new GameObject("RoadPart_" + roadPartId);
        roadPart.transform.SetParent(parent);
        roadPart.layer = LayerMask.NameToLayer("Ground");
        MeshFilter meshFilter = roadPart.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = roadPart.AddComponent<MeshRenderer>();
        MeshCollider meshCol = roadPart.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        
        float len = Vector3.Distance(new Vector3(start.x, 0, start.z), new Vector3(end.x, 0, end.z));

        int zLen = Mathf.RoundToInt(len / data.meshSampleSize.y);
        int xLen = Mathf.RoundToInt(width / data.meshSampleSize.x);
        Vector3 zUnit = Quaternion.AngleAxis(yRot, Vector3.up) * new Vector3(0, 0, len / zLen);
        Vector3 xUnit = Quaternion.AngleAxis(yRot, Vector3.up) * new Vector3(width / xLen, 0, 0);

        List<Vector3> vertices = new List<Vector3>();
        int[] indices;

        List<Vector2> uvs = new List<Vector2>();

        Vector3 offset;

        Vector3 zeroCorner = start + Quaternion.AngleAxis(yRot, Vector3.up) * (width / 2 * Vector3.left);
        zeroCorner.y = 0;

        for (int z = 0; z < zLen + 1; z++)
        {
            offset = z * zUnit;
            offset.y = Arith.Lerp(EaseFuncs.EaseInOutSine((float)z / zLen), start.y, end.y) + data.heightOffset;
            for (int x = 0; x < xLen + 1; x++)
            {
                vertices.Add(zeroCorner + offset);

                uvs.Add(new Vector2((float)x / xLen,
                    Arith.Scale((float)z / zLen, 0, 1, uvZStart, uvZEnd)));

                offset += xUnit;

            }
        }

        indices = new int[zLen * xLen * 6];

        int currLineOffset = 0;
        int upperLineOffset;
        int indOffset = 0;
        for (int z = 0; z < zLen; z++)
        {
            upperLineOffset = (z + 1) * (xLen + 1);
            for (int x = 0; x < xLen; x++)
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


}
