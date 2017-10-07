using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Primitives
{
    internal interface IPipeData
    {
        string Name { get; set; }
        DataNode ContainerNode { get; }
        List<string> Tags { get; }
    }

    public abstract class PipeData<T>: IPipeData
    {
        #region-fields
        protected T _value;
        protected string _name;
        protected DataNode _containerNode;
        protected List<string> _tags;
        #endregion

        #region-properties
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }

        #region-IPipeDataImplementation
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
    }
}
