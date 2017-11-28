using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Utils;

namespace PipeDataModel.Types
{
    [Serializable]
    public class PipeInteger: IPipeMemberType
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public PipeInteger(int val)
        {
            _value = val;
        }

        public bool Equals(IPipeMemberType other)
        {
            if (other.GetType() != GetType()) { return false; }
            PipeInteger cast = (PipeInteger)other;
            return cast.Value.Equals(Value);
        }
    }

    [Serializable]
    public class PipeNumber: IPipeMemberType
    {
        private double _value;
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public PipeNumber(double val)
        {
            _value = val;
        }

        public bool Equals(IPipeMemberType other)
        {
            if (other.GetType() != GetType()) { return false; }
            PipeNumber cast = (PipeNumber)other;
            return cast.Value.Equals(Value);
        }
    }

    [Serializable]
    public class PipeString:IPipeMemberType
    {
        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public PipeString(string val)
        {
            _value = val;
        }

        public bool Equals(IPipeMemberType other)
        {
            if (other.GetType() != GetType()) { return false; }
            PipeString cast = (PipeString)other;
            return cast.Value.Equals(Value);
        }
    }
}
