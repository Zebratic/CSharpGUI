#region Using's
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion

namespace CSharpGUI
{
    public class CSharpGUI
    {
        #region Variables
        public bool SetupDone = false;
        public Form TargetForm;
        public Graphics Overlay;
        public Bitmap Bm;
        public Random rnd = new Random();
        #endregion

        #region Theme Settings
        public Theme CurrentTheme = new Theme();
        public class Theme
        {
            public int OutlineSize = 2;
            public Color StringColor = Color.White;
            public Color StringOutlineColor = Color.Black;
            public Color WindowColor = Color.DarkBlue;
            public Color WindowOutlineColor = Color.Blue;
            public Color ControlColor = Color.DarkBlue;
            public Color ControlOutlineColor = Color.Blue;

            public Font Font = new Font(FontFamily.GenericSansSerif, 17, FontStyle.Regular);
        }
        #endregion

        #region AddControls's
        public List<Controls.Window> AddedWindows = new List<Controls.Window>();
        public List<Controls.Particles> AddedParticles = new List<Controls.Particles>();
        public void AddControl(CSharpGUI currentGUI, object control, Controls.Window window = null)
        {
            if (window != null)
                window.Controls.Add(control);
            else if (control.GetType() == typeof(Controls.Window))
                currentGUI.AddedWindows.Add(control as Controls.Window);
            else if (control.GetType() == typeof(Controls.Particles))
                currentGUI.AddedParticles.Add(control as Controls.Particles);
            else
                MessageBox.Show("Unable to add control: " + control.ToString() + "\nYou might consider assigning this control to a window.");

            currentGUI.TargetForm.Invalidate();
        }
        #endregion

        #region Setup
        public void Setup(Form Target)
        {
            if (!SetupDone)
            {
                TargetForm = Target;
                Bm = new Bitmap(Target.Width, Target.Height);
                Overlay = TargetForm.CreateGraphics();
                Overlay.Clear(TargetForm.BackColor);
                new Thread(() => ParticleTimer(0)).Start();
                TargetForm.Paint += delegate (object sender, PaintEventArgs e) { Window_Paint(sender, e); };
                TargetForm.FormClosing += delegate (object sender, FormClosingEventArgs e) { Window_FormClosing(sender, e); };

                SetupDone = true;
            }
        }
        #endregion

        #region ResetPath
        public GraphicsPath ResetPath()
        {
            return new GraphicsPath();
        }
        #endregion

        #region Form Paint
        public void Window_Paint(object sender, PaintEventArgs e)
        {
            GraphicsPath p = new GraphicsPath(); // For text rendering

            e.Graphics.InterpolationMode = InterpolationMode.High;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            //e.Graphics.DrawImage(Bm, 0, 0);
            try
            {
                foreach (var control in AddedParticles)
                {
                    if (control.GetType() == typeof(Controls.Particles))
                    {
                        #region Render Particles
                        #region Convert and get variables
                        Controls.Particles w = (Controls.Particles)Convert.ChangeType(control, typeof(Controls.Particles)); // Convert to type
                        ParticleStyle Style = w.Style;
                        int Count = w.Count;
                        int Speed = w.Speed;
                        int MovementDegrees = w.MovementDegrees;
                        int Distance = w.Distance;
                        int ParticleSize = w.ParticleSize;
                        Point Location = w.Location;
                        Size Size = w.Size;
                        List<Controls.Particles.Particle> particles = w.ParticlesList;
                        #endregion

                        foreach (Controls.Particles.Particle particle in particles)
                        {
                            #region Move Particle
                            if (particle.Location.X >= Size.Width || particle.Location.Y >= Size.Height || particle.Location.X <= 0 || particle.Location.Y <= 0)
                            {
                                particle.Location = new Point(rnd.Next(0, Size.Width + 1), rnd.Next(0, Size.Height + 1));
                                if (MovementDegrees == 0)
                                    particle.Direction = rnd.Next(0, 361);

                                particle.i = 0;
                            }
                            else
                            {
                                particle.Location = MoveTowards(particle.Location, particle.Direction, Speed);
                            }
                            #endregion

                            #region Render Particles by Style
                            if (Style == ParticleStyle.Polygons)
                            {
                                e.Graphics.DrawEllipse(Pens.Black, new Rectangle(particle.Location, new Size(ParticleSize, ParticleSize)));

                                #region Draw lines between particles
                                foreach (Controls.Particles.Particle particle2 in particles)
                                {
                                    if (particle != particle2)
                                    {
                                        float DotDistanceX = particle.Location.X - particle2.Location.X;
                                        float DotDistanceY = particle.Location.Y - particle2.Location.Y;
                                        int DotDistance = (int)Math.Sqrt(DotDistanceX * DotDistanceX + DotDistanceY * DotDistanceY);
                                        if (DotDistance <= Distance)
                                        {
                                            e.Graphics.DrawLine(new Pen(Color.Black), particle.Location.X, particle.Location.Y, particle2.Location.X, particle2.Location.Y);
                                        }
                                    }
                                }

                                foreach (Controls.Particles.Particle particle2 in particles)
                                    particle2.LineDrawn = false; // Reset lines
                                #endregion
                            }
                            else if (Style == ParticleStyle.Cubes)
                            {
                                // Render 3D Cube Spinning
                            }
                            else if (Style == ParticleStyle.Text)
                            {
                                #region Render Text
                                e.Graphics.DrawString(particle.Text, CurrentTheme.Font, new SolidBrush(Color.Black), particle.Location);
                                #endregion
                            }

                            else if (Style == ParticleStyle.Dicks)
                            {
                                // Render some PPs on screen ;)
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                foreach (var control in AddedWindows)
                {
                    if (control.GetType() == typeof(Controls.Window))
                    {
                        #region Render Window
                        #region Convert and get variables
                        Controls.Window w = (Controls.Window)Convert.ChangeType(control, typeof(Controls.Window)); // Convert to type
                        string Title = w.Title;
                        Size Size = w.Size;
                        Point Location = w.Location;
                        bool Dragable = w.Dragable;
                        bool Window3D = w.Window3D;
                        bool Outline = w.Outline;
                        List<object> Controls = w.Controls;
                        #endregion

                        if (w.Visible)
                        {
                            #region Main Window
                            int size3D = CurrentTheme.OutlineSize * 3;

                            if (Window3D)
                            {
                                e.Graphics.FillRectangle(new SolidBrush(CurrentTheme.WindowColor), new Rectangle(new Point(Location.X - size3D, Location.Y - size3D), Size));

                                // Fill out corners
                                List<PointF> toprightp = new List<PointF>();
                                toprightp.Add(new PointF(Location.X + Size.Width - size3D - 1, Location.Y - size3D));
                                toprightp.Add(new PointF(Location.X + Size.Width, Location.Y));
                                toprightp.Add(new PointF(Location.X + Size.Width - size3D - 1, Location.Y));

                                e.Graphics.FillPolygon(new SolidBrush(CurrentTheme.WindowColor), toprightp.ToArray()); // Top Right Corner

                                List<PointF> bottomleftp = new List<PointF>();
                                bottomleftp.Add(new PointF(Location.X - size3D, Location.Y + Size.Height - size3D - 1));
                                bottomleftp.Add(new PointF(Location.X, Location.Y + Size.Height));
                                bottomleftp.Add(new PointF(Location.X, Location.Y + Size.Height - size3D - 1));

                                e.Graphics.FillPolygon(new SolidBrush(CurrentTheme.WindowColor), bottomleftp.ToArray()); // Bottom Left Corner
                            }

                            e.Graphics.FillRectangle(new SolidBrush(CurrentTheme.WindowColor), new Rectangle(Location, Size));

                            if (Outline)
                            {
                                if (Window3D)
                                {
                                    List<PointF> outline = new List<PointF>();
                                    outline.Add(new PointF(Location.X - (CurrentTheme.OutlineSize / 2), Location.Y + Size.Height + (CurrentTheme.OutlineSize / 4)));
                                    outline.Add(new PointF(Location.X - size3D, Location.Y + Size.Height - size3D));
                                    outline.Add(new PointF(Location.X - size3D, Location.Y - size3D));
                                    outline.Add(new PointF(Location.X + Size.Width - size3D, Location.Y - size3D));
                                    outline.Add(new PointF(Location.X + Size.Width + (CurrentTheme.OutlineSize / 4), Location.Y - (CurrentTheme.OutlineSize / 2)));

                                    e.Graphics.DrawLines(new Pen(CurrentTheme.WindowOutlineColor, CurrentTheme.OutlineSize), outline.ToArray());

                                    e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor, CurrentTheme.OutlineSize), Location.X - size3D, Location.Y - size3D, Location.X, Location.Y); // Top Left Corner
                                }

                                e.Graphics.DrawRectangle(new Pen(CurrentTheme.WindowOutlineColor, CurrentTheme.OutlineSize), Location.X - (CurrentTheme.OutlineSize / 2), Location.Y - (CurrentTheme.OutlineSize / 2), Size.Width + (CurrentTheme.OutlineSize / 2), Size.Height + (CurrentTheme.OutlineSize / 2)); // Frame
                            }
                            #endregion

                            #region Title
                            string FinalTitle = CalculateStringByWidth(Title, Size.Width, CurrentTheme.Font, e.Graphics, out SizeF StringSize);

                            p = ResetPath();
                            p.AddString(FinalTitle, CurrentTheme.Font.FontFamily, (int)CurrentTheme.Font.Style, e.Graphics.DpiY * CurrentTheme.Font.Size / 72, Location, new StringFormat());
                            e.Graphics.DrawPath(Pens.Black, p);
                            e.Graphics.FillPath(new SolidBrush(CurrentTheme.StringColor), p);
                            e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor), Location.X, Location.Y + StringSize.Height, Location.X + Size.Width, Location.Y + StringSize.Height);
                            w.DragBar = new Rectangle(Location.X, Location.Y, Size.Width, Location.Y + (int)StringSize.Height);
                            if (!w.EventsAdded)
                            {
                                TargetForm.MouseDown += delegate (object sender2, MouseEventArgs e2) { Window_MouseDown(sender2, e2, w); };
                                TargetForm.MouseMove += delegate (object sender2, MouseEventArgs e2) { Window_MouseMove(sender2, e2, w); };
                                TargetForm.MouseUp += delegate (object sender2, MouseEventArgs e2) { Window_MouseUp(w); };
                                TargetForm.KeyDown += delegate (object sender2, KeyEventArgs e2) { Window_KeyDown(sender2, e2, w); };
                                TargetForm.Resize += delegate (object sender2, EventArgs e2) { Window_ResizeForm(sender2, e2); };

                                w.EventsAdded = true;
                            }
                            #endregion

                            #region Render Controls
                            if (Controls != null)
                            {
                                foreach (var con in Controls)
                                {
                                    Controls.IsVisible x = Cast<Controls.IsVisible>(con); // Cast control
                                    if (x.Visible)
                                    {
                                        if (con.GetType() == typeof(Controls.Button))
                                        {
                                            #region Convert and get variables
                                            Controls.Button z = (Controls.Button)Convert.ChangeType(con, typeof(Controls.Button)); // Convert to type
                                            string ButtonText = z.Text;
                                            Size ButtonSize = z.Size;
                                            float ButtonTextSize = z.TextSize;
                                            bool ButtonLocationPredefined = z.LocationPredefined;
                                            Point ButtonLocation = z.Location;

                                            if (ButtonLocationPredefined) { }
                                            // Calculate location that is not used
                                            #endregion

                                            #region Calculate string size and text height
                                            z.ClickArea = new Rectangle(new Point(Location.X + ButtonLocation.X - 1, w.DragBar.Size.Height + ButtonLocation.Y - 1), new Size(ButtonSize.Width + 2, ButtonSize.Height + 2));

                                            e.Graphics.FillRectangle(new SolidBrush(CurrentTheme.ControlColor), new Rectangle(new Point(Location.X + ButtonLocation.X, w.DragBar.Size.Height + ButtonLocation.Y), ButtonSize));
                                            e.Graphics.DrawRectangle(new Pen(CurrentTheme.ControlOutlineColor, CurrentTheme.OutlineSize / 2), z.ClickArea); // Frame

                                            Point ButtonTextLocation = new Point((int)(Location.X + ButtonLocation.X + (ButtonTextSize / 2)),
                                                                                 (int)(ButtonLocation.Y + w.DragBar.Size.Height + (ButtonSize.Height / 2) - (ButtonTextSize / 2)));
                                            #endregion

                                            #region Render button
                                            p = ResetPath();
                                            p.AddString(ButtonText, CurrentTheme.Font.FontFamily, (int)CurrentTheme.Font.Style, ButtonTextSize, ButtonTextLocation, new StringFormat());
                                            e.Graphics.DrawPath(Pens.Black, p);
                                            e.Graphics.FillPath(new SolidBrush(CurrentTheme.StringColor), p);
                                            #endregion
                                        }
                                        else if (con.GetType() == typeof(Controls.Label))
                                        {
                                            #region Convert and get variables
                                            Controls.Label z = (Controls.Label)Convert.ChangeType(con, typeof(Controls.Label)); // Convert to type
                                            string LabelText = z.Text;
                                            bool LabelLocationPredefined = z.LocationPredefined;
                                            Point LabelLocation = z.Location;
                                            #endregion

                                            #region Calculate string size and text height
                                            int TextSize = (int)(CurrentTheme.Font.Size - (CurrentTheme.Font.Size / 4));
                                            Point LabelTextLocation = new Point(Location.X + LabelLocation.X - (TextSize / 4), LabelLocation.Y + w.DragBar.Size.Height - (TextSize / 4));
                                            #endregion

                                            #region Render text
                                            p = ResetPath();
                                            p.AddString(LabelText, CurrentTheme.Font.FontFamily, (int)CurrentTheme.Font.Style, TextSize, LabelTextLocation, new StringFormat());
                                            e.Graphics.DrawPath(Pens.Black, p);
                                            e.Graphics.FillPath(new SolidBrush(CurrentTheme.StringColor), p);
                                            #endregion
                                        }
                                        else if (con.GetType() == typeof(Controls.TextBox))
                                        {
                                            #region Convert and get variables
                                            Controls.TextBox z = (Controls.TextBox)Convert.ChangeType(con, typeof(Controls.TextBox)); // Convert to type

                                            string TextBoxText = z.Text;
                                            float TextBoxTextSize = z.TextSize;
                                            int TextBoxTextLimit = z.TextLimit;
                                            bool TextBoxNumbersOnly = z.NumbersOnly;
                                            Size TextBoxSize = z.Size;
                                            bool TextBoxLocationPredefined = z.LocationPredefined;
                                            Point TextBoxLocation = z.Location;

                                            if (TextBoxLocationPredefined) { }
                                            // Calculate location that is not used
                                            #endregion

                                            #region Calculate string size and text height
                                            z.ClickArea = new Rectangle(new Point(Location.X + TextBoxLocation.X - 1, w.DragBar.Size.Height + TextBoxLocation.Y - 1), new Size(TextBoxSize.Width + 2, TextBoxSize.Height + 2));

                                            Point TextBoxTextLocation = new Point((int)(Location.X + TextBoxLocation.X + (TextBoxTextSize / 2)),
                                                                                 (int)(TextBoxLocation.Y + w.DragBar.Size.Height + (TextBoxSize.Height / 2) - (TextBoxTextSize / 2)));

                                            SizeF TextBoxStringSize = e.Graphics.MeasureString(TextBoxText, CurrentTheme.Font);
                                            #endregion

                                            #region Render TextBox
                                            #region Render Frame
                                            e.Graphics.FillRectangle(new SolidBrush(CurrentTheme.ControlColor), new Rectangle(new Point(Location.X + TextBoxLocation.X, w.DragBar.Size.Height + TextBoxLocation.Y), TextBoxSize));
                                            e.Graphics.DrawRectangle(new Pen(CurrentTheme.ControlOutlineColor, CurrentTheme.OutlineSize / 2), z.ClickArea); // Frame
                                            #endregion

                                            #region Render Cursor
                                            if (z.Editing)
                                            {
                                                if (System.Environment.TickCount - z.CursorValue >= 1000)
                                                {
                                                    z.CursorShown = !z.CursorShown;
                                                    z.CursorValue = 0;
                                                    z.CursorValue = System.Environment.TickCount;
                                                }

                                                if (z.CursorShown)
                                                {
                                                    Point p1 = new Point((int)(TextBoxTextLocation.X + (TextBoxStringSize.Width / 2)), (int)(z.ClickArea.Location.Y + 4));
                                                    Point p2 = new Point((int)(TextBoxTextLocation.X + (TextBoxStringSize.Width / 2)), (int)(z.ClickArea.Location.Y + z.ClickArea.Size.Height - 4));
                                                    if (TextBoxText.Length == 0) // Fix 
                                                    {
                                                        p1.X = TextBoxTextLocation.X;
                                                        p2.X = TextBoxTextLocation.X;
                                                    }
                                                    e.Graphics.DrawLine(new Pen(CurrentTheme.StringColor, CurrentTheme.OutlineSize / 2), p1, p2);
                                                }
                                            }

                                            #endregion

                                            #region Render string
                                            p = ResetPath();
                                            p.AddString(TextBoxText, CurrentTheme.Font.FontFamily, (int)CurrentTheme.Font.Style, TextBoxTextSize, TextBoxTextLocation, new StringFormat());
                                            e.Graphics.DrawPath(Pens.Black, p);
                                            e.Graphics.FillPath(new SolidBrush(CurrentTheme.StringColor), p);
                                            #endregion
                                            #endregion
                                        }
                                        else if (con.GetType() == typeof(Controls.Checkbox))
                                        {
                                            #region Convert and get variables
                                            Controls.Checkbox z = (Controls.Checkbox)Convert.ChangeType(con, typeof(Controls.Checkbox)); // Convert to type
                                            string CheckboxText = z.Text;
                                            CheckedStyle CheckboxStyle = z.Style;
                                            Color CheckboxColor = z.Color;
                                            bool CheckboxLocationPredefined = z.LocationPredefined;
                                            Point CheckboxLocation = z.Location;

                                            if (CheckboxLocationPredefined) { }
                                            // Calculate location that is not used
                                            #endregion

                                            #region Calculate string size and checkbox height
                                            SizeF CheckboxStringSize = e.Graphics.MeasureString(CheckboxText, CurrentTheme.Font);
                                            int CheckboxSize = (int)(CurrentTheme.Font.Size - (CurrentTheme.Font.Size / 4));
                                            #endregion

                                            #region Render checkbox
                                            #region Render checkbox base
                                            Rectangle CheckboxRect = new Rectangle();
                                            if (CheckboxStyle == CheckedStyle.Switch)
                                            {
                                                CheckboxRect = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size((CheckboxSize * 2) + 2, CheckboxSize + 2));

                                                Rectangle c1 = new Rectangle(new Point(Location.X + CheckboxLocation.X - 2, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));
                                                Rectangle c2 = new Rectangle(new Point(Location.X + CheckboxLocation.X + CheckboxSize, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));
                                                Rectangle fill = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1 + (CheckboxSize / 2), w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));

                                                e.Graphics.FillEllipse(new SolidBrush(Color.LightGray), c1);
                                                e.Graphics.FillEllipse(new SolidBrush(Color.LightGray), c2);
                                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), fill);

                                            }
                                            else if (CheckboxStyle == CheckedStyle.SwitchInverted)
                                            {
                                                CheckboxRect = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size((CheckboxSize * 2) + 2, CheckboxSize + 2));

                                                Rectangle c1 = new Rectangle(new Point(Location.X + CheckboxLocation.X - 2, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));
                                                Rectangle c2 = new Rectangle(new Point(Location.X + CheckboxLocation.X + CheckboxSize, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));
                                                Rectangle fill = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1 + (CheckboxSize / 2), w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));

                                                if (z.Checked)
                                                {
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.LightGreen), c1);
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.LightGreen), c2);
                                                    e.Graphics.FillRectangle(new SolidBrush(Color.LightGreen), fill);
                                                }
                                                else
                                                {
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.LightGray), c1);
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.LightGray), c2);
                                                    e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), fill);
                                                }
                                            }
                                            else
                                            {
                                                CheckboxRect = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size(CheckboxSize + 2, CheckboxSize + 2));
                                                e.Graphics.DrawRectangle(new Pen(CurrentTheme.WindowOutlineColor, CurrentTheme.OutlineSize / 2), CheckboxRect); // Frame
                                            }
                                            #endregion

                                            #region Calculate text size and click area
                                            Point CheckboxTextLocation = new Point((int)(Location.X + CheckboxLocation.X + CheckboxRect.Size.Width),
                                                                                 (int)(CheckboxLocation.Y + w.DragBar.Size.Height - (CheckboxStringSize.Height / (CheckboxLocation.Y + w.DragBar.Size.Height))));

                                            z.ClickArea = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y - 1), new Size((int)((CheckboxSize * 2) + 2 + (CheckboxStringSize.Width / 2)), CheckboxSize + 2));
                                            #endregion

                                            #region Render ON/OFF
                                            Rectangle CheckArea = new Rectangle(CheckboxRect.Location.X + 2, CheckboxRect.Location.Y + 2, CheckboxRect.Width - 4, CheckboxRect.Height - 4);
                                            if (z.Checked)
                                            {
                                                if (CheckboxStyle == CheckedStyle.Filled)
                                                {
                                                    e.Graphics.FillRectangle(new SolidBrush(CurrentTheme.WindowOutlineColor), CheckArea);
                                                }
                                                else if (CheckboxStyle == CheckedStyle.X)
                                                {
                                                    e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor, 2), new Point(CheckArea.X, CheckArea.Y), new Point(CheckArea.X + CheckArea.Size.Width, CheckArea.Y + CheckArea.Size.Height));
                                                    e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor, 2), new Point(CheckArea.X, CheckArea.Y + CheckArea.Size.Height), new Point(CheckArea.X + CheckArea.Size.Width, CheckArea.Y));
                                                }
                                                else if (CheckboxStyle == CheckedStyle.Checkmark)
                                                {
                                                    e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor, 2), new Point(CheckArea.X, CheckArea.Y + (CheckArea.Size.Height / 2)), new Point(CheckArea.X + (CheckArea.Size.Width / 2), CheckArea.Y + CheckArea.Size.Height));
                                                    e.Graphics.DrawLine(new Pen(CurrentTheme.WindowOutlineColor, 2), new Point(CheckArea.X + (CheckArea.Size.Width / 2), CheckArea.Y + CheckArea.Size.Height), new Point(CheckArea.X + CheckArea.Size.Width, CheckArea.Y));
                                                }
                                                else if (CheckboxStyle == CheckedStyle.Switch)
                                                {
                                                    z.ClickArea = new Rectangle(new Point(CheckArea.X - 4, CheckArea.Y - 2), new Size((int)((CheckboxSize * 2) + 8 + (CheckboxStringSize.Width / 2)), (int)((CheckboxStringSize.Height / 2) + 2))); // Fix area for switch
                                                    Rectangle on = new Rectangle(new Point(Location.X + CheckboxLocation.X + CheckboxSize + 1, w.DragBar.Size.Height + CheckboxLocation.Y), new Size(CheckboxSize, CheckboxSize));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(50, 0, 0, 0)), new Rectangle(on.X - 2, on.Y, on.Size.Width, on.Size.Height + 1));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.LightGreen), on);
                                                }
                                                else if (CheckboxStyle == CheckedStyle.SwitchInverted)
                                                {
                                                    z.ClickArea = new Rectangle(new Point(CheckArea.X - 4, CheckArea.Y - 2), new Size((int)((CheckboxSize * 2) + 8 + (CheckboxStringSize.Width / 2)), (int)((CheckboxStringSize.Height / 2) + 2))); // Fix area for switch
                                                    Rectangle on = new Rectangle(new Point(Location.X + CheckboxLocation.X + CheckboxSize + 1, w.DragBar.Size.Height + CheckboxLocation.Y), new Size(CheckboxSize, CheckboxSize));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(50, 0, 0, 0)), new Rectangle(on.X - 2, on.Y, on.Size.Width, on.Size.Height + 1));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.White), on);
                                                }
                                            }
                                            else
                                            {
                                                if (CheckboxStyle == CheckedStyle.Switch)
                                                {
                                                    z.ClickArea = new Rectangle(new Point(CheckArea.X - 4, CheckArea.Y - 2), new Size((int)((CheckboxSize * 2) + 8 + (CheckboxStringSize.Width / 2)), (int)((CheckboxStringSize.Height / 2) + 2))); // Fix area for switch
                                                    Rectangle off = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y), new Size(CheckboxSize, CheckboxSize));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(50, 0, 0, 0)), new Rectangle(off.X + 2, off.Y, off.Size.Width, off.Size.Height + 1));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.IndianRed), off);
                                                }
                                                else if (CheckboxStyle == CheckedStyle.SwitchInverted)
                                                {
                                                    z.ClickArea = new Rectangle(new Point(CheckArea.X - 4, CheckArea.Y - 2), new Size((int)((CheckboxSize * 2) + 8 + (CheckboxStringSize.Width / 2)), (int)((CheckboxStringSize.Height / 2) + 2))); // Fix area for switch
                                                    Rectangle off = new Rectangle(new Point(Location.X + CheckboxLocation.X - 1, w.DragBar.Size.Height + CheckboxLocation.Y), new Size(CheckboxSize, CheckboxSize));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(50, 0, 0, 0)), new Rectangle(off.X + 2, off.Y, off.Size.Width, off.Size.Height + 1));
                                                    e.Graphics.FillEllipse(new SolidBrush(Color.White), off);
                                                }
                                            }
                                            #endregion

                                            #region Render text
                                            p = ResetPath();
                                            p.AddString(CheckboxText, CurrentTheme.Font.FontFamily, (int)CurrentTheme.Font.Style, CheckboxSize, CheckboxTextLocation, new StringFormat());
                                            e.Graphics.DrawPath(Pens.Black, p);
                                            e.Graphics.FillPath(new SolidBrush(CurrentTheme.StringColor), p);
                                            #endregion
                                            #endregion
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                UpdateFPS();
            }
            catch (Exception ex)
            {
                #region Show error message
                p = new GraphicsPath();
                p.AddString(ex.ToString(), FontFamily.GenericSansSerif, (int)FontStyle.Regular, e.Graphics.DpiY * 12 / 72, new Point(10, 10), new StringFormat());
                e.Graphics.DrawPath(Pens.Black, p);
                e.Graphics.FillPath(new SolidBrush(Color.Red), p);
                #endregion
            }
        }
        #endregion

        #region FPS Counter
        private int lastTick;
        private int lastFrameRate;
        public int FPS;

        private void UpdateFPS()
        {
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                FPS = lastFrameRate;
                lastFrameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            lastFrameRate++;
        }
        #endregion

        #region Drag/Click/Type/Resize Functions
        Task latestTask = null;
        public void Window_MouseDown(object sender, MouseEventArgs e, Controls.Window Window)
        {
            Point MousePos = e.Location;
            #region Window Drag
            if (e.Button == MouseButtons.Left && Window.Dragable && Window.Visible)
            {
                if (MousePos.X > TargetForm.Width || MousePos.X < 0 || MousePos.Y > TargetForm.Height || MousePos.Y < 0)
                    return;

                if (MousePos.X >= Window.DragBar.X && MousePos.X <= (Window.DragBar.X + Window.DragBar.Width) && MousePos.Y >= Window.DragBar.Y && MousePos.Y <= Window.DragBar.Height) // If MouseDown is on dragbar
                {
                    bool Draggable = true;
                    List<Rectangle> dragbars = new List<Rectangle>();
                    List<Rectangle> windows = new List<Rectangle>();
                    foreach (var control in AddedWindows)
                    {
                        if (control.GetType() == typeof(Controls.Window))
                        {
                            Controls.Window w = (Controls.Window)Convert.ChangeType(control, typeof(Controls.Window)); // Convert to type
                            dragbars.Add(new Rectangle(w.DragBar.Location, w.DragBar.Size));
                            windows.Add(new Rectangle(w.Location, w.Size));
                        }
                    }

                    bool DragbarBlocked = false;
                    foreach (Rectangle rect in windows)
                    {
                        if (rect == new Rectangle(Window.Location, Window.Size))
                        {
                            Rectangle dragbar;
                            foreach (Rectangle rect2 in dragbars)
                                if (rect2 == new Rectangle(Window.DragBar.Location, Window.DragBar.Size))
                                    dragbar = rect2;

                            foreach (Rectangle rect3 in windows)
                            {
                                if (rect3 != new Rectangle(AddedWindows[AddedWindows.Count - 1].Location, AddedWindows[AddedWindows.Count - 1].Size))
                                {
                                }
                                else if (rect3.Contains(MousePos) && rect3 != rect)
                                {
                                    DragbarBlocked = true;
                                }
                            }
                        }
                    }

                    if (Draggable && !DragbarBlocked)
                    {
                        Window.Dragging = true;
                        Window.MouseGrabPoint = MousePos;
                        AddedWindows.Remove(Window);
                        AddedWindows.Add(Window);
                    }
                }
            }
            #endregion

            #region Button Clicks
            if (e.Button == MouseButtons.Left)
            {
                foreach (Controls.Button button in Window.Controls.OfType<Controls.Button>())
                {
                    if (button.ClickArea.IsPointInside(MousePos) && button.Visible)
                    {
                        try
                        {
                            if (latestTask == null || latestTask.IsCompleted)
                            {
                                latestTask = new Task(button.Click);
                                new Thread(latestTask.Start).Start();
                            }
                        }
                        catch { }
                    }
                }
            }
            #endregion

            #region Checkbox Clicks
            if (e.Button == MouseButtons.Left)
            {
                foreach (Controls.Checkbox checkbox in Window.Controls.OfType<Controls.Checkbox>())
                {
                    if (checkbox.ClickArea.IsPointInside(MousePos) && checkbox.Visible)
                    {
                        checkbox.Checked = !checkbox.Checked;
                    }
                }
            }
            #endregion

            #region TextBox Editing
            if (e.Button == MouseButtons.Left)
            {
                foreach (Controls.TextBox textbox in Window.Controls.OfType<Controls.TextBox>())
                {
                    if (textbox.ClickArea.IsPointInside(MousePos) && textbox.Visible)
                    {
                        textbox.Editing = true;
                        textbox.CursorShown = true;
                        textbox.CursorValue = System.Environment.TickCount;
                        ;
                    }
                    else
                    {
                        textbox.Editing = false;
                        textbox.CursorShown = false;
                    }
                }
            }
            #endregion
        }
        public void Window_MouseMove(object sender, MouseEventArgs e, Controls.Window Window)
        {
            #region Window Drag
            if (Window.Dragging && Window.Dragable)
            {
                Point MousePos = e.Location;
                if (MousePos.X > TargetForm.Width || MousePos.X < 0 || MousePos.Y > TargetForm.Height || MousePos.Y < 0)
                    return;

                int xdif = Window.MouseGrabPoint.X - Window.DragBar.X;
                int ydif = Window.MouseGrabPoint.Y - Window.DragBar.Y;
                Point CalcPoint = new Point(MousePos.X - xdif, MousePos.Y - ydif);

                if (MousePos.X < Window.MouseGrabPoint.X)
                    CalcPoint.X += 1;
                if (MousePos.X + xdif > Window.MouseGrabPoint.X)
                    CalcPoint.X -= 1;

                if (MousePos.Y + ydif < Window.MouseGrabPoint.Y)
                    CalcPoint.Y += 1;
                if (MousePos.Y + ydif > Window.MouseGrabPoint.Y)
                    CalcPoint.Y -= 1;

                Window.MouseGrabPoint = new Point(CalcPoint.X + xdif, CalcPoint.Y + ydif);
                Window.Location = CalcPoint;

                TargetForm.Invalidate();
            }
            #endregion
        }
        public void Window_MouseUp(Controls.Window Window)
        {
            #region Reset Window Drag
            Window.Dragging = false;
            TargetForm.Invalidate();
            #endregion
        }

        public void Window_KeyDown(object sender, KeyEventArgs e, Controls.Window Window)
        {
            #region Write in textbox
            foreach (Controls.TextBox textbox in Window.Controls.OfType<Controls.TextBox>())
            {
                if (textbox.Editing)
                {
                    if (e.KeyCode == Keys.ShiftKey
                        || e.KeyCode == Keys.LShiftKey
                        || e.KeyCode == Keys.RShiftKey
                        || e.KeyCode == Keys.ControlKey
                        || e.KeyCode == Keys.LControlKey
                        || e.KeyCode == Keys.RControlKey
                        || e.KeyCode == Keys.Capital
                        || e.KeyCode == Keys.CapsLock
                        || e.KeyCode == Keys.Tab
                        || e.KeyCode == Keys.LWin
                        || e.KeyCode == Keys.RWin) { } // cancel some keys

                    else if (e.KeyCode == Keys.Back)
                    {
                        if (textbox.Text.Length > 0)
                            textbox.Text = textbox.Text.Substring(0, textbox.Text.Length - 1);
                    }
                    else if (e.KeyCode == Keys.Enter)
                    {
                        textbox.Editing = false;
                        textbox.CursorShown = false;
                    }
                    else
                    {
                        if (textbox.Text.Length < textbox.TextLimit)
                        {

                            if (e.KeyCode == Keys.D1
                                || e.KeyCode == Keys.D2
                                || e.KeyCode == Keys.D3
                                || e.KeyCode == Keys.D4
                                || e.KeyCode == Keys.D5
                                || e.KeyCode == Keys.D6
                                || e.KeyCode == Keys.D7
                                || e.KeyCode == Keys.D8
                                || e.KeyCode == Keys.D9
                                || e.KeyCode == Keys.D0)
                            {
                                textbox.Text += e.KeyCode.ToString().Substring(1, 1);
                            }
                            else if (!textbox.NumbersOnly)
                            {
                                if (e.KeyCode == Keys.Space)
                                {
                                    textbox.Text += " ";
                                }
                                else if (e.KeyCode.ToString().Length == 1) // stop random keys to get through
                                {
                                    if (e.Shift) // find a way to check if capslock is enabled
                                    {
                                        textbox.Text += e.KeyCode.ToString().ToUpper();
                                    }
                                    else
                                    {
                                        textbox.Text += e.KeyCode.ToString().ToLower();
                                    }
                                }
                            }
                        }
                    }
                    e.SuppressKeyPress = true; // Block the keypress to interact with anything else other than the textbox
                }
            }
            #endregion
        }

        public void Window_ResizeForm(object sender, EventArgs e)
        {
            #region Update particle size
            foreach (Controls.Particles particle in AddedParticles)
            {
                particle.Size = TargetForm.Size;
            }
            #endregion
        }
        #endregion

        #region Particle Timer
        public void ParticleTimer(int updatetime)
        {
            while (true)
            {
                TargetForm.Invalidate();
                Thread.Sleep(updatetime);
            }
        }
        #endregion

        #region Math Functions
        public static Point MoveTowards(Point A, int Direction, int Speed)
        {
            Point B = A;
            B.X = (int)(A.X + Math.Cos(Direction * Math.PI / 180.0) * Speed);
            B.Y = (int)(A.Y + Math.Sin(Direction * Math.PI / 180.0) * Speed);
            return B;
        }

        public string CalculateStringByWidth(string String, float Width, Font Font, Graphics Graphics, out SizeF StringSize)
        {
            string NewString = String;
            ReCalcString:
            StringSize = Graphics.MeasureString(NewString, Font);
            if (StringSize.Width > Width - Graphics.MeasureString("...", Font).Width)
            {
                if (NewString.Length != 0)
                {
                    NewString = NewString.Substring(0, NewString.Length - 1);
                    goto ReCalcString;
                }
            }
            if (NewString != String)
                NewString += "...";

            return NewString;
        }

        public static float GetBiggestFontSize(string String, FontFamily FontFamily, FontStyle FontStyle, Graphics Graphics, Rectangle Size)
        {
            float FontSize = 1000.0f;

            ReCalcString:
            FontSize -= 0.01f;
            Font font = new Font(FontFamily, FontSize, FontStyle);
            SizeF StringSize = Graphics.MeasureString(String, font);

            if (StringSize.Width > Size.Width)
                goto ReCalcString;
            if (StringSize.Height > Size.Height)
                goto ReCalcString;

            return FontSize;
        }
        #endregion

        #region Class Caster
        public T Cast<T>(Object @class)
        {
            Type classType = @class.GetType();
            Type classTarget = typeof(T);
            var x = Activator.CreateInstance(classTarget, false);
            var z = from source in classType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in classTarget.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = @class.GetType().GetProperty(memberInfo.Name).GetValue(@class, null);

                propertyInfo.SetValue(x, value, null);
            }
            return (T)x;
        }
        #endregion

        #region Controls
        public class Controls
        {
            public class Window
            {
                public string Title { get; set; }
                public Size Size { get; set; }
                public Point Location { get; set; }
                public bool Dragable { get; set; }
                public bool Window3D { get; set; }
                public bool Outline { get; set; }
                public List<object> Controls { get; set; }
                public Rectangle DragBar { get; set; }
                public bool Dragging { get; set; }
                public Point MouseGrabPoint { get; set; }
                public bool EventsAdded { get; set; }
                public bool Visible { get; set; }
                public Window(CSharpGUI currentGUI, string title, Size size, bool dragable, bool window3d, bool outline)
                {
                    Title = title;
                    Size = size;
                    Dragable = dragable;
                    Window3D = window3d;
                    Outline = outline;
                    Controls = new List<object>();

                    Random rnd = new Random();
                    Location = new Point(rnd.Next(0, 251), rnd.Next(0, 251));

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this);
                }
            }
            public class TextBox
            {
                public string Text { get; set; }
                public float TextSize { get; set; }
                public int TextLimit { get; set; }
                public bool NumbersOnly { get; set; }
                public Size Size { get; set; }
                public Rectangle ClickArea { get; set; }
                public bool LocationPredefined { get; set; }
                public Point Location { get; set; }
                public bool Editing { get; set; }
                public int CursorValue { get; set; }
                public bool CursorShown { get; set; }
                public bool Visible { get; set; }
                public TextBox(CSharpGUI currentGUI, Window window, string text, int textlimit, bool numbersonly, Size size) // Without location
                {
                    Text = text;
                    TextLimit = textlimit;
                    NumbersOnly = numbersonly;
                    Size = size;
                    LocationPredefined = true;
                    Location = new Point(-12345, -12345);
                    this.RecalculateTextSize(currentGUI);

                    Editing = false;
                    CursorValue = 0;
                    CursorShown = false;
                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
                public TextBox(CSharpGUI currentGUI, Window window, string text, int textlimit, bool numbersonly, Size size, Point location) // With location
                {
                    Text = text;
                    TextLimit = textlimit;
                    NumbersOnly = numbersonly;
                    Size = size;
                    LocationPredefined = false;
                    Location = location;
                    this.RecalculateTextSize(currentGUI);

                    Editing = false;
                    CursorValue = 0;
                    CursorShown = false;
                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
            }
            public class Label
            {
                public string Text { get; set; }
                public bool LocationPredefined { get; set; }
                public Point Location { get; set; }
                public bool Visible { get; set; }
                public Label(CSharpGUI currentGUI, Window window, string text) // Without location
                {
                    Text = text;
                    LocationPredefined = true;
                    Location = new Point(-12345, -12345);

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
                public Label(CSharpGUI currentGUI, Window window, string text, Point location) // With location
                {
                    Text = text;
                    LocationPredefined = false;
                    Location = location;

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
            }
            public class Button
            {
                public string Text { get; set; }
                public float TextSize { get; set; }
                public Size Size { get; set; }
                public Rectangle ClickArea { get; set; }
                public bool LocationPredefined { get; set; }
                public Point Location { get; set; }
                public Action Click { get; set; }
                public bool Visible { get; set; }
                public Button(CSharpGUI currentGUI, Window window, string text, Action click, Size size) // Without location
                {
                    Text = text;
                    Click = click;
                    Size = size;
                    LocationPredefined = false;
                    Location = new Point(-12345, -12345);
                    ClickArea = new Rectangle(new Point(Location.X - 1), new Size(Size.Width + 2, Size.Height + 2));
                    this.RecalculateTextSize(currentGUI);

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
                public Button(CSharpGUI currentGUI, Window window, string text, Action click, Size size, Point location) // With location
                {
                    Text = text;
                    Click = click;
                    Size = size;
                    LocationPredefined = true;
                    Location = location;
                    ClickArea = new Rectangle(new Point(Location.X - 1), new Size(Size.Width + 2, Size.Height + 2));
                    this.RecalculateTextSize(currentGUI);

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
            }
            public class Checkbox
            {
                public string Text { get; set; }
                public bool Checked { get; set; }
                public CheckedStyle Style { get; set; }
                public Color Color { get; set; }
                public bool LocationPredefined { get; set; }
                public Point Location { get; set; }
                public Rectangle ClickArea { get; set; }
                public bool Visible { get; set; }
                public Checkbox(CSharpGUI currentGUI, Window window, string text, bool defaultvalue, CheckedStyle style, Color color) // Without location
                {
                    Text = text;
                    Checked = defaultvalue;
                    Style = style;
                    Color = color;
                    LocationPredefined = false;
                    Location = new Point(-12345, -12345);

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
                public Checkbox(CSharpGUI currentGUI, Window window, string text, bool defaultvalue, CheckedStyle style, Color color, Point location) // With location
                {
                    Text = text;
                    Checked = defaultvalue;
                    Style = style;
                    Color = color;
                    LocationPredefined = true;
                    Location = location;

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this, window);
                }
            }
            public class Particles
            {
                public ParticleStyle Style { get; set; }
                public string[] Text { get; set; }
                public int Count { get; set; }
                public int Speed { get; set; }
                public int MovementDegrees { get; set; }
                public int Distance { get; set; }
                public int ParticleSize { get; set; }
                public Point Location { get; set; }
                public Size Size { get; set; }
                public List<Particle> ParticlesList { get; set; }
                public bool Visible { get; set; }

                public Particles(CSharpGUI currentGUI, ParticleStyle style, int count, int speed, int direction, int distance, int particlesize, string[] text = null)
                {
                    if (text == null)
                        text = new string[] { "Insert string[] at 8th parameter" };

                    Text = text;
                    Count = count;
                    Style = style;
                    Speed = speed;
                    MovementDegrees = direction;
                    if (MovementDegrees != 0)
                        MovementDegrees -= 90;
                    Distance = distance;
                    ParticleSize = particlesize;
                    Size = currentGUI.TargetForm.Size;
                    ParticlesList = GetParticles(Count, MovementDegrees, ParticleSize, Size, Text);

                    Visible = true;

                    new CSharpGUI().AddControl(currentGUI, this);
                }
                public class Particle
                {
                    public string Text { get; set; }
                    public int i { get; set; }
                    public int Direction { get; set; }
                    public Point Location { get; set; }
                    public Size Size { get; set; }
                    public int ParticleSize { get; set; }
                    public bool LineDrawn = false;
                }

                public List<Particle> GetParticles(int count, int movementdegrees, int particlesize, Size size, string[] text)
                {
                    Random rnd = new Random();
                    List<Particle> dots = new List<Particle>();
                    for (int i = 0; i < count; i++)
                    {
                        Particle particle = new Particle();
                        particle.Text = text[rnd.Next(0, text.Length)];
                        particle.i = 0;
                        particle.Location = new Point(rnd.Next(0, size.Width + 1), rnd.Next(0, size.Height + 1));
                        particle.Size = size;
                        particle.ParticleSize = particlesize;
                        if (movementdegrees == 0)
                            particle.Direction = rnd.Next(0, 361);
                        else
                            particle.Direction = movementdegrees;

                        dots.Add(particle);
                    }
                    return dots;
                }
            }

            public class IsVisible
            {
                public bool Visible { get; set; }
            }
        }
        #endregion

        #region Enums
        public enum CheckedStyle
        {
            Filled,
            X,
            Checkmark,
            Switch,
            SwitchInverted
        }

        public enum ParticleStyle
        {
            Polygons,
            Cubes,
            Text,
            Dicks
        }
        #endregion

        #region Force Close
        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion
    }

    #region Extension methods
    public static class Extension
    {
        public static void ApplyTheme(this CSharpGUI currentGUI, CSharpGUI.Theme theme)
        {
            currentGUI.CurrentTheme = theme;
        }

        #region RecalculateTextSize
        public static void RecalculateTextSize(this Object control, CSharpGUI currentGUI)
        {
            if (control.GetType() == typeof(CSharpGUI.Controls.Button))
            {
                CSharpGUI.Controls.Button z = (CSharpGUI.Controls.Button)Convert.ChangeType(control, typeof(CSharpGUI.Controls.Button)); // Convert to type

                z.ClickArea = new Rectangle(new Point(z.Location.X - 1), new Size(z.Size.Width + 2, z.Size.Height + 2));
                z.TextSize = CSharpGUI.GetBiggestFontSize(z.Text, currentGUI.CurrentTheme.Font.FontFamily, currentGUI.CurrentTheme.Font.Style, currentGUI.Overlay, z.ClickArea);
            }
            else if (control.GetType() == typeof(CSharpGUI.Controls.TextBox))
            {
                CSharpGUI.Controls.TextBox z = (CSharpGUI.Controls.TextBox)Convert.ChangeType(control, typeof(CSharpGUI.Controls.TextBox)); // Convert to type

                z.ClickArea = new Rectangle(new Point(z.Location.X - 1), new Size(z.Size.Width + 2, z.Size.Height + 2));
                z.TextSize = CSharpGUI.GetBiggestFontSize(z.Text, currentGUI.CurrentTheme.Font.FontFamily, currentGUI.CurrentTheme.Font.Style, currentGUI.Overlay, z.ClickArea);
            }
        }
        #endregion

        public static void RegenerateParticle(this CSharpGUI.Controls.Particles.Particle particle, int movementdegrees, int particlesize)
        {
            Random rnd = new Random();
            particle.i = 0;
            particle.ParticleSize = particlesize;
            if (movementdegrees == 0)
                particle.Direction = rnd.Next(0, 361);
            else
                particle.Direction = movementdegrees;
        }

        public static bool IsPointInside(this Rectangle Rect, Point MousePos)
        {
            if (MousePos.X >= Rect.X && MousePos.X <= (Rect.X + Rect.Width) && MousePos.Y >= Rect.Y && MousePos.Y <= (Rect.Y + Rect.Height))
                return true;
            else
                return false;
        }
    }
    #endregion
}
