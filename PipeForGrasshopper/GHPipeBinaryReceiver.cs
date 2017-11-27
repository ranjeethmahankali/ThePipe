using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;
using Grasshopper;

namespace PipeForGrasshopper
{
    public class GHPipeBinaryReceiver: GH_Component, IPipeEmitter
    {
        private List<IGH_Goo> _oldData = null, _newData = null;
        private LocalNamedPipe _localReceiverPipe;
        private MyWebPipe _webPipe;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHPipeBinaryReceiver()
          : base("LocalBinaryPipeReceiver", "LBPR",
              "PipeForGrasshopper",
              "Data", "Data Transfer")
        {
            Params.ParameterChanged += new GH_ComponentParamServer.ParameterChangedEventHandler(OnParameterChange);
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            if(_localReceiverPipe != null)
            {
                _localReceiverPipe.ClosePipe();
                _localReceiverPipe = null;
            }
        }

        protected virtual void OnParameterChange(object sender, GH_ParamServerEventArgs e)
        {
            ExpireSolution(true);
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddTextParameter("Pipe Name", "P", "The unique name of the pipe.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            Param_GenericObject parameter = new Param_GenericObject();
            parameter.Name = "Pipe Output";
            parameter.NickName = "pipe_name";
            parameter.Description = "Output from the pipe.";
            parameter.Access = GH_ParamAccess.list;
            parameter.DataMapping = GH_DataMapping.Flatten;
            pManager.AddParameter(parameter);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeIdentifier = Params.Output[0].NickName;

            Uri uriResult;
            bool isWebUrl = Uri.TryCreate(pipeIdentifier, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isWebUrl)
            {
                PullFromWebPipe(pipeIdentifier);
            }
            else
            {
                PullFromLocalPipe(pipeIdentifier);
            }

            if (_newData != null)
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "received new data.");
                DA.SetDataList(0, _newData);
                _oldData = _newData.Select(x => x?.Duplicate()).ToList();
                _newData = null;
            }
            else if (_newData == null && _oldData != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "did not receive any new data.");
                DA.SetDataList(0, _oldData);
            }
            else if (_newData == null && _oldData == null)
            {
                DA.AbortComponentSolution();
            }
        }

        private void PullFromLocalPipe(string pipeName)
        {
            if (_localReceiverPipe != null && _localReceiverPipe.Name != pipeName)
            {
                _localReceiverPipe.ClosePipe();
                _localReceiverPipe = null;
            }
            if (_localReceiverPipe == null)
            {
                _localReceiverPipe = new LocalNamedPipe(pipeName);
                _localReceiverPipe.SetEmitter(this);
            }
            _localReceiverPipe.Update();
        }

        private void PullFromWebPipe(string pipeUrl)
        {
            if (_webPipe == null || (_webPipe != null && _webPipe.Url != pipeUrl))
            {
                _webPipe = new MyWebPipe(pipeUrl);
                _webPipe.SetEmitter(this);
            }
            _webPipe.Update();
        }

        public void EmitPipeData(DataNode node)
        {
            if(node == null) { return; }
            _newData = new List<IGH_Goo>();
            foreach(var child in node.ChildrenList)
            {
                _newData.Add(GHPipeConverter.FromPipe(child.Data));
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2b8291e6-7c39-413b-bcc4-694d371e0698"); }
        }
    }
}
