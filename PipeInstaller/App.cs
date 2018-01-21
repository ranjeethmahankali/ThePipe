using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PipeInstaller
{
    internal class App
    {
        #region fields
        private string _name, _setupFileDir, _targetDir, _targetDirDesc;
        private string _message = null;
        private bool _createTargetDir;
        List<SetupFile> _setupFiles = new List<SetupFile>();
        List<string> _confirmedPaths = new List<string>();
        #endregion

        #region properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string TargetDirectory
        {
            get { return _targetDir; }
            set { _targetDir = value; }
        }
        public List<SetupFile> SetupFiles
        {
            get { return _setupFiles; }
        }
        public string TargetDirectoryDescription
        {
            get { return _targetDirDesc; }
            set { _targetDirDesc = value; }
        }
        public string SetupFileDirectory
        {
            get { return _setupFileDir; }
            set { _setupFileDir = value; }
        }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public bool CreateTargetDirectory
        {
            get { return _createTargetDir; }
            set { _createTargetDir = value; }
        }
        public string TargetContainerDirectory
        {
            get
            {
                if (Path.GetDirectoryName(_targetDir) == _name)
                {
                    return Directory.GetParent(_targetDir).FullName;
                }
                else
                {
                    return _targetDir;
                }
            }
        }
        public List<string> ConfirmedPaths
        {
            get { return _confirmedPaths; }
        }
        #endregion

        #region constructors
        public App(string name, string setupFileDir, string targetDir, string targetDirDesc, bool pathFlexible)
        {
            _name = name;
            _targetDir = Path.Combine(targetDir, _name);
            _setupFileDir = setupFileDir;
            _targetDirDesc = targetDirDesc;
            _createTargetDir = pathFlexible;
        }
        #endregion

        #region methods
        public void AddSetupFile(SetupFile file, string targetDirPath, string targetDirDesc)
        {
            var file2 = file.Duplicate();
            file2.TargetDirectory = targetDirPath;
            file2.TargetDirectoryDescription = targetDirDesc;

            file2.CreateTargetDirectory = file2.TargetDirectory == _targetDir ? _createTargetDir : false;

            file2.OwnerApp = this;
            _setupFiles.Add(file2);
        }
        public void AddSetupFile(SetupFile file, string targetDirDesc)
        {
            var file2 = file.Duplicate();
            if(file2.TargetDirectory == null)
            {
                file2.TargetDirectory = _targetDir;
            }
            file2.CreateTargetDirectory = _createTargetDir;
            AddSetupFile(file2, file2.TargetDirectory, targetDirDesc);
        }
        public void AddSetupFile(SetupFile file)
        {
            var file2 = file.Duplicate();
            if(file2.TargetDirectoryDescription == null)
            {
                file2.TargetDirectoryDescription = _targetDirDesc;
            }
            file2.CreateTargetDirectory = _createTargetDir;
            AddSetupFile(file, file2.TargetDirectoryDescription);
        }

        //this is the method that installs the app by copying all the setup files to the right places
        public void Install()
        {
            Console.WriteLine(string.Format("\n-----------------------------------\nInstalling {0} ...", _name));
            if (!_createTargetDir)
            {
                Console.Write(string.Format("\nInstalling {0} to the {2} located at {1}.\nEnter 'y' to confirm the path, or else enter " +
                    "the correct path for {2}: ", _name, TargetContainerDirectory, _targetDirDesc));
                string response = Console.ReadLine();
                if(response.ToLower() != "y")
                {
                    if (!Directory.Exists(response))
                    {
                        throw new DirectoryNotFoundException(string.Format("Given {0} at {1} could not be found.", _targetDirDesc, response));
                    }
                    _targetDir = Path.Combine(response, _name);
                }
                _confirmedPaths.Add(_targetDir);
            }
            Directory.CreateDirectory(_targetDir);

            foreach(var file in _setupFiles)
            {
                file.Install();
            }
            if(_message != null)
            {
                Console.WriteLine(string.Format("\n{0}\n", _message));
            }
        }
        //this method will delete all the setup files that were copied to the target paths, in case the installation fails
        //and should be aborted
        public void Uninstall()
        {
            foreach(var file in _setupFiles)
            {
                file.Uninstall();
            }
        }
        #endregion
    }
}
