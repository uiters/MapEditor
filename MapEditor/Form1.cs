using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bunifu.Framework.UI;

namespace MapEditor
{
    public partial class MapEditorForm : MetroFramework.Forms.MetroForm
    {
        private int width = 0;
        private int height = 0;
        private int[,] tiles;
        private int rows;
        private int cols;

        public int Round { get; private set; }

        public MapEditorForm()
        {
            InitializeComponent();
            openImage.Filter = "Image File (*.bmp, *.png, *.jpg, *.jpeg) | *.bmp; *.png; *.jpg; *.jpeg";
            panel1.HorizontalScroll.LargeChange = 5;
            panel1.VerticalScroll.LargeChange = 5;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void bunifuThinButton22_Click(object sender, EventArgs e)
        {

        }

        private void bunifuThinButton22_Click_1(object sender, EventArgs e)
        {
            txb_Leave(txbHeight, null);
            txb_Leave(txbWidth, null);
            if (openImage.ShowDialog() == DialogResult.OK)
            {
                labelNameImage.Text = Path.GetFileName(openImage.FileName);
                pictureBoxMain.BackgroundImage = Image.FromFile(openImage.FileName);
                pictureBoxMain.Width = pictureBoxMain.BackgroundImage.Width;
                pictureBoxMain.Height = pictureBoxMain.BackgroundImage.Height;
                labelSize.Text = pictureBoxMain.Width + " X " + pictureBoxMain.Height;
                width = int.Parse(txbWidth.Text);
                height = int.Parse(txbHeight.Text);

                rows = (int)Math.Round( pictureBoxMain.BackgroundImage.Width / (float)width, 1, MidpointRounding.ToEven);
                cols = (int)Math.Round(pictureBoxMain.BackgroundImage.Height / (float)height, 1, MidpointRounding.ToEven);

                tiles = new int[width, height];
                
            };
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBoxSub_Click(object sender, EventArgs e)
        {

        }


        private void txb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txb_Leave(object sender, EventArgs e)
        {
            var txtBox = ((TextBox)sender);
            if (txtBox.Text == "0")
                txtBox.Text = "1";
        }
    }
}
