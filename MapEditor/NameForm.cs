using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapEditor
{
    public partial class NameForm : MetroFramework.Forms.MetroForm
    {
        public string[] Names
        {
            get
            {
                List<string> vs = new List<string>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    vs.Add(row.Cells[0].Value.ToString());
                }
                return vs.ToArray();
            }
        }
        public NameForm(String[] data)
        {
            InitializeComponent();
            AddData(data);
            
        }
        public void AddData(String[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                dataGridView.Rows.Add(data[i]);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView.SelectedRows[0];
                txbName.Text = row.Cells[0].Value.ToString();
            }
        }

        private void bunifuThinButton21_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    row.Cells[0].Value = txbName.Text;
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Add(txbName.Text);
        }

        private void bunifuThinButton22_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure", "Warring", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        dataGridView.Rows.Remove(row);
                    }
                }
            }
        }


        private void bunifuThinButton23_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
