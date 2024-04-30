using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.UIElements;

public struct OBBArea
{
    // pool objects for collision
    static Vector2[] dirs = new Vector2[] { -Vector2.one, new Vector2(1, -1), Vector2.one, new Vector2(-1, 1) };
    static Vector2[] area1Corners = new Vector2[4];
    static Vector2[] area2Corners = new Vector2[4];
    static Vector2[] axes = new Vector2[4];

    public Vector2 center;
    public Vector2 halfDiagonal;
    public float angle; 

    public OBBArea(Vector2 center, Vector2 halfDiagonal, float angle)
    {
        this.center = center;
        this.halfDiagonal = halfDiagonal;
        this.angle = angle;
    }

    public OBBArea(Vector2 center, float xLen, float yLen, float angle)
    {
        this.center = center;
        this.halfDiagonal = new Vector2(xLen / 2, yLen / 2);
        this.angle = angle;
    }

    public Vector2[] GetCorners()
    {
        Vector2[] corners = new Vector2[4];

        for(int  i = 0; i < 4; i++)
        {
            corners[i] = new Vector2(halfDiagonal.x * dirs[i].x, halfDiagonal.y * dirs[i].y);
            corners[i] = center + (Vector2)(Quaternion.AngleAxis(angle, Vector3.forward) * new Vector3(corners[i].x, corners[i].y));
        }
        return corners;
    }

    public static bool CheckCollision(OBBArea area1, OBBArea area2)
    {
        float a1Min, a1Max, a2Min, a2Max, a1, a2;

        bool useY;

        area1Corners = area1.GetCorners();
        area2Corners = area2.GetCorners();

        // Generate axes
        axes[0] = area1Corners[1] - area1Corners[0];
        axes[1] = area1Corners[3] - area1Corners[0];
        axes[2] = area2Corners[1] - area2Corners[0];
        axes[3] = area2Corners[3] - area2Corners[0];

        for (int i = 0; i < 4; i++)
        {

            useY = Mathf.Abs(axes[i].y) > Mathf.Abs(axes[i].x);

            if (useY) {
                a1 = PointProjectionToLine(area1Corners[0], axes[i]).y;
                a2 = PointProjectionToLine(area2Corners[0], axes[i]).y;
            }
            else {
                a1 = PointProjectionToLine(area1Corners[0], axes[i]).x;
                a2 = PointProjectionToLine(area2Corners[0], axes[i]).x;
            }

            a1Min = a1;
            a1Max = a1;
            a2Min = a2;
            a2Max = a2;

            for (int j = 1; j < 4; j++)
            {

                if (useY) {
                    a1 = PointProjectionToLine(area1Corners[j], axes[i]).y;
                    a2 = PointProjectionToLine(area2Corners[j], axes[i]).y;
                }
                else {
                    a1 = PointProjectionToLine(area1Corners[j], axes[i]).x;
                    a2 = PointProjectionToLine(area2Corners[j], axes[i]).x;
                }


                if (a1 < a1Min) { a1Min = a1; }
                else if (a1 > a1Max) { a1Max = a1; }

                if (a2 < a2Min) { a2Min = a2; }
                else if (a2 > a2Max) { a2Max = a2; }

            }

            if (!((a1Min > a2Min && a1Min < a2Max) || (a1Max > a2Min && a1Max < a2Max) || (a2Min > a1Min && a2Min < a1Max) || (a2Max > a1Min && a2Max < a1Max)))
            {
                return false;
            }

        }

        return true;

    }


    public static bool CheckCollisionTest(OBBArea area1, OBBArea area2)
    {
        float a1Min, a1Max, a2Min, a2Max, a1, a2;

        bool useY;

        area1Corners = area1.GetCorners();
        area2Corners = area2.GetCorners();

        

        // Generate axes
        axes[0] = area1Corners[1] - area1Corners[0];
        axes[1] = area1Corners[3] - area1Corners[0];
        axes[2] = area2Corners[1] - area2Corners[0];
        axes[3] = area2Corners[3] - area2Corners[0];

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(new Vector3(axes[i].x, 0, axes[i].y) * 1000, new Vector3(axes[i].x, 0, axes[i].y) * -1000, Color.red, 10);
        }
        Debug.DrawLine(new Vector3(axes[3].x, 0, axes[3].y) * 1000, new Vector3(axes[3].x, 0, axes[3].y) * -1000, Color.magenta, 10);

        

        for (int i = 0; i < 4; i++)
        {

            useY = Mathf.Abs(axes[i].y) > Mathf.Abs(axes[i].x);

            if (useY)
            {
                a1 = PointProjectionToLine(area1Corners[0], axes[i]).y;
                a2 = PointProjectionToLine(area2Corners[0], axes[i]).y;
            }
            else
            {
                a1 = PointProjectionToLine(area1Corners[0], axes[i]).x;
                a2 = PointProjectionToLine(area2Corners[0], axes[i]).x;
            }

            

            a1Min = a1;
            a1Max = a1;
            a2Min = a2;
            a2Max = a2;

            for (int j = 1; j < 4; j++)
            {

                if (useY)
                {
                    a1 = PointProjectionToLine(area1Corners[j], axes[i]).y;
                    a2 = PointProjectionToLine(area2Corners[j], axes[i]).y;
                }
                else
                {
                    a1 = PointProjectionToLine(area1Corners[j], axes[i]).x;
                    a2 = PointProjectionToLine(area2Corners[j], axes[i]).x;
                }
                

                if (a1 < a1Min) { a1Min = a1; }
                else if (a1 > a1Max) { a1Max = a1; }

                if (a2 < a2Min) { a2Min = a2; }
                else if (a2 > a2Max) { a2Max = a2; }

            }

            Debug.Log("x1: " + a1Min + " _ " + a1Max + "\n" + "x2: " + a2Min + " _ " + a2Max);

            if (!((a1Min > a2Min && a1Min < a2Max) || (a1Max > a2Min && a1Max < a2Max) || (a2Min > a1Min && a2Min < a1Max) || (a2Max > a1Min && a2Max < a1Max)))
            {
                return false;
            }

        }

        return true;

    }




    public static void Draw(OBBArea area, Color color, float duration = 10, float height = 0)
    {

        Vector2[] corners = area.GetCorners();

        for(int i = 0; i < 3; i++)
        {
            Debug.DrawLine(new Vector3(corners[i].x, height, corners[i].y), new Vector3(corners[i + 1].x, height, corners[i + 1].y), color, duration);
        }
        Debug.DrawLine(new Vector3(corners[0].x, height, corners[0].y), new Vector3(corners[3].x, height, corners[3].y), color, duration);
    }

    static Vector2 PointProjectionToLine(Vector3 point, Vector3 line)
    {
        line = line.normalized;
        return (Vector2)(point + (Quaternion.AngleAxis(-90, line) * Vector3.Cross(point, line)));

    }

    static Vector2 PointProjectionToLine(Vector2 point, Vector2 line)
    {
        return PointProjectionToLine(new Vector3(point.x, point.y), new Vector3(line.x, line.y));
    }

    

}


