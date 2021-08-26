using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpGUI
{
    public partial class TestWindow : Form
    {
        public CSharpGUI MyGUI = new CSharpGUI();
        public CSharpGUI.Theme MyTheme = new CSharpGUI.Theme();
        public new CSharpGUI.Controls.Window Menu;
        public CSharpGUI.Controls.Checkbox Aimbot;
        public CSharpGUI.Controls.Checkbox SilentAimbot;
        public CSharpGUI.Controls.Checkbox Spinbot;
        public CSharpGUI.Controls.Checkbox BoxESP;
        public CSharpGUI.Controls.Checkbox CornerESP;
        public CSharpGUI.Controls.Label FPSText;
        public CSharpGUI.Controls.TextBox TextBox;
        public CSharpGUI.Controls.Button Button;
        public CSharpGUI.Controls.Button Button2;
        public CSharpGUI.Controls.Particles Particles;
        public Random rnd = new Random();
        public TestWindow()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // IMPORTANT TO REMOVE FLICKER
            MyGUI.Setup(this);

            Menu = new CSharpGUI.Controls.Window(MyGUI, "CSharpGUI Example Window", new Size(345, 220), true, false, true);
            Aimbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Aimbot", true, CSharpGUI.CheckedStyle.Switch, Color.DarkGray, new Point(10, 10));
            SilentAimbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Silent Aimbot", true, CSharpGUI.CheckedStyle.SwitchInverted, Color.DarkGray, new Point(10, 30));
            Spinbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Spinbot", true, CSharpGUI.CheckedStyle.Checkmark, Color.DarkGray, new Point(10, 50));
            BoxESP = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Box ESP", true, CSharpGUI.CheckedStyle.X, Color.DarkGray, new Point(10, 70));
            CornerESP = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Corner ESP", true, CSharpGUI.CheckedStyle.Filled, Color.DarkGray, new Point(10, 90));
            FPSText = new CSharpGUI.Controls.Label(MyGUI, Menu, "FPS: " + MyGUI.FPS, new Point(10, 110));
            TextBox = new CSharpGUI.Controls.TextBox(MyGUI, Menu, "MyTextbox", 25, false, new Size(180, 15), new Point(10, 130));
            Button = new CSharpGUI.Controls.Button(MyGUI, Menu, "SAVE", new Action(ButtonAction), new Size(50, 15), new Point(10, 150));

            string[] textarray = { "Zebratic", "MintyFN", "CSharpGUI" };
            Particles = new CSharpGUI.Controls.Particles(MyGUI, CSharpGUI.ParticleStyle.Polygons, 100, 2, 0, 90, 1, textarray);
        }

        public void ButtonAction()
        {
            for (int i = 0; i < 500; i++)
            {
                MyGUI.Overlay.DrawString("Saved!", MyGUI.CurrentTheme.Font, new SolidBrush(Color.Black), new Point(10, 10));
            }
        }

        public void ChangeTheme()
        {
            MyTheme.ControlColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.ControlOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.Font = new Font(FontFamily.GenericSansSerif, 17, FontStyle.Regular);
            MyTheme.OutlineSize = rnd.Next(1, 10);
            MyTheme.StringColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.StringOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.WindowColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.WindowOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyGUI.ApplyTheme(MyTheme);
        }

        private void UpdateFPS()
        {
            while (true)
            {
                FPSText.Text = "FPS: " + MyGUI.FPS;
            }
        }

        private void TestWindow_Load(object sender, EventArgs e)
        {
            new Thread(UpdateFPS).Start();
        }
    }
}