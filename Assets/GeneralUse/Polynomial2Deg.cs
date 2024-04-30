using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Polynomial2Deg
{

    private float coeff, cons;

    public float Coeff { get { return coeff; } }
    public float Cons { get { return cons; } }

    public Polynomial2Deg(Vector2 point1, Vector2 point2)
    {
        this.coeff = (point1.y - point2.y) / (point1.x - point2.x);
        this.cons = point1.y - (point1.x * coeff);
    }

    public Polynomial2Deg(float coeff, float cons)
    {
        this .coeff = coeff;
        this.cons = cons;
    }

    public Polynomial2Deg(Polynomial2Deg poly2deg)
    {
        this.coeff = poly2deg.Coeff;
        this.cons = poly2deg.Cons;
    }

    public float yByX(float x)
    {
        return x * coeff + cons;
    }


}
