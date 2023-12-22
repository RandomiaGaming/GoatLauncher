using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private ScaleController _listContainer;
        private SaveFile[] _saveFiles;
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
            Panel listContainer = new Panel();
            listContainer.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
            listContainer.BackColor = Color.Transparent;
            Controls.Add(listContainer);

            _listContainer = new ScaleController(listContainer, 0.0, 0.0, 1.0, 1.0);

            //Create buttons
            RefreshSaves();
        }
        public void RefreshSaves()
        {
            //Delete old buttons
            if (_saveFiles != null)
            {
                for (int i = 0; i < _saveFiles.Length; i++)
                {
                    SaveFile saveFile = _saveFiles[i];

                    Controls.Remove(saveFile.PlayButton.Target);
                    Controls.Remove(saveFile.DeleteButton.Target);
                    Controls.Remove(saveFile.RenameButton.Target);
                    Controls.Remove(saveFile.ContentContainer.Target);
                    Controls.Remove(saveFile.RowContainer.Target);
                }
            }

            //Overwrite old save files
            _saveFiles = new SaveFile[10];

            //Initialize new save files.
            for (int i = 0; i < _saveFiles.Length; i++)
            {
                SaveFile saveFile = new SaveFile();
                saveFile.Name = $"Save file {i}";
                _saveFiles[i] = saveFile;
            }

            //Initialize buttons.
            for (int i = 0; i < _saveFiles.Length; i++)
            {
                SaveFile saveFile = _saveFiles[i];

                Panel rowContainer = new Panel();
                rowContainer.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                rowContainer.BackColor = Color.Transparent;
                _listContainer.Target.Controls.Add(rowContainer);

                Panel contentContainer = new Panel();
                contentContainer.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                contentContainer.BackColor = Color.Transparent;
                rowContainer.Controls.Add(contentContainer);

                Button playButton = new Button();
                playButton.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                playButton.Text = saveFile.Name;
                playButton.MouseEnter += (object a, EventArgs e) => { playButton.ForeColor = Color.White; playButton.BackColor = Color.DarkSlateGray; };
                playButton.MouseLeave += (object a, EventArgs e) => { playButton.ForeColor = Color.Black; playButton.BackColor = Color.LightGray; };
                playButton.ForeColor = Color.Black;
                playButton.BackColor = Color.LightGray;
                contentContainer.Controls.Add(playButton);

                Button deleteButton = new Button();
                deleteButton.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                deleteButton.Text = "Delete";
                deleteButton.MouseEnter += (object a, EventArgs e) => { deleteButton.ForeColor = Color.White; deleteButton.BackColor = Color.DarkSlateGray; };
                deleteButton.MouseLeave += (object a, EventArgs e) => { deleteButton.ForeColor = Color.Black; deleteButton.BackColor = Color.LightGray; };
                deleteButton.ForeColor = Color.Black;
                deleteButton.BackColor = Color.LightGray;
                contentContainer.Controls.Add(deleteButton);

                Button renameButton = new Button();
                renameButton.GotFocus += (object o, EventArgs e) => { ActiveControl = null; };
                renameButton.Text = "Rename";
                renameButton.MouseEnter += (object a, EventArgs e) => { renameButton.ForeColor = Color.White; renameButton.BackColor = Color.DarkSlateGray; };
                renameButton.MouseLeave += (object a, EventArgs e) => { renameButton.ForeColor = Color.Black; renameButton.BackColor = Color.LightGray; };
                renameButton.ForeColor = Color.Black;
                renameButton.BackColor = Color.LightGray;
                contentContainer.Controls.Add(renameButton);

                saveFile.RowContainer = new ScaleController(rowContainer, 0.0, RowHeight * i, 1.0, 0.1);
                saveFile.ContentContainer = new ScaleController(contentContainer, 0.05, 0.1, 0.9, 0.8);
                saveFile.PlayButton = new ScaleController(playButton, 0.0, 0.0, 0.8, 1);
                saveFile.DeleteButton = new ScaleController(deleteButton, 0.8, 0.0, 0.1, 1);
                saveFile.RenameButton = new ScaleController(renameButton, 0.9, 0.0, 0.1, 1);
            }
        }
        public void ScrollContent(int mouseWheelDelta)
        {
            double listContainerY = _listContainer.Y;
            listContainerY += (mouseWheelDelta / (double)SystemInformation.MouseWheelScrollDelta) * ScrollDelta;

            if (listContainerY > 0.0)
            {
                listContainerY = 0.0;
            }
            double minScrollValue = (_saveFiles.Length - 1) * (-RowHeight);
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

            _listContainer.Y = listContainerY;
        }
        public sealed class SaveFile
        {
            public string Name = "Unnamed Save Slot";
            public string MetaFilePath = "";
            public string DataFolderPath = "";
            public bool Loaded = false;
            public long LastPlayedTime = 0;

            public ScaleController RowContainer;
            public ScaleController ContentContainer;
            public ScaleController PlayButton;
            public ScaleController RenameButton;
            public ScaleController DeleteButton;
        }
        public sealed class ScaleController
        {
            public Control Target { get; private set; } = null;
            public Control Parent { get; private set; } = null;
            public double X
            {
                get
                {
                    return _x;
                }
                set
                {
                    _x = value;
                    Reposition();
                }
            }
            public double Y
            {
                get
                {
                    return _y;
                }
                set
                {
                    _y = value;
                    Reposition();
                }
            }
            public double Width
            {
                get
                {
                    return _width;
                }
                set
                {
                    if (_width <= 0.0)
                    {
                        throw new Exception($"{nameof(Width)} must be greater than 0.");
                    }
                    _width = value;
                    Resize();
                }
            }
            public double Height
            {
                get
                {
                    return _height;
                }
                set
                {
                    if (_height <= 0.0)
                    {
                        throw new Exception($"{nameof(Height)} must be greater than 0.");
                    }
                    _height = value;
                    Resize();
                }
            }

            private double _x = 0.0;
            private double _y = 0.0;
            private double _width = 1.0;
            private double _height = 1.0;
            public ScaleController(Control target, double x = 0.0, double y = 0.0, double width = 1.0, double height = 1.0)
            {
                if (target is null)
                {
                    throw new Exception($"{nameof(target)} may not be null.");
                }
                if (target is Form)
                {
                    throw new Exception($"{nameof(target)} may not be a form.");
                }

                if (width <= 0.0)
                {
                    throw new Exception($"{nameof(width)} must be greater than 0.");
                }
                if (height <= 0.0)
                {
                    throw new Exception($"{nameof(height)} must be greater than 0.");
                }

                Target = target;
                _x = x;
                _y = y;
                _width = width;
                _height = height;

                if (Target.Parent is null)
                {
                    Parent = Target.FindForm();
                }
                else
                {
                    Parent = Target.Parent;
                }

                if (Parent is null)
                {
                    throw new Exception("target must be added to parent or form before creating a ScaleController. Use Form.Controls.Add");
                }

                Parent.Resize += (object o, EventArgs e) => { Reposition(); Resize(); };

                Resize();
                Reposition();
            }

            private void Resize()
            {
                int pixelWidth = (int)(Width * Parent.ClientSize.Width);
                int pixelHeight = (int)(Height * Parent.ClientSize.Height);
                Target.Size = new Size(pixelWidth, pixelHeight);
            }
            private void Reposition()
            {
                int pixelX = (int)(X * Parent.ClientSize.Width);
                int pixelY = (int)(Y * Parent.ClientSize.Height);
                Target.Location = new Point(pixelX, pixelY);
            }
        }
    }
}
