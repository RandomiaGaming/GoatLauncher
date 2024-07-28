using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GoatLauncher
{
    public sealed class GoatLauncher
    {
        #region Private Constants
        private const string Goat2ExePath = "C:\\Program Files\\Epic Games\\GoatSimulator3\\Goat2.exe";
        private static readonly string Goat2FolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Goat2";
        private static readonly string GoatLauncherFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\GoatLauncher";
        private const string GoatTagFileName = "GoatTag.txt";
        #endregion
        #region Public Variables
        public List<SaveMeta> SaveList = new List<SaveMeta>(); //Plz no write
        #endregion
        #region Public Constructors
        public GoatLauncher()
        {
            if (!Directory.Exists(GoatLauncherFolderPath))
            {
                Directory.CreateDirectory(GoatLauncherFolderPath);
            }
            if (!Directory.Exists(Goat2FolderPath))
            {
                Directory.CreateDirectory(Goat2FolderPath);
            }

            string[] savePaths = Directory.GetDirectories(GoatLauncherFolderPath);
            foreach (string savePath in savePaths)
            {
                SaveMeta newSave = new SaveMeta();

                newSave.SaveFolderPath = savePath;

                string goatTagPath = $"{newSave.SaveFolderPath}\\{GoatTagFileName}";
                if (!File.Exists(goatTagPath))
                {
                    File.WriteAllText(goatTagPath, "Unnamed Save File");
                }

                newSave.Name = File.ReadAllText(goatTagPath);

                SaveList.Add(newSave);
            }

            SaveMeta loadedSave = new SaveMeta();

            loadedSave.SaveFolderPath = Goat2FolderPath;

            string loadedGoatTagPath = $"{loadedSave.SaveFolderPath}\\{GoatTagFileName}";
            if (!File.Exists(loadedGoatTagPath))
            {
                File.WriteAllText(loadedGoatTagPath, "Unnamed Save File");
            }

            loadedSave.Name = File.ReadAllText(loadedGoatTagPath);

            SaveList.Add(loadedSave);
        }
        #endregion
        #region Public Methods
        public bool LaunchSave(SaveMeta target)
        {
            if (!target.IsLoaded)
            {
                if (ProcessHelper.IsGoatSimulatorOpen())
                {
                    return false;
                }

                SaveMeta oldSave = null;
                for (int i = 0; i < SaveList.Count; i++)
                {
                    if (SaveList[i].IsLoaded)
                    {
                        oldSave = SaveList[i];
                    }
                }

                Directory.Move(oldSave.SaveFolderPath, $"{GoatLauncherFolderPath}\\Temp");
                Directory.Move(target.SaveFolderPath, oldSave.SaveFolderPath);
                Directory.Move($"{GoatLauncherFolderPath}\\Temp", target.SaveFolderPath);

                string temp = oldSave.SaveFolderPath;
                oldSave.SaveFolderPath = target.SaveFolderPath;
                target.SaveFolderPath = temp;
            }

            Process.Start(Goat2ExePath);

            return true;
        }
        public bool DeleteSave(SaveMeta target)
        {
            if (target.IsLoaded && ProcessHelper.IsGoatSimulatorOpen())
            {
                return false;
            }

            FileSystem.DeleteDirectory(target.SaveFolderPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            SaveList.Remove(target);

            if (target.IsLoaded)
            {
                if (SaveList.Count is 0)
                {
                    SaveMeta newSave = new SaveMeta();
                    newSave.Name = "Unnamed Save";
                    newSave.SaveFolderPath = Goat2FolderPath;
                    Directory.CreateDirectory(newSave.SaveFolderPath);
                    File.WriteAllText($"{newSave.SaveFolderPath}\\{GoatTagFileName}", newSave.Name);
                    SaveList.Add(newSave);
                }
                else
                {
                    SaveMeta newSave = SaveList[0];
                    Directory.Move(newSave.SaveFolderPath, Goat2FolderPath);
                    newSave.SaveFolderPath = Goat2FolderPath;
                }
            }

            return true;
        }
        public bool RenameSave(SaveMeta target, string newName)
        {
            File.WriteAllText($"{target.SaveFolderPath}\\{GoatTagFileName}", newName);

            target.Name = newName;

            return true;
        }
        public bool CreateSave()
        {
            SaveMeta newSave = new SaveMeta();
            newSave.Name = "Unnamed Save";

            int id = 0;
            while (true)
            {
                newSave.SaveFolderPath = $"{GoatLauncherFolderPath}\\{id}";
                if (!Directory.Exists(newSave.SaveFolderPath))
                {
                    break;
                }
                id++;
            }

            Directory.CreateDirectory(newSave.SaveFolderPath);
            File.WriteAllText($"{newSave.SaveFolderPath}\\{GoatTagFileName}", newSave.Name);
            SaveList.Add(newSave);

            return true;
        }
        #endregion
        #region Public Subclasses
        public sealed class SaveMeta
        {
            public string Name { get; set; } = "Unnamed Save Slot"; //Plz no write
            public string SaveFolderPath = ""; //Plz no write
            public bool IsLoaded => SaveFolderPath == Goat2FolderPath;
        }
        #endregion
    }
}
