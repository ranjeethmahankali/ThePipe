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

namespace PipeForRhinoV6
{
    [System.Runtime.InteropServices.Guid("a9b7d2ce-55de-410a-b3a6-947e442a24b3")]
    public class PipePushCommand : Command, IPipeCollector
    {
        private List<RhinoObject> _objectsToSend;
        private Pipe _pipe;
        private static string _prevPipeName = null;

        public PipePushCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PipePushCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "PipePush"; }
        }

        public DataNode CollectPipeData()
        {
            DataNode node = new DataNode();
            foreach (var obj in _objectsToSend)
            {
                node.AddChild(new DataNode(PipeConverter.ToPipe(obj.Geometry)));
            }
            return node;
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            string pipeIdentifier;
            using (GetString getter = new GetString())
            {
                getter.SetCommandPrompt("Enter the name/url for the pipe");
                if (_prevPipeName != null) { getter.SetDefaultString(_prevPipeName); }
                if (getter.Get() != GetResult.String)
                {
                    RhinoApp.WriteLine("Invalid Input");
                    return getter.CommandResult();
                }
                pipeIdentifier = getter.StringResult();
                _prevPipeName = pipeIdentifier;
            }

            if (_pipe != null)
            {
                _pipe.ClosePipe();
                _pipe = null;
            }

            if (PipeDataUtil.IsValidUrl(pipeIdentifier))
            {
                _pipe = new MyWebPipe(pipeIdentifier);
            }
            else
            {
                _pipe = new LocalNamedPipe(pipeIdentifier);
            }

            _pipe.SetCollector(this);

            _objectsToSend = new List<RhinoObject>();
            using (GetObject getter = new GetObject())
            {
                getter.EnablePreSelect(true, false);
                getter.SetCommandPrompt("Select all the objects to be pushed through the pipe");
                getter.GroupSelect = true;
                getter.GetMultiple(1, 0);
                if (getter.CommandResult() != Result.Success) { return getter.CommandResult(); }

                for (int i = 0; i < getter.ObjectCount; i++)
                {
                    _objectsToSend.Add(getter.Object(i).Object());
                }
            }

            try
            {
                _pipe.Update();
            }
            catch (Exception e)
            {
                Rhino.UI.Dialogs.ShowMessage(e.Message, "Error");
            }

            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
