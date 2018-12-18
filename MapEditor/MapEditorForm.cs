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
    enum Type{
        All = -1,
        Static = 0,
        Dynamic = 1
    }



    public partial class MapEditorForm : MetroFramework.Forms.MetroForm
    {

        private int id = 0;//id Objects
        private int idName = 0;
        private int width = 0;
        private int height = 0;

        private int widthMap = 0;
        private int heightMap = 0;

        private int rows;
        private int cols;
        private int rowsTiles = 4;//row titles
        private int colsTiles = 0;//titles
        private float zoom = 1.0f;
        private int depth = 10;
        private int minWidth = 128;
        private int minHeight = 128;
        private bool isDrawCells = true;
        private bool isLoadTile = false;
        private bool success = false;
        private int quality = 80;
        private StringBuilder matrixCells;
        private List<Bitmap> tilesImage;
        private Bitmap image;
        private Bitmap imageSub;

        private Pen pen;

        private bool isDraw = false;

        private Dictionary<int, CObject> objects;
        private Dictionary<int, Color> colors;
        private Dictionary<int, string> names;

        private Point start;
        private Point end;
        private List<CObject> selectObjects;
        private readonly Random random = new Random();
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
            objects = new Dictionary<int, CObject>();

            selectObjects = new List<CObject>();
            colors = new Dictionary<int, Color>();
            names = new Dictionary<int, string>();

            pen = new Pen(Color.SeaGreen);
            cbbType.SelectedIndex = 0;
            cbbName.SelectedIndex = 0;
            cbbExport.SelectedIndex = 0;
            cbbDirection.SelectedIndex = 0;
        }

        private void bunifuThinButton22_Click_1(object sender, EventArgs e)
        {
            txb_Leave(txbHeight, null);
            txb_Leave(txbWidth, null);
            objects.Clear();
            txbObjects.Text = "0";
            if (openImage.ShowDialog() == DialogResult.OK)
            {
                labelNameImage.Text = Path.GetFileName(openImage.FileName);
                image = (Bitmap)Image.FromFile(openImage.FileName);//layer draw background

                widthMap = image.Width;
                heightMap = image.Height;

                GetCellSize();

                pictureBoxMain.BackgroundImage = (Bitmap)image.Clone();
                pictureBoxMain.Width = pictureBoxMain.BackgroundImage.Width;
                pictureBoxMain.Height = pictureBoxMain.BackgroundImage.Height;
                labelSize.Text = pictureBoxMain.Width + " X " + pictureBoxMain.Height;


                pictureBoxMain.Image = new Bitmap(widthMap, heightMap);//layer draw cells

                DrawCells();
            };
        }

        private void GetCellSize()
        {
            CheckValueTrue(txbWidth, 128, out width);
            CheckValueTrue(txbHeight, 128, out height);

            cols = (int)(widthMap / (float)width + 0.99f);
            rows = (int)(heightMap / (float)height + 0.99f);

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

        private void DrawCellsSub()
        {
            if (!isDrawCells || pictureBoxSub.Image is null) return;
            using (Graphics graphic = Graphics.FromImage(pictureBoxSub.Image))
            {
                graphic.Clear(Color.Transparent);
                DrawCols(graphic, pen, colsTiles);
                DrawRows(graphic, pen, rowsTiles);
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
            if (e.KeyCode == Keys.P)
            {
                zoom += 0.1f;
                pictureBoxMain.Width = (int)(zoom * widthMap);
                pictureBoxMain.Height = (int)(zoom * heightMap);

            }
            if (e.KeyCode == Keys.O)
            {
                zoom -= 0.1f;
                pictureBoxMain.Width = (int)(zoom * widthMap);
                pictureBoxMain.Height = (int)(zoom * heightMap);

            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            isDrawCells = !isDrawCells;
            DrawCells();
            DrawCellsSub();
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
            if (e.KeyCode == Keys.Enter)
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
            CheckValueTrue(txtSplitRows, 4, out rowsTiles);
            tilesImage.Clear(); //clear tiles
            matrixCells.Clear(); //clear matrixcells
            ClearCells(pictureBoxSub); //clear dash sub
            pictureBoxSub.BackgroundImage = null;
            pictureBoxSub.Image = null;
            GC.Collect();

            isLoadTile = true;
            success = false;

            new Thread(LoadTileSet)
            {
                IsBackground = true
            }.Start();
        }

        private void LoadTileSet()
        {
            try
            {
                if (image == null)
                {
                    return;
                }

                float width = this.width;
                float height = this.height;


                bool isExists = false;
                int id = 0;

                for (int row = 0; row < rows; ++row)
                {
                    for (int col = 0; col < cols; ++col)
                    {
                        Bitmap b = new Bitmap((int)width, (int)height);
                        using (Graphics graphic = Graphics.FromImage(b))
                        {
                            graphic.DrawImage(image, new RectangleF(0, 0, width, height), new RectangleF(col * width, row * height, width, height), GraphicsUnit.Pixel);
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
                        SetSizeSub((int)width, (int)height)
                    ));
                }
                else SetSizeSub((int)width, (int)height);
                if (pictureBoxSub.BackgroundImage == null)
                {
                    return;
                }
                success = true;

                SplitImageTiles();
                if (labelTiles.InvokeRequired)
                    labelTiles.Invoke(new Action(() => labelTiles.Text = rowsTiles + " X " + colsTiles));
                else labelTiles.Text = rowsTiles + " X " + colsTiles;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                StopLoad();
                isLoadTile = false;
            }
        }

        private void SplitImageTiles()
        {
            //like 300 / 4 = 0 
            // 299 / 4 = 1
            int colSurplus = (tilesImage.Count % 4) == rowsTiles ? 0 : 1;

            //round cols 300 / 4 = 75cells in row 
            // 299 / 4 = 75cells (row last has 74 cells)
            colsTiles = tilesImage.Count / rowsTiles + colSurplus; 

            int indexTiles = 0;
            using (Graphics graphic = Graphics.FromImage(imageSub))
            {
                for (int row = 0; row < rowsTiles; ++row)
                {
                    for (int col = 0; col < colsTiles; ++col)
                    {
                        if (indexTiles >= tilesImage.Count) break;
                        graphic.DrawImage(tilesImage[indexTiles], new RectangleF(col * width, row * height, width, height), new RectangleF(0, 0, width, height), GraphicsUnit.Pixel);
                        indexTiles += 1;
                    }
                }
            }
            pictureBoxSub.BackgroundImage = imageSub;
            if (isDrawCells)
                using (Graphics graphic = Graphics.FromImage(pictureBoxSub.Image))
                {
                    DrawCols(graphic, pen, colsTiles);
                    DrawRows(graphic, pen, rowsTiles);
                }
        }

        private void SetSizeSub(int width, int height)
        {
            if (tilesImage.Count == 0)
            {
                return;
            }
            pictureBoxSub.Width = (width * tilesImage.Count) / rowsTiles;
            pictureBoxSub.Height = height * rowsTiles;
            pictureBoxSub.BackgroundImage = new Bitmap(pictureBoxSub.Width, pictureBoxSub.Height);
            pictureBoxSub.Image = new Bitmap(pictureBoxSub.Width, pictureBoxSub.Height);
            imageSub = new Bitmap(pictureBoxSub.Width, pictureBoxSub.Height);
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
                MessageBox.Show("You must load tileset before you export its", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            saveImage.FileName = Path.GetFileName(openImage.FileName);
            if (saveImage.ShowDialog() == DialogResult.OK)
            {
                CheckValueTrue(txbQuality, 1, 100, 80, out quality);
                pictureBox1.Show();
                isLoadTile = true;
                GetImageSave(out string pathImage, out string pathTxt, out ImageFormat format);
                new Thread(SaveTileSet)
                {
                    IsBackground = true
                }.Start(new object[] { pathImage, pathTxt, format });
            }


        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        private void SaveTileSet(object parameter)
        {
            try
            {
                object[] info = (object[])parameter;
                string pathImage = (string)info[0];
                string pathTxt = (string)info[1];
                ImageFormat format = (ImageFormat)info[2];
                ImageCodecInfo encoder = GetEncoder(format);
                if (encoder is null) return;
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(myEncoder, quality);

                imageSub.Save(pathImage, encoder, encoderParameters);
                string text = tilesImage.Count + " " + rows + " " + cols + " " + width + " " + height + " ";
                text += rowsTiles + " " + colsTiles;
                text += Environment.NewLine + matrixCells.ToString();

                SaveFileTXT(pathTxt, text);
            }
            catch (Exception ex)
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
                if (rect.Width > 0 && rect.Height > 0)
                {
                    objects.Add(id, new CObject(id++, rect, pictureBox2.BackColor, cbbName.SelectedIndex, cbbType.SelectedIndex, cbbDirection.SelectedIndex == 0));
                    AddGridView(rect, cbbName.Text, cbbType.Text, cbbDirection.Text);
                    
                    pictureBoxMain.Invalidate();
                    txbObjects.Text = objects.Count.ToString();
                }
            }
        }

        private void AddGridView(Rectangle rect, string name, string type, string direction)
        {
            dataGridView.Rows.Add(name, type, direction, rect.ToString());
            dataGridView.Rows[dataGridView.RowCount - 1].Tag = id - 1;
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
            if (objects.Count > 0)
            {
                if (checkColor.Checked)
                    foreach (var item in objects)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(item.Value.color), item.Value.region);
                    }
                else
                    foreach (var item in objects)
                    {
                        e.Graphics.DrawRectangle(new Pen(item.Value.color) { Width = 3}, item.Value.region);
                    }

                for (int i = 0; i < selectObjects.Count; ++i)
                {
                    e.Graphics.DrawRectangle(new Pen(selectObjects[i].color) { Width = 6 }, selectObjects[i].region);
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
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.BackColor = colorDialog1.Color;
            }
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            //e.Row;
            objects.Remove((int)e.Row.Tag);
            pictureBoxMain.Invalidate();
            txbObjects.Text = objects.Count.ToString();
        }

        private void bunifuThinButton25_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            objects.Clear();
            pictureBoxMain.Invalidate();
            txbObjects.Text = "0";
        }

        private void bunifuThinButton24_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    dataGridView.Rows.Remove(row);
                    objects.Remove((int)row.Tag);
                    txbObjects.Text = objects.Count.ToString();
                }
                pictureBoxMain.Invalidate();
            }
        }

        private void bunifuThinButton23_Click_2(object sender, EventArgs e)
        {
            if (image is null)
            {
                MessageBox.Show("You must load map before you export quadtree", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            saveQuadtree.FileName = Path.GetFileName(openImage.FileName);
            if (saveQuadtree.ShowDialog() == DialogResult.OK)
            {
                string path = Path.GetDirectoryName(saveQuadtree.FileName);
                string name = Path.GetFileName(saveQuadtree.FileName).Split('.')[0];

                CheckValueTrue(txbDepth, 10,out depth);
                CheckValueTrue(txbMinHeight, 128,out minHeight);
                CheckValueTrue(txbMinWidth, 128, out minWidth);

                switch (cbbExport.SelectedIndex)
                {
                    case 0:
                        SaveAll(path, name);
                        break;
                    case 1:
                        SaveStatic(path, name, true);
                        break;
                    case 2:
                        SaveDynamic(path, name, true);
                        break;
                    default:
                        SaveAll(path, name);
                        break;
                }
                SaveInfo(path, name);
            }
        }

        private void SaveInfo(string path, string name)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < cbbName.Items.Count; i++)
            {
                stringBuilder.Append(i + " " + cbbName.Items[i].ToString());
                stringBuilder.AppendLine();
            }
            string file = Path.Combine(path, name + "_info.txt");
            File.WriteAllText(file, stringBuilder.ToString());
        }

        private void CheckValueTrue(TextBox textBox,int defaultValue, out int value)
        {
            if (int.TryParse(textBox.Text, out value))
            {
                if(value > 0)
                    return;
            }

            textBox.Text = defaultValue.ToString();
            value = defaultValue;
        }

        private void SaveDynamic(string path, string name, bool saveObject)
        {
         
            QuadNode root = AutoBuild((int)Type.Dynamic, out List<CObject> objs);
            string file = Path.Combine(path, name + "_dynamic.txt");
            string fileObject = Path.Combine(path, name + ".txt");
            if (saveObject) SaveObject(objs, fileObject);

            if(checkBoxInclude.Checked)
                SaveQuadTree(root, file);
        }

        private void SaveStatic(string path, string name, bool saveObject)
        {
            QuadNode root = AutoBuild((int)Type.Static, out List<CObject> objs);
            string file = Path.Combine(path, name + "_static.txt");
            string fileObject = Path.Combine(path, name + ".txt");
            if(saveObject) SaveObject(objs, fileObject);
            if(checkBoxInclude.Checked)
                SaveQuadTree(root, file);
        }

        private void SaveAll(string path, string name)
        {
            
            string fileObject = Path.Combine(path, name + ".txt");
            if (!checkBoxOnly.Checked)
            {
                SaveObject(objects.Values.ToList(), fileObject);
                SaveDynamic(path, name, false);
                SaveStatic(path, name, false);
                return;
            }

            QuadNode root = AutoBuild((int)Type.All, out List<CObject> objs);
            string file = Path.Combine(path, name + "_all.txt");
            SaveObject(objs, fileObject);
            if(checkBoxInclude.Checked)
                SaveQuadTree(root, file);
        }

        private QuadNode AutoBuild(int type, out List<CObject> objs)
        {
            QuadNode.depth = depth;
            QuadNode.minSize.Width = minWidth;
            QuadNode.minSize.Height = minHeight;
            QuadNode root = new QuadNode(0, 0, new Rectangle(0, 0, image.Width, image.Height));
            
            objs = new List<CObject>();
            if (type != (int)Type.All)
            {
                foreach (var item in objects)
                    if (item.Value.type == type)
                    {
                        root.Build(item.Value);
                        objs.Add(item.Value);
                    }
            }else
                foreach (var item in objects)
                {
                    root.Build(item.Value);
                    objs.Add(item.Value);
                }
            return root;
        }

        private void SaveQuadTree(QuadNode root,string path)
        {
            StreamWriter stream = new StreamWriter(path);
            root.Save(stream);
            stream.Close();
        }
        private void SaveObject(List<CObject> objs, string path)
        {
            StreamWriter stream = new StreamWriter(path);
            foreach (var item in objs)
            {
                stream.Write(item.ID + " " + item.idName + " " + item.type + " " + item.region.X + " " + item.region.Y + " " + item.region.Width + " " + item.region.Height + " " + (item.direction ? "1" : "0"));
                stream.WriteLine();
                stream.Flush();
            }
            stream.Close();
        }


        private void cbbExport_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbExport.SelectedIndex > 0)
            {
                checkBoxOnly.Enabled = false;
            }
            else checkBoxOnly.Enabled = true;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView.SelectedRows.Count > 0)
            {
                selectObjects.Clear();
                if (objects.Count <= 1) return;
                //if (objects.Count == 1) selectObjects.Add(objects.ElementAt(0).Value);
                foreach (DataGridViewRow item in dataGridView.SelectedRows){
                    if(item.Tag != null) selectObjects.Add(objects[(int)item.Tag]);
                }
                pictureBoxMain.Invalidate();
            }
        }

        private void bunifuThinButton26_Click(object sender, EventArgs e)
        {
            if(image is null)
            {
                MessageBox.Show("You must load map before you import objects", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                if(openFileTxt.ShowDialog() == DialogResult.OK)
                {
                    if (MessageBox.Show("All Objects will be cleaned. Are you sure?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                        return;
                    colors.Clear();
                    objects.Clear();
                    names.Clear();
                    id = 0;
                    ReadObjects(openFileTxt.FileName);
                }
            }
        }
        void ReadObjects(string path)
        {
            try
            {
                cbbName.Items.Clear();
                string[] lines = File.ReadAllLines(path);
                for(int i = 0; i<lines.Length; ++i)
                {
                    string line = lines[i];
                    string[] obj = line.Split(' ');
                    CObject cObject = CreateObjects(obj);
                    objects[cObject.ID] = cObject;
                }
               if(cbbName.Items.Count > 0)
                {
                    cbbName.SelectedIndex = 0;
                    txbObjects.Text = objects.Count.ToString();
                }
            }
            catch
            {

            }
            finally
            {
                pictureBoxMain.Invalidate();
            }
        }
        CObject CreateObjects(string[] obj)
        {
            int id, idName, idType, x, y, width, height;
            id = int.Parse(obj[0]);
            if (id >= this.id)
                this.id = id + 1;
            idName = int.Parse(obj[1]);
            idType = int.Parse(obj[2]);
            x = int.Parse(obj[3]);
            y = int.Parse(obj[4]);
            width = int.Parse(obj[5]);
            height = int.Parse(obj[6]);
            string direction = obj[7] == "1" ? "Left" : "Right";
            if (!names.ContainsKey(idName))
            {
                
                names[idName] = "Name " + idName;
                cbbName.Items.Add(names[idName]);
                colors[idName] = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                
            }
            string type = idType == 0 ? "Static" : "Dynamic";
            
            Rectangle rect = new Rectangle(x, y, width, height);
            AddGridView(rect, names[idName], type, direction);
            return new CObject(id, rect, colors[idName], idName, idType, obj[7] == "1");
        }
        void CheckValueTrue(TextBox textBox, int minValue, int maxValue, int defaultValue, out int value)
        {
            if (int.TryParse(textBox.Text, out value))
            {
                if (value < minValue || value > maxValue)
                {
                    textBox.Text = defaultValue.ToString();
                    value = defaultValue;
                }
            }


        }
        private void bunifuThinButton27_Click(object sender, EventArgs e)
        {
            if (isLoadTile) return;
            if (tilesImage.Count == 0)
            {
                MessageBox.Show("You must load tiles map before you try to split tiles again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                CheckValueTrue(txtSplitRows, 4, out rowsTiles);
                SetSizeSub((int)width, (int)height);
                SplitImageTiles();
                GC.Collect();
                labelTiles.Text = rowsTiles + " X " + colsTiles;
            }
        }
    }
}
