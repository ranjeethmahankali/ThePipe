using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry
{
    [Serializable]
    public class Plane : IPipeMemberType
    {
        #region-fields
        private Vec _origin, _x, _y, _z;
        #endregion

        #region-properties
        public Vec X
        {
            get { return _x; }
            set { _x = value.Unitized; }
        }
        public Vec Y
        {
            get { return _y; }
            set { _y = value.Unitized; }
        }
        public Vec Z
        {
            get { return _z; }
            set { _z = value.Unitized; }
        }
        public Vec Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }
        #endregion

        #region-constructors
        public Plane()
            :this(new Vec(0,0,0), new Vec(1,0,0), new Vec(0,1,0)) { }
        public Plane(Vec origin): this(origin, new Vec(1, 0, 0), new Vec(0, 1, 0)) { }
        public Plane(Vec origin, Vec x, Vec y)
        {
            _origin = origin;
            X = x;
            Y = y;
            Z = Vec.Cross(X, Y);
        }
        public Plane(Vec origin, Vec x, Vec y, Vec z)
        {
            _origin = origin;
            X = x;
            Y = y;
            Z = z;
            if (Math.Abs(Vec.BoxProduct(X, Y, Z) - 1) > 1e-5)
            {
                throw new ArgumentException("Cannot initialize a plane with non orthogonal system");
            }
        }
        public Plane(Vec origin, Vec z)
        {
            _origin = origin;
            //try constructing x and y
            Z = z;
            X = Vec.Dot(Z, new Vec(1, 0, 0)) == 1 ? new Vec(0, 1, 0): new Vec(1, 0, 0);
            Y = Vec.Cross(Z, X);
        }
        #endregion

        #region-methods
        //transforms the given point 'vec' from the coord system of this plane to global (or what ever system the cooridnates of this plane
        //are in 
        public Vec ToGlobal(Vec vec)
        {
            Vec v = Vec.Ensure3D(vec);
            Vec fromOrigin = Vec.Sum(
                    X.Multiply(v.Coordinates[0]),
                    Y.Multiply(v.Coordinates[1]),
                    Z.Multiply(v.Coordinates[2])
                );

            return Vec.Sum(fromOrigin, Origin);
        }

        public bool Equals(IPipeMemberType other)
        {
            if(GetType() != other.GetType()) { return false; }
            Plane otherPlane = (Plane)other;
            return X.Equals(otherPlane.X) && Y.Equals(otherPlane.Y) && Z.Equals(otherPlane.Z) && Origin.Equals(otherPlane.Origin);
        }
        #endregion
    }
}
