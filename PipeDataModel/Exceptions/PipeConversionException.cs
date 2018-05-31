using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Exceptions
{
    public class PipeConversionException:Exception
    {
        public PipeConversionException(Type fromType, Type toType):
            this(fromType, toType, "")
        { }

        public PipeConversionException(Type fromType, Type toType, string msg):
            base(string.Format("Conversion from {0} to {1} is not supported. Aborting conversion.{2}", fromType.FullName, toType.FullName, msg))
        { }
    }
}
