using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

using PipeForRevit.Converters;
using PipeDataModel.Types;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace PipeForRevit
{
    public class PipeForRevit : IExternalApplication
    {
        private static string _baseDir = @"C:\PipeForRevit";
        private static TextBoxData _txtBoxData;
        private static TextBox _textBox;
        private static RevitPipeConverter _converter = new RevitPipeConverter();

        internal static string PipeIdentifier
        {
            get { return _textBox == null ? null : (string)_textBox.Value; }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = application.CreateRibbonPanel("PipeForRevit");
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData btnData = new PushButtonData("PipePush", "Pipe-Push", assemblyPath, "PipeForRevit.RevitPipeSender");
            PushButton btn = panel.AddItem(btnData) as PushButton;
            btn.ToolTip = "Push the data through the pipe";
            BitmapImage image = new BitmapImage(new Uri(Path.Combine(_baseDir, "PipeArrow.png")));
            btn.LargeImage = image;

            _txtBoxData = new TextBoxData("Pipe Identifier");
            _textBox.Width = 100;
            _textBox = panel.AddItem(_txtBoxData) as TextBox;
            _textBox.PromptText = "Pipe Identifier";

            PushButtonData btnData2 = new PushButtonData("PipePull", "Pipe-Pull", assemblyPath, "PipeForRevit.RevitPipeReceiver");
            PushButton btn2 = panel.AddItem(btnData2) as PushButton;
            btn2.ToolTip = "Pull data from the pipe";
            BitmapImage image2 = new BitmapImage(new Uri(Path.Combine(_baseDir, "PipeArrow.png")));
            btn2.LargeImage = image2;

            return Result.Succeeded;
        }

        internal static GeometryObject ConvertFromPipe(IPipeMemberType obj)
        {
            return _converter.FromPipe<GeometryObject, IPipeMemberType>(obj);
        }
        internal static IPipeMemberType ConvertToPipe(GeometryObject obj)
        {
            return _converter.ToPipe<GeometryObject, IPipeMemberType>(obj);
        }
    }
}
