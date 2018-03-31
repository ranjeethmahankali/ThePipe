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
    public class PipeInteger: IPipeMemberType, IEquatable<PipeInteger>
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
            return Equals((PipeInteger)other);
        }

        public bool Equals(PipeInteger other)
        {
            return other.Value.Equals(Value);
        }
    }

    [Serializable]
    public class PipeNumber: IPipeMemberType, IEquatable<PipeNumber>
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
            return Equals((PipeNumber)other);
            
        }

        public bool Equals(PipeNumber other)
        {
            return other.Value.Equals(Value);
        }
    }

    [Serializable]
    public class PipeString: IPipeMemberType, IEquatable<PipeString>
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
            return Equals((PipeString)other);
            
        }

        public bool Equals(PipeString other)
        {
            return other.Value.Equals(Value);
        }
    }
}
