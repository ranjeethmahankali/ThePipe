using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PipeForGrasshopper
{
    public class PipeForGrasshopperV6Info : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PipeForGrasshopperV6";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("96febb0c-aa65-4390-b338-e8c5889a24c5");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
