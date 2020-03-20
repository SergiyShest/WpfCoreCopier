using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfCoreCopier
{
    public class ViewModel : ViewModelBase
    {
        #region AllFiles
        private List<CFile> _allFiles;

        public List<CFile> AllFiles
        {

            get
            {

                if (_allFiles == null)
                {
                    _allFilesCollectionView = null;
                    _allFiles = new List<CFile>();
                    var files = Directory.GetFiles(FromDir, "", SearchOption.AllDirectories);
                    foreach (var filePath in files)
                    {
                        if (isIgnored(filePath)) continue;
                        var distFilePath = GetDistFilePath(filePath, FromDir, ToDir);
                        _allFiles.Add(new CFile(distFilePath, filePath));
                    }
                }
                return _allFiles;
                bool isIgnored(string filePath)
                {
                    foreach (var dir in IgnoredDirFilesAndExt)
                    {
                        if (filePath.Contains(dir)) return true;
                    }
                    return false;
                }
            }
        }

        private ICollectionView _allFilesCollectionView;
     public   ICollectionView AllFilesCollectionView
        {
            get
            {

                if (_allFilesCollectionView == null)
                {
                    _allFilesCollectionView = CollectionViewSource.GetDefaultView(AllFiles);
                    _allFilesCollectionView.Filter = Filter;
                }
                return _allFilesCollectionView;
            }
        }

     private bool Filter(object obj)
     {
         if (ShowAll) return true;
         var file = obj as CFile;
         return file.NeedReload;
     }




        #endregion

        private bool _showAll;
        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                _showAll = value;
                RaisePropertyChanged();
                AllFilesCollectionView.Refresh();
            }
        }



        #region FromDir

        //   private string _fromDir;

        public string FromDir
        {
            get => INI.FromDir;
            set
            {
                INI.FromDir = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ToDir



        public string ToDir
        {
            get => INI.ToDir;
            set
            {
                INI.ToDir = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region IgnoredDirFilesAndExt

        public string SelectedIgnored
        {
            get => _selectedIgnored;
            set
            {
                _selectedIgnored = value;
                RaisePropertyChanged();
                DeleteIgnoredCommand = new RelayCommand(DeleteIgnoredMethod, CanAddIgnoredFileMethod);
            }
        }

        public List<string> IgnoredDirFilesAndExt
        {
            get => INI.IgnoredDir.Split(';').ToList();
            set
            {
                INI.IgnoredDir = String.Join(";", value);

                RaisePropertyChanged();
                _allFiles = null;
                RaisePropertyChanged(nameof(AllFiles));
            }
        }

        #endregion

        IniFile INI = new IniFile("config.ini");
        private string _selectedIgnored;
        private ICommand _deleteIgnoredCommand;

        public ViewModel()
        {
            CopyCommand = new RelayCommand(CopyMethod);
            SetFromCommand = new RelayCommand(FromMethod);
            SetToCommand = new RelayCommand(ToMethod);
            AddIgnoredDirCommand = new RelayCommand(AddIgnoredDirMethod);
            AddIgnoredFileNameCommand = new RelayCommand(AddIgnoredFileMethod);
            DeleteIgnoredCommand = new RelayCommand(DeleteIgnoredMethod, CanAddIgnoredFileMethod);
            RefreshCommand = new RelayCommand(RefreshMethod);
        }

        public ICommand CopyCommand { get; private set; }
        public ICommand SetFromCommand { get; private set; }
        public ICommand SetToCommand { get; private set; }
        public ICommand AddIgnoredDirCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand AddIgnoredFileNameCommand { get; private set; }

        public ICommand DeleteIgnoredCommand
        {
            get => _deleteIgnoredCommand;
            private set { _deleteIgnoredCommand = value; RaisePropertyChanged(); }
        }

        public void SaveEmployeesMethod()
        {
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Employees Saved."));
        }
        private void CopyMethod()
        {
            int fCount = 0;
            foreach (var file in AllFiles)
            {
                if (!file.NeedReload) continue;
                var dict = GetDistFilePath(file.SourcePath, FromDir, ToDir);
                var bytes = File.ReadAllBytes(file.SourcePath);
                try
                {
                    var dir = Path.GetDirectoryName(dict);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllBytes(dict, bytes);

                }
                catch (Exception ex)
                {
                    file.Error = ex.Message;
                    Debug.WriteLine(dict + " " + ex.Message);

                }
                file.Refresh();
            }

            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(fCount + " files copied."));
        }
        string GetDistFilePath(string filePath, string sourseDir, string distDir)
        {
            var res = string.Empty;
            res = filePath.Substring(sourseDir.Length, filePath.Length - sourseDir.Length);
            return distDir + res;
        }

        private void ToMethod()
        {
            var dir = SelectDir();
            if (dir != null)
            {
                ToDir = dir;
            }
        }

        private void FromMethod()
        {
            var dir = SelectDir();
            if (dir != null)
            {
                FromDir = dir;
            }
        }

        private string SelectDir()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                return dialog.FileNames.ToList()[0];
            }
            return null;

        }

        private void AddIgnoredDirMethod()
        {
            var dir = SelectDir();
            if (dir != null)
            {
                var res = dir.Substring(FromDir.Length, dir.Length - FromDir.Length);
                var ign = IgnoredDirFilesAndExt;
                ign.Add(res);
                IgnoredDirFilesAndExt = ign;
            }
        }

        private void AddIgnoredFileMethod()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = false;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    var res = fileName.Substring(FromDir.Length, fileName.Length - FromDir.Length);
                    var ign = IgnoredDirFilesAndExt;
                    ign.Add(res);
                    IgnoredDirFilesAndExt = ign;
                }
            }
        }
        private bool CanAddIgnoredFileMethod()
        {
            return SelectedIgnored != null;
        }
        private void DeleteIgnoredMethod()
        {
            var ign = IgnoredDirFilesAndExt;
            ign.Remove(SelectedIgnored);
            IgnoredDirFilesAndExt = ign;
        }

 private void RefreshMethod()
 {
     Parallel.ForEach(AllFiles, (currentFile) => {currentFile.Refresh(); });
     AllFilesCollectionView.Refresh();

 }


    }
}
