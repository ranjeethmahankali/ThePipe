using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using ppg = PipeDataModel.Types.Geometry;
using ppc = PipeDataModel.Types.Geometry.Curve;
using pps = PipeDataModel.Types.Geometry.Surface;
using Autodesk.DesignScript.Geometry;

namespace PipeForDynamo.Converters
{
    internal class DynamoPipeConverter:PipeConverter<object, IPipeMemberType>
    {
        internal DynamoPipeConverter()
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
            //conversion of geometry
            var geomConv = new GeometryConverter();
            AddConverter(geomConv);

            //mesh converter
            AddConverter(new PipeConverter<Mesh, ppg.Mesh>(
                (dm) => {
                    return new ppg.Mesh(dm.VertexPositions.Select((v) => geomConv.ptConv.ToPipe<Point, ppg.Vec>(v)).ToList(), 
                        dm.FaceIndices.Select((f) => {
                            if(f.Count == 3) { return new ulong[] { f.A, f.B, f.C }; }
                            else if(f.Count == 4) { return new ulong[] { f.A, f.B, f.C, f.D }; }
                            else { throw new ArgumentException("A Mesh face can only have either 3 or 4 vertices!"); }
                        }).ToList());
                },
                (ppm) => {
                    return Mesh.ByPointsFaceIndices(ppm.Vertices.Select((v) => geomConv.ptConv.FromPipe<Point, ppg.Vec>(v)), 
                        ppm.Faces.Select((f) => ppg.Mesh.FaceIsTriangle(f) ? IndexGroup.ByIndices((uint)f[0], (uint)f[1], (uint)f[2]) :
                        IndexGroup.ByIndices((uint)f[0], (uint)f[1], (uint)f[2], (uint)f[3])));
                }
            ));
        }
    }
}
