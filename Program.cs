using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using ScaleForms;
using System.Diagnostics;

namespace GoatLauncher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GoatLauncherGUI());
        }
    }
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
                if (IsGoatSimulatorOpen())
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

            //Launch goat simulator
            Process.Start(Goat2ExePath);

            return true;
        }
        public bool DeleteSave(SaveMeta target)
        {
            if (target.IsLoaded && IsGoatSimulatorOpen())
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
            if (target.IsLoaded && IsGoatSimulatorOpen())
            {
                return false;
            }

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
        public bool IsGoatSimulatorOpen()
        {
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string processPath = process.MainModule.FileName;
                        if (processPath.EndsWith("Goat2.exe") || processPath.EndsWith("Goat2-Win64-Shipping.exe") || processPath.EndsWith("Goat2-Win32-Shipping.exe"))
                        {
                            return true;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }

            return false;
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
    public sealed class GoatLauncherGUI : Form
    {
        #region Private Constants
        private const double RowHeight = 0.1; // The height of one row in screens.
        private const double ScrollDelta = RowHeight / 2.0; // The distance scrolled with one click in screens.
        #endregion
        #region Private Variables
        private GoatLauncher _goatLauncher;

        private GUIRow[] _guiRows;

        private Scaled<Panel> _scrollContainer;
        private Scaled<Panel> _addButtonRow;
        private Scaled<Button> _addButton;
        #endregion
        #region Public Constructors
        public GoatLauncherGUI()
        {
            _goatLauncher = new GoatLauncher();

            //Create form
            Text = "Goat Launcher 2.0";
            BackColor = Color.LightPink;
            Size = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);
            Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width / 4, Screen.PrimaryScreen.Bounds.Height / 4);
            MouseWheel += (object sender, MouseEventArgs e) => { ScrollContent(e.Delta); };
            ActiveControl = null;
            GotFocus += (object o, EventArgs e) => { ActiveControl = null; };

            //Create listContainer
            _scrollContainer = new Scaled<Panel>(this, 0.0, 0.0, 1.0, 1.0);
            _scrollContainer.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            _scrollContainer.Control.BackColor = Color.Transparent;

            //Create buttons
            ReloadGUI();
        }
        #endregion
        #region Private Methods
        private void ReloadGUI()
        {
            //Delete old GUI
            if (!(_guiRows is null))
            {
                for (int i = 0; i < _guiRows.Length; i++)
                {
                    GUIRow guiRow = _guiRows[i];

                    if (!(guiRow.PlayButton is null))
                    {
                        guiRow.PlayButton.Destroy();
                    }
                    if (!(guiRow.DeleteButton is null))
                    {
                        guiRow.DeleteButton.Destroy();
                    }
                    if (!(guiRow.RenameButton is null))
                    {
                        guiRow.RenameButton.Destroy();
                    }
                    if (!(guiRow.ContentContainer is null))
                    {
                        guiRow.ContentContainer.Destroy();
                    }
                    if (!(guiRow.RowContainer is null))
                    {
                        guiRow.RowContainer.Destroy();
                    }
                }
            }

            if (!(_addButton is null))
            {
                _addButton.Destroy();
            }
            if (!(_addButtonRow is null))
            {
                _addButtonRow.Destroy();
            }

            //Create rows from save list.
            _guiRows = new GUIRow[_goatLauncher.SaveList.Count];
            for (int i = 0; i < _guiRows.Length; i++)
            {
                GUIRow guiRow = new GUIRow();
                guiRow.TargetSave = _goatLauncher.SaveList[i];
                _guiRows[i] = guiRow;
            }

            //Init new GUI

            for (int i = 0; i < _guiRows.Length; i++)
            {
                GUIRow guiRow = _guiRows[i];

                guiRow.TargetSave = _goatLauncher.SaveList[i];

                guiRow.RowContainer = new Scaled<Panel>(_scrollContainer, 0.0, RowHeight * i, 1.0, 0.1);
                guiRow.ContentContainer = new Scaled<Panel>(guiRow.RowContainer, 0.05, 0.1, 0.9, 0.8);
                guiRow.PlayButton = new Scaled<Button>(guiRow.ContentContainer, 0.0, 0.0, 0.8, 1);
                guiRow.DeleteButton = new Scaled<Button>(guiRow.ContentContainer, 0.8, 0.0, 0.1, 1);
                guiRow.RenameButton = new Scaled<Button>(guiRow.ContentContainer, 0.9, 0.0, 0.1, 1);

                guiRow.RowContainer.Control.Name = $"{guiRow.TargetSave.Name} - RowContainer";
                guiRow.RowContainer.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                if (_goatLauncher.SaveList[i].IsLoaded)
                {
                    guiRow.RowContainer.Control.BackColor = Color.LimeGreen;
                }
                else
                {
                    guiRow.RowContainer.Control.BackColor = Color.Transparent;
                }

                guiRow.ContentContainer.Control.Name = $"{guiRow.TargetSave.Name} - ContentContainer";
                guiRow.ContentContainer.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                guiRow.ContentContainer.Control.BackColor = Color.Transparent;

                guiRow.PlayButton.Control.Name = $"{guiRow.TargetSave.Name} - PlayButton";
                guiRow.PlayButton.Control.Click += (object o, EventArgs e) => { OnLoad(guiRow.TargetSave); };
                guiRow.PlayButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                guiRow.PlayButton.Control.Text = guiRow.TargetSave.Name;
                guiRow.PlayButton.Control.MouseEnter += (object a, EventArgs e) => { guiRow.PlayButton.Control.ForeColor = Color.White; guiRow.PlayButton.Control.BackColor = Color.DarkSlateGray; };
                guiRow.PlayButton.Control.MouseLeave += (object a, EventArgs e) => { guiRow.PlayButton.Control.ForeColor = Color.Black; guiRow.PlayButton.Control.BackColor = Color.LightGray; };
                guiRow.PlayButton.Control.ForeColor = Color.Black;
                guiRow.PlayButton.Control.BackColor = Color.LightGray;

                guiRow.DeleteButton.Control.Name = $"{guiRow.TargetSave.Name} - DeleteButton";
                guiRow.DeleteButton.Control.Click += (object o, EventArgs e) => { OnDelete(guiRow.TargetSave); };
                guiRow.DeleteButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                guiRow.DeleteButton.Control.Text = "Delete";
                guiRow.DeleteButton.Control.MouseEnter += (object a, EventArgs e) => { guiRow.DeleteButton.Control.ForeColor = Color.White; guiRow.DeleteButton.Control.BackColor = Color.DarkSlateGray; };
                guiRow.DeleteButton.Control.MouseLeave += (object a, EventArgs e) => { guiRow.DeleteButton.Control.ForeColor = Color.Black; guiRow.DeleteButton.Control.BackColor = Color.LightGray; };
                guiRow.DeleteButton.Control.ForeColor = Color.Black;
                guiRow.DeleteButton.Control.BackColor = Color.LightGray;

                guiRow.RenameButton.Control.Name = $"{guiRow.TargetSave.Name} - RenameButton";
                guiRow.RenameButton.Control.Click += (object o, EventArgs e) => { OnRename(guiRow.TargetSave); };
                guiRow.RenameButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                guiRow.RenameButton.Control.Text = "Rename";
                guiRow.RenameButton.Control.MouseEnter += (object a, EventArgs e) => { guiRow.RenameButton.Control.ForeColor = Color.White; guiRow.RenameButton.Control.BackColor = Color.DarkSlateGray; };
                guiRow.RenameButton.Control.MouseLeave += (object a, EventArgs e) => { guiRow.RenameButton.Control.ForeColor = Color.Black; guiRow.RenameButton.Control.BackColor = Color.LightGray; };
                guiRow.RenameButton.Control.ForeColor = Color.Black;
                guiRow.RenameButton.Control.BackColor = Color.LightGray;
            }

            _addButtonRow = new Scaled<Panel>(_scrollContainer, 0.0, RowHeight * _guiRows.Length, 1.0, 0.1);

            _addButton = new Scaled<Button>(_addButtonRow, 0.05, 0.1, 0.9, 0.8);

            _addButtonRow.Control.Name = "addButtonRow";
            _addButtonRow.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            _addButtonRow.Control.BackColor = Color.Transparent;

            _addButton.Control.Name = "addButton";
            _addButton.Control.Click += (object o, EventArgs e) => { OnCreate(); };
            _addButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            _addButton.Control.Text = "Create New Save File!";
            _addButton.Control.MouseEnter += (object a, EventArgs e) => { _addButton.Control.ForeColor = Color.White; _addButton.Control.BackColor = Color.DarkSlateGray; };
            _addButton.Control.MouseLeave += (object a, EventArgs e) => { _addButton.Control.ForeColor = Color.Black; _addButton.Control.BackColor = Color.LightGray; };
            _addButton.Control.ForeColor = Color.Black;
            _addButton.Control.BackColor = Color.LightGray;
        }
        private void OnLoad(GoatLauncher.SaveMeta saveFile)
        {
            if (_goatLauncher.LaunchSave(saveFile))
            {
                ReloadGUI();

                Close();
            }
            else
            {
                MessageBox.Show("Action cannot be preformed while Goat Simulator 3 is running.", "Error: Close Goat Simulator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnDelete(GoatLauncher.SaveMeta save)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to perminantly delete save \"{save.Name}\"?", "Confirm Perminant Deletion", MessageBoxButtons.OKCancel);
            if (result is DialogResult.OK)
            {
                if (_goatLauncher.DeleteSave(save))
                {
                    ReloadGUI();
                }
                else
                {
                    MessageBox.Show("Action cannot be preformed while Goat Simulator 3 is running.", "Error: Close Goat Simulator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void OnRename(GoatLauncher.SaveMeta saveFile)
        {
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Name: ", "Rename Save File", saveFile.Name);

            if (!(newName is null) && !(newName is "") && newName != saveFile.Name)
            {
                if (_goatLauncher.RenameSave(saveFile, newName))
                {
                    ReloadGUI();
                }
                else
                {
                    MessageBox.Show("Action cannot be preformed while Goat Simulator 3 is running.", "Error: Close Goat Simulator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void OnCreate()
        {
            if (_goatLauncher.CreateSave())
            {
                ReloadGUI();
            }
            else
            {
                MessageBox.Show("Action cannot be preformed while Goat Simulator 3 is running.", "Error: Close Goat Simulator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ScrollContent(int mouseWheelDelta)
        {
            double listContainerY = _scrollContainer.Y;
            listContainerY += (mouseWheelDelta / (double)SystemInformation.MouseWheelScrollDelta) * ScrollDelta;

            if (listContainerY > 0.0)
            {
                listContainerY = 0.0;
            }
            double minScrollValue = (_guiRows.Length) * (-RowHeight);
            if (listContainerY < minScrollValue)
            {
                if (minScrollValue < 0)
                {
                    listContainerY = minScrollValue;
                }
                else
                {
                    listContainerY = 0;
                }
            }

            _scrollContainer.Y = listContainerY;
        }
        #endregion
        #region Private Subclasses
        private sealed class GUIRow
        {
            public GoatLauncher.SaveMeta TargetSave;

            public Scaled<Panel> RowContainer;
            public Scaled<Panel> ContentContainer;
            public Scaled<Button> PlayButton;
            public Scaled<Button> RenameButton;
            public Scaled<Button> DeleteButton;
        }
        #endregion
    }
}