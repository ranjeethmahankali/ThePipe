using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PipeDataModel.Types
{
    //this is to be used by the various data types in the pipe data model
    //its current job is only to enfore equals method and to tell the pipe data types apart    
    public interface IPipeMemberType : IEquatable<IPipeMemberType>
    {
        //byte[] SerializeToStreamAsBinary();
        //string SerializeToStreamAsString();
    }

    //[Serializable]
    //public class PipeMemberType: IPipeMemberType
    //{

    //}
}
