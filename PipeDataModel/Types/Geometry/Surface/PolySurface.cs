using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types.Geometry.Curve;
using PipeDataModel.Utils;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public class PolySurface: Surface, IEquatable<PolySurface>
    {
        #region fields
        private List<Surface> _surfaces = new List<Surface>();
        private Dictionary<int, List<int>> _adjacentSurfDict = new Dictionary<int, List<int>>();
        #endregion

        #region properties
        public List<Surface> Surfaces { get => _surfaces; }
        //normal does not make sense for a polysurface
        public override Vec SurfaceNormal { get => null; set { } }
        public Dictionary<int, List<int>> AdjacentFaces { get => _adjacentSurfDict; set => _adjacentSurfDict = value; }
        #endregion

        #region constructors
        public PolySurface(List<Surface> surfaces, List<List<int>> adjFaces)
        {
            _surfaces.Clear();
            if(adjFaces.Count != surfaces.Count)
            {
                throw new InvalidOperationException("The adjacency data is not of the same length as the surfaces");
            }
            for(int i = 0; i < surfaces.Count; i++)
            {
                AddSurface(surfaces[i], adjFaces[i]);
            }

            SortFacesByAdjacency();
        }
        #endregion

        #region methods
        public override bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            return Equals((PolySurface)other);
        }
        public bool Equals(PolySurface otherPolySurface)
        {
            return Utils.PipeDataUtil.EqualCollections(_surfaces, otherPolySurface.Surfaces);
        }

        public override List<Vec> Vertices()
        {
            List<Vec> verts = new List<Vec>();
            Action<Vec> uniqueAdd = (v) => {
                if (!verts.Any((v1) => Vec.Difference(v, v1).Length < 1e-6)) { verts.Add(v); }
            };
            foreach(var surf in _surfaces)
            {
                var surfVerts = surf.Vertices();
                foreach(var svert in surfVerts)
                {
                    if(!GeometryUtil.ListContainsCoincidentPoint(verts, svert)){ verts.Add(svert); }
                }
            }
            return verts;
        }

        public override List<Curve.Curve> Edges()
        {
            List<Curve.Curve> edges = new List<Curve.Curve>();
            foreach(var surf in _surfaces)
            {
                var surfEdges = surf.Edges();
                foreach(var edge in surfEdges)
                {
                    if (!edges.Contains(edge)) { edges.Add(edge); }
                }
            }

            return edges;
        }

        public void AddSurface(Surface surf, List<int> adjacentFaces)
        {
            _adjacentSurfDict.Add(_surfaces.Count, adjacentFaces);
            _surfaces.Add(surf);
        }

        public bool FacesAreConnected(int face1, int face2)
        {
            List<int> adj1, adj2;
            if (!_adjacentSurfDict.TryGetValue(face1, out adj1)) { return false; }
            if (!_adjacentSurfDict.TryGetValue(face2, out adj2)) { return false; }

            return adj1.Contains(face2) || adj2.Contains(face1);
        }

        private void SortFacesByAdjacency()
        {
            int count = 0;
            List<int> order = _surfaces.Select((s) => count++).ToList();
            List<int> newOrder = new List<int>() { order[0] };
            List<int> hold = new List<int>();

            for(int i = 1; i < count; i++)
            {
                bool connected = false;
                for(int j = 0; j < i; j++)
                {
                    if(FacesAreConnected(order[i], order[j]))
                    {
                        connected = true;
                        break;
                    }
                }
                if (connected) { newOrder.Add(order[i]); }
                else { hold.Add(order[i]); }
            }

            int maxAttempts = (int)(hold.Count * (hold.Count + 1) * 0.5) + 1;
            int attempt = 0;
            while(hold.Count > 0)
            {
                var held = hold[0];
                hold.RemoveAt(0);

                if(newOrder.Any((i) => FacesAreConnected(held, i)))
                {
                    newOrder.Add(held);
                    continue;
                }
                else { hold.Add(held); }

                attempt += 1;
                if(attempt > maxAttempts)
                {
                    break;
                }
            }

            if(newOrder.Count != order.Count) { throw new InvalidOperationException("Failed to sort the faces"); }

            //now we have the new sorted order - lets update the polysurface
            List<Surface> newSurfList = new List<Surface>();
            Dictionary<int, List<int>> newAdjacency = new Dictionary<int, List<int>>();
            foreach(var index in newOrder)
            {
                newAdjacency.Add(newSurfList.Count, _adjacentSurfDict[index]);
                newSurfList.Add(_surfaces[index]);
            }

            _surfaces = newSurfList;
            _adjacentSurfDict = newAdjacency;
        }
        #endregion
    }
}
