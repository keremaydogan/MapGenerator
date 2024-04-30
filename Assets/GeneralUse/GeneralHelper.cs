using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralHelper
{
   
    public static class BoundsCalculator
    {

        public static Bounds CalculateTotalBounds(GameObject go)
        {

            MeshCollider[] meshCols = go.GetComponentsInChildren<MeshCollider>();

            if (meshCols.Length == 0)
            {
                return new Bounds();
            }

            Bounds totalBounds = meshCols[0].sharedMesh.bounds;

            for (int i = 1; i < meshCols.Length; i++)
            {
                totalBounds.Encapsulate(meshCols[i].sharedMesh.bounds);
            }
            return totalBounds;
        }

    }

    public static class EaseFuncs
    {

        public static float EaseInOutSine(float x)
        {
            return -(MathF.Cos(MathF.PI * x) - 1) / 2;
        }

        public static float EaseInOutQuart(float x)
        {
            return x < 0.5 ? 8 * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 4) / 2;
        }

        public static float EaseOutQubic(float x)
        {
            return 1 - MathF.Pow(1f - x, 3f);
        }

        public static float EaseInQubic(float x)
        {
            return x * x * x;
        }

    }

    public static class Arith
    {
        public static float Scale(float val, float oldMin, float oldMax, float newMin = 0, float newMax = 1)
        {
            return (val - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
        }


        public static float Lerp(float t, float a, float b)
        {
            return t * (b - a) + a;
        }
    }


}


