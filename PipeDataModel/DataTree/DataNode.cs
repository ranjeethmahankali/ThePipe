using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;

namespace PipeDataModel.DataTree
{
    public class DataNode: IEquatable<DataNode>
    {
        #region-fields
        private DataNode _parent;
        private Dictionary<string, DataNode> _children;
        private IPipeData _data;
        #endregion

        #region-properties
        public DataNode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public Dictionary<string, DataNode> ChildrenDict { get { return _children; } }
        public List<DataNode> ChildrenList { get { return _children.Values.ToList(); } }
        public List<string> ChildrenNames { get { return _children.Keys.ToList(); } }
        public IPipeData Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public DataNode Root
        {
            get
            {
                if(_parent == null) { return this; }
                else { return _parent.Root; }
            }
        }
        public List<string> Address
        {
            get
            {
                if(_parent == null) { return new List<string>(); }
                else
                {
                    List<string> address = _parent.Address;
                    address.Add(_data.Name);
                    return address;
                }
            }
        }
        #endregion

        #region-constructors
        #endregion

        #region-methods
        public void AddChild(DataNode child)
        {
            child.Parent = this;
            _children.Add(child.Data.Name, child);
        }
        public void RemoveChild(string childName)
        {
            if (_children.ContainsKey(childName))
            {
                _children.Remove(childName);
            }
        }
        public void RemoveChild(DataNode child)
        {
            if (_children.ContainsValue(child))
            {
                _children.Remove(child.Data.Name);
            }
        }
        public DataNode GetChild(string childName)
        {
            DataNode child;
            if(!_children.TryGetValue(childName, out child))
            {
                return null;
            }

            return child;
        }
        public bool Equals(DataNode other)
        {
            if (!_data.Equals(other.Data)) { return false; }
            foreach(string key in _children.Keys)
            {
                DataNode otherChild = null;
                other.ChildrenDict.TryGetValue(key, out otherChild);
                if(otherChild == null) { return false; }
                else if (!_children[key].Equals(otherChild)) { return false; }
            }

            return true;
        }
        #endregion
    }
}
