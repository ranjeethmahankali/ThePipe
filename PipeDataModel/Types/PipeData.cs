using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Utils;

namespace PipeDataModel.Types
{
    //this is to be used by the various data types in the pipe data model
    //its current job is only to enfore equals method and to tell the pipe data types apart
    public interface IPipeMemberType: IEquatable<IPipeMemberType>
    {
    }

    public interface IPipeData:IEquatable<IPipeData>
    {
        string Name { get; set; }
        DataNode ContainerNode { get; }
        List<string> Tags { get; }
        object Value { get; set; }
        string ToString();
    }

    [Serializable]
    public class PipeData: IPipeData
    {
        #region-fields
        private object _value;

        private string _name;
        private DataNode _containerNode;
        private List<string> _tags;

        private static readonly List<Type> _allowedValueTypes = new List<Type>
        {
            typeof(string),
            typeof(double),
            typeof(int),
            typeof(float),
            typeof(long),
            typeof(uint),
            typeof(ulong),
        };
        #endregion

        #region-properties
        #endregion

        #region-constructors
        public PipeData(int integer)
        {
            Value = integer;
        }
        public PipeData(double realNumber)
        {
            Value = realNumber;
        }
        public PipeData(string text)
        {
            Value = text;
        }
        #endregion

        #region-IPipeDataImplementation
        public object Value
        {
            get { return _value; }
            set
            {
                if (!IsAllowedType(value.GetType()))
                {
                    throw new InvalidCastException("The Pipe does not support this data type !");
                }
                _value = value;
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(IPipeData other)
        {
            if(_name != other.Name) { return false; }
            if(!Value.Equals(other.Value)) { return false; }
            return PipeDataUtil.EqualIgnoreOrder(_tags, other.Tags);
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public DataNode ContainerNode
        {
            get { return _containerNode; }
            set { _containerNode = value; }
        }
        public List<string> Tags
        {
            get
            {
                if ( _tags == null ) { _tags = new List<string>(); }
                return _tags;
            }
        }
        #endregion

        #region-methods
        private static bool IsAllowedType(Type type)
        {
            if (typeof(IPipeMemberType).IsAssignableFrom(type)) { return true; }
            foreach(var t in _allowedValueTypes)
            {
                if (t.IsAssignableFrom(type)) { return true; }
            }

            return false;
        }
        #endregion
    }
}
