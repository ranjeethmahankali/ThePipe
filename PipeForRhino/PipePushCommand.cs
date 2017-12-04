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
    [System.Runtime.InteropServices.Guid("04041fc2-c18a-4d91-b25f-c51def3e6773")]
    public class PipePushCommand : Command, IPipeCollector
    {
        private List<RhinoObject> _objectsToSend;
        private Pipe _pipe;

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
            foreach(var obj in _objectsToSend)
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
                if (getter.Get() != GetResult.String)
                {
                    RhinoApp.WriteLine("Invalid Input");
                    return getter.CommandResult();
                }
                pipeIdentifier = getter.StringResult();
            }

            Action finisher = () =>
            {
                _pipe.ClosePipe();
                _pipe = null;
            };

            if (_pipe != null)
            {
                finisher.Invoke();
            }

            if (PipeDataUtil.IsValidUrl(pipeIdentifier))
            {
                _pipe = new MyWebPipe(pipeIdentifier, finisher);
            }
            else
            {
                _pipe = new LocalNamedPipe(pipeIdentifier, finisher);
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

            _pipe.Update();
            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
