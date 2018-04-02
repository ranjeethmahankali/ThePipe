using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using PipeDataModel.Utils;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;

namespace PipeForRhino
{
    [System.Runtime.InteropServices.Guid("77e390dc-fbd8-487e-886d-5409d0128d7a")]
    public class PipePullCommand : Command, IPipeEmitter
    {
        private static string APP_KEY = "rhinoPipe_77e390dc-fbd8-487e-886d-5409d0128d7a";
        private List<GeometryBase> _objectsReceived;
        private Pipe _pipe;
        private static string _prevPipeName = null;

        public PipePullCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PipePullCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "PipePull"; }
        }

        public void EmitPipeData(DataNode data)
        {
            _objectsReceived = new List<GeometryBase>();
            foreach(var child in data.ChildrenList)
            {
                _objectsReceived.Add(PipeConverter.FromPipe(child.Data));
            }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            string pipeIdentifier;
            using (GetString getter = new GetString())
            {
                getter.SetCommandPrompt("Enter the name/url for the pipe");
                if(_prevPipeName != null) { getter.SetDefaultString(_prevPipeName); }
                if (getter.Get() != GetResult.String)
                {
                    RhinoApp.WriteLine("Invalid Input");
                    return getter.CommandResult();
                }
                pipeIdentifier = getter.StringResult();
                _prevPipeName = pipeIdentifier;
            }

            if (PipeDataUtil.IsValidUrl(pipeIdentifier))
            {
                _pipe = new MyWebPipe(pipeIdentifier);
            }
            else
            {
                _pipe = new LocalNamedPipe(pipeIdentifier);
            }

            _pipe.SetEmitter(this);

            _pipe.Update();
            if(_objectsReceived.Count > 0)
            {
                DeletePulledObjects(pipeIdentifier, ref doc);
            }
            List<Guid> received = new List<Guid>();
            foreach (var geom in _objectsReceived)
            {
                Guid id = doc.Objects.Add(geom);
                received.Add(id);
            }
            doc.Views.Redraw();

            SavePulledObjects(pipeIdentifier, received, ref doc);
            return Result.Success;
        }

        private static List<Guid> GetPulledObjectDictionary(ref RhinoDoc doc, string pipeIdentifier)
        {
            string dataStr = doc.Strings.GetValue(APP_KEY, pipeIdentifier);
            List<Guid> list = DeserializeToGuidList(dataStr);
            return list;
        }

        private static void SavePulledObjects(string pipeIdentifier, List<Guid> pulledObjs, ref RhinoDoc doc)
        {
            string jsonStr = SerializeGuidList(pulledObjs);
            doc.Strings.SetString(APP_KEY, pipeIdentifier, jsonStr);
        }

        private static void DeletePulledObjects(string pipeIdentifier, ref RhinoDoc doc)
        {
            List<Guid> pulledObjs = GetPulledObjectDictionary(ref doc, pipeIdentifier);
            if(pulledObjs == null) { return; }
            foreach(var id in pulledObjs)
            {
                doc.Objects.Delete(id, true);
            }
        }

        private static List<Guid> DeserializeToGuidList(string str)
        {
            string[] guidStrArr = str.Split(';');
            List<Guid> ids = new List<Guid>();
            foreach(var idStr in guidStrArr)
            {
                if(idStr == "") { continue; }
                Guid id;
                if(Guid.TryParse(idStr, out id)) { ids.Add(id); }
            }
            return ids;
        }

        private static string SerializeGuidList(List<Guid> idList)
        {
            string str = "";
            for(int i = 0; i < idList.Count; i++)
            {
                str += idList[i].ToString();
                if(i != idList.Count - 1)
                {
                    str += ";";
                }
            }

            return str;
        }
    }
}
