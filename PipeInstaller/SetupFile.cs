using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PipeInstaller
{
    internal class SetupFile
    {
        #region fields
        private string _fileName = null, _targetDir = null, _targetDirDesc = null;
        private App _ownerApp = null;
        private bool _createTargetDir = false;
        #endregion

        #region properties
        public string TargetDirectory
        {
            get { return _targetDir; }
            set { _targetDir = value; }
        }
        public string TargetDirectoryDescription
        {
            get { return _targetDirDesc; }
            set { _targetDirDesc = value; }
        }
        public App OwnerApp
        {
            get { return _ownerApp; }
            set { _ownerApp = value; }
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
                if(Path.GetDirectoryName(_targetDir) == _ownerApp.Name)
                {
                    return Directory.GetParent(_targetDir).FullName;
                }
                else
                {
                    return _targetDir;
                }
            }
        }
        public string SourcePath
        {
            get { return Path.Combine(_ownerApp.SetupFileDirectory, _fileName); }
        }
        public string TargetPath
        {
            get { return Path.Combine(_targetDir, _fileName); }
        }
        #endregion

        #region constructors
        public SetupFile(string fileName)
        {
            _fileName = fileName;
        }
        public SetupFile(string fileName, string targetDirDesc)
        {
            _fileName = fileName;
            _targetDirDesc = targetDirDesc;
        }
        public SetupFile(string fileName, string targetDir, string targetDirDesc)
        {
            _fileName = fileName;
            _targetDir = targetDir;
            _targetDirDesc = targetDirDesc;
        }
        #endregion

        #region methods
        public SetupFile Duplicate()
        {
            var copy = new SetupFile(_fileName, _targetDir, _targetDirDesc);
            copy.CreateTargetDirectory = _createTargetDir;
            return copy;
        }
        public void Install()
        {
            if(_targetDir == null)
            {
                throw new InvalidOperationException("The target directory for this file is not set.");
            }
            if (_targetDirDesc == null)
            {
                throw new InvalidOperationException("The target directory description for this file is not set.");
            }
            if(_ownerApp == null)
            {
                throw new InvalidOperationException("The owner app for this file is not set.");
            }

            if (!File.Exists(SourcePath))
            {
                throw new FileNotFoundException("The setup file is missing.");
            }

            if (!_createTargetDir && !_ownerApp.ConfirmedPaths.Contains(_targetDir))
            {
                Console.Write(string.Format("\nCopying {0} to the {2} located at {1}.\nEnter 'y' to confirm the path, or else enter the correct" +
                    " path for {2}: ", _fileName, _targetDir, _targetDirDesc));
                string response = Console.ReadLine();
                if (response.ToLower() != "y")
                {
                    if (!Directory.Exists(response))
                    {
                        throw new DirectoryNotFoundException(string.Format("Given {0} at {1} could not be found.", _targetDirDesc, response));
                    }
                    _targetDir = Path.Combine(response);
                }
            }
            InitializeTargetDirectory();

            //Debug.WriteLine(string.Format("{0}, {1}", SourcePath, TargetPath));
            Console.Write(string.Format("\t...Copying file {0}", TargetPath));
            File.Copy(SourcePath, TargetPath, true);
            Console.WriteLine("...done.");
        }

        public void Uninstall()
        {
            if (File.Exists(TargetPath))
            {
                File.Delete(TargetPath);
            }
        }

        public void InitializeTargetDirectory()
        {
            if(!_createTargetDir && !Directory.Exists(TargetContainerDirectory))
            {
                throw new DirectoryNotFoundException(string.Format("The directory {0} could not be found.", _targetDir));
            }
            Directory.CreateDirectory(_targetDir);
        }
        #endregion
    }
}
