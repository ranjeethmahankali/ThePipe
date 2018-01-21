using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PipeInstaller
{
    class Program
    {
        private static string setupFileDir = "SetupFiles";
        private static string programDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string appAuthorName = "RanjeethMahankali";

        public static string GetHeader()
        {
            StringBuilder str = new StringBuilder("=================================\n");
            str.AppendLine("ThePipe Installer");
            str.AppendLine("=================================");
            str.AppendLine("This Installer can install plugins/extensions of ThePipe for Revit 2017, Dynamo 9x, Rhinoceros 5 and Grasshopper 5.");
            str.AppendLine("To learn what ThePipe is and how to use it watch the video at: https://www.youtube.com/watch?v=20S1--5kT98&t=9s.");
            str.AppendLine("To view the source code and high level documentation, please visit: https://github.com/ranjeethmahankali/ThePipe.");
            str.AppendLine("=================================\n");
            return str.ToString();
        }

        public static void ConsolePause(string message)
        {
            Console.WriteLine(message);
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            //shared setup files
            SetupFile pipeDataModel = new SetupFile("PipeDataModel.dll");
            SetupFile rhPipeConverter = new SetupFile("RhinoPipeConverter.dll");
            /*
             * Then create all the apps - i.e. ThePipe plugins and extensions for all the apps with proper setup files
             */
            //revit pipe addin
            string rvtApp = "PipeForRevit";
            App revitPipe = new App(rvtApp, setupFileDir, @"C:\", "PipeForRevit Installation Folder", true);
            revitPipe.AddSetupFile(pipeDataModel);
            revitPipe.AddSetupFile(new SetupFile("PipeForRevit.dll"));
            revitPipe.AddSetupFile(new SetupFile("PipeArrow.png"));
            revitPipe.AddSetupFile(new SetupFile("PipeForRevit.addin", Path.Combine(programDataDir, @"Autodesk\Revit\Addins\2017"), 
                "Revit Addin Manifests Folder"));

            //dynamo pipe library
            string dynAppName = "PipeForDynamo";
            App dynamoPipe = new App(dynAppName, setupFileDir, Path.Combine(programDataDir, appAuthorName),
                "PipeForDynamo Installation Folder", true);
            dynamoPipe.AddSetupFile(pipeDataModel);
            dynamoPipe.AddSetupFile(new SetupFile("PipeForDynamo.dll"));
            dynamoPipe.Message = string.Format("NOTE: The files have been copied to {0}.\n" +
                "To use the dynamo Pipe library, from within Dynamo, select File > Import Library, browse to the above path\n" +
                "and select PipeForDynamo.dll.");

            //rhino pipe plugin
            string rhinoAppName = "PipeForRhino";
            App rhinoPipe = new App(rhinoAppName, setupFileDir, Path.Combine(programDataDir, appAuthorName),
                "PipeForRhino Installation folder", true);
            rhinoPipe.AddSetupFile(pipeDataModel);
            rhinoPipe.AddSetupFile(rhPipeConverter);
            rhinoPipe.AddSetupFile(new SetupFile("PipeForRhino.rhp"));
            rhinoPipe.Message = string.Format("NOTE: Installation of ThePipe Rhinoceros plugin is NOT finished. The files copied to {0},\n" +
                "but you need to load the RHP file (browse to that path) using Rhino's PluginManager to finish installation.", 
                rhinoPipe.TargetDirectory);

            //grasshopper pipe plugin
            string ghAppName = "PipeForGrasshopper";
            App ghPipe = new App(ghAppName, setupFileDir, Path.Combine(appDataDir, "Grasshopper", "Libraries"),
                "Grasshopper Plugins Folder", false);
            ghPipe.AddSetupFile(pipeDataModel);
            ghPipe.AddSetupFile(rhPipeConverter);
            ghPipe.AddSetupFile(new SetupFile("PipeForGrasshopper.gha"));

            List<App> appList = new List<App> {
                revitPipe,
                dynamoPipe,
                rhinoPipe,
                ghPipe
            };

            Console.WriteLine(GetHeader());

            foreach (var app in appList)
            {
                Console.Write(string.Format("Do you want to install {0} ?(y/n): ", app.Name));
                string response = Console.ReadLine();
                if (response.ToLower() != "y") { continue; }
                try
                {
                    app.Install();
                }
                catch(Exception e)
                {
                    Console.WriteLine(string.Format("ERROR: Installation failed - {0}\n\tAborting.", e.Message));
                }
                Console.WriteLine("================================");
            }

            ConsolePause("The Installer is finished, now exiting.");
        }
    }
}
