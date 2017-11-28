using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;
using PipeDataModel.Utils;

namespace PipeDataModel.Types
{
    public abstract class PipeData<T> : IPipeMemberType
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public PipeData(T val)
        {
            _value = val;
        }
        public bool Equals(IPipeMemberType other)
        {
            if(other.GetType() != GetType()) { return false; }
            PipeData<T> cast = (PipeData<T>)other;
            return cast.Value.Equals(Value);
        }
    }

    [Serializable]
    public class PipeInteger : PipeData<int>
    {
        public PipeInteger(int val) : base(val) { }
    }

    [Serializable]
    public class PipeNumber : PipeData<double>
    {
        public PipeNumber(double val): base(val) { }
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
