using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScaleForms
{
    public sealed class Scaled<T> where T : System.Windows.Forms.Control, new()
    {
        public bool DaSpecialOne = false;
        #region Public Variables
        public bool Destroyed
        {
            get
            {
                return _destroyed;
            }
        }
        public T Control
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _control;
            }
        }
        public System.Windows.Forms.Control Parent
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _parent;
            }
        }
        public double X
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _x;
            }
            set
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                _x = value;
                Reposition();
            }
        }
        public double Y
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _y;
            }
            set
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                _y = value;
                Reposition();
            }
        }
        public double Width
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _width;
            }
            set
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                if (_width <= 0.0)
                {
                    throw new System.Exception($"{nameof(Width)} must be greater than 0.");
                }
                _width = value;
                Resize();
            }
        }
        public double Height
        {
            get
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _height;
            }
            set
            {
                if (Destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                if (_height <= 0.0)
                {
                    throw new System.Exception($"{nameof(Height)} must be greater than 0.");
                }
                _height = value;
                Resize();
            }
        }
        #endregion
        #region Private Variables
        private bool _destroyed = false;
        private T _control = null;
        private System.Windows.Forms.Control _parent = null;
        private double _x = 0.0;
        private double _y = 0.0;
        private double _width = 0.0;
        private double _height = 0.0;
        #endregion
        #region Public Constructors
        public Scaled(System.Windows.Forms.Control parent, double x = 0.0, double y = 0.0, double width = 1.0, double height = 1.0, bool daspe = false)
        {
            DaSpecialOne = daspe;

            if (parent is null)
            {
                throw new System.Exception($"{nameof(parent)} may not be null.");
            }
            if (width <= 0.0)
            {
                throw new System.Exception($"{nameof(width)} must be greater than 0.");
            }
            if (height <= 0.0)
            {
                throw new System.Exception($"{nameof(height)} must be greater than 0.");
            }

            _destroyed = false;
            _control = new T();
            _parent = parent;
            _x = x;
            _y = y;
            _width = width;
            _height = height;

            _parent.ControlRemoved += (object o, System.Windows.Forms.ControlEventArgs e) =>
            {
                if (e.Control == _control)
                {
                    OnDestroy();
                }
            };
            _parent.Controls.Add(Control);
            _parent.Resize += (object o, System.EventArgs e) => { Reposition(); Resize(); };

            Resize();
            Reposition();
        }
        #endregion
        #region Public Methods
        public void Destroy()
        {
            if (Destroyed)
            {
                throw new System.Exception($"Scaled<{nameof(T)}> has already been destroyed.");
            }
            Parent.Controls.Remove(Control);
            _destroyed = true;
        }
        #endregion
        #region Private Methods
        private void Resize()
        {
            int pixelWidth = (int)(Width * Parent.ClientSize.Width);
            int pixelHeight = (int)(Height * Parent.ClientSize.Height);
            _control.Size = new System.Drawing.Size(pixelWidth, pixelHeight);

            if (DaSpecialOne)
            {
                Console.WriteLine($"Width {pixelWidth} Height {pixelHeight}");
            }
        }
        private void Reposition()
        {
            int pixelX = (int)(X * Parent.ClientSize.Width);
            int pixelY = (int)(Y * Parent.ClientSize.Height);
            _control.Location = new System.Drawing.Point(pixelX, pixelY);

            if (DaSpecialOne)
            {
                Console.WriteLine($"X {pixelX} Y {pixelY}");
            }
        }
        private void OnDestroy()
        {
            _destroyed = true;
            _control = null;
            _parent = null;
            _x = 0.0;
            _y = 0.0;
            _width = 0.0;
            _height = 0.0;
        }
        #endregion
        #region Public Opperators
        public static implicit operator T(Scaled<T> source)
        {
            if (source is null)
            {
                throw new System.Exception("source cannot be null.");
            }
            return source.Control;
        }
        #endregion
    }
    public static class ScaleFormsDemo
    {
        public static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DemoForm());
        }
        public sealed class DemoForm : System.Windows.Forms.Form
        {
            public DemoForm()
            {
                BackColor = Color.LightGray;

                Scaled<Panel> navbar = new Scaled<Panel>(this, 0, 0, 1, 0.1);
                navbar.Control.BackColor = Color.CornflowerBlue;

                Scaled<Button> homeButton = new Scaled<Button>(navbar, 0.1, 0.1, 0.1, 0.8);
                homeButton.Control.BackColor = Color.Red;
            }
        }
    }
}