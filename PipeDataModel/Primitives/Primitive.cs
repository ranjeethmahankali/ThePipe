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

    internal abstract class PipeData<T>
    {
        protected T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
