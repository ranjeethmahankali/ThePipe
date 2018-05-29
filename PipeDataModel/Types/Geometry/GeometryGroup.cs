using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Utils;

namespace PipeDataModel.Types.Geometry
{
    public class GeometryGroup : IPipeMemberType, IEquatable<GeometryGroup>
    {
        #region properties
        private List<IPipeMemberType> _members = new List<IPipeMemberType>();
        #endregion

        #region constructors
        public GeometryGroup(IEnumerable<IPipeMemberType> members)
        {
            _members = new List<IPipeMemberType>();
            foreach(var mem in members)
            {
                if(IsValidMember(mem)) { _members.Add(mem); }
            }
        }
        public GeometryGroup():this(new List<IPipeMemberType>()) { }
        #endregion

        #region methods
        public void AddMember(IPipeMemberType mem)
        {
            if (IsValidMember(mem)) { _members.Add(mem); }
            else { throw new InvalidOperationException("This type not supported as a group member."); }
        }
        public bool RemoveMember(IPipeMemberType mem)
        {
            return _members.Remove(mem);
        }
        public void RemoveMember(int index)
        {
            if(_members.Count > index && index > -1)
            {
                _members.RemoveAt(index);
            }
        }
        public List<IPipeMemberType> GetMembers()
        {
            return _members;
        }

        public bool Equals(IPipeMemberType other)
        {
            if (!GetType().IsAssignableFrom(other.GetType())) { return false; }
            return Equals((GeometryGroup)other);
        }

        public bool Equals(GeometryGroup other)
        {
            return PipeDataUtil.EqualCollections(_members, other._members);
        }

        public static bool IsValidMember(IPipeMemberType member)
        {
            return member is Curve.Curve || member is Surface.Surface || member is Mesh;
        }
        #endregion
    }
}
