using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;

namespace PipeForGrasshopper
{
    public class GHPipeBinarySender : GH_Component, IPipeCollector
    {
        private IGH_Goo _pipeData;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHPipeBinarySender()
          : base("Binary Pipe Sender", "BPS",
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
            pManager.AddGenericParameter("Data", "D", "Data to send over the pipe.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Pipe Status", "S", "Status of the data transfer over the pipe.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeName = null;
            IGH_Goo data = null;

            if(!DA.GetData(0, ref pipeName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Please provide a unique name for the pipe.");
            }
            if(!DA.GetData(1, ref data))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "please provide some data to send over the pipe.");
            }

            if (!typeof(GH_String).IsAssignableFrom(data.GetType()))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only strings are supported at this stage!");
                return;
            }

            _pipeData = (GH_String)data;

            LocalNamedPipe senderPipe = new LocalNamedPipe(pipeName);
            senderPipe.SetCollector(this);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Waiting for the data to go through the pipe...");
            Action finishingDelegate = () =>
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Blank, "");
                DA.SetData(0, "Data transfer successful!");
            };
            Common.UpdatePipeAsync(senderPipe, DA, finishingDelegate);
        }

        public DataNode CollectPipeData()
        {
            string msg = _pipeData.ToString();
            DataNode node = new DataNode(new PipeData(msg));
            return node;
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
            get { return new Guid("046a5a93-32d5-499b-981d-189d3a0997f8"); }
        }
    }
}
