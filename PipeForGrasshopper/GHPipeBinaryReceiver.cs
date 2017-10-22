using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;

namespace PipeForGrasshopper
{
    public class GHPipeBinaryReceiver: GH_Component, IPipeEmitter
    {
        private IGH_DataAccess _da;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHPipeBinaryReceiver()
          : base("Binary Pipe Receiver", "BPR",
              "PipeForGrasshopper",
              "Data", "Data Transfer")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pipe Name", "P", "The unique name of the pipe.", GH_ParamAccess.item);
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
            string pipeName = null;

            if(!DA.GetData(0, ref pipeName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Please provide a unique name for the pipe.");
            }

            _da = DA;
            LocalNamedPipe receiverPipe = new LocalNamedPipe(pipeName);
            receiverPipe.SetEmitter(this);
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Waiting to receive the data...");

            receiverPipe.Update();
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Data transfer successful!");
        }

        public void EmitPipeData(DataNode node)
        {
            //IGH_Goo data = (IGH_Goo)node.Data;
            _da.SetData(0, node.Data);
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
