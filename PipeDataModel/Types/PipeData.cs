using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Types
{
    public enum DataType
    {
        INTEGER,
        REALNUMBER,
        TEXT
    }

    internal interface IPipeData
    {
        string Name { get; set; }
        DataNode ContainerNode { get; }
        List<string> Tags { get; }
        object Value { get; set; }
        DataType DataType { get; }
        string ToString();
    }

    public class PipeData: IPipeData
    {
        #region-fields
        protected int? _integer;
        protected double? _realNumber;
        protected string _text;
        protected DataType _dataType;

        protected string _name;
        protected DataNode _containerNode;
        protected List<string> _tags;
        #endregion

        #region-properties
        public object Value
        {
            get
            {
                if(_dataType == DataType.INTEGER) { return _integer; }
                else if (_dataType == DataType.REALNUMBER) { return _realNumber; }
                else if (_dataType == DataType.TEXT) { return _text; }
                else
                {
                    throw new InvalidOperationException("Datatype is unknown.");
                }
            }
            set
            {
                _integer = null; _realNumber = null; _text = null;
                if (typeof(int).IsAssignableFrom(value.GetType()))
                {
                    _integer = (int)value;
                    _dataType = DataType.INTEGER;
                }
                else if (typeof(double).IsAssignableFrom(value.GetType()))
                {
                    _realNumber = (double)value;
                    _dataType = DataType.REALNUMBER;
                }
                else if (typeof(string).IsAssignableFrom(value.GetType()))
                {
                    _text = (string)value;
                    _dataType = DataType.TEXT;
                }
            }
        }
        public DataType DataType
        {
            get { return _dataType; }
        }
        #endregion

        #region-IPipeDataImplementation
        public override string ToString()
        {
            return Value.ToString();
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
    }
}
