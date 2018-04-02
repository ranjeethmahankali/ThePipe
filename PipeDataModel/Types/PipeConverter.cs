using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PipeDataModel.Exceptions;

namespace PipeDataModel.Types
{
    internal interface IPipeConverter
    {
        Type UserType { get; }
        Type PipeType { get; }
        T1 ConvertFromPipe<T1, T2>(T2 pipeObj);
        T2 ConvertToPipe<T1, T2>(T1 userObj);
        bool CanConvert(Type fromType, Type toType, ConversionDirection direction);
    }

    public enum ConversionDirection { ToPipe, FromPipe }

    public class PipeConverter<UserT, PipeT>:IPipeConverter
        where PipeT:IPipeMemberType
    {
        #region-fields
        private List<IPipeConverter> _childrenConverters = new List<IPipeConverter>();
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
            var matchingChild = GetChildConverter(typeof(T1), typeof(T2));
            if(matchingChild != null)
            {
                throw new InvalidCastException("This converter already contains a child converter with this signature!");
            }
            _childrenConverters.Add(converter);
            return converter;
        }

        private IPipeConverter GetChildConverter(Type userT, Type pipeT)
        {
            foreach (var child in _childrenConverters)
            {
                if (child.PipeType == pipeT && child.UserType == userT)
                {
                    return child;
                }
            }
            return null;
        }

        private IPipeConverter GetToPipeConverter(Type userType, Type pipeType)
        {
            IPipeConverter converter = null;
            foreach (var child in _childrenConverters)
            {
                if(child.UserType == userType && pipeType.IsAssignableFrom(child.PipeType))
                {
                    if(child.CanConvert(userType, pipeType, ConversionDirection.ToPipe))
                    {
                        return child;
                    }
                }
                if (child.UserType.IsAssignableFrom(userType) && pipeType.IsAssignableFrom(child.PipeType))
                {
                    if(converter == null) { converter = child; }
                }
            }
            return converter;
        }

        private IPipeConverter GetFromPipeConverter(Type userType, Type pipeType)
        {
            IPipeConverter converter = null;
            foreach (var child in _childrenConverters)
            {
                if(child.PipeType == pipeType && userType.IsAssignableFrom(child.UserType))
                {
                    if(child.CanConvert(pipeType, userType, ConversionDirection.FromPipe))
                    {
                        return child;
                    }
                }
                if (child.PipeType.IsAssignableFrom(pipeType) && userType.IsAssignableFrom(child.UserType))
                {
                    if(converter == null){converter = child;}
                }
            }
            return converter;
        }

        public T2 ConvertToPipe<T1, T2>(T1 obj)
        {
            if (!(typeof(UserT).IsAssignableFrom(obj.GetType()) && typeof(T2).IsAssignableFrom(typeof(PipeT))))
            {
                throw new PipeConversionException(typeof(T2), obj.GetType());
            }
            return (T2)(object)ToPipe<UserT, PipeT>((UserT)(object)obj);
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

            var childConverter = GetToPipeConverter(obj.GetType(), typeof(T2));
            if (childConverter != null)
            {
                return childConverter.ConvertToPipe<T1, T2>(obj);
            }

            throw new PipeConversionException(obj.GetType(), typeof(T2));
        }

        public T1 ConvertFromPipe<T1, T2>(T2 obj)
        {
            if (!(typeof(T1).IsAssignableFrom(typeof(UserT)) && typeof(PipeT).IsAssignableFrom(obj.GetType())))
            {
                throw new PipeConversionException(typeof(T1), obj.GetType());
            }
            return (T1)(object)FromPipe<UserT, PipeT>((PipeT)(object)obj);
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

            var childConverter = GetFromPipeConverter(typeof(T1), obj.GetType());
            if(childConverter != null)
            {
                return childConverter.ConvertFromPipe<T1, T2>(obj);
            }

            throw new PipeConversionException(obj.GetType(), typeof(T1));
        }

        public bool CanConvert(Type fromType, Type toType, ConversionDirection direction)
        {
            IPipeConverter childConv;
            bool hasConversionDelegate;
            if(direction == ConversionDirection.ToPipe)
            {
                childConv = GetToPipeConverter(fromType, toType);
                hasConversionDelegate = _toPipeConversionDelegate != null;
            }
            else if(direction == ConversionDirection.FromPipe)
            {
                childConv = GetFromPipeConverter(fromType, toType);
                hasConversionDelegate = _fromPipeConversionDelegate != null;
            }
            else { return false; }

            bool hasValidChild = childConv != null && childConv.CanConvert(fromType, toType, direction);
            return hasValidChild || hasConversionDelegate;
        }
        #endregion
    }
}
