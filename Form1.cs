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
        private string path;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = Path.GetDirectoryName(openFileDialog1.FileName);
                //CopyDB();
                Connection();
            }
        }
        private void CopyDB()
        {
            File.Copy(path + @"\\SynDB.sqlite",
                path + @"\\ShortSynDB.sqlite", true);
        }
        private void Connection()
        {
            originalHelper = new SQLiteHelper(path + @"\\SynDB.sqlite");
            newHelper = new SQLiteHelper(path + @"\\ShortSynDB.sqlite");
            originalHelper.CreateConnection();
            newHelper.CreateConnection();
        }
    }
}

