using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types.Geometry.Surface
{
    [Serializable]
    public abstract class Surface : IPipeMemberType, IEquatable<Surface>
    {
        public abstract bool Equals(IPipeMemberType other);

        public bool Equals(Surface other)
        {
            return Equals((IPipeMemberType)other);
        }
    }
}
