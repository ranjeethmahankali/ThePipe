using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PipeForGrasshopper
{
    public class PipeForGrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PipeForGrasshopper";
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
                return new Guid("2a8ba2ae-c313-4342-ad2c-e8be35f90fab");
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
