using System.Windows.Forms;

namespace s.ad_gp
{
    class LevelingNetwork
    {
        private int _observationNum;  //观测总数
        private int _pointNum;  //总点数
        private int _knownPointNum;  //已知点数
        private double _transcendentalSigma0;  //先验单位权中误差
        private double _posteriorSigma0;  //验后单位权中误差
        private LevelingPoint[] _levelingPoints;  //水准点
        private LevelingLine[] _levelingLines;  //水准路线
        private Adjustment _adjustment;  //平差信息

        public int ObservationNum { get => _observationNum; set => _observationNum = value; }
        public int PointNum { get => _pointNum; set => _pointNum = value; }
        public int KnownPointNum { get => _knownPointNum; set => _knownPointNum = value; }
        public double TranscendentalSigma0 { get => _transcendentalSigma0; set => _transcendentalSigma0 = value; }
        internal LevelingPoint[] LevelingPoints { get => _levelingPoints; set => _levelingPoints = value; }
        internal LevelingLine[] LevelingLines { get => _levelingLines; set => _levelingLines = value; }
        internal Adjustment Adjustment { get => _adjustment; set => _adjustment = value; }
        public double PosteriorSigma0 { get => _posteriorSigma0; set => _posteriorSigma0 = value; }

        public LevelingNetwork(int AHNum, int APNum, int KPNum, double Sigma)
        {
            ObservationNum = AHNum;
            PointNum = APNum;
            KnownPointNum = KPNum;
            TranscendentalSigma0 = Sigma;
            PosteriorSigma0 = 0;

            LevelingPoints = new LevelingPoint[PointNum];
            LevelingLines = new LevelingLine[ObservationNum];
            Adjustment = new Adjustment(ObservationNum, (PointNum - KnownPointNum));
        }

        ~LevelingNetwork()
        {

        }

        //建立水准网
        public bool BuildLevelingNetwork(DataGridView dgvKnow, DataGridView dgvUKnow)
        {
            try
            {
                int kpn = KnownPointNum;

                //点建立

                for (int i = 0; i < dgvKnow.RowCount; i++)
                {
                    string N = dgvKnow[0, i].Value.ToString();
                    double H = double.Parse(dgvKnow[1, i].Value.ToString());
                    LevelingPoints[i] = new LevelingPoint(N, H);
                }

                for (int j = 0; j < dgvUKnow.RowCount; j++)
                {
                    if (kpn < PointNum)
                    {
                        LevelingPoint LP1 = new LevelingPoint(dgvUKnow[0, j].Value.ToString(), -999);
                        LevelingPoint LP2 = new LevelingPoint(dgvUKnow[1, j].Value.ToString(), -999);

                        //PL1不在
                        if (PointInPointsOrNot(LP1, LevelingPoints) && !PointInPointsOrNot(LP2, LevelingPoints))
                        {
                            LevelingPoints[kpn] = LP1;
                            kpn += 1;
                        }
                        //PL2不在
                        else if (PointInPointsOrNot(LP2, LevelingPoints) && !PointInPointsOrNot(LP1, LevelingPoints))
                        {
                            LevelingPoints[kpn] = LP2;
                            kpn += 1;
                        }
                        //PL1和PL2都不在
                        else if (PointInPointsOrNot(LP1, LevelingPoints) && PointInPointsOrNot(LP2, LevelingPoints))
                        {
                            LevelingPoints[kpn] = LP1;
                            kpn += 1;
                            LevelingPoints[kpn] = LP2;
                            kpn += 1;
                        }
                    }
                }

                //线建立
                for (int k = 0; k < dgvUKnow.RowCount; k++)
                {
                    LevelingPoint SP = FindPointByName(dgvUKnow[0, k].Value.ToString());
                    LevelingPoint EP = FindPointByName(dgvUKnow[1, k].Value.ToString());
                    double HD = double.Parse(dgvUKnow[2, k].Value.ToString());
                    double RL = double.Parse(dgvUKnow[3, k].Value.ToString());
                    string N = SP.Name + EP.Name;
                    LevelingLines[k] = new LevelingLine(N, HD, RL, SP, EP);
                }

                //计算各点的近似高程
                if (KnownPointNum != 0)
                {
                    //该点为水准路线终点，加法
                    for (int m = KnownPointNum; m < PointNum; m++)
                    {
                        for (int n = 0; n < LevelingLines.Length; n++)
                        {
                            if (LevelingPoints[m].Name == LevelingLines[n].EndPoint.Name && LevelingLines[n].StartPoint.Height != -999)
                            {
                                LevelingPoints[m].Height = LevelingLines[n].StartPoint.Height + LevelingLines[n].HeightDisparity;
                            }
                        }
                    }

                    //该点为水准路线起点，减法
                    for (int m = KnownPointNum; m < PointNum; m++)
                    {
                        if (LevelingPoints[m].Height == -999)
                        {
                            for (int n = 0; n < LevelingLines.Length; n++)
                            {
                                if (LevelingPoints[m].Name == LevelingLines[n].StartPoint.Name && LevelingLines[n].EndPoint.Height != -999)
                                {
                                    LevelingPoints[m].Height = LevelingLines[n].EndPoint.Height - LevelingLines[n].HeightDisparity;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        //内部处理方法
        private bool PointInPointsOrNot(LevelingPoint p, LevelingPoint[] ps)
        {
            int number = 0;
            bool notIn = false;

            for (int i = 0; i < ps.Length && ps[i] != null; i++)
            {
                if (p.Name == ps[i].Name)
                {
                    number++;
                }
            }

            if (number != 1)
            {
                notIn = true;
            }

            return notIn;
        }

        private LevelingPoint FindPointByName(string pointStr)
        {
            LevelingPoint LP = new LevelingPoint("null", -999);

            for (int i = 0; i < LevelingPoints.Length; i++)
            {
                if (LevelingPoints[i].Name == pointStr)
                {
                    LP = LevelingPoints[i];
                }
            }

            return LP;
        }
    }
}
