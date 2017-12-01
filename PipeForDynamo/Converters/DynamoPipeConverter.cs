using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using Autodesk.DesignScript.Geometry;

namespace PipeForDynamo.Converters
{
    public class DynamoPipeConverter:PipeConverter<object, IPipeMemberType>
    {
        public DynamoPipeConverter()
        {
            //conversion of strings
            AddConverter(new PipeConverter<string, PipeString>(
                    (str) => { return new PipeString(str); },
                    (pStr) => { return pStr.Value; }
                ));
            //conversion of integers
            AddConverter(new PipeConverter<int, PipeInteger>(
                    (i) => { return new PipeInteger(i); },
                    (pi) => { return pi.Value; }
                ));
            //conversion of doubles
            AddConverter(new PipeConverter<double, PipeNumber>(
                    (val) => { return new PipeNumber(val); },
                    (pval) => { return pval.Value; }
                ));
            //conversion of Lines
            //AddConverter(new GeometryConverter());
        }
    }
}
