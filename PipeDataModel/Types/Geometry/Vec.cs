using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry
{
    [Serializable]
    public class Vec:IPipeMemberType
    {
        #region-fields
        private List<double> _coords;
        #endregion
        #region-properties
        public List<double> Coordinates
        {
            get
            {
                if(_coords == null) { _coords = new List<double>(); }
                return _coords;
            }
        }
        public double Length
        {
            get
            {
                double sqSum = 0;
                List<double> coords = Coordinates;
                foreach(var coord in coords)
                {
                    sqSum += coord * coord;
                }
                return Math.Sqrt(sqSum);
            }
        }
        public int Dimensions
        {
            get { return Coordinates.Count; }
        }

        public Vec Unitized
        {
            get { return Multiply(1 / Length); }
        }
        #endregion

        #region-constructors
        public Vec(double x, double y)
        {
            _coords = new List<double>();
            _coords.Add(x);
            _coords.Add(y);
        }
        public Vec(double x, double y, double z)
        {
            _coords = new List<double>();
            _coords.Add(x);
            _coords.Add(y);
            _coords.Add(z);
        }
        public Vec(List<double> coords)
        {
            _coords = new List<double>();
            foreach(var c in coords)
            {
                _coords.Add(c);
            }
        }
        #endregion

        #region-methods
        public static void EnsureDimensionMatch(Vec v1, Vec v2)
        {
            if (v1.Dimensions != v2.Dimensions)
            {
                throw new InvalidOperationException("The two vectors have different dimensions!");
            }
        }
        public static List<T> ElementWise<T>(Vec a, Vec b, Func<double,double,T> op)
        {
            EnsureDimensionMatch(a, b);
            int dim = a.Dimensions;
            List<T> newVals = new List<T>();
            for(int i = 0; i < dim; i++)
            {
                newVals.Add(op.Invoke(a.Coordinates[i], b.Coordinates[i]));
            }
            return newVals;
        }

        public static double AngleBetween(Vec x1, Vec x2)
        {
            return Math.Acos(Dot(x1, x2) / (x1.Length * x2.Length));
        }

        public static List<T> ElementWise<T>(Vec a, Func<double, T> op)
        {
            int dim = a.Dimensions;
            List<T> newVals = new List<T>();
            for (int i = 0; i < dim; i++)
            {
                newVals.Add(op.Invoke(a.Coordinates[i]));
            }
            return newVals;
        }
        public static Vec Sum(Vec a, Vec b)
        {
            return new Vec(ElementWise(a, b, (x1, x2) => { return x1 + x2; }));
        }

        public static Vec Sum(params Vec[] vecs)
        {
            Vec sum = new Vec(0, 0, 0);
            foreach(Vec v in vecs)
            {
                sum = Sum(sum, v);
            }
            return sum;
        }
        public static Vec Difference(Vec a, Vec b)
        {
            return new Vec(ElementWise(a, b, (x1, x2) => { return x1 - x2; }));
        }
        public static Vec Cross(Vec a, Vec b)
        {
            return a.Cross(b);
        }
        public static double Dot(Vec a, Vec b)
        {
            return a.Dot(b);
        }
        public static double BoxProduct(Vec a, Vec b, Vec c)
        {
            return Dot(a, Cross(b, c));
        }

        public Vec Multiply(double scalar)
        {
            List<double> newCoords = ElementWise(this, (x) => x * scalar);
            return new Vec(newCoords);
        }

        public double Dot(Vec another)
        {
            List<double> prods = ElementWise(this, another, (x1, x2) => x1 * x2);
            return prods.Sum();
        }

        public Vec Cross(Vec another)
        {
            EnsureDimensionMatch(this, another);

            Vec u = Ensure3D(this);
            Vec v = Ensure3D(another);

            return new Vec(
                    u.Coordinates[1] * v.Coordinates[2] - u.Coordinates[2] * v.Coordinates[1],
                    u.Coordinates[2] * v.Coordinates[0] - u.Coordinates[0] * v.Coordinates[2],
                    u.Coordinates[0] * v.Coordinates[1] - u.Coordinates[1] * v.Coordinates[0]
                );
        }

        public static Vec Ensure3D(Vec vec)
        {
            Vec v = new Vec(vec.Coordinates);
            if(v.Dimensions > 3)
            {
                throw new ArgumentException("Invalid vector with too many dimensions.");
            }
            if(v.Dimensions < 3)
            {
                while(v.Dimensions < 3)
                {
                    v.Coordinates.Add(0);
                }
            }

            return v;
        }

        public static Vec Rotate(Vec vec, Vec axis, double angle)
        {
            vec = Ensure3D(vec);
            axis = Ensure3D(axis);
            Vec uAxis = axis.Unitized;
            Vec parallelComp = uAxis.Multiply(uAxis.Dot(vec));
            Vec perpComp = Difference(vec, parallelComp);
            Vec uPerp = perpComp.Unitized;
            Vec wVec = Cross(uAxis, uPerp);

            Vec rotated = Sum(uPerp.Multiply(Math.Cos(angle)), wVec.Multiply(Math.Sin(angle)));
            return rotated;
        }

        public Vec RotateAbout(Vec axis, double angle)
        {
            return Rotate(this, axis, angle);
        }

        public bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            Vec otherVec = (Vec)other;
            return ElementWise(this, otherVec, (x1, x2) => { return x1 == x2; }).All(r => r == true);
        }
        #endregion
    }
}
