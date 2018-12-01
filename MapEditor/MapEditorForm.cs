using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Bunifu.Framework.UI;

namespace MapEditor
{

    public partial class MapEditorForm : MetroFramework.Forms.MetroForm
    {

        private int count = 0;
        private int width = 0;
        private int height = 0;
        
        //private int[ , ] tiles;
        private int rows;
        private int cols;
        private float zoom = 1.0f;
        private bool isDrawCells = true;
        private bool isLoadTile = false;
        private bool success = false;
        private StringBuilder matrixCells;
        private List<Bitmap> tilesImage;
        private Bitmap image;
        private Pen pen;

        private bool isDraw = false;
        private Dictionary<int, ColorRegion> regions;
        private Point start;
        private Point end;

        public int Round { get; private set; }

        public MapEditorForm()
        {
            InitializeComponent();
            pictureBoxMain.Size = System.Drawing.Size.Empty;
            pictureBoxSub.Size = System.Drawing.Size.Empty;

            panel1.HorizontalScroll.LargeChange = 5;
            panel1.VerticalScroll.LargeChange = 5;
            labelSize.Text = string.Empty;
            label7.Text = "0";
            tilesImage = new List<Bitmap>();
            matrixCells = new StringBuilder();
            regions = new Dictionary<int, ColorRegion>();
            pen = new Pen(Color.SeaGreen);
            cbbType.SelectedIndex = 0;
            cbbName.SelectedIndex = 0;
            cbbExport.SelectedIndex = 0;
        }

        private void bunifuThinButton22_Click_1(object sender, EventArgs e)
        {
            txb_Leave(txbHeight, null);
            txb_Leave(txbWidth, null);
            regions.Clear();
            if (openImage.ShowDialog() == DialogResult.OK)
            {
                labelNameImage.Text = Path.GetFileName(openImage.FileName);
                image = (Bitmap)Image.FromFile(openImage.FileName);//layer draw background
                pictureBoxMain.BackgroundImage = (Bitmap)image.Clone();
                pictureBoxMain.Width = pictureBoxMain.BackgroundImage.Width;
                pictureBoxMain.Height = pictureBoxMain.BackgroundImage.Height;
                labelSize.Text = pictureBoxMain.Width + " X " + pictureBoxMain.Height;

                GetCellSize();

                pictureBoxMain.Image = new Bitmap(pictureBoxMain.Width, pictureBoxMain.Height);//layer draw cells
                
                DrawCells();
            };
        }

        private void GetCellSize()
        {
            width = int.Parse(txbWidth.Text);
            height = int.Parse(txbHeight.Text);

            cols = (int)Math.Round(pictureBoxMain.BackgroundImage.Width / (float)width + 0.99f, 1, MidpointRounding.ToEven);
            rows = (int)Math.Round(pictureBoxMain.BackgroundImage.Height / (float)height + 0.99f, 1, MidpointRounding.ToEven);

            txbTile.Text = (rows * cols).ToString();
        }

        private void DrawCells()
        {
            if (!isDrawCells || pictureBoxMain.Image is null) return;

            using (Graphics graphic = Graphics.FromImage(pictureBoxMain.Image))
            {
                graphic.Clear(Color.Transparent);

                DrawCols(graphic, pen, cols);
                DrawRows(graphic, pen, rows);
            }

            pictureBoxMain.Invalidate();//refesh
        }
        private void DrawCellsSub(int cols)
        {
            if (!isDrawCells || pictureBoxSub.Image is null) return;
            using (Graphics graphic = Graphics.FromImage(pictureBoxSub.Image))
            {
                graphic.Clear(Color.Transparent);
                DrawCols(graphic, pen, cols);
            }
            pictureBoxSub.Invalidate();
        }
        private void DrawCols(Graphics graphic, Pen pen, int cols)
        {
            for (int i = 0; i < cols; ++i)
            {
                graphic.DrawLine(pen, new Point(i * width, 0), new Point(i * width, pictureBoxMain.Height));
            }
        }
        private void DrawRows(Graphics graphic, Pen pen, int rows)
        {
            for (int i = 0; i < rows; ++i)
            {
                graphic.DrawLine(pen, new Point(0, i * height), new Point(pictureBoxMain.Width, i * height));
            }
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

        private void MapEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.P)
            {
                zoom += 0.1f;
                pictureBoxMain.Width = (int)(zoom * pictureBoxMain.Width);
                pictureBoxMain.Height = (int)(zoom * pictureBoxMain.Height);
            }
            if(e.KeyCode == Keys.O){
                zoom -= 0.1f;
                pictureBoxMain.Width = (int)(zoom * pictureBoxMain.Width);
                pictureBoxMain.Height = (int)(zoom * pictureBoxMain.Height);
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            isDrawCells = !isDrawCells;
            DrawCells();
            DrawCellsSub(tilesImage.Count);
            if (!isDrawCells)
            {
                ClearCells(pictureBoxMain);
                ClearCells(pictureBoxSub);
            }
        }

        private void ClearCells(PictureBox pictureBox)
        {
            if (pictureBox.Image is null) return;
            using (Graphics graphic = Graphics.FromImage(pictureBox.Image))
            {
                graphic.Clear(Color.Transparent);
            }
            pictureBox.Invalidate();//redraw


        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        private bool Compare(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            BitmapData data1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData data2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                IntPtr bd1scan0 = data1.Scan0;
                IntPtr bd2scan0 = data2.Scan0;

                int stride = data1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(data1);
                b2.UnlockBits(data2);
            }
        }

        private void txb_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                txb_Leave(sender, null);
                GetCellSize();
                DrawCells();
            }
        }

        private void bunifuThinButton21_Click(object sender, EventArgs e)
        {
            if (image is null) return;
            txb_Leave(txbWidth, null);
            txb_Leave(txbHeight, null);
            GetCellSize();
            DrawCells();
        }

        private void bunifuThinButton23_Click(object sender, EventArgs e)
        {
            if (isLoadTile || image is null) return;//lock
            pictureBox1.Show();

            tilesImage.Clear();
            matrixCells.Clear();

            GC.Collect();
            
            isLoadTile = true;
            success = false;
            pictureBoxSub.BackgroundImage = null;
            new Thread(LoadTileSet)
            {
                IsBackground = true
            }.Start();
        }

        private void LoadTileSet()
        {
            try
            {
                int width = this.width;
                int height = this.height;
                //tiles = new int[width, height];
                if (image == null)
                {
                    return;
                }
                bool isExists = false;
                int id = 0;

                for (int i = 0; i < rows; ++i)
                {
                    for (int j = 0; j < cols; ++j)
                    {
                        Bitmap b = new Bitmap(width, height);
                        using (Graphics graphic = Graphics.FromImage(b))
                        {
                            graphic.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(j * width, i * height, width, height), GraphicsUnit.Pixel);
                        }
                        isExists = false;
                        for (int index = 0; index < tilesImage.Count; ++index)
                        {
                            if (Compare(b, tilesImage[index]))
                            {
                                isExists = true;
                                id = index;
                                break;
                            }
                        }
                        if (!isExists)
                        {
                            id = tilesImage.Count;
                            tilesImage.Add(b);
                            if (label7.InvokeRequired)
                            {
                                label7.Invoke(new Action(() => { label7.Text = tilesImage.Count.ToString(); }));
                            }
                            else label7.Text = tilesImage.Count.ToString();
                        }
                        matrixCells.Append(id);
                        matrixCells.Append(" ");
                    }
                    matrixCells.AppendLine();
                }
                
                if (pictureBoxSub.InvokeRequired)
                {
                    pictureBoxSub.Invoke(new Action(() =>
                    {
                        SetSizeSub(width, height);
                    }));
                }
                else SetSizeSub(width, height);
                if (pictureBoxSub.BackgroundImage == null)
                {
                    return;
                }
                success = true;
                using (Graphics graphic = Graphics.FromImage(pictureBoxSub.BackgroundImage))
                {
                    for (int i = 0; i < tilesImage.Count; ++i)
                    {
                        graphic.DrawImage(tilesImage[i], new Rectangle(i * width, 0, width, height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                    }
                    
                }
                if(isDrawCells)
                    using (Graphics graphic = Graphics.FromImage(pictureBoxSub.Image))
                        DrawCols(graphic, pen, tilesImage.Count);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StopLoad();
                isLoadTile = false;
            }
        }
        private void SetSizeSub(int width, int height)
        {
            if (tilesImage.Count == 0)
            {
                return;
            }
            pictureBoxSub.Width = width * tilesImage.Count;
            pictureBoxSub.Height = height;
            pictureBoxSub.BackgroundImage = new Bitmap(pictureBoxSub.Width, pictureBoxSub.Height);
            pictureBoxSub.Image = new Bitmap(pictureBoxSub.Width, pictureBoxSub.Height);
        }
        private void StopLoad()
        {
            if (pictureBox1.InvokeRequired)
                pictureBox1.Invoke(new Action(() =>
                {
                    pictureBox1.Hide();
                }));
            else
                pictureBox1.Hide();
        }
      
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (isLoadTile) return;

            if (pictureBoxSub.BackgroundImage is null || !success)
            {
                MessageBox.Show("Please, You must load tileset before you export its", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            saveImage.FileName = Path.GetFileName(openImage.FileName);
            if (saveImage.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Show();
                isLoadTile = true;
                GetImageSave(out string pathImage, out string pathTxt, out ImageFormat format);
                new Thread(SaveTileSet)
                {
                    IsBackground = true
                }.Start(new object[] { pathImage, pathTxt, format });
            }
        }

        private void SaveTileSet(object parameter)
        {
            try
            {
                object[] info = (object[])parameter;
                string pathImage = (string)info[0];
                string pathTxt = (string)info[1];
                ImageFormat format = (ImageFormat)info[2];
                pictureBoxSub.BackgroundImage.Save(pathImage, format);
                string text = tilesImage.Count + " " + rows + " " + cols + Environment.NewLine + matrixCells.ToString();
                SaveFileTXT(pathTxt, text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoadTile = false;
                StopLoad();
            }
            
        }

        private void SaveFileTXT(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        private void GetImageSave(out string pathImage, out string pathTxt, out ImageFormat format)
        {
            string path = Path.GetDirectoryName(saveImage.FileName);
            string fileImage = Path.GetFileName(saveImage.FileName).Split('.')[0];
            pathTxt = Path.Combine(path, fileImage + ".txt");
            switch (saveImage.FilterIndex)
            {
                case 1:
                    fileImage += ".png";
                    format = ImageFormat.Png;
                    break;
                case 2:
                    fileImage += ".bmp";
                    format = ImageFormat.Bmp;
                    break;
                case 3:
                    fileImage += ".jpg";
                    format = ImageFormat.Jpeg;
                    break;
                default:
                    fileImage += ".png";
                    format = ImageFormat.Png;
                    break;
            }
            pathImage = Path.Combine(path, fileImage);
        }

        private void PictureBoxMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (cbbName.Text == "") return;
            if (e.Button == MouseButtons.Left)
            {
                if (e.X <= pictureBoxMain.Width && e.Y <= pictureBoxMain.Height)
                {
                    isDraw = true;
                    start = end = e.Location;
                }
            }
        }

        private void pictureBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraw)
            {
                end = e.Location;

                pictureBoxMain.Invalidate();
            }
        }

        private void pictureBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraw)
            {
                isDraw = false;
                Rectangle rect = GetRectangle();
                if (rect.Width > 0 && rect.Height > 0) regions.Add(count++, new ColorRegion( rect, pictureBox2.BackColor));
                pictureBoxMain.Invalidate();
                AddGridView(rect, cbbName.Text, cbbType.Text);
            }
        }

        private void AddGridView(Rectangle rect, string name, string type)
        {
            dataGridView.Rows.Add(name, type, rect.ToString());
            dataGridView.Rows[dataGridView.RowCount - 1].Tag = count - 1;
        }

        private Rectangle GetRectangle()
        {
            return new Rectangle(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(start.X - end.X),
                Math.Abs(start.Y - end.Y));
        }

        private void pictureBoxMain_Paint(object sender, PaintEventArgs e)
        {
            if (regions.Count > 0)
            {
                if (checkColor.Checked)
                    foreach (var item in regions)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(item.Value.color), item.Value.region);
                    }
                else
                    foreach (var item in regions)
                    {
                        e.Graphics.DrawRectangle(new Pen(item.Value.color), item.Value.region);
                    }

            }
            if (isDraw) e.Graphics.DrawRectangle(new Pen(pictureBox2.BackColor), GetRectangle());
        }

        private void checkColor_CheckedChanged(object sender, EventArgs e)
        {
            pictureBoxMain.Invalidate();
        }

        private void bunifuThinButton21_Click_1(object sender, EventArgs e)
        {
            var form = new NameForm(cbbName.Items.Cast<String>().ToArray());
            form.ShowDialog();
            cbbName.Items.Clear();
            cbbName.Items.AddRange(form.Names);
            if (cbbName.Items.Count > 0)
                cbbName.SelectedIndex = 0;
        }

        private void bunifuThinButton23_Click_1(object sender, EventArgs e)
        {
            if(colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.BackColor = colorDialog1.Color;
            }
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            //e.Row;
            regions.Remove((int)e.Row.Tag);
            pictureBoxMain.Invalidate();
        }

        private void bunifuThinButton25_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            regions.Clear();
            pictureBoxMain.Invalidate();
        }

        private void bunifuThinButton24_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    dataGridView.Rows.Remove(row);
                    regions.Remove((int)row.Tag);
                }
                pictureBoxMain.Invalidate();
            }
        }

        private void bunifuThinButton23_Click_2(object sender, EventArgs e)
        {
            if(saveImage.ShowDialog() == DialogResult.OK)
            {
                GetImageSave(out string p1, out string p2, out ImageFormat p3);
                QuadNode root = new QuadNode(0, 0, new Rectangle(0, 0, image.Width, image.Height));
                StreamWriter writer = new StreamWriter(p2);

                foreach (var item in regions)
                {
                    root.Build(item.Key, regions);
                }
                root.Save(writer);
                writer.Close();
            }
        }
    }
    public class ColorRegion
    {
        public Rectangle region;
        public Color color;
        public ColorRegion(Rectangle r, Color c)
        {
            region = r;
            color = c;
        }
    }
}
