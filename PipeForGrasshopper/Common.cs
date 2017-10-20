using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using PipeDataModel.Pipe;

namespace PipeForGrasshopper
{
    public class Common
    {
        public static async Task UpdatePipeAsync(Pipe pipe, IGH_DataAccess DA, Action finishingDelegate)
        {
            await Task.Run(() => pipe.Update());
            finishingDelegate.Invoke();
        }
    }
}
