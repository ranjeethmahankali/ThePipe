using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Types;
using pp = PipeDataModel.Types.Geometry.Curve;
using rh = Rhino.Geometry;

namespace RhinoPipeConverter
{
    public class CurveConverter:PipeConverter<rh.Curve, pp.Curve>
    {
        public CurveConverter()
        {
            AddConverter(new PipeConverter<rh.ArcCurve,pp.Arc>(
                    (rhArc) =>
                    {
                        throw new NotImplementedException();
                    },
                    (ppArc) =>
                    {
                        throw new NotImplementedException();
                    }
                ));
        }

        public class ArcConverter: PipeConverter<rh.Arc, pp.Arc>
        {
            public ArcConverter():
                base(
                        (rhArc) =>
                        {
                            throw new NotImplementedException();
                        },
                        (ppArc) =>
                        {
                            throw new NotImplementedException();
                        }
                    )
            { }
        }
    }
}
