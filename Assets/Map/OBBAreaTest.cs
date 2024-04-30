using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBAreaTest : MonoBehaviour
{
    [Header("AREA1")]
    public Vector2 center1;
    public Vector2 halfDiagonal1;
    public float angle1;

    [Header("AREA2")]
    public Vector2 center2;
    public Vector2 halfDiagonal2;
    public float angle2;

    [Header("OTHER")]
    public float duration;

    public bool testTrigger;

    void OnValidate()
    {
        if (testTrigger)
        {
            Test();
            testTrigger = false;
        }
    }

    void Test()
    {
        
        OBBArea area1 = new OBBArea(center1, halfDiagonal1, angle1);
        OBBArea area2 = new OBBArea(center2, halfDiagonal2, angle2);

        OBBArea.Draw(area1, Color.red, duration, 0);
        OBBArea.Draw(area2, Color.blue, duration, 0);

        Debug.Log("OBBAreaTest: " + OBBArea.CheckCollisionTest(area1, area2));

    }

}
