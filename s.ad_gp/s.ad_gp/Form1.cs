using LineControl;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace s.ad_gp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //划分初始化
            spcMain.SplitterDistance = spcMain.Width / 3;  //1/3用于显示数字
            spcInput.SplitterDistance = spcInput.Width / 2;  //平分
            spcPro.SplitterDistance = spcPro.Height * 3 / 4;  //1/4用于显示中间过程
            spcDraw.SplitterDistance = spcDraw.Width * 2 / 3;  //1/3用于显示表格
            spcRes.SplitterDistance = spcRes.Width / 2;  //结果区平分
        }

        LevelingNetwork LN;
        bool LNOK = false;
        Series seriesV;
        static int nowPoint;  //画线辅助计数
        PictureBox ppt1;  //画线保存点
        PictureBox ppt2;

        private void 新建NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LNOK = false;

            dgvKnow.Rows.Clear();
            dgvObs.Rows.Clear();
            dgvHHat.Rows.Clear();
            dgvV.Rows.Clear();

            txtbAHNum.Text = "";
            txtbAPNum.Text = "";
            txtbKPNum.Text = "";
            txtbSigma.Text = "";
            txtbAfterSigma.Text = "";
            txtResult.Text = "";

            catV.Series[0].Points.Clear();

            dgvKnow.Columns[0].HeaderCell.Value = "已知点号";
            dgvKnow.Columns[1].HeaderCell.Value = "已知点高程";

            tabControl1.SelectedTab = tpInput;

            LevelingPicture.InitPic();
            nowPoint = 0;
            panelDraw.Controls.Clear();
        }

        private void 打开OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                新建NToolStripMenuItem_Click(sender, e);  //先新建一次避免数据重叠

                OpenFileDialog open = new OpenFileDialog();
                open.Title = "请选择水准网文件";
                open.Filter = "水准网文件(*.txt)|*.txt";

                if (open.ShowDialog() == DialogResult.OK)
                {
                    string line = "";
                    string[] lines;

                    using (Stream stream = new FileStream(open.FileName, FileMode.Open))
                    {
                        StreamReader read = new StreamReader(stream, Encoding.Default);

                        line = read.ReadLine();
                        lines = line.Split('\t');
                        txtbAHNum.Text = lines[0];
                        txtbAPNum.Text = lines[1];
                        txtbKPNum.Text = lines[2];
                        txtbSigma.Text = lines[3];

                        //添加数据行
                        for (int i = 0; i < int.Parse(txtbAHNum.Text); i++)
                        {
                            dgvObs.Rows.Add();
                        }

                        //非自由网
                        if (txtbKPNum.Text != "0")
                        {
                            for (int i = 0; i < int.Parse(txtbKPNum.Text); i++)
                            {
                                dgvKnow.Rows.Add();
                            }

                            for (int k = 0; k < int.Parse(txtbKPNum.Text); k++)
                            {
                                line = read.ReadLine();
                                lines = line.Split('\t');
                                dgvKnow[0, k].Value = lines[0];
                                dgvKnow[1, k].Value = lines[1];
                            }

                            read.ReadLine();
                        }
                        //自由网
                        else
                        {
                            dgvKnow.Columns[0].HeaderCell.Value = "点号";
                            dgvKnow.Columns[1].HeaderCell.Value = "近似高程";
                            int m = 0;

                            line = read.ReadLine();

                            while (line != "")
                            {
                                dgvKnow.Rows.Add();
                                lines = line.Split('\t');
                                dgvKnow[0, m].Value = lines[0];
                                dgvKnow[1, m].Value = lines[1];
                                line = read.ReadLine();
                                m += 1;

                                if (line == "")
                                {
                                    break;
                                }
                            }
                        }

                        for (int k = 0; k < int.Parse(txtbAHNum.Text); k++)
                        {
                            line = read.ReadLine();
                            lines = line.Split('\t');
                            dgvObs[0, k].Value = lines[0];
                            dgvObs[1, k].Value = lines[1];
                            dgvObs[2, k].Value = lines[2];
                            dgvObs[3, k].Value = lines[3];
                        }

                        read.Close();
                    }
                }
            }
            catch
            {
                MessageBox.Show("打开文件失败，请确认文件格式是否正确", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void 退出QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();  //强制退出
        }

        private void 建立水准网ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (出题模式ToolStripMenuItem.Checked == true)
                {
                    Random rnd = new Random();
                    double[] pointAndHei = new double[int.Parse(txtbAPNum.Text)];
                    string[] pointIndex = new string[int.Parse(txtbAPNum.Text)];

                    //生成先验权中误差
                    txtbSigma.Text = "0.001";

                    //赋值路线长度
                    for (int i = 0; i < dgvObs.RowCount; i++)
                    {
                        for (int j = panelDraw.Controls.Count - 1; j >= 0; j--)
                        {
                            if (panelDraw.Controls[j] is StraightLine)
                            {
                                StraightLine line = (StraightLine)panelDraw.Controls[j];

                                if (dgvObs[0, i].Value.ToString() == line.startP && dgvObs[1, i].Value.ToString() == line.endP)
                                {
                                    dgvObs[3, i].Value = line.Lenght;
                                }
                            }
                        }
                    }

                    //赋值高程点
                    int q = 0;

                    for (int j = panelDraw.Controls.Count - 1; j >= 0; j--)
                    {
                        if (panelDraw.Controls[j] is PictureBox)
                        {
                            PictureBox point = (PictureBox)panelDraw.Controls[j];
                            pointIndex[q] = point.Name;
                            pointAndHei[q] = Math.Round((10 * rnd.NextDouble() + rnd.Next(100, 200)), 2);

                            //已知点赋值
                            if (pointIndex[q].Substring(0, 1) == "K")
                            {
                                for (int k = 0; k < dgvKnow.RowCount; k++)
                                {
                                    if (dgvKnow[0, k].Value.ToString() == pointIndex[q])
                                    {
                                        dgvKnow[1, k].Value = pointAndHei[q];
                                    }
                                }
                            }

                            q += 1;
                        }
                    }

                    //赋值路线高差
                    for (int m = 0; m < dgvObs.RowCount; m++)
                    {
                        double sP = 0;
                        double eP = 0;

                        for (int n = 0; n < pointAndHei.Length; n++)
                        {
                            if (dgvObs[0, m].Value.ToString() == pointIndex[n])
                            {
                                sP = pointAndHei[n];
                            }
                            if (dgvObs[1, m].Value.ToString() == pointIndex[n])
                            {
                                eP = pointAndHei[n];
                            }
                        }

                        dgvObs[2, m].Value = Math.Round(((eP - sP) + rnd.NextDouble()), 2);
                    }
                }

                if (txtbAHNum.Text != "" && txtbAPNum.Text != "" && txtbKPNum.Text != "" && txtbSigma.Text != "")
                {
                    int AHNum = int.Parse(txtbAHNum.Text);
                    int APNum = int.Parse(txtbAPNum.Text);
                    int KPNum = int.Parse(txtbKPNum.Text);
                    double Sigma = double.Parse(txtbSigma.Text);
                    LN = new LevelingNetwork(AHNum, APNum, KPNum, Sigma);

                    LNOK = LN.BuildLevelingNetwork(dgvKnow, dgvObs);
                    MessageBox.Show("建立水准网成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //测试
                    txtResult.Text += "水准网初始高程：\r\n";
                    for (int i = 0; i < LN.LevelingPoints.Length; i++)
                    {
                        txtResult.Text += LN.LevelingPoints[i].Name + "   " + LN.LevelingPoints[i].Height + "\r\n";
                    }
                    txtResult.Text += "\r\n水准网路线信息：\r\n";
                    for (int j = 0; j < LN.LevelingLines.Length; j++)
                    {
                        txtResult.Text += LN.LevelingLines[j].LineName + "   " + LN.LevelingLines[j].HeightDisparity + "   " + LN.LevelingLines[j].RoadLength + "\r\n";
                    }
                }
                else
                {
                    MessageBox.Show("请先确定水准网图形", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                MessageBox.Show("未知错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 水准网平差ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (LNOK)
                {
                    dgvHHat.Rows.Clear();
                    dgvV.Rows.Clear();

                    //重置粗差
                    for (int j = 0; j < LN.ObservationNum; j++)
                    {
                        LN.LevelingLines[j].Error = false;
                    }

                    LN.Adjustment.OneStopService(LN, dgvKnow, 粗差探测ToolStripMenuItem.Checked);
                    ShowAdjustDataAndDrawChart();
                    tabControl1.SelectedTab = tpResult;
                    MessageBox.Show("平差完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //测试
                    txtResult.Text += "\r\n";
                    txtResult.Text += LN.Adjustment.B.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.B) + "\r\n";
                    txtResult.Text += LN.Adjustment.L.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.L) + "\r\n";
                    txtResult.Text += LN.Adjustment.P.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.P) + "\r\n";
                    txtResult.Text += LN.Adjustment.NBB.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.NBB) + "\r\n";
                    txtResult.Text += LN.Adjustment.WL.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.WL) + "\r\n";
                    txtResult.Text += LN.Adjustment.XHat.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.XHat) + "\r\n";
                    txtResult.Text += LN.Adjustment.V.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.V) + "\r\n";
                    txtResult.Text += LN.Adjustment.QXX.Name + " =\r\n" + Matrix.OutputMatrix(LN.Adjustment.QXX) + "\r\n";

                    for (int j = 0; j < LN.ObservationNum; j++)
                    {
                        if (LN.LevelingLines[j].Error == true)
                        {
                            txtResult.Text += "存在粗差:" + LN.LevelingLines[j].LineName + "\r\n";
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请先建立水准网", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                MessageBox.Show("未知错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //过程数据展示
        private void ShowAdjustDataAndDrawChart()
        {
            /********** 数据显示 **********/
            dgvHHat.Rows.Add(LN.PointNum - 1);
            dgvV.Rows.Add(LN.ObservationNum - 1);

            txtbAfterSigma.Text = LN.Adjustment.PosteriorSigma0.ToString();

            for (int i = 0; i < LN.PointNum; i++)
            {
                dgvHHat[0, i].Value = LN.LevelingPoints[i].Name;
                dgvHHat[1, i].Value = LN.LevelingPoints[i].AdjustHeight;
            }
            for (int j = 0; j < LN.ObservationNum; j++)
            {
                dgvV[0, j].Value = LN.LevelingLines[j].LineName;
                dgvV[1, j].Value = LN.LevelingLines[j].AdjustHeightDisparity;
            }

            /********** 绘制图表 **********/
            seriesV = catV.Series[0];
            seriesV.ChartType = SeriesChartType.Spline;
            seriesV.BorderWidth = 2;
            seriesV.Color = System.Drawing.Color.Red;
            seriesV.LegendText = "残差值";
            //准备数据
            float[] valuesV = new float[LN.LevelingLines.Length];
            string[] valuesS = new string[LN.LevelingLines.Length];

            for (int i = 0; i < LN.LevelingLines.Length; i++)
            {
                valuesV[i] = (float)(1e3 * (LN.LevelingLines[i].AdjustHeightDisparity - LN.LevelingLines[i].HeightDisparity));
                valuesS[i] = LN.LevelingLines[i].LineName;
            }

            //在chart中显示数据
            seriesV.Points.DataBindXY(valuesS, valuesV);

            //设置显示范围
            ChartArea chartArea = catV.ChartAreas[0];
            double max = -1e10;

            for (int i = 0; i < valuesV.Length; i++)
            {
                if ((valuesV[i] > 0 || valuesV[i] == 0) && valuesV[i] > max)
                {
                    max = valuesV[i];
                }
                else if (valuesV[i] < 0 && -valuesV[i] > max)
                {
                    max = -valuesV[i];
                }
            }

            chartArea.AxisX.Minimum = 1;
            chartArea.AxisX.Maximum = LN.ObservationNum;
            chartArea.AxisY.Minimum = -(int)(1.2 * max);
            chartArea.AxisY.Maximum = (int)(1.2 * max);
        }

        //自动切换到最后一行
        private void txtResult_TextChanged(object sender, EventArgs e)
        {
            txtResult.SelectionStart = txtResult.Text.Length;
            txtResult.ScrollToCaret();
        }

        private void 粗差探测ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (粗差探测ToolStripMenuItem.Checked == false)
            {
                粗差探测ToolStripMenuItem.Checked = true;
            }
            else
            {
                粗差探测ToolStripMenuItem.Checked = false;
            }
        }

        private void panelDraw_Click(object sender, MouseEventArgs e)
        {
            if (tspKP.Checked == true)
            {
                Point LPoint = new Point(e.X - 10, e.Y - 10);
                PictureBox ctl = LevelingPicture.DrawPoint(LPoint, panelDraw, dgvKnow);
                ctl.Click += Ctl_Click;
            }
            else if (tspUKP.Checked == true)
            {
                Point LPoint = new Point(e.X - 10, e.Y - 10);
                PictureBox ctl = LevelingPicture.DrawPoint(LPoint, panelDraw);
                ctl.Click += Ctl_Click;
            }

            UpData();
        }

        private void Ctl_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox)
            {
                //画线
                if (tspLine.Checked == true)
                {
                    if (nowPoint == 0)
                    {
                        ppt1 = (PictureBox)sender;
                        nowPoint = 1;
                    }
                    else if (nowPoint == 1)
                    {
                        ppt2 = (PictureBox)sender;

                        if (ppt1 != ppt2)  //不同的两个点
                        {
                            LevelingPicture.DrawLine(ppt1, ppt2, panelDraw, dgvObs);
                            UpData();
                            nowPoint = 0;
                        }
                    }
                }
                //橡皮
                if (tspE.Checked == true)
                {
                    PictureBox ppte = (PictureBox)sender;

                    if (ppte.Name.Substring(0, 1) == "K")
                    {
                        LevelingPicture.Erase(ppte, panelDraw, dgvKnow, dgvObs);
                    }
                    if (ppte.Name.Substring(0, 1) == "P")
                    {
                        LevelingPicture.Erase(ppte, panelDraw, dgvObs);
                    }

                    UpData();
                }
            }
        } 

        //更新画点画线的统计数据
        private void UpData()
        {
            txtbAPNum.Text = (LevelingPicture.KnowPointNum + LevelingPicture.UnKnowPointNum).ToString();
            txtbKPNum.Text = LevelingPicture.KnowPointNum.ToString();
            txtbAHNum.Text = LevelingPicture.LineNum.ToString();
        }

        #region 绘图按钮设置
        private void tspKP_CheckedChanged(object sender, EventArgs e)
        {
            if (tspKP.Checked == true)
            {
                tspUKP.Checked = false;
                tspLine.Checked = false;
                tspE.Checked = false;

                nowPoint = 0;  //清空画线的第一个点
            }
        }

        private void tspUKP_CheckedChanged(object sender, EventArgs e)
        {
            if (tspUKP.Checked == true)
            {
                tspKP.Checked = false;
                tspLine.Checked = false;
                tspE.Checked = false;

                nowPoint = 0;
            }
        }

        private void tspLine_CheckedChanged(object sender, EventArgs e)
        {
            if (tspLine.Checked == true)
            {
                tspKP.Checked = false;
                tspUKP.Checked = false;
                tspE.Checked = false;

                nowPoint = 0;
            }
        }

        private void tspE_CheckedChanged(object sender, EventArgs e)
        {
            if (tspE.Checked == true)
            {
                tspKP.Checked = false;
                tspUKP.Checked = false;
                tspLine.Checked = false;

                nowPoint = 0;
            }
        }

        private void tspClear_Click(object sender, EventArgs e)
        {
            tspKP.Checked = false;
            tspUKP.Checked = false;
            tspLine.Checked = false;
            tspE.Checked = false;

            新建NToolStripMenuItem_Click(sender, e);
        }
        #endregion

        private void 中间过程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (中间过程ToolStripMenuItem.Checked == true)
            {
                中间过程ToolStripMenuItem.Checked = false;
                spcPro.Panel2Collapsed = true;  //折叠
            }
            else
            {
                中间过程ToolStripMenuItem.Checked = true;
                spcPro.Panel2Collapsed = false;
            }
        }

        private void 出题模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (出题模式ToolStripMenuItem.Checked == true)
            {
                出题模式ToolStripMenuItem.Checked = false;
            }
            else
            {
                出题模式ToolStripMenuItem.Checked = true;
            }
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "请选择保存位置";
            save.Filter = "水准网文件(*.txt)|*.txt";

            if (save.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Stream stream = new FileStream(save.FileName, FileMode.Create))
                    {
                        StreamWriter writer = new StreamWriter(stream, Encoding.Default);

                        writer.Write(txtbAHNum.Text + "\t" + txtbAPNum.Text + "\t" + txtbKPNum.Text + "\t" + txtbSigma.Text);
                        writer.WriteLine();

                        for (int i = 0; i < dgvKnow.RowCount; i++)
                        {
                            writer.Write(dgvKnow[0, i].Value.ToString() + "\t" + dgvKnow[1, i].Value.ToString() + "\r\n");
                        }
                        writer.WriteLine();

                        for (int j = 0; j < dgvObs.RowCount - 1; j++)
                        {
                            writer.Write(dgvObs[0, j].Value.ToString() + "\t" + dgvObs[1, j].Value.ToString()
                                + "\t" + dgvObs[2, j].Value.ToString() + "\t" + dgvObs[3, j].Value.ToString() + "\r\n");
                        }
                        writer.Write(dgvObs[0, dgvObs.RowCount - 1].Value.ToString() + "\t" + dgvObs[1, dgvObs.RowCount - 1].Value.ToString()
                                + "\t" + dgvObs[2, dgvObs.RowCount - 1].Value.ToString() + "\t" + dgvObs[3, dgvObs.RowCount - 1].Value.ToString());

                        writer.Close();
                        MessageBox.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("保存文件失败，请确认数据是否完整无误", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                } 
            }
        }

        private void 数据文件格式说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("观测总数\t" + "总点数\t" + "已知点数\t" + "先验单位权中误差\r\n"
                + "已知点号\t" + "已知点高程\r\n" + "……\r\n\r\n"
                + "高差起点\t" + "高差终点\t" + "高差测值\t" + "路线长度\r\n" + "……\r\n", "格式说明");
        }
    }
}
