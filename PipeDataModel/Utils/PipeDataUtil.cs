using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.DataTree;

namespace PipeDataModel.Utils
{
    public class PipeDataUtil
    {
        public static bool EqualIgnoreOrder<T>(ICollection<T> a, ICollection<T> b)
        {
            foreach(T item in a)
            {
                if (!b.Contains(item)) { return false; }
            }
            return true;
        }

        public static bool Equal<T>(ICollection<T> a, ICollection<T> b, Func<T,T,bool> equalityComparer = null)
        {
            if(equalityComparer == null) { equalityComparer = (p, q) => p.Equals(q); }
            int aCount = a.Count;
            int bCount = b.Count;
            if(aCount != bCount) { return false; }
            for(int i = 0; i < aCount; i++)
            {
                if(!equalityComparer.Invoke(a.ElementAt(i),b.ElementAt(i))) { return false; }
            }

            return true;
        }

        public static bool IsValidUrl(string pipeIdentifier)
        {
            Uri uriResult;
            return Uri.TryCreate(pipeIdentifier, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static double RadiansToDegrees(double rads)
        {
            return (rads / Math.PI) * 180;
        }

        public static double DegreesToRadians(double degs)
        {
            return (degs/180)*Math.PI;
        }

        public static bool Equals(double val, double target, double tolerance = 0)
        {
            return Math.Abs(val - target) <= tolerance;
        }
    }
}
