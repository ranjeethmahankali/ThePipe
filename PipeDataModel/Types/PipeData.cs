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

    public class PipeInteger : PipeData<int>
    {
        public PipeInteger(int val) : base(val) { }
    }

    public class PipeNumber : PipeData<double>
    {
        public PipeNumber(double val): base(val) { }
    }

    public class PipeString : PipeData<string>
    {
        public PipeString(string val) : base(val) { }
    }
}
