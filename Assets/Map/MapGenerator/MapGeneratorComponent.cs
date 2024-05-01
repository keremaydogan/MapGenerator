using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GeneralHelper;
using Unity.VisualScripting;

public class MapGeneratorComponent : MonoBehaviour
{

    public TerrainGenerator terraGen;

    public PathGenerator pathGen;

    public MapDecorator mapDecorator;

    public bool generateMap;

    public bool useRandomSeed = true;

    public int seed = 0;

    public LayerMask groundLayer;

    List<OBBArea> occupiedAreas = new List<OBBArea>();

    string guiTxt;

    private void Reset()
    {
        terraGen = new TerrainGenerator();
        pathGen = new PathGenerator();
    }

    private void OnValidate()
    {
        GenerateMapController();
    }

    IEnumerator GenerateMap()
    {
        yield return new WaitForEndOfFrameUnit();

        occupiedAreas = new List<OBBArea>();

        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        Transform terrain = transform.Find("Terrain");
        Transform constructs = transform.Find("Constructs");
        Transform decorations = transform.Find("Decorations");

        if (terrain == null)
        {
            terrain = new GameObject("Terrain").transform;
            terrain.parent = transform;
        }

        if (constructs == null)
        {
            constructs = new GameObject("Constructs").transform;
            constructs.parent = transform;
        }

        if (decorations == null)
        {
            decorations = new GameObject("Decorations").transform;
            decorations.parent = transform;
        }

        while (terrain.childCount != 0)
        {
            DestroyImmediate(terrain.GetChild(0).gameObject);
        }

        while (constructs.childCount != 0)
        {
            DestroyImmediate(constructs.GetChild(0).gameObject);
        }

        while (decorations.childCount != 0)
        {
            DestroyImmediate(decorations.GetChild(0).gameObject);
        }

        terraGen.SetParentTransform(terrain);
        pathGen.SetParentTransform(constructs);
        mapDecorator.SetParentTransform(decorations);

        terraGen.GenerateTerrainInfo(seed);

        pathGen.SetTerrainInfos(terraGen.data.unitSize, terraGen.data.mapSize, terraGen.terrainZeroCorner, groundLayer);

        mapDecorator.SetTerrainInfos(terraGen.data.unitSize, terraGen.data.mapSize, terraGen.terrainZeroCorner);

        pathGen.GeneratePath(seed);

        List<(Vector2Int, Vector2Int)> positionsForSections = pathGen.GeneratePositionsForSections();

        List<float[]> roadSections = new List<float[]>();

        foreach ((Vector2Int, Vector2Int) posForSec in positionsForSections)
        {
            roadSections.Add(terraGen.GetRoadSection(posForSec.Item1, posForSec.Item2, pathGen.data.roadSectionStepSize, 50));
        }

        pathGen.SetRoadSections(roadSections);

        pathGen.ApplyNodesToTerrain(ref terraGen.heightMap, ref occupiedAreas);

        pathGen.ApplyRoadsToTerrain(ref terraGen.heightMap, ref occupiedAreas);

        //terraGen.PlaceLandforms(ref occupiedAreas);

        mapDecorator.PlaceDecorationsRandomOccupy(seed, ref terraGen.heightMap, ref occupiedAreas);

        terraGen.GenerateAllChunks();

        Debug.Log("[MAP GENERATED]\nSeed: " + seed);

        // TO SEE OCCUPIED AREAS
        /*
        foreach(OBBArea area in occupiedAreas)
        {
            OBBArea.Draw(area, Color.red, 4, 500);
        }
        */
    }

    void GenerateMapController()
    {

        if (generateMap)
        {
            StartCoroutine(GenerateMap());
            generateMap = false;
        }
    }

    private void Start()
    {
        useRandomSeed = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            generateMap = true;
        }

        GenerateMapController();
    }


    void OnGUI()
    {

        guiTxt = "SEED: " + seed + "\n(Generate new map with 'G')";

        GUI.TextField(new Rect(10, 10, 200, 40), guiTxt);

    }


}


