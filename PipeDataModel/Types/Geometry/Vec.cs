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
                throw new InvalidOperationException("The two vectors have different dimensions, so distance cannot be calculated.");
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
        public static Vec Sum(Vec a, Vec b)
        {
            return new Vec(ElementWise(a, b, (x1, x2) => { return x1 + x2; }));
        }
        public static Vec Difference(Vec a, Vec b)
        {
            return new Vec(ElementWise(a, b, (x1, x2) => { return x1 - x2; }));
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
