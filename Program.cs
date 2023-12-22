using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using ScaleForms;

namespace GoatLauncher
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
    public sealed class MainForm : Form
    {
        public const double RowHeight = 0.1; // The height of one row in screens.
        public const double ScrollDelta = RowHeight / 2.0; // The distance scrolled with one click in screens.

        private Scaled<Panel> _scrollContainer;
        private List<SaveData> _saveFiles;
        private Scaled<Panel> _addButtonRow;
        private Scaled<Button> _addButton;
        public MainForm()
        {
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
            RefreshSaves();
        }
        public void RefreshSaves()
        {
            DeleteGUI();

            _saveFiles = new List<SaveData>();
            for (int i = 0; i < 5; i++)
            {
                SaveData saveFile = new SaveData();
                saveFile.Name = $"goaty time number {i}";
                _saveFiles.Add(saveFile);
            }

            InitGUI();
        }
        public void DeleteGUI()
        {
            if (!(_saveFiles is null))
            {
                for (int i = 0; i < _saveFiles.Count; i++)
                {
                    SaveData saveFile = _saveFiles[i];

                    if (!(saveFile.PlayButton is null))
                    {
                        saveFile.PlayButton.Destroy();
                    }
                    if (!(saveFile.DeleteButton is null))
                    {
                        saveFile.DeleteButton.Destroy();
                    }
                    if (!(saveFile.RenameButton is null))
                    {
                        saveFile.RenameButton.Destroy();
                    }
                    if (!(saveFile.ContentContainer is null))
                    {
                        saveFile.ContentContainer.Destroy();
                    }
                    if (!(saveFile.RowContainer is null))
                    {
                        saveFile.RowContainer.Destroy();
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
        }
        public void InitGUI()
        {
            for (int i = 0; i < _saveFiles.Count; i++)
            {
                SaveData saveFile = _saveFiles[i];

                saveFile.RowContainer = new Scaled<Panel>(_scrollContainer, 0.0, RowHeight * i, 1.0, 0.1);
                saveFile.ContentContainer = new Scaled<Panel>(saveFile.RowContainer, 0.05, 0.1, 0.9, 0.8);
                saveFile.PlayButton = new Scaled<Button>(saveFile.ContentContainer, 0.0, 0.0, 0.8, 1);
                saveFile.DeleteButton = new Scaled<Button>(saveFile.ContentContainer, 0.8, 0.0, 0.1, 1);
                saveFile.RenameButton = new Scaled<Button>(saveFile.ContentContainer, 0.9, 0.0, 0.1, 1);

                saveFile.RowContainer.Control.Name = $"{saveFile.Name} - RowContainer";
                saveFile.RowContainer.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                saveFile.RowContainer.Control.BackColor = Color.Transparent;

                saveFile.ContentContainer.Control.Name = $"{saveFile.Name} - ContentContainer";
                saveFile.ContentContainer.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                saveFile.ContentContainer.Control.BackColor = Color.Transparent;

                saveFile.PlayButton.Control.Name = $"{saveFile.Name} - PlayButton";
                saveFile.PlayButton.Control.Click += (object o, EventArgs e) => { LoadSaveData(saveFile); };
                saveFile.PlayButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                saveFile.PlayButton.Control.Text = saveFile.Name;
                saveFile.PlayButton.Control.MouseEnter += (object a, EventArgs e) => { saveFile.PlayButton.Control.ForeColor = Color.White; saveFile.PlayButton.Control.BackColor = Color.DarkSlateGray; };
                saveFile.PlayButton.Control.MouseLeave += (object a, EventArgs e) => { saveFile.PlayButton.Control.ForeColor = Color.Black; saveFile.PlayButton.Control.BackColor = Color.LightGray; };
                saveFile.PlayButton.Control.ForeColor = Color.Black;
                saveFile.PlayButton.Control.BackColor = Color.LightGray;

                saveFile.DeleteButton.Control.Name = $"{saveFile.Name} - DeleteButton";
                saveFile.DeleteButton.Control.Click += (object o, EventArgs e) => { DeleteSaveData(saveFile); };
                saveFile.DeleteButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                saveFile.DeleteButton.Control.Text = "Delete";
                saveFile.DeleteButton.Control.MouseEnter += (object a, EventArgs e) => { saveFile.DeleteButton.Control.ForeColor = Color.White; saveFile.DeleteButton.Control.BackColor = Color.DarkSlateGray; };
                saveFile.DeleteButton.Control.MouseLeave += (object a, EventArgs e) => { saveFile.DeleteButton.Control.ForeColor = Color.Black; saveFile.DeleteButton.Control.BackColor = Color.LightGray; };
                saveFile.DeleteButton.Control.ForeColor = Color.Black;
                saveFile.DeleteButton.Control.BackColor = Color.LightGray;

                saveFile.RenameButton.Control.Name = $"{saveFile.Name} - RenameButton";
                saveFile.RenameButton.Control.Click += (object o, EventArgs e) => { RenameSaveData(saveFile); };
                saveFile.RenameButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                saveFile.RenameButton.Control.Text = "Rename";
                saveFile.RenameButton.Control.MouseEnter += (object a, EventArgs e) => { saveFile.RenameButton.Control.ForeColor = Color.White; saveFile.RenameButton.Control.BackColor = Color.DarkSlateGray; };
                saveFile.RenameButton.Control.MouseLeave += (object a, EventArgs e) => { saveFile.RenameButton.Control.ForeColor = Color.Black; saveFile.RenameButton.Control.BackColor = Color.LightGray; };
                saveFile.RenameButton.Control.ForeColor = Color.Black;
                saveFile.RenameButton.Control.BackColor = Color.LightGray;
            }

            _addButtonRow = new Scaled<Panel>(_scrollContainer, 0.0, RowHeight * _saveFiles.Count, 1.0, 0.1, true);

            _addButton = new Scaled<Button>(_addButtonRow, 0.05, 0.1, 0.9, 0.8);

            _addButtonRow.Control.Name = "addButtonRow";
            _addButtonRow.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            _addButtonRow.Control.BackColor = Color.Transparent;

            _addButton.Control.Name = "addButton";
            _addButton.Control.Click += (object o, EventArgs e) => { AddSaveFile(); };
            _addButton.Control.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            _addButton.Control.Text = "Create New Save File!";
            _addButton.Control.MouseEnter += (object a, EventArgs e) => { _addButton.Control.ForeColor = Color.White; _addButton.Control.BackColor = Color.DarkSlateGray; };
            _addButton.Control.MouseLeave += (object a, EventArgs e) => { _addButton.Control.ForeColor = Color.Black; _addButton.Control.BackColor = Color.LightGray; };
            _addButton.Control.ForeColor = Color.Black;
            _addButton.Control.BackColor = Color.LightGray;
        }
        public void LoadSaveData(SaveData saveFile)
        {
            MessageBox.Show($"Loaded save file {saveFile.Name}.");
        }
        public void DeleteSaveData(SaveData saveFile)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to perminantly delete save file \"{saveFile.Name}\"?", "Confirm Perminant Save File Deletion", MessageBoxButtons.OKCancel);
            if (result is DialogResult.OK)
            {
                DeleteGUI();
                _saveFiles.Remove(saveFile);
                InitGUI();
            }
        }
        public void RenameSaveData(SaveData saveFile)
        {
            string newName = Interaction.InputBox("Name: ", "Rename Save File", saveFile.Name);

            if (!(newName is null) && !(newName is "") && newName != saveFile.Name)
            {
                DeleteGUI();
                saveFile.Name = newName;
                InitGUI();
            }
        }
        public void AddSaveFile()
        {
            MessageBox.Show("Ballz");
        }
        public void ScrollContent(int mouseWheelDelta)
        {
            double listContainerY = _scrollContainer.Y;
            listContainerY += (mouseWheelDelta / (double)SystemInformation.MouseWheelScrollDelta) * ScrollDelta;

            if (listContainerY > 0.0)
            {
                listContainerY = 0.0;
            }
            double minScrollValue = (_saveFiles.Count) * (-RowHeight);
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
        public sealed class SaveData
        {
            public string Name = "Unnamed Save Slot";
            public string MetaFilePath = "";
            public string DataFolderPath = "";
            public bool Loaded = false;
            public long LastPlayedTime = 0;

            public Scaled<Panel> RowContainer;
            public Scaled<Panel> ContentContainer;
            public Scaled<Button> PlayButton;
            public Scaled<Button> RenameButton;
            public Scaled<Button> DeleteButton;
        }
    }
}
