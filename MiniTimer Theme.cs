namespace MiniTimer_Theme
{

#region  Imports

    using System.Collections.Generic;
    using System;
    using System.Drawing;
    using System.Collections;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;
    using System.ComponentModel;
	
#endregion
	
	//|------DO-NOT-REMOVE------|
	//
	// Creator: HazelDev
	// Site   : HazelDev.com
	// Created: 13.Oct.2014
	// Changed: 15.Oct.2014
	// Version: 1.0.0
	//
	//|------DO-NOT-REMOVE------|
	
#region  ThemeContainer
	
	public class MiniTimer_ThemeContainer : ContainerControl
	{
		
#region  Enums
		
		public enum MouseState
		{
			None = 0,
			Over = 1,
			Down = 2,
			Block = 3
		}
		
#endregion
#region  Variables
		
		private Rectangle HeaderRect;
		protected MouseState State;
		private int MoveHeight;
		private Point MouseP = new Point(0, 0);
		private bool Cap = false;
		private bool HasShown;
		
#endregion
#region  Properties
		
		private bool _Sizable = true;
public bool Sizable
		{
			get
			{
				return _Sizable;
			}
			set
			{
				_Sizable = value;
			}
		}
		
		private bool _SmartBounds = true;
public bool SmartBounds
		{
			get
			{
				return _SmartBounds;
			}
			set
			{
				_SmartBounds = value;
			}
		}
		
		private bool _IsParentForm;
protected bool IsParentForm
		{
			get
			{
				return _IsParentForm;
			}
		}
		
protected bool IsParentMdi
		{
			get
			{
				if (Parent == null)
				{
					return false;
				}
				return Parent.Parent != null;
			}
		}
		
		private bool _ControlMode;
protected bool ControlMode
		{
			get
			{
				return _ControlMode;
			}
			set
			{
				_ControlMode = value;
				Invalidate();
			}
		}
		
		private FormStartPosition _StartPosition;
public FormStartPosition StartPosition
		{
			get
			{
				if (_IsParentForm && !_ControlMode)
				{
					return ParentForm.StartPosition;
				}
				else
				{
					return _StartPosition;
				}
			}
			set
			{
				_StartPosition = value;
				
				if (_IsParentForm && !_ControlMode)
				{
					ParentForm.StartPosition = value;
				}
			}
		}
		
#endregion
#region  EventArgs
		
		protected sealed override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			
			if (Parent == null)
			{
				return ;
			}
			_IsParentForm = Parent is Form;
			
			if (!_ControlMode)
			{
				InitializeMessages();
				
				if (_IsParentForm)
				{
					this.ParentForm.FormBorderStyle = FormBorderStyle.None;
					this.ParentForm.TransparencyKey = Color.Fuchsia;
					
					if (!DesignMode)
					{
						ParentForm.Shown += FormShown;
					}
				}
				Parent.BackColor = BackColor;
				Parent.MinimumSize = new Size(261, 65);
			}
		}
		
		protected sealed override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (!_ControlMode)
			{
				HeaderRect = new Rectangle(0, 0, Width - 14, MoveHeight - 7);
			}
			Invalidate();
		}
		
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				SetState(MouseState.Down);
			}
			if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized || _ControlMode))
			{
				if (HeaderRect.Contains(e.Location))
				{
					Capture = false;
					WM_LMBUTTONDOWN = true;
					DefWndProc(ref Messages[0]);
				}
				else if (_Sizable && !(Previous == 0))
				{
					Capture = false;
					WM_LMBUTTONDOWN = true;
					DefWndProc(ref Messages[Previous]);
				}
			}
		}
		
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseUp(e);
			Cap = false;
		}
		
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized))
			{
				if (_Sizable && !_ControlMode)
				{
					InvalidateMouse();
				}
			}
			if (Cap)
			{
				Parent.Location = (System.Drawing.Point) ((object) (System.Convert.ToDouble(MousePosition ) - System.Convert.ToDouble( MouseP)));
			}
		}
		
		protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
		{
			base.OnInvalidated(e);
			ParentForm.Text = Text;
		}
		
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
		}
		
		protected override void OnTextChanged(System.EventArgs e)
		{
			base.OnTextChanged(e);
			Invalidate();
		}
		
		private void FormShown(object sender, EventArgs e)
		{
			if (_ControlMode || HasShown)
			{
				return ;
			}
			
			if (_StartPosition == FormStartPosition.CenterParent || _StartPosition == FormStartPosition.CenterScreen)
			{
				Rectangle SB = Screen.PrimaryScreen.Bounds;
				Rectangle CB = ParentForm.Bounds;
				ParentForm.Location = new Point(SB.Width / 2 - CB.Width / 2, SB.Height / 2 - CB.Width / 2);
			}
			HasShown = true;
		}
		
#endregion
#region  Mouse & Size
		
		private void SetState(MouseState current)
		{
			State = current;
			Invalidate();
		}
		
		private Point GetIndexPoint;
		private bool B1x;
		private bool B2x;
		private bool B3;
		private bool B4;
		private int GetIndex()
		{
			GetIndexPoint = PointToClient(MousePosition);
			B1x = GetIndexPoint.X < 7;
			B2x = GetIndexPoint.X > Width - 7;
			B3 = GetIndexPoint.Y < 7;
			B4 = GetIndexPoint.Y > Height - 7;
			
			if (B1x && B3)
			{
				return 4;
			}
			if (B1x && B4)
			{
				return 7;
			}
			if (B2x && B3)
			{
				return 5;
			}
			if (B2x && B4)
			{
				return 8;
			}
			if (B1x)
			{
				return 1;
			}
			if (B2x)
			{
				return 2;
			}
			if (B3)
			{
				return 3;
			}
			if (B4)
			{
				return 6;
			}
			return 0;
		}
		
		private int Current;
		private int Previous;
		private void InvalidateMouse()
		{
			Current = GetIndex();
			if (Current == Previous)
			{
				return ;
			}
			
			Previous = Current;
			switch (Previous)
			{
				case 0:
					Cursor = Cursors.Default;
					break;
				case 6:
					Cursor = Cursors.SizeNS;
					break;
				case 8:
					Cursor = Cursors.SizeNWSE;
					break;
				case 7:
					Cursor = Cursors.SizeNESW;
					break;
			}
		}
		
		private Message[] Messages = new Message[9];
		private void InitializeMessages()
		{
			Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
			for (int I = 1; I <= 8; I++)
			{
				Messages[I] = Message.Create(Parent.Handle, 161, new IntPtr(I + 9), IntPtr.Zero);
			}
		}
		
		private void CorrectBounds(Rectangle bounds)
		{
			if (Parent.Width > bounds.Width)
			{
				Parent.Width = bounds.Width;
			}
			if (Parent.Height > bounds.Height)
			{
				Parent.Height = bounds.Height;
			}
			
			int X = Parent.Location.X;
			int Y = Parent.Location.Y;
			
			if (X < bounds.X)
			{
				X = bounds.X;
			}
			if (Y < bounds.Y)
			{
				Y = bounds.Y;
			}
			
			int Width = bounds.X + bounds.Width;
			int Height = bounds.Y + bounds.Height;
			
			if (X + Parent.Width > Width)
			{
				X = Width - Parent.Width;
			}
			if (Y + Parent.Height > Height)
			{
				Y = Height - Parent.Height;
			}
			
			Parent.Location = new Point(X, Y);
		}
		
		private bool WM_LMBUTTONDOWN;
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			
			if (WM_LMBUTTONDOWN && m.Msg == 513)
			{
				WM_LMBUTTONDOWN = false;
				
				SetState(MouseState.Over);
				if (!_SmartBounds)
				{
					return ;
				}
				
				if (IsParentMdi)
				{
					CorrectBounds(new Rectangle(Point.Empty, Parent.Parent.Size));
				}
				else
				{
					CorrectBounds(Screen.FromControl(Parent).WorkingArea);
				}
			}
		}
		
#endregion
		
		protected override void CreateHandle()
		{
			base.CreateHandle();
		}
		
		public MiniTimer_ThemeContainer()
		{
			SetStyle((ControlStyles) (139270), true);
			BackColor = Color.White;
			Padding = new Padding(20, 56, 20, 16);
			DoubleBuffered = true;
			Dock = DockStyle.Fill;
			MoveHeight = 48;
			Font = new Font("Segoe UI", 9);
			_Sizable = false;
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics G = e.Graphics;
			G.Clear(Color.FromArgb(255, 255, 255));
			
			G.DrawRectangle(new Pen(Color.FromArgb(221, 221, 221)), new Rectangle(0, 0, Width - 1, Height - 1));
			
			G.FillRectangle(new LinearGradientBrush(new Point(0, 0), new Point(0, 39), Color.FromArgb(255, 255, 255), Color.FromArgb(255, 255, 255)), new Rectangle(1, 1, Width - 2, 39));
			G.DrawLine(new Pen(Color.FromArgb(221, 221, 221)), 1, 41, Width - 2, 41);
			G.FillRectangle(new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(238, 238, 238), Color.FromArgb(238, 238, 238)), new Rectangle(1, Height - 42, Width - 2, 41));
			G.DrawLine(new Pen(Color.FromArgb(222, 222, 222)), 1, Height - 44, Width - 2, Height - 44);
			G.DrawLine(new Pen(Color.FromArgb(251, 251, 251)), 1, Height - 43, Width - 2, Height - 43);		
			G.DrawString(Text, new Font("Tahoma", (float) (10.5F), FontStyle.Bold), new SolidBrush(Color.FromArgb(129, 151, 172)), new Rectangle(0, 12, Width - 12, Height), new StringFormat() {Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near});
		}
	}
	
#endregion
#region  ControlBox

    public class MiniTimer_ControlBox : Control
    {

        #region  Enums

        public enum MouseState
        {
            None = 0,
            Over = 1,
            Down = 2
        }

        #endregion
        #region  MouseStates
        MouseState State = MouseState.None;
        int X;
        Rectangle CloseBtn = new Rectangle(3, 2, 11, 11);
        Rectangle MinBtn = new Rectangle(23, 2, 11, 11);
        Rectangle MaxBtn = new Rectangle(43, 2, 11, 11);

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            State = MouseState.Down;
            Invalidate();
        }
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (X > 3 && X < 20)
            {
                FindForm().Close();
                //this.Parent.Default.Opacity2.Enabled = true;
                //Program.mainForm.Default.Opacity1.Enabled = false;
            }
            else if (X > 23 && X < 40)
            {
                FindForm().WindowState = FormWindowState.Minimized;
            }
            else if (X > 43 && X < 60)
            {
                if (_EnableMaximize == true)
                {
                    if (FindForm().WindowState == FormWindowState.Maximized)
                    {
                        FindForm().WindowState = FormWindowState.Minimized;
                        FindForm().WindowState = FormWindowState.Normal;
                    }
                    else
                    {
                        FindForm().WindowState = FormWindowState.Minimized;
                        FindForm().WindowState = FormWindowState.Maximized;
                    }
                }
            }
            State = MouseState.Over;
            Invalidate();
        }
        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            State = MouseState.Over;
            Invalidate();
        }
        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            State = MouseState.None;
            Invalidate();
        }
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            X = e.Location.X;
            Invalidate();
        }
        #endregion
        #region  Properties

        bool _EnableMaximize = true;
        public bool EnableMaximize
        {
            get
            {
                return _EnableMaximize;
            }
            set
            {
                _EnableMaximize = value;
                if (_EnableMaximize == true)
                {
                    this.Size = new Size(64, 22);
                }
                else
                {
                    this.Size = new Size(40, 16);
                }
                Invalidate();
            }
        }

        #endregion

        public MiniTimer_ControlBox()
        {
            SetStyle((System.Windows.Forms.ControlStyles)(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer), true);
            DoubleBuffered = true;
            _EnableMaximize = false;
            BackColor = Color.Transparent;
            Font = new Font("Marlett", 7);
            Anchor = (System.Windows.Forms.AnchorStyles)(AnchorStyles.Top | AnchorStyles.Left);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_EnableMaximize == true)
            {
                this.Size = new Size(58, 16);
            }
            else
            {
                this.Size = new Size(40, 16);
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            // Auto-decide control location on the theme container
            Location = new Point(15, 13);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            Bitmap B = new Bitmap(Width, Height);
            Graphics G = Graphics.FromImage(B);

            base.OnPaint(e);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            LinearGradientBrush LGBClose = new LinearGradientBrush(CloseBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
            G.FillEllipse(LGBClose, CloseBtn);
            G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), CloseBtn);

            LinearGradientBrush LGBMinimize = new LinearGradientBrush(MinBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
            G.FillEllipse(LGBMinimize, MinBtn);
            G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), MinBtn);

            if (_EnableMaximize == true)
            {
                LinearGradientBrush LGBMaximize = new LinearGradientBrush(MaxBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
                G.FillEllipse(LGBMaximize, MaxBtn);
                G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), MaxBtn);
            }

            switch (State)
            {
                case MouseState.None:
                    LinearGradientBrush xLGBClose_1 = new LinearGradientBrush(CloseBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
                    G.FillEllipse(xLGBClose_1, CloseBtn);
                    G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), CloseBtn);

                    LinearGradientBrush xLGBMinimize_1 = new LinearGradientBrush(MinBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
                    G.FillEllipse(xLGBMinimize_1, MinBtn);
                    G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), MinBtn);

                    if (_EnableMaximize == true)
                    {
                        LinearGradientBrush xLGBMaximize = new LinearGradientBrush(MaxBtn, Color.FromArgb(219, 223, 226), Color.FromArgb(213, 217, 218), 90);
                        G.FillEllipse(xLGBMaximize, MaxBtn);
                        G.DrawEllipse(new Pen(Color.FromArgb(196, 200, 203)), MaxBtn);
                    }
                    break;
                case MouseState.Over:
                    if (X > 3 && X < 20)
                    {
                        LinearGradientBrush xLGBClose = new LinearGradientBrush(CloseBtn, Color.FromArgb(210, 101, 104), Color.FromArgb(210, 101, 104), 90);
                        G.FillEllipse(xLGBClose, CloseBtn);
                        G.DrawEllipse(new Pen(Color.FromArgb(210, 101, 104)), CloseBtn);
                    }
                    else if (X > 23 && X < 40)
                    {
                        LinearGradientBrush xLGBMinimize = new LinearGradientBrush(MinBtn, Color.FromArgb(129, 151, 172), Color.FromArgb(129, 151, 172), 90);
                        G.FillEllipse(xLGBMinimize, MinBtn);
                        G.DrawEllipse(new Pen(Color.FromArgb(129, 151, 172)), MinBtn);
                    }
                    else if (X > 43 && X < 60)
                    {
                        if (_EnableMaximize == true)
                        {
                            LinearGradientBrush xLGBMaximize = new LinearGradientBrush(MaxBtn, Color.FromArgb(173, 173, 173), Color.FromArgb(173, 173, 173), 90);
                            G.FillEllipse(xLGBMaximize, MaxBtn);
                            G.DrawEllipse(new Pen(Color.FromArgb(173, 173, 173)), MaxBtn);
                        }
                    }
                    break;
            }

            e.Graphics.DrawImage((Image)(B.Clone()), 0, 0);
            G.Dispose();
            B.Dispose();
        }
    }

    #endregion
#region  Label
	
	public class MiniTimer_Label : Label
	{
		
		public MiniTimer_Label()
		{
			Font = new Font("Tahoma", (float) (9.75F), FontStyle.Regular);
			ForeColor = Color.FromArgb(136, 136, 136);
			BackColor = Color.Transparent;
		}
	}
	
#endregion
#region  Link Label
	public class MiniTimer_LinkLabel : LinkLabel
	{
		
		public MiniTimer_LinkLabel()
		{
			Font = new Font("Tahoma", (float) (9.75F), FontStyle.Regular);
			BackColor = Color.Transparent;
			LinkColor = Color.FromArgb(129, 151, 172);
			ActiveLinkColor = Color.FromArgb(109, 134, 158);
			VisitedLinkColor = Color.FromArgb(129, 151, 172);
			LinkBehavior = LinkBehavior.NeverUnderline;
		}
	}
	
#endregion
#region  Header Label
	
	public class MiniTimer_HeaderLabel : Label
	{
		
		public MiniTimer_HeaderLabel()
		{
			Font = new Font("Segoe UI", (float) (20.25F), FontStyle.Bold);
			ForeColor = Color.FromArgb(103, 103, 103);
			BackColor = Color.Transparent;
		}
	}
	
#endregion
#region  Button Base
	
	public class MiniTimer_ButtonBase : ContainerControl
	{
		
		public MiniTimer_ButtonBase()
		{
			DoubleBuffered = true;
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Size = new Size(116, 46);
		}
		
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint(e);
			
			Graphics G = e.Graphics;
			G.Clear(Color.FromArgb(255, 255, 255));
			
			G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			
			LinearGradientBrush LGB1 = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(230, 230, 230), Color.FromArgb(240, 240, 240), 90.0F);
			
			G.FillEllipse(LGB1, new Rectangle(0, 0, 44, 44));
			G.FillEllipse(LGB1, new Rectangle(35, 0, 44, 44));
			G.FillEllipse(LGB1, new Rectangle(70, 0, 44, 44));
			
			LGB1.Dispose();
		}
	}
	
#endregion
#region  Button
	
	public class MiniTimer_Button_1 : Control
	{
		
#region  Variables
		
		private int MouseState;
		private LinearGradientBrush InactiveGB;
		private LinearGradientBrush PressedGB;
		private LinearGradientBrush PressedContourGB;
		private Rectangle R1;
		private Pen P1;
		private Pen P3;
		private Image _Image;
		private Size _ImageSize;
		private ContentAlignment _ImageAlign = ContentAlignment.MiddleCenter;
		
#endregion
#region  Image Designer
		
		private static PointF ImageLocation(StringFormat SF, SizeF Area, SizeF ImageArea)
		{
			PointF MyPoint = new PointF();
			switch (SF.Alignment)
			{
				case StringAlignment.Center:
					MyPoint.X = (float) ((Area.Width - ImageArea.Width) / 2);
					break;
				case StringAlignment.Near:
					MyPoint.X = 2;
					break;
				case StringAlignment.Far:
					MyPoint.X = Area.Width - ImageArea.Width - 2;
					break;
					
			}
			
			switch (SF.LineAlignment)
			{
				case StringAlignment.Center:
					MyPoint.Y = (float) ((Area.Height - ImageArea.Height) / 2);
					break;
				case StringAlignment.Near:
					MyPoint.Y = 2;
					break;
				case StringAlignment.Far:
					MyPoint.Y = Area.Height - ImageArea.Height - 2;
					break;
			}
			return MyPoint;
		}
		
		private StringFormat GetStringFormat(ContentAlignment _ContentAlignment)
		{
			StringFormat SF = new StringFormat();
			switch (_ContentAlignment)
			{
				case ContentAlignment.MiddleCenter:
					SF.LineAlignment = StringAlignment.Center;
					SF.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.MiddleLeft:
					SF.LineAlignment = StringAlignment.Center;
					SF.Alignment = StringAlignment.Near;
					break;
				case ContentAlignment.MiddleRight:
					SF.LineAlignment = StringAlignment.Center;
					SF.Alignment = StringAlignment.Far;
					break;
				case ContentAlignment.TopCenter:
					SF.LineAlignment = StringAlignment.Near;
					SF.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.TopLeft:
					SF.LineAlignment = StringAlignment.Near;
					SF.Alignment = StringAlignment.Near;
					break;
				case ContentAlignment.TopRight:
					SF.LineAlignment = StringAlignment.Near;
					SF.Alignment = StringAlignment.Far;
					break;
				case ContentAlignment.BottomCenter:
					SF.LineAlignment = StringAlignment.Far;
					SF.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.BottomLeft:
					SF.LineAlignment = StringAlignment.Far;
					SF.Alignment = StringAlignment.Near;
					break;
				case ContentAlignment.BottomRight:
					SF.LineAlignment = StringAlignment.Far;
					SF.Alignment = StringAlignment.Far;
					break;
			}
			return SF;
		}
		
#endregion
#region  Properties
		
public Image Image
		{
			get
			{
				return _Image;
			}
			set
			{
				if (value == null)
				{
					_ImageSize = Size.Empty;
				}
				else
				{
					_ImageSize = value.Size;
				}
				
				_Image = value;
				Invalidate();
			}
		}
		
protected Size ImageSize
		{
			get
			{
				return _ImageSize;
			}
		}
		
public ContentAlignment ImageAlign
		{
			get
			{
				return _ImageAlign;
			}
			set
			{
				_ImageAlign = value;
				Invalidate();
			}
		}
		
#endregion
#region  EventArgs
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
			MouseState = 0;
			Invalidate();
			base.OnMouseUp(e);
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			MouseState = 1;
			Invalidate();
			base.OnMouseDown(e);
		}
		
		protected override void OnMouseLeave(EventArgs e)
		{
			MouseState = 0;
			Invalidate();
			base.OnMouseLeave(e);
		}
		
		protected override void OnTextChanged(System.EventArgs e)
		{
			Invalidate();
			base.OnTextChanged(e);
		}
		
#endregion
		
		public MiniTimer_Button_1()
		{
			SetStyle((System.Windows.Forms.ControlStyles) (ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint), true);
			
			BackColor = Color.Transparent;
			DoubleBuffered = true;
			Size = new Size(27, 27);
			P1 = new Pen(Color.FromArgb(190, 190, 190)); // P1 = Border color
		}
		
		protected override void OnResize(System.EventArgs e)
		{
			this.Size = new Size(27, 27);
			R1 = new Rectangle(0, 0, Width, Height);
			
			InactiveGB = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(255, 255, 255), Color.FromArgb(240, 240, 240), 90.0F);
			PressedGB = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(245, 245, 245), Color.FromArgb(240, 240, 240), 90.0F);
			PressedContourGB = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(167, 167, 167), Color.FromArgb(167, 167, 167), 90.0F);
			P3 = new Pen(PressedContourGB);
			
			Invalidate();
			base.OnResize(e);
		}
		private void FinishDrawing(Graphics g, Point center, int radius)
		{
			Rectangle MyCircle = new Rectangle((int) (center.X / 2), (int) (center.Y / 2), radius * 2, radius * 2);
			PointF ImagePoint = ImageLocation(GetStringFormat(ImageAlign), Size, ImageSize);
			
			switch (MouseState)
			{
				case 0: //Inactive
					g.FillEllipse(InactiveGB, MyCircle);
					g.DrawEllipse(P1, MyCircle);
					
					if (Image == null)
					{
					}
					else
					{
						g.DrawImage(_Image, 5, 5, 16, 16);
					}
					break;
					
				case 1: //Pressed
					g.FillEllipse(PressedGB, MyCircle);
					g.DrawEllipse(P1, MyCircle);
					
					if (Image == null)
					{
					}
					else
					{
						g.DrawImage(_Image, 5, 5, 16, 16);
					}
					break;
			}
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			FinishDrawing(e.Graphics, new Point(0, 0), 13);
		}
	}
	
#endregion
#region  NumericUpDown
	
	public class MiniTimer_NumericUpDown : Control
	{
		
#region  Enums
		
		public enum MouseState
		{
			None = 0,
			Over = 1,
			Down = 2
		}
		
#endregion
#region  Variables
		
		internal System.Drawing.Graphics G;
		internal Bitmap B;
		private int Xval;
		private int Yval;
		private long _Value;
		private long _Minimum;
		private long _Maximum;
		private bool Flag;
		
#endregion
#region  Properties
		
public long Value
		{
			get
			{
				return _Value;
			}
			set
			{
				if (value <= _Maximum & value >= _Minimum)
				{
					_Value = value;
				}
				Invalidate();
			}
		}
		
public long Minimum
		{
			get
			{
				return _Minimum;
			}
			set
			{
				if (value < _Maximum)
				{
					_Minimum = value;
				}
				if (_Value < _Minimum)
				{
					_Value = Minimum;
				}
				Invalidate();
			}
		}
		
public long Maximum
		{
			get
			{
				return _Maximum;
			}
			set
			{
				if (value > _Minimum)
				{
					_Maximum = value;
				}
				if (_Value > _Maximum)
				{
					_Value = _Maximum;
				}
				Invalidate();
			}
		}
		
#endregion
#region  EventArgs
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Xval = e.Location.X;
			Yval = e.Location.Y;
			Invalidate();
			if (e.X < Width - 23)
			{
				Cursor = Cursors.IBeam;
			}
			else
			{
				Cursor = Cursors.Hand;
			}
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (Xval > Width - 21 && Xval < Width - 3)
			{
				if (Yval < 15)
				{
					if ((Value + 1) <= _Maximum)
					{
						_Value++;
					}
				}
				else
				{
					if ((Value - 1) >= _Minimum)
					{
						_Value--;
					}
				}
			}
			else
			{
				Flag = !Flag;
				Focus();
			}
			Invalidate();
		}
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			try
			{
				if (Flag)
				{
					_Value = long.Parse((_Value).ToString() + e.KeyChar.ToString().ToString());
				}
				if (_Value > _Maximum)
				{
					_Value = _Maximum;
				}
				Invalidate();
			}
			catch
			{
			}
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Back)
			{
				Value = 0;
			}
		}
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Height = 27;
		}
		
#endregion
		
		public MiniTimer_NumericUpDown()
		{
			SetStyle((System.Windows.Forms.ControlStyles) (ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor), true);
			DoubleBuffered = true;
			Font = new Font("Segoe UI", 10);
			_Minimum = 0;
			_Maximum = 100;
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			B = new Bitmap(Width, Height);
			G = Graphics.FromImage(B);
			
			Rectangle CurrentRect = new Rectangle(0, 0, Width, Height);
			
			G.SmoothingMode = (System.Drawing.Drawing2D.SmoothingMode) 2;
			G.PixelOffsetMode = (System.Drawing.Drawing2D.PixelOffsetMode) 2;
			G.TextRenderingHint = (System.Drawing.Text.TextRenderingHint) 5;
			G.Clear(BackColor);
			
			G.FillRectangle(new SolidBrush(Color.White), CurrentRect);
			
			G.DrawString("+", new Font("Tahoma", 9, FontStyle.Bold), new SolidBrush(Color.FromArgb(136, 136, 136)), new Point(Width - 11, 10), new StringFormat() {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center});
			G.DrawString("-", new Font("Tahoma", 9, FontStyle.Bold), new SolidBrush(Color.FromArgb(136, 136, 136)), new Point(Width - 11, 20), new StringFormat() {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center});
			
			G.DrawString(System.Convert.ToString(Value), Font, new SolidBrush(Color.FromArgb(136, 136, 136)), new Rectangle(5, 1, Width, Height), new StringFormat() {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center});
			
			
			G.Dispose();
			e.Graphics.InterpolationMode = (System.Drawing.Drawing2D.InterpolationMode) 7;
			e.Graphics.DrawImageUnscaled(B, 0, 0);
			B.Dispose();
		}
		
	}
}
#endregion