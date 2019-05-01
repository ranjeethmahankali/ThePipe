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
            str.AppendLine("This Installer can install plugins/extensions of ThePipe for Revit, Dynamo 1.3x, Rhinoceros 5," +
                "Rhinoceros 6 and Grasshopper.");
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
            Console.WriteLine(GetHeader());
            //shared setup files
            SetupFile pipeDataModel = new SetupFile("PipeDataModel.dll");
            SetupFile rhPipeConverter = new SetupFile("RhinoPipeConverter.dll");
            SetupFile rhV6PipeConverter = new SetupFile("RhinoV6PipeConverter.dll");
            /*
             * Then create all the apps - i.e. ThePipe plugins and extensions for all the apps with proper setup files
             */
            //revit pipe addin
            string rvtApp = "PipeForRevit";
            App revitPipe = new App(rvtApp, setupFileDir, @"C:\", "PipeForRevit Installation Folder", true);
            revitPipe.AddSetupFile(pipeDataModel);
            revitPipe.AddSetupFile(new SetupFile("PipeForRevit.dll"));
            revitPipe.AddSetupFile(new SetupFile("PipeArrow.png"));
            Console.Write("Do you want to install the Revit plugin for ThePipe ? (y/n): ");
            string response = Console.ReadLine().ToLower().Trim();
            int revitVersion = 2019;
            if(response == "y")
            {
                Console.Write("Which major version of Revit do you use ? (Only input the year and not the number after the dot e.g. 2017, 2018 or 2019): ");
                string versionStr = Console.ReadLine();
                if(!int.TryParse(versionStr, out revitVersion))
                {
                    Console.WriteLine("The version you input could not be parsed into a valid year. The installer will try to " +
                        "install the plugin for 2019 version. Please try running the installer again if that is not what you wanted.");
                    revitVersion = 2019;
                }
            }
            revitPipe.AddSetupFile(new SetupFile("PipeForRevit.addin", Path.Combine(programDataDir, string.Format(@"Autodesk\Revit\Addins\{0}", revitVersion)), 
                "Revit Addin Manifests Folder"));

            //dynamo pipe library
            string dynAppName = "PipeForDynamo";
            App dynamoPipe = new App(dynAppName, setupFileDir, Path.Combine(programDataDir, appAuthorName),
                "PipeForDynamo Installation Folder", true);
            dynamoPipe.AddSetupFile(pipeDataModel);
            dynamoPipe.AddSetupFile(new SetupFile("PipeForDynamo.dll"));
            dynamoPipe.Message = string.Format("NOTE: The files have been copied to {0}.\n" +
                "To use the dynamo Pipe library, from within Dynamo, select File > Import Library, browse to the above path\n" +
                "and select PipeForDynamo.dll.", dynamoPipe.TargetDirectory);

            //rhino pipe plugin
            string rhinoV5AppName = "PipeForRhinoV5";
            App rhinoPipe = new App(rhinoV5AppName, setupFileDir, Path.Combine(programDataDir, appAuthorName),
                "PipeForRhino Installation folder", true);
            rhinoPipe.AddSetupFile(pipeDataModel);
            rhinoPipe.AddSetupFile(rhPipeConverter);
            rhinoPipe.AddSetupFile(new SetupFile("PipeForRhino.rhp"));
            rhinoPipe.Message = string.Format("NOTE: Installation of ThePipe Rhinoceros plugin is NOT finished. The files are copied to {0},\n" +
                "but you need to load the RHP file (browse to that path) using Rhino's PluginManager to finish installation.", 
                rhinoPipe.TargetDirectory);

            //rhino v6 pipe plugin
            string rhinoV6AppName = "PipeForRhinoV6";
            App rhinoV6Pipe = new App(rhinoV6AppName, setupFileDir, Path.Combine(programDataDir, appAuthorName),
                "PipeForRhino Installation folder", true);
            rhinoV6Pipe.AddSetupFile(pipeDataModel);
            rhinoV6Pipe.AddSetupFile(rhV6PipeConverter);
            rhinoV6Pipe.AddSetupFile(new SetupFile("PipeForRhinoV6.rhp"));
            rhinoV6Pipe.Message = string.Format("NOTE: Installation of ThePipe Rhinoceros(6) plugin is NOT finished. The files are copied to {0},\n" +
                "but you need to load the RHP file (browse to that path) using Rhino's PluginManager to finish installation.",
                rhinoV6Pipe.TargetDirectory);

            //grasshopperV5 pipe plugin
            string ghAppName = "PipeForGrasshopperV5";
            App ghPipe = new App(ghAppName, setupFileDir, Path.Combine(appDataDir, "Grasshopper", "Libraries"),
                "Grasshopper Plugins Folder", false);
            ghPipe.AddSetupFile(pipeDataModel);
            ghPipe.AddSetupFile(rhPipeConverter);
            ghPipe.AddSetupFile(new SetupFile("PipeForGrasshopper.gha"));
            ghPipe.Message = "Installtion of GH Pipe plugin for v5 will be overriden by the GH Pipe plugin for v6 if you choose to install v6." +
                "use this if you use GH with Rhino 5 and skip the GH v6 installtion";

            //grasshopperV6 pipe plugin
            string ghV6AppName = "PipeForGrasshopperV6";
            App ghPipeV6 = new App(ghV6AppName, setupFileDir, Path.Combine(appDataDir, "Grasshopper", "Libraries"),
                "Grasshopper Plugins Folder", false);
            ghPipeV6.AddSetupFile(pipeDataModel);
            ghPipeV6.AddSetupFile(rhV6PipeConverter);
            ghPipeV6.AddSetupFile(new SetupFile("PipeForGrasshopperV6.gha"));
            ghPipeV6.Message = "If you already installed GH Pipe plugin for v5, it was just overwritten by the v6 installtion just now." +
                "This will work if you use GH with Rhino 6.";

            List<App> appList = new List<App> {
                revitPipe,
                dynamoPipe,
                rhinoPipe,
                rhinoV6Pipe,
                ghPipe,
                ghPipeV6
            };

            foreach (var app in appList)
            {
                Console.Write(string.Format("Do you want to install {0} ?(y/n): ", app.Name));
                response = Console.ReadLine();
                if (response.ToLower() != "y") { continue; }
                try
                {
                    app.Install();
                }
                catch(Exception e)
                {
                    Console.WriteLine(string.Format("ERROR: Installation failed - {0}\n\tAborting.", e.Message));
                    app.Uninstall();
                }
                Console.WriteLine("================================");
            }

            ConsolePause("The Installer is finished, now exiting.");
        }
    }
}
