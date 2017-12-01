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

        public static bool IsValidUrl(string pipeIdentifier)
        {
            Uri uriResult;
            return Uri.TryCreate(pipeIdentifier, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
