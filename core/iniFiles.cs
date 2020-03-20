using System;

using System.Collections.Generic;
using System.Text;

namespace WpfCoreCopier
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;


    class IniFile
    {
        #region FromDir

        private string _fromDir;
        public string FromDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fromDir))
                {
                    _fromDir = baseIniFile.ReadINI(ConfSection.dir, "FromDir");
                }
                return _fromDir;
            }

            set
            {
                _fromDir = value;
                if (!string.IsNullOrWhiteSpace(_fromDir))
                { baseIniFile.Write(ConfSection.dir, "FromDir", _fromDir); }
            }
        }
        #endregion

        #region ToDir

        private string _toDir;
        public string ToDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_toDir))
                {
                    _toDir = baseIniFile.ReadINI(ConfSection.dir, "ToDir");
                }
                return _toDir;
            }

            set
            {
                _toDir = value;
                if (!string.IsNullOrWhiteSpace(_toDir))
                { baseIniFile.Write(ConfSection.dir, "ToDir", _toDir); }
            }
        }
        #endregion


        #region IgnoredDir

        private string _IgnoredDir;
        public string IgnoredDir
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_IgnoredDir))
                {
                    _IgnoredDir = baseIniFile.ReadINI(ConfSection.dir, "IgnoredDir");
                }
                return _IgnoredDir;
            }

            set
            {
                _IgnoredDir = value;
                if (!string.IsNullOrWhiteSpace(_IgnoredDir))
                { baseIniFile.Write(ConfSection.dir, "IgnoredDir", _IgnoredDir); }
            }
        }
        #endregion



        private BaseIniFile baseIniFile;

        public IniFile(string IniPath)
        {
            baseIniFile = new BaseIniFile(new FileInfo(IniPath).FullName);
        }


    }


    class BaseIniFile
    {
        string Path; //Имя файла.

        [DllImport("kernel32")] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        // С помощью конструктора записываем пусть до файла и его имя.
        public BaseIniFile(string IniPath)
        {
            Path = new FileInfo(IniPath).FullName.ToString();
        }

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(ConfSection section, string Key)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(section.ToString(), Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }
        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void Write(ConfSection section, string Key, string Value)
        {
            WritePrivateProfileString(section.ToString(), Key, Value, Path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string Key, ConfSection section)
        {
            Write(section, Key, null);
        }
        //Удаляем выбранную секцию
        public void DeleteSection(ConfSection section)
        {
            Write(section, null, null);
        }
        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string Key, ConfSection section)
        {
            return ReadINI(section, Key).Length > 0;
        }
    }

    public enum ConfSection
    {
        dir,
        other
    }

}
