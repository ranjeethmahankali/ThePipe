using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;
using Grasshopper;

namespace PipeForGrasshopper
{
    public class GHPipeBinaryReceiver: GH_Component, IPipeEmitter
    {
        private DataNode _oldData = null, _newData = null;
        private LocalNamedPipe _receiverPipe;
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
            if(_receiverPipe != null)
            {
                _receiverPipe.ClosePipe();
                _receiverPipe = null;
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
            pManager.AddGenericParameter("Data", "D", "Data received", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeName = Params.Output[0].NickName;

            if(_receiverPipe != null && _receiverPipe.Name != pipeName)
            {
                _receiverPipe.ClosePipe();
                _receiverPipe = null;
            }
            if (_receiverPipe == null)
            {
                _receiverPipe = new LocalNamedPipe(pipeName);
                _receiverPipe.SetEmitter(this);
            }
            _receiverPipe.Update();

            if (_newData != null)
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "received new data.");
                DA.SetData(0, _newData.Data);
                _oldData = _newData.Duplicate();
                _newData = null;
            }
            else if (_newData == null && _oldData != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "did not receive any new data.");
                DA.SetData(0, _oldData.Data);
            }
            else if (_newData == null && _oldData == null)
            {
                DA.AbortComponentSolution();
            }
        }

        public void EmitPipeData(DataNode node)
        {
            _newData = node;
        }

        public object ConvertFromPipe(IPipeMemberType pipeObj)
        {
            //incomplete
            throw new NotImplementedException();
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
