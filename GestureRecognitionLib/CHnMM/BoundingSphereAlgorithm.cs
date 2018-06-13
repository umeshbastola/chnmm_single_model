
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.CHnMM
{
    public static class BoundingSphereAlgorithm
    {

        public static float SqrMagnitude(this Vector<float> v)
        {
            return v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        }
    }

    public class BoundingSphere
    {
        public Vector<float> center;
        public float radius;

        public BoundingSphere(Vector<float> aCenter, float aRadius)
        {
            center = aCenter;
            radius = aRadius;
        }

        public static BoundingSphere Calculate(IEnumerable<Vector<float>> aPoints)
        {
            Vector<float> xmin, xmax, ymin, ymax, zmin, zmax;
            xmin = ymin = zmin = Vector<float>.Build.Dense(3, float.PositiveInfinity);
            xmax = ymax = zmax = Vector<float>.Build.Dense(3, float.NegativeInfinity);
            foreach (var p in aPoints)
            {
                if (p[0] < xmin[0]) xmin = p;
                if (p[0] > xmax[0]) xmax = p;
                if (p[1] < ymin[1]) ymin = p;
                if (p[1] > ymax[1]) ymax = p;
                if (p[2] < zmin[2]) zmin = p;
                if (p[2] > zmax[2]) zmax = p;
            }
            var xSpan = (xmax - xmin).SqrMagnitude();
            var ySpan = (ymax - ymin).SqrMagnitude();
            var zSpan = (zmax - zmin).SqrMagnitude();
            var dia1 = xmin;
            var dia2 = xmax;
            var maxSpan = xSpan;
            if (ySpan > maxSpan)
            {
                maxSpan = ySpan;
                dia1 = ymin; dia2 = ymax;
            }
            if (zSpan > maxSpan)
            {
                dia1 = zmin; dia2 = zmax;
            }
            var center = (dia1 + dia2) * 0.5f;
            var sqRad = (dia2 - center).SqrMagnitude();
            var radius = (float)Math.Sqrt(sqRad);

            foreach (var p in aPoints)
            {
                float d = (p - center).SqrMagnitude();
                if (d > sqRad)
                {
                    var r = (float)Math.Sqrt(d);
                    radius = (radius + r) * 0.5f;
                    sqRad = radius * radius;
                    var offset = r - radius;
                    center = (radius * center + offset * p) / r;
                }
            }
            return new BoundingSphere(center, radius);
        }
    }
}
