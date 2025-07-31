namespace ScaleForms
{
    public sealed class Scaled<T> where T : System.Windows.Forms.Control, new()
    {
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
                if (_destroyed)
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
                if (_destroyed)
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
                if (_destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _x;
            }
            set
            {
                if (_destroyed)
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
                if (_destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _y;
            }
            set
            {
                if (_destroyed)
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
                if (_destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _width;
            }
            set
            {
                if (_destroyed)
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
                if (_destroyed)
                {
                    throw new System.Exception($"Scaled<{nameof(T)}> has been destroyed.");
                }
                return _height;
            }
            set
            {
                if (_destroyed)
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
        private bool _removedFromParent = false;
        private T _control = null;
        private System.Windows.Forms.Control _parent = null;
        private double _x = 0.0;
        private double _y = 0.0;
        private double _width = 0.0;
        private double _height = 0.0;
        #endregion
        #region Public Constructors
        public Scaled(System.Windows.Forms.Control parent, double x = 0.0, double y = 0.0, double width = 1.0, double height = 1.0)
        {
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
                if (!(_control is null) && e.Control == _control)
                {
                    OnRemoved();
                }
            };
            _control.Disposed += (object o, System.EventArgs e) =>
            {
                OnDisposed();
            };
            _parent.Controls.Add(Control);
            _parent.Resize += (object o, System.EventArgs e) =>
            {
                Reposition();
                Resize();
            };

            Resize();
            Reposition();
        }
        #endregion
        #region Public Methods
        public void Destroy()
        {
            if (_destroyed)
            {
                throw new System.Exception($"Scaled<{nameof(T)}> has already been destroyed.");
            }

            _destroyed = true;

            _x = 0.0;
            _y = 0.0;
            _width = 0.0;
            _height = 0.0;

            if (!(_parent is null) && !(_control is null))
            {
                _parent.Controls.Remove(_control);
            }

            if (!(_control is null) && !_control.IsDisposed)
            {
                _control.Dispose();
            }

            _control = null;
            _parent = null;
        }
        #endregion
        #region Private Methods
        private void Resize()
        {
            int pixelWidth = (int)(Width * Parent.ClientSize.Width);
            int pixelHeight = (int)(Height * Parent.ClientSize.Height);
            _control.Size = new System.Drawing.Size(pixelWidth, pixelHeight);
        }
        private void Reposition()
        {
            int pixelX = (int)(X * Parent.ClientSize.Width);
            int pixelY = (int)(Y * Parent.ClientSize.Height);
            _control.Location = new System.Drawing.Point(pixelX, pixelY);
        }
        private void OnRemoved()
        {
            if (_destroyed)
            {
                return; //Because the control is being destroyed with the Destroy function.
            }

            _destroyed = true;

            _x = 0.0;
            _y = 0.0;
            _width = 0.0;
            _height = 0.0;

            if (!(_control is null) && !_control.IsDisposed)
            {
                _control.Dispose();
            }

            _control = null;
            _parent = null;
        }
        private void OnDisposed()
        {
            if (_destroyed)
            {
                return; //Because the control is being destroyed with the destroy function.
            }

            _destroyed = true;

            _x = 0.0;
            _y = 0.0;
            _width = 0.0;
            _height = 0.0;

            _control = null;
            _parent = null;
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
}