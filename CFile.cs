using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GalaSoft.MvvmLight;

namespace WpfCoreCopier
{
    public class CFile : ObservableObject
    {
        public string DistPath { get; }
        public string SourcePath { get; }
        #region DistFileInfo
        private FileInfo _distFileInfo;
        public FileInfo DistFileInfo
        {
            get
            {
                if (_distFileInfo == null)
                {
                    if (File.Exists(DistPath))
                    {
                        _distFileInfo = new FileInfo(DistPath);
                    }
                }
                return _distFileInfo;
            }
        }
        #endregion
        #region SourceFileInfo
        private FileInfo _sourceFileInfo;
        public FileInfo SourceFileInfo
        {
            get
            {
                if (_sourceFileInfo == null)
                {
                    if (File.Exists(SourcePath))
                    {
                        _sourceFileInfo = new FileInfo(SourcePath);
                    }
                }
                return _sourceFileInfo;
            }
        }
        #endregion
        #region DistCheckSummInfo
        private string _distCheckSumm;
        public string DistCheckSumm
        {
            get
            {
                if (_distCheckSumm == null)
                {
                    if (File.Exists(DistPath))
                    {
                        _distCheckSumm = ComputeMD5Checksum(DistPath);
                    }
                }
                return _distCheckSumm;
            }
        }
        #endregion
        #region SourceCheckSummInfo
        private string _sourceCheckSumm;
        public string SourceCheckSumm
        {
            get
            {
                if (_sourceCheckSumm == null)
                {
                    if (File.Exists(SourcePath))
                    {
                        _sourceCheckSumm = ComputeMD5Checksum(SourcePath);
                    }
                }
                return _sourceCheckSumm;
            }
        }
        #endregion
        public string Name
        {
            get { return System.IO.Path.GetFileName(DistPath); }
        }

        public CFile(string distPath, string sourcePath)
        {
            DistPath = distPath;
            SourcePath = sourcePath;
        }

        private bool? _needReload;
        private string _error;

        public bool NeedReload
        {
            get
            {
                if (_needReload == null)
                {
                    _needReload = true;
                    if (DistFileInfo != null && SourceFileInfo != null)
                    {
                        if (DistFileInfo.Length == SourceFileInfo.Length)
                        {
                            if (SourceCheckSumm == DistCheckSumm)
                            {
                                _needReload = false;
                                DifType = DifType.Equal;
                            }
                            else
                            {
                                DifType = DifType.ByCheckSumm;
                            }
                        }
                        else
                        {
                            DifType = DifType.BySize;
                        }
                    }
                    else
                    {
                        DifType = DifType.FileDontExist;
                    }
                }
                return _needReload == true;
            }
        }

      //  private string _difTypeDescription;
        public string DifTypeDescription
        {
            get
            {
                string _difTypeDescription = null;
                {
                    switch (DifType)
                    {
                        case DifType.Equal:_difTypeDescription=String.Empty; break;
                        case DifType.FileDontExist:
                            _difTypeDescription = $"Файл отсутствует";  break;
                        case DifType.BySize:
                            _difTypeDescription = $"Размер файла получателя {DistFileInfo.Length}   ";  break;
                        case DifType.ByCheckSumm:
                            _difTypeDescription = $"Разная контрольная сумма ";  break;
                    }
                }
                return _difTypeDescription;
            }

        }
        DifType DifType { get; set; }

        public string Error
        {
            get => _error;
            internal set
            {
                _error = value; RaisePropertyChanged();
            }
        }

        public void Refresh()
        {
            Error = null;
            _sourceCheckSumm = null;
            _sourceFileInfo = null;
            _distCheckSumm = null;
            _needReload = null;
            _distFileInfo = null;
            RaisePropertyChanged(nameof(DifTypeDescription));
            RaisePropertyChanged(nameof(NeedReload));
        }

        private static string ComputeMD5Checksum(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }
    }


    public enum DifType
    {
        Equal,
        FileDontExist,
        BySize,
        ByCheckSumm
    }

}
