using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class PolyCurve : Curve
    {
        private List<Curve> _segments;
        public List<Curve> Segments
        {
            get
            {
                if(_segments == null) { _segments = new List<Curve>(); }
                return _segments;
            }
        }

        public PolyCurve(List<Curve> segments)
        {
            foreach(var seg in segments)
            {
                Segments.Add(seg);
            }
        }

        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            PolyCurve opc = (PolyCurve)other;
            if(opc.Segments.Count != Segments.Count) { return false; }
            for(int i = 0; i < Segments.Count; i++)
            {
                if (!Segments[i].Equals(opc.Segments[i])) { return false; }
            }
            return true;
        }
    }
}
