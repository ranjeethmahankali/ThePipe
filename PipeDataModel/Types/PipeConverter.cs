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
        private List<IPipeConverter> _childrenConverters = null;
        private readonly Func<UserT, PipeT> _toPipeConversionDelegate = null;
        private readonly Func<PipeT, UserT> _fromPipeConversionDelegate = null;
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

        #region-constructors
        public PipeConverter(Func<UserT, PipeT> toConversion, Func<PipeT, UserT> fromConversion)
        {
            _toPipeConversionDelegate = toConversion;
            _fromPipeConversionDelegate = fromConversion;
        }
        public PipeConverter()
        {
            _childrenConverters = new List<IPipeConverter>();
        }
        #endregion

        #region-methods
        public PipeConverter<T1, T2> AddConverter<T1, T2>(PipeConverter<T1, T2> converter)
            where T1:UserT
            where T2:PipeT
        {
            var matchingChild = GetChildConverter<T1, T2>();
            if(matchingChild != null)
            {
                throw new InvalidCastException("This converter already contains a child converter with this signature!");
            }
            _childrenConverters.Add(converter);
            return converter;
        }

        private PipeConverter<uT, pT> GetChildConverter<uT,pT>()
            where uT : UserT
            where pT : PipeT
        {
            foreach (var child in _childrenConverters)
            {
                if (typeof(uT) == child.UserType && typeof(pT) == child.PipeType)
                {
                    return (PipeConverter<uT, pT>)child;
                }
            }
            return null;
        }

        public PipeT ToPipe<T1, T2>(T1 obj)
            where T1 : UserT
            where T2 : PipeT
        {
            if (PipeType == typeof(T2) && UserType == typeof(T1)
                && _toPipeConversionDelegate != null)
            {
                return (T2)_toPipeConversionDelegate.Invoke(obj);
            }

            var childConverter = GetChildConverter<T1, T2>();
            if (childConverter != null)
            {
                return childConverter.ToPipe<T1, T2>(obj);
            }

            return default(PipeT);
        }

        public UserT FromPipe<T1, T2>(T2 obj)
            where T1 : UserT
            where T2 : PipeT
        {
            if (PipeType == typeof(T2) && UserType == typeof(T1)
                && _fromPipeConversionDelegate != null)
            {
                return (T1)_fromPipeConversionDelegate.Invoke(obj);
            }

            var childConverter = GetChildConverter<T1,T2>();
            if(childConverter != null)
            {
                return childConverter.FromPipe<T1, T2>(obj);
            }

            return default(UserT);
        }
        #endregion
    }
}
