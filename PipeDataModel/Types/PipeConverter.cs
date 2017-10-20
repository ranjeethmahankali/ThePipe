using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Types
{
    public interface IPipeConverter
    {
        List<Type> AllowedTypes { get; }
    }
    public interface IPipeConverter<T>: IPipeConverter
    {
        DataNode Convert(T obj);
        T ConvertFrom(DataNode node);
    }
}
