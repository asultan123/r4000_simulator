using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace r400sim
{
    public partial class Form1 : Form
    {
        int currentClock = 0;
        r4000sim.Simulator sim;
        int maxClock;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentClock = (currentClock > 0) ? currentClock - 1 : 0;
            clock.Text = currentClock.ToString();
            updateGui(currentClock);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sim = new r4000sim.Simulator(openFileDialog1.FileName);
                currentClock = 0;
                maxClock = sim.run();
                updateGui(0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currentClock = (currentClock < maxClock) ? currentClock + 1 : maxClock;
            clock.Text = currentClock.ToString();
            updateGui(currentClock);
        }

        private void updateGui(int clockCount)
        {
            updateReg(clockCount);
            updateDmem(clockCount);
            updateStack(clockCount);
            updateBranchPredictionBuffer(clockCount);
        }

        private void updateBranchPredictionBuffer(int clockCount)
        {
            DataTable bpTable = new DataTable();
            
        }
        private void updateStack(int clockCount)
        {
            DataTable stackTable = new DataTable();
            stackView.DataSource = stackTable;
            stackTable.Columns.Add("Stack");

            foreach(int entry in sim.stack.history[clockCount])
            {
                stackTable.Rows.Add(entry);
            }
            for(int i = 0; i<stackView.Rows.Count; i++)
            {
                stackView.Rows[i].HeaderCell.Value = i;
            }
        }

        private void updateDmem(int clockCount)
        {
            DataTable dmemTable = new DataTable();
            dmemView.DataSource = dmemTable;
            dmemTable.Columns.Add("Data Mem (B)");

            for (int i = 0; i < 64; i++)
            {
                dmemTable.Rows.Add(sim.dmem.history[clockCount][i]);
            }
            for(int i = 0; i< dmemTable.Rows.Count; i++)
            {
                dmemView.Rows[i].HeaderCell.Value = i.ToString();
            }
        }

        private void updateReg(int clockCount)
        {
            DataTable regTable = new DataTable();
            regView.DataSource = regTable;
            regTable.Columns.Add("Registers");

            for (int i = 0; i<16; i++)
            {
                regTable.Rows.Add(sim.reg.history[clockCount][i]);
            }

            for(int i = 0; i<regView.Rows.Count; i++)
            {
                regView.Rows[i].HeaderCell.Value = i.ToString();
            }
        }
    }

}
