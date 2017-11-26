using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public abstract class Curve : IPipeMemberType
    {
        public abstract bool Equals(IPipeMemberType other);
    }
}
