using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using PipeDataModel.Types;
using PipeDataModel.DataTree;
using PipeDataModel.Pipe;
using Grasshopper;

namespace PipeForGrasshopper
{
    public class GHPipeBinarySender : GH_Component, IPipeCollector
    {
        private LocalNamedPipe _localSenderPipe;
        private WebPipe _webPipe;
        private List<IGH_Goo> _pipeData;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHPipeBinarySender()
          : base("LocalBinaryPipeSender", "LBPS",
              "PipeForGrasshopper",
              "Data", "Data Transfer")
        {
            Params.ParameterChanged += new GH_ComponentParamServer.ParameterChangedEventHandler(OnParameterChange);
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            CloseLocalPipe();
            if(_webPipe != null)
            {
                _webPipe.ClosePipe();
                _webPipe = null;
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
            Param_GenericObject parameter = new Param_GenericObject();
            parameter.Name = "Pipe Input";
            parameter.NickName = "pipe_name_" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            parameter.Description = "Input for the pipe.";
            parameter.Access = GH_ParamAccess.list;
            parameter.DataMapping = GH_DataMapping.Flatten;
            pManager.AddParameter(parameter);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Pipe Status", "S", "Status of the data transfer over the pipe.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_Goo> data = new List<IGH_Goo>();
            string pipeIdentifier = Params.Input[0].NickName;
            if (!DA.GetDataList(0, data))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "please provide some data to send over the pipe.");
            }

            _pipeData = data;

            Uri uriResult;
            bool isWebUrl = Uri.TryCreate(pipeIdentifier, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (isWebUrl)
            {
                SendViaWebPipe(pipeIdentifier);
            }
            else
            {
                SendViaLocalPipe(pipeIdentifier);
            }
        }

        private void CloseLocalPipe()
        {
            if(_localSenderPipe != null)
            {
                _localSenderPipe.ClosePipe();
                _localSenderPipe = null;
            }
        }

        private void SendViaLocalPipe(string pipeName)
        {
            Action finishingDelegate = () =>
            {
                ClearRuntimeMessages();
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Data Transfer finished");
                ExpireSolution(true);
            };

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "waiting for listener...");
            //if(_senderPipe != null) { _senderPipe.ClosePipe(); }
            if (_localSenderPipe != null && _localSenderPipe.Name != pipeName)
            {
                CloseLocalPipe();
            }
            if (_localSenderPipe == null)
            {
                _localSenderPipe = new LocalNamedPipe(pipeName, finishingDelegate);
                _localSenderPipe.SetCollector(this);
            }
            _localSenderPipe.Update();
        }

        private void SendViaWebPipe(string pipeUrl)
        {
            CloseLocalPipe();
            Action finishingDelegate = () =>
            {
                ClearRuntimeMessages();
                if (_webPipe.DataPostedToUrlSuccessful)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Data posted to the url");
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "failed to post the data to url");
                }
            };
            //now send the data to the webpipe
            if (_webPipe == null || (_webPipe != null && _webPipe.Url != pipeUrl))
            {
                _webPipe = new WebPipe(pipeUrl, finishingDelegate);
                _webPipe.SetCollector(this);
            }
            _webPipe.Update();
        }

        public DataNode CollectPipeData()
        {
            DataNode node = new DataNode(new PipeData(null));
            for (int i = 0; i < _pipeData.Count; i++)
            {
                node.AddChild(new DataNode(ConvertToPipe.ConvertObject(_pipeData[i])));
            }
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
