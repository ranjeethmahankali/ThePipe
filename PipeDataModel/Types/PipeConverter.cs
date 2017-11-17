using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeDataModel.Types
{
    internal interface IPipeConverter
    {
        Type UserType { get; }
        Type PipeType { get; }
    }
    public class PipeConverter<UserT, PipeT>:IPipeConverter
        where PipeT:IPipeMemberType
    {
        #region-fields
        private Dictionary<Type, IPipeConverter> _pipeTypeMap;
        private Dictionary<Type, IPipeConverter> _userTypeMap;
        #endregion

        #region-properties
        public Type UserType
        {
            get { return typeof(UserT); }
        }
        public Type PipeType
        {
            get { return typeof(PipeT); }
        }
        #endregion

        #region-methods
        public void AddToPipeConverter<T1, T2>(PipeConverter<T1, T2> converter)
            where T1:UserT
            where T2:PipeT
        {
            _userTypeMap.Add(typeof(T1), converter);
            _pipeTypeMap.Add(typeof(T2), converter);
        }

        public T2 ToPipe<T1, T2>(T1 obj)
            where T2: PipeT
            where T1: UserT
        {
            //incomplete
            throw new NotImplementedException();
        }

        public T1 FromPipe<T1, T2>(T2 obj)
            where T2 : PipeT
            where T1 : UserT
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}
