using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino;

namespace RhinoV6PipeConverter
{
    public class DebugUtil
    {
        #region fields
        private static RhinoDoc _document = null;
        #endregion

        #region properties
        public static RhinoDoc Document { get => _document; set => _document = value; }
        #endregion

        public static void AddObjectToDocument(GeometryBase obj)
        {
            if(obj == null) { return; }
            _document.Objects.Add(obj);
        }
    }
}
