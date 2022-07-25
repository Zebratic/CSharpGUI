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
using GUI;

namespace CSharpGUI_Test
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
        public CSharpGUI.Controls.Slider Fov;
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

            Menu = new CSharpGUI.Controls.Window(MyGUI, "CSharpGUI Example Window", new Size(0, 0), true, false, true);
            Aimbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Aimbot", true, CSharpGUI.CheckedStyle.Switch, Color.DarkGray);
            SilentAimbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Silent Aimbot", true, CSharpGUI.CheckedStyle.SwitchInverted, Color.DarkGray);
            Spinbot = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Spinbot", true, CSharpGUI.CheckedStyle.Checkmark, Color.DarkGray);
            BoxESP = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Box ESP", true, CSharpGUI.CheckedStyle.X, Color.DarkGray);
            CornerESP = new CSharpGUI.Controls.Checkbox(MyGUI, Menu, "Enable Corner ESP", true, CSharpGUI.CheckedStyle.Filled, Color.DarkGray);
            Fov = new CSharpGUI.Controls.Slider(MyGUI, Menu, "Aim FOV [%°]", 90, 0, 360, false, new Size(200, 20));
            FPSText = new CSharpGUI.Controls.Label(MyGUI, Menu, "FPS: " + MyGUI.FPS);
            TextBox = new CSharpGUI.Controls.TextBox(MyGUI, Menu, "MyTextbox", 25, false, new Size(180, 15));
            Button = new CSharpGUI.Controls.Button(MyGUI, Menu, "Randomize Theme", new Action(ChangeTheme), new Size(150, 30));

            string[] textarray = { "Zebratic", "MintyFN", "CSharpGUI" };
            Particles = new CSharpGUI.Controls.Particles(MyGUI, CSharpGUI.ParticleStyle.Polygons, 100, 2, 0, 90, 1, textarray);
        }
        
        public void ChangeTheme()
        {
            Menu.Window3D = (rnd.Next(0, 2) == 0);
            MyTheme.ControlColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.ControlOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.Font = new Font(FontFamily.GenericSansSerif, 17, FontStyle.Regular);
            MyTheme.OutlineSize = rnd.Next(1, 10);
            MyTheme.StringColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.StringOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.WindowColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.WindowOutlineColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            MyTheme.ControlMargin = new Point(rnd.Next(3, 20), rnd.Next(3, 20));
            MyTheme.ControlPadding = new Point(rnd.Next(3, 20), rnd.Next(3, 20));
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