using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Utils;

namespace PipeDataModel.Types.Geometry
{
    [Serializable]
    public class Mesh : IPipeMemberType
    {
        #region-fields
        private List<Vec> _vertices;
        private List<ulong[]> _faces;
        #endregion

        #region-properties
        public List<Vec> Vertices
        {
            get { return _vertices; }
        }
        public List<ulong[]> Faces
        {
            get { return _faces; }
        }
        public long TraingleCount
        {
            get { return _faces.Count((face) => face.Length == 3); }
        }
        public long QuadCount
        {
            get { return _faces.Count((face) => face.Length == 4); }
        }
        #endregion

        #region-constructors
        public Mesh()
        {
            _vertices = new List<Vec>();
            _faces = new List<ulong[]>();
        }
        public Mesh(List<Vec> vertices, List<ulong[]> faces)
        {
            _vertices = vertices.Select((v) => Vec.Ensure3D(v)).ToList();
            _faces = new List<ulong[]>();
            foreach(var face in faces)
            {
                //skipping the faces that have other than 3 or 4 vertices
                if(face.Length != 3 && face.Length != 4) { continue; }
                _faces.Add(face);
            }
        }
        #endregion

        #region-methods
        public static bool FaceIsTriangle(ulong[] face)
        {
            if(face.Length == 3) { return true; }
            else if(face.Length == 4) { return false; }
            else { throw new ArgumentException("Mesh face can only have either 3 or 4 vertices"); }
        }
        public bool Equals(IPipeMemberType other)
        {
            if (GetType() != other.GetType()) { return false; }
            Mesh otherMesh = (Mesh)other;
            return PipeDataUtil.EqualIgnoreOrder(_vertices, otherMesh.Vertices) &&
                PipeDataUtil.Equal(_faces, otherMesh.Faces, (f1, f2) => PipeDataUtil.EqualIgnoreOrder(f1, f2));
        }
        #endregion
    }
}
