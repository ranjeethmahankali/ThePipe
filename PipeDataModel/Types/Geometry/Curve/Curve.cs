using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public abstract class Curve : IPipeMemberType
    {
        public abstract Vec StartPoint { get; }
        public abstract Vec EndPoint { get; }
        public abstract bool Equals(IPipeMemberType other);

        public List<Curve> FlattenedCurveList()
        {
            List<Curve> curs = new List<Curve>();
            if (typeof(PolyCurve).IsAssignableFrom(GetType()))
            {
                foreach(var cur in ((PolyCurve)this).Segments) { curs.AddRange(cur.FlattenedCurveList()); }
            }
            else if (typeof(Polyline).IsAssignableFrom(GetType()))
            {
                curs.AddRange(((Polyline)this).ExplodedLines());
            }
            else { curs.Add(this); }

            return curs;
        }
    }
}
