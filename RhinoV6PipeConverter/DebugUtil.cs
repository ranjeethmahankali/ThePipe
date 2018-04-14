using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino;

namespace RhinoV6PipeConverter
{
#if DEBUG
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

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static void SaveObjectToFile(GeometryBase obj, string name)
        {
            string path = Path.Combine(@"C:\Users\Ranjeeth Mahankali\Desktop", name + ".txt");
            File.WriteAllBytes(path, ObjectToByteArray(obj));
        }
    }
#endif
}
