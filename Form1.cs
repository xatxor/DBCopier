using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace DBCopier
{
    public partial class Form1 : Form
    {
        public SQLiteHelper originalHelper;
        public SQLiteHelper newHelper;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                Connection(path);
            }
        }
        private void Connection(string path)
        {
            originalHelper = new SQLiteHelper(Path.GetDirectoryName(path) + @"\\SynDB.sqlite");
            newHelper = new SQLiteHelper(Path.GetDirectoryName(path) + @"\\ShortSynDB.sqlite");
            originalHelper.CreateConnection();
            newHelper.CreateConnection();
        }
    }
}

