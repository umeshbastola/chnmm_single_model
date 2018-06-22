using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib.CHnMM.Algorithms;
using MathNet.Numerics.LinearAlgebra;

namespace GestureRecognitionLib.CHnMM
{
    using ProbabilityDistribution = Func<double, double>;

	public abstract class Area
	{
        public int ID { get; protected set; }
        public abstract string CreateSymbol(TrajectoryPoint point);
        public abstract double GetProbability(string symbol);
	}

    public abstract class Circle: Area
    {
        public double X { get; protected set; }
        public double Y { get; protected set; }
    }

    public abstract class Sphere : Circle
    {
        public double Z { get; protected set; }
    }

    public class DiscreteCircle: Circle
    {
        public double Radius { get; protected set; }
        public double ToleranceRadius { get; protected set; }

        private double hitProbability;


        public override string CreateSymbol(TrajectoryPoint point)
        {
            var difX = point.X - X;
            var difY = point.Y - Y;
            var dis = Math.Sqrt(difX * difX + difY * difY);

            if (dis <= Radius)
            {
                return $"A{ID + 1}_Hit";
            }
            else if (dis <= ToleranceRadius)
            {
                return $"A{ID + 1}_Tolerance";
            }
            else return null;
        }

        public override double GetProbability(string symbol)
        {
            if (symbol.EndsWith("Hit"))
                return hitProbability;
            else
                return 1 - hitProbability;
        }

        public DiscreteCircle(int ID, double x, double y, double radius, double toleranceRadius, double hitProbability)
        {
            //Console.WriteLine(x + "---" + y + "----" + radius + "-----" + toleranceRadius+"-----" + hitProbability);
            //Console.WriteLine("["+ x + "," + y + "," + radius + "," + toleranceRadius+ "],");
            this.ID = ID;
            X = x;
            Y = y;
            Radius = radius;
            ToleranceRadius = toleranceRadius;
            this.hitProbability = hitProbability;
        }
    }

    public class ContinuousCircle: Circle
    {
        private ProbabilityDistribution distanceDistribution;

        public double StandardDeviation { get; private set; }

        public ContinuousCircle(int ID, double x, double y, ProbabilityDistribution dist, double std)
        {
            this.ID = ID;
            X = x;
            Y = y;
            distanceDistribution = dist;
            StandardDeviation = std;
        }

        public override string CreateSymbol(TrajectoryPoint point)
        {
            var difX = X - point.X;
            var difY = Y - point.Y;
            var dis = Math.Sqrt(difX * difX + difY * difY);

            return $"A{ID + 1}_{dis:F4}";
        }

        public override double GetProbability(string symbol)
        {
            var split = symbol.Split('_');
            var dis = double.Parse(split[split.Length - 1]);

            return distanceDistribution(dis);
        }

    }

    public class DiscreteSphere : Sphere
    {
        public double Radius { get; protected set; }
        public double ToleranceRadius { get; protected set; }

        private double hitProbability;


        public override string CreateSymbol(TrajectoryPoint point)
        {
            if (point is TrajectoryPoint3D p3d)
            {

                var difX = p3d.X - X;
                var difY = p3d.Y - Y;
                var difZ = p3d.Z - Z;
                var dis = Math.Sqrt(difX * difX + difY * difY + difZ * difZ);

                if (dis <= Radius)
                {
                    return $"A{ID + 1}_Hit";
                }
                else if (dis <= ToleranceRadius)
                {
                    return $"A{ID + 1}_Tolerance";
                }
                else return null;
            }

            throw new ArgumentException("The point has to be 3 dimensional");
        }

        public override double GetProbability(string symbol)
        {
            if (symbol.EndsWith("Hit"))
                return hitProbability;
            else
                return 1 - hitProbability;
        }

        public DiscreteSphere(int ID, double x, double y, double z, double radius, double toleranceRadius, double hitProbability)
        {
            this.ID = ID;
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            ToleranceRadius = toleranceRadius;
            this.hitProbability = hitProbability;
        }
    }

    public class DiscreteEllipsoid : Area
    {
        private Ellipsoid ellipsoid;
        private Matrix<double> Atol;
        //private double hitProbability;
        //private double toleranceFactor;

        public DiscreteEllipsoid(int ID, IEnumerable<TrajectoryPoint3D> points)
        {
            this.ID = ID;
            var minRadius = StrokeMap.minimumRadius;
            var n = points.Count();

            var allZero = points.All(p => p.X == 0 && p.Y == 0 && p.Z == 0);


            if (n > 1 && !allZero)
            {
                var minEllipsoid = Ellipsoid.MinVolEllipsoid(points);

                var svd = minEllipsoid.A.Svd();
                var D = svd.W;

                var rx = 1 / Math.Sqrt(D[0, 0]);
                var ry = 1 / Math.Sqrt(D[1, 1]);
                var rz = 1 / Math.Sqrt(D[2, 2]);

                var minRValue = 1 / (minRadius * minRadius);

                if (rx < minRadius) D[0, 0] = minRValue;
                if (ry < minRadius) D[1, 1] = minRValue;
                if (rz < minRadius) D[2, 2] = minRValue;

                ellipsoid = new Ellipsoid(svd.U * D * svd.VT, minEllipsoid.Centroid);
            }
            else if (n == 1 || allZero)
            {
                var p = points.First();
                var c = Vector<double>.Build.DenseOfArray(new double[] { p.X, p.Y, p.Z });
                ellipsoid = new Ellipsoid(minRadius, c);
            }

            Atol = GetToleranceEllipsoid(ellipsoid, StrokeMap.toleranceFactor);
        }

        private Matrix<double> GetToleranceEllipsoid(Ellipsoid org, double tolF)
        {
            var svd = org.A.Svd();
            var D = svd.W;

            var rx = (1 / Math.Sqrt(D[0, 0])) * tolF;
            var ry = (1 / Math.Sqrt(D[1, 1])) * tolF;
            var rz = (1 / Math.Sqrt(D[2, 2])) * tolF;

            D[0, 0] = 1 / (rx * rx);
            D[1, 1] = 1 / (ry * ry);
            D[2, 2] = 1 / (rz * rz);

            return svd.U * D * svd.VT;
        }

        public override string CreateSymbol(TrajectoryPoint point)
        {
            if (point is TrajectoryPoint3D p3d)
            {
                var p = Vector<double>.Build.DenseOfArray(new double[] { p3d.X, p3d.Y, p3d.Z });

                var pp = p - ellipsoid.Centroid;
                var disV = pp * (ellipsoid.A * pp);
                var disVtol = pp * (Atol * pp);

                if (disV <= 1)
                {
                    return $"A{ID + 1}_Hit";
                }
                else if (disVtol <= 1)
                {
                    return $"A{ID + 1}_Tolerance";
                }
                else return null;
            }

            throw new ArgumentException("The point has to be 3 dimensional");
        }

        public override double GetProbability(string symbol)
        {
            if (symbol.EndsWith("Hit"))
                return StrokeMap.hitProbability;
            else
                return 1 - StrokeMap.hitProbability;
        }
    }

    public class Ellipsoid
    {
        public Matrix<double> A { get; }
        public Vector<double> Centroid { get; }

        public Ellipsoid(Matrix<double> A, Vector<double> c)
        {
            this.A = A;
            Centroid = c;
        }

        //sphere - shape
        public Ellipsoid(double radius, Vector<double> c)
        {
            var dValue = 1 / (radius * radius);
            A = Matrix<double>.Build.Diagonal(3, 3, dValue);
            Centroid = c;
        }

        public static Ellipsoid MinVolEllipsoid(IEnumerable<TrajectoryPoint3D> points, double tolerance = 0.01)
        {
            var MB = Matrix<double>.Build;
            var VB = Vector<double>.Build;

            var cols = points.Select(p => new double[] { p.X, p.Y, p.Z });
            var P = MB.DenseOfColumns(cols);

            int d = 3;
            int N = P.ColumnCount;

            var Q = MB.DenseOfRowVectors(P.EnumerateRows().Concat(new Vector<double>[] { VB.Dense(N, 1.0) }));

            int count = 1;
            double err = 1.0;
            var u = VB.Dense(N, 1.0 / N);

            // Khachiyan Algorithm
            while (err > tolerance)
            {
                // Matrix multiplication: 
                // diag(u) : if u is a vector, places the elements of u 
                // in the diagonal of an NxN matrix of zeros
                var diagU = MB.DiagonalOfDiagonalVector(u);
                var Qt = Q.Transpose();
                var X = Q * diagU * Qt;
                var M = (Qt * X.Inverse() * Q).Diagonal();

                var max = M.Maximum();
                var j = M.MaximumIndex();

                // Calculate the step size for the ascent
                var step_size = (max - d - 1) / ((d + 1) * (max - 1));

                // Calculate the new_u:
                // Take the vector u, and multiply all the elements in it by (1-step_size)
                var new_u = (1 - step_size) * u;

                new_u[j] += step_size;


                // Store the error by taking finding the square root of the SSD 
                // between new_u and u
                // The SSD or sum-of-square-differences, takes two vectors 
                // of the same size, creates a new vector by finding the 
                // difference between corresponding elements, squaring 
                // each difference and adding them all together. 

                // So if the vectors were: a = [1 2 3] and b = [5 4 6], then:
                // SSD = (1-5)^2 + (2-4)^2 + (3-6)^2;
                // And the norm(a-b) = sqrt(SSD);
                err = (new_u - u).L2Norm();

                // Increment count and replace u
                count = count + 1;
                u = new_u;
            }

            // Put the elements of the vector u into the diagonal of a matrix
            // U with the rest of the elements as 0
            var U = MB.DiagonalOfDiagonalVector(u);
            var Pu = P * u;
            var m = Pu.OuterProduct(Pu);

            var A = (1.0 / d) * (P * U * P.Transpose() - m).Inverse();

            return new Ellipsoid(A, Pu);
        }
    }
}
