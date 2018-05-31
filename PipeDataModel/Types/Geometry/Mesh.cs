using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Utils;

namespace PipeDataModel.Types.Geometry
{
    [Serializable]
    public class Mesh : IPipeMemberType, IEquatable<Mesh>
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
        public void Triangulate()
        {
            List<ulong[]> newFaces = new List<ulong[]>();
            for(int i = 0; i < _faces.Count; i++)
            {
                if (FaceIsTriangle(_faces[i]) || FaceIsPlanar(_faces[i]))
                {
                    newFaces.Add(_faces[i]);
                }
                else
                {
                    newFaces.AddRange(TriangulateFace(_faces[i]));
                }
            }

            _faces = newFaces;
        }
        public bool FaceIsPlanar(ulong[] face)
        {
            List<Vec> verts = new List<Vec>();
            foreach(ulong vert in face)
            {
                int i = (int)vert;
                verts.Add(_vertices[i]);
            }
            return FaceIsPlanar(verts);
        }
        public static bool FaceIsPlanar(List<Vec> faceVerts)
        {
            if(faceVerts.Count > 4) { throw new ArgumentException("Mesh face can only have either 3 or 4 vertices"); }
            if(faceVerts.Count < 4) { return true; }

            Vec A = Vec.Difference(faceVerts[1], faceVerts[0]);
            Vec B = Vec.Difference(faceVerts[2], faceVerts[0]);
            Vec C = Vec.Difference(faceVerts[3], faceVerts[0]);

            return Vec.BoxProduct(A, B, C) == 0;
        }
        public static bool FaceIsTriangle(ulong[] face)
        {
            if(face.Length == 3) { return true; }
            else if(face.Length == 4) { return false; }
            else { throw new ArgumentException("Mesh face can only have either 3 or 4 vertices"); }
        }
        public bool Equals(IPipeMemberType other)
        {
            if (GetType() != other.GetType()) { return false; }
            return Equals((Mesh)other);
        }
        public bool Equals(Mesh otherMesh)
        {
            return PipeDataUtil.EqualIgnoreOrder(_vertices, otherMesh.Vertices) &&
                PipeDataUtil.Equal(_faces, otherMesh.Faces, (f1, f2) => PipeDataUtil.EqualIgnoreOrder(f1, f2));
        }
        public void MergeMesh(Mesh other)
        {
            int vertCount = _vertices.Count;
            int runningIndex = vertCount;
            Dictionary<int, int> vertMap = new Dictionary<int, int>();
            for(int i = 0; i < other.Vertices.Count; i++)
            {
                bool found = false;
                for(int j = 0; j < vertCount; j++)
                {
                    if (_vertices[j].Equals(other.Vertices[i]))
                    {
                        found = true;
                        vertMap.Add(i, j);
                        break;
                    }
                }
                if (!found)
                {
                    _vertices.Add(other.Vertices[i]);
                    vertMap.Add(i, runningIndex++);
                }
            }
            foreach(var face in other.Faces)
            {
                _faces.Add(face.Select((vi) => (ulong)vertMap[(int)vi]).ToArray());
            }
        }

        public static List<ulong[]> TriangulateFace(ulong[] face)
        {
            if(face.Length < 3) { throw new ArgumentException("Too few points to define a face"); }
            if(face.Length == 3) { return new List<ulong[]>() { face }; }
            if(face.Length > 4) { throw new ArgumentException("Too many vertices in a single face."); }

            List<ulong[]> faces = new List<ulong[]>();
            faces.Add(new ulong[] { face[0], face[1], face[2] });
            faces.Add(new ulong[] { face[0], face[2], face[3] });
            return faces;
        }
        #endregion
    }
}
