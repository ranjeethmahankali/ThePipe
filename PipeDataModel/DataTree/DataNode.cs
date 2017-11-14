using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Types;
using PipeDataModel.Utils;
using Newtonsoft.Json;

namespace PipeDataModel.DataTree
{
    [Serializable]
    public class DataNode: IEquatable<DataNode>
    {
        #region-fields
        public static string DEFAULT_NODE_NAME = "ROOT";

        private DataNode _parent;
        private Dictionary<string, DataNode> _children;
        private PipeData _data;
        #endregion

        #region-properties
        [JsonIgnore]
        public DataNode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public Dictionary<string, DataNode> ChildrenDict {
            get
            {
                if(_children == null) { _children = new Dictionary<string, DataNode>(); }
                return _children;
            }
        }
        public List<DataNode> ChildrenList { get { return ChildrenDict.Values.ToList(); } }
        public List<string> ChildrenNames { get { return ChildrenDict.Keys.ToList(); } }
        public PipeData Data
        {
            get { return _data; }
            set { _data = value; }
        }
        [JsonIgnore]
        public DataNode Root
        {
            get
            {
                if(_parent == null) { return this; }
                else { return _parent.Root; }
            }
        }
        public string Name
        {
            get { return _data.Name; }
            set { _data.Name = value; }
        }
        public List<string> Address
        {
            get
            {
                if(_parent == null) { return new List<string>(); }
                else
                {
                    List<string> address = _parent.Address;
                    address.Add(Name);
                    return address;
                }
            }
        }
        #endregion

        #region-constructors
        internal DataNode() { }
        public DataNode(PipeData data)
        {
            _data = data;
            EnsureValidNodeName();
        }
        #endregion

        #region-methods
        public void AddChild(DataNode child)
        {
            child.Parent = this;
            child.EnsureValidNodeName();
            _children.Add(child.Name, child);
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
                _children.Remove(child.Name);
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
        public string TreeAsString(int depth = int.MaxValue, int tabNum = 0)
        {
            StringBuilder str = new StringBuilder();
            str.Append(string.Format("{0}{1}: {2}\n", string.Concat(Enumerable.Repeat("\t", tabNum)),
                Name, Data));
            if(depth > 0)
            {
                foreach(var child in ChildrenList)
                {
                    str.Append(child.TreeAsString(depth - 1, tabNum + 1));
                }
            }

            return str.ToString();
        }
        public override string ToString()
        {
            return TreeAsString();
        }
        public void EnsureValidNodeName()
        {
            if(Parent == null && Name == null)
            {
                Name = DEFAULT_NODE_NAME;
            }
            else if(Parent != null && (Name == Parent.Name || Name == DEFAULT_NODE_NAME || Name == null))
            {
                Name = string.Format("{0}_{1}", Parent.Name,
                    Parent.ChildrenDict.Count);
            }
            else if (Parent != null && Parent.ChildrenDict.ContainsKey(Name))
            {
                Name = string.Format("{0}_{1}", Name, Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
            }
        }
        public DataNode Duplicate()
        {
            DataNode copy = new DataNode(_data);
            foreach(var ch in ChildrenList)
            {
                copy.AddChild(ch);
            }
            return copy;
        }
        #endregion
    }
}
