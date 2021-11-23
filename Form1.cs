using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace connection_manager
{
    public partial class Form1 : Form
    {
        private bool setup = false;
        public string netAdapter = "Ethernet";

        public Form1()
        {
            InitializeComponent();
            setAdapters();

            listView1.ItemChecked += new ItemCheckedEventHandler(checked_item);
        }

        private void setAdapters()
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "netsh.exe";
            p.StartInfo.Arguments = "interface show interface";
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            string[] lines = output.Split('\n');

            listView1.BeginUpdate();

            for (int i=3; i < lines.Length; i++)
            {
                if (!String.IsNullOrEmpty(lines[i]) && (!lines[i].StartsWith("\n") && !lines[i].StartsWith("\r")))
                {
                    int j = 16;
                    string adapterName = "";
                    string[] adapter = lines[i].Split(' ');
                    bool check = false;


                    if (adapter[0].Contains("Enabled"))
                        check = true;


                    for (; j< adapter.Length; j++)
                    {
                        adapterName += adapter[j] + " ";
                    }

                    listView1.Items.Add(new ListViewItem(adapterName.Trim())
                    {
                        Checked = check
                    });
                }
            }

            listView1.EndUpdate();
        }

        private void checked_item(object sender, ItemCheckedEventArgs e)
        {
            if (setup)
            {
                string action;
                if (e.Item.Checked)
                    action = "enabled";
                else
                    action = "disabled";

                Process.Start("netsh.exe", $"interface set interface \"{e.Item.Text}\" {action}");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                netAdapter = listView1.SelectedItems[0].Text;
            }

            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
            else
                backgroundWorker1.CancelAsync();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setup = true;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            InterceptKeys.InitializeComponent(netAdapter);
        }
    }
}
