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
        public const string Goat2Exe = "C:\\Program Files\\Epic Games\\GoatSimulator3\\Goat2.exe";
        public static readonly string Goat2Folder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Goat2";
        public static readonly string GoatLauncherFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\GoatLauncher";

        public List<SaveMeta> SaveList = new List<SaveMeta>();

        public GoatLauncher()
        {
            if (!Directory.Exists(GoatLauncherFolder))
            {
                Directory.CreateDirectory(GoatLauncherFolder);
            }

            string[] savePaths = Directory.GetDirectories(GoatLauncherFolder);

            foreach (string savePath in savePaths)
            {
                SaveMeta newSave = new SaveMeta();

                newSave.SaveFolderPath = savePath;
                newSave.Name = Directory.GetFiles(savePath, "*.gsb", System.IO.SearchOption.TopDirectoryOnly)[0];

                SaveList.Add(newSave);
            }

            SaveMeta loadedSave = new SaveMeta();

            loadedSave.SaveFolderPath = Goat2Folder;
            loadedSave.Name = Directory.GetFiles(Goat2Folder, "*.gsb", System.IO.SearchOption.TopDirectoryOnly)[0];

            SaveList.Add(loadedSave);
        }
        public void LaunchSave(SaveMeta target)
        {
            if (target is null)
            {
                throw new Exception($"{nameof(target)} may not be null.");
            }

            if (!target.IsLoaded)
            {
                //Get old save outta da way.
                SaveMeta oldSave = null;

                for (int i = 0; i < SaveList.Count; i++)
                {
                    if (SaveList[i].IsLoaded)
                    {
                        oldSave = SaveList[i];
                    }
                }

                int id = 0;
                while (id != -1)
                {
                    oldSave.SaveFolderPath = $"{GoatLauncherFolder}\\{id}";
                    if (Directory.Exists(oldSave.SaveFolderPath))
                    {
                        id++;
                    }
                    else
                    {
                        id = -1;
                    }
                }

                Directory.Move(Goat2Folder, oldSave.SaveFolderPath);

                //Load target
                Directory.Move(target.SaveFolderPath, Goat2Folder);

                target.SaveFolderPath = Goat2Folder;
            }

            //Launch goat simulator
            Process.Start(Goat2Exe);

            //Close current program.
            Environment.Exit(0);
        }
        public void DeleteSave(SaveMeta target)
        {
            FileSystem.DeleteDirectory(target.SaveFolderPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

            SaveList.Remove(target);
        }
        public void RenameSave(SaveMeta target, string newName)
        {
            //Find old name file
            string oldNameFile = Directory.GetFiles(target.SaveFolderPath, "*.gsb", System.IO.SearchOption.TopDirectoryOnly)[0];

            //Rename name file to new name.
            File.Move(oldNameFile, $"{new FileInfo(oldNameFile).Directory.FullName}\\{newName}.gsb");

            //Rename save meta
            target.Name = newName;
        }
        public void CreateSave(string name)
        {
            foreach (SaveMeta save in SaveList)
            {
                if (save.Name == name)
                {
                    MessageBox.Show($"A save with name \"{name}\" already exists.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            SaveMeta newSave = new SaveMeta();

            newSave.Name = name;
            int id = 0;
            while (id != -1)
            {
                newSave.SaveFolderPath = $"{GoatLauncherFolder}\\{id}";
                if (Directory.Exists(newSave.SaveFolderPath))
                {
                    id++;
                }
                else
                {
                    id = -1;
                }
            }
            Directory.CreateDirectory(newSave.SaveFolderPath);
            File.WriteAllText($"{newSave.SaveFolderPath}\\{name}.gbs", "");

            SaveList.Add(newSave);
        }
        public sealed class SaveMeta
        {
            public string Name { get; set; } = "Unnamed Save Slot";
            public string SaveFolderPath = "";
            public bool IsLoaded => SaveFolderPath == Goat2Folder;
        }
    }
    public sealed class GoatLauncherGUI : Form
    {
        #region Private Constants
        private const double RowHeight = 0.1; // The height of one row in screens.
        private const double ScrollDelta = RowHeight / 2.0; // The distance scrolled with one click in screens.
        #endregion
        #region Private Variables
        private GoatLauncher _goatLauncher;

        private List<GUIRow> _guiRows;

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
                for (int i = 0; i < _guiRows.Count; i++)
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
            _guiRows = new List<GUIRow>(_goatLauncher.SaveList.Count);
            for (int i = 0; i < _goatLauncher.SaveList.Count; i++)
            {
                GUIRow guiRow = new GUIRow();
                guiRow.TargetSave = _goatLauncher.SaveList[i];
                _guiRows[i] = guiRow;
            }

            //Init new GUI

            for (int i = 0; i < _goatLauncher.SaveList.Count; i++)
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
                guiRow.RowContainer.Control.BackColor = Color.Transparent;

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

            _addButtonRow = new Scaled<Panel>(_scrollContainer, 0.0, RowHeight * _guiRows.Count, 1.0, 0.1);

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
            _goatLauncher.LaunchSave(saveFile);

            ReloadGUI();
        }
        private void OnDelete(GoatLauncher.SaveMeta save)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to perminantly delete save \"{save.Name}\"?", "Confirm Perminant Deletion", MessageBoxButtons.OKCancel);
            if (result is DialogResult.OK)
            {
                _goatLauncher.DeleteSave(save);

                ReloadGUI();
            }
        }
        private void OnRename(GoatLauncher.SaveMeta saveFile)
        {
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Name: ", "Rename Save File", saveFile.Name);

            if (!(newName is null) && !(newName is "") && newName != saveFile.Name)
            {
                _goatLauncher.RenameSave(saveFile, newName);

                ReloadGUI();
            }
        }
        private void OnCreate()
        {
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Name: ", "Name New Save", "Unnamed Save");

            if (!(newName is null) && !(newName is ""))
            {
                _goatLauncher.CreateSave(newName);

                ReloadGUI();
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
            double minScrollValue = (_guiRows.Count) * (-RowHeight);
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
