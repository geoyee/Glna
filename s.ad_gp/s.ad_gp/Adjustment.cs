using System;
using System.Windows.Forms;

namespace s.ad_gp
{
    class Adjustment
    {
        private Matrix _b;
        private Matrix _l;
        private Matrix _p;
        private Matrix _s;
        private Matrix _nBB;
        private Matrix _wL;
        private Matrix _xHat;
        private Matrix _v;
        private Matrix _qXX;
        private double _posteriorSigma0;
        private int r;
        private Matrix QVV;

        internal Matrix B { get => _b; set => _b = value; }
        internal Matrix L { get => _l; set => _l = value; }
        internal Matrix P { get => _p; set => _p = value; }
        internal Matrix S { get => _s; set => _s = value; }
        internal Matrix NBB { get => _nBB; set => _nBB = value; }
        internal Matrix WL { get => _wL; set => _wL = value; }
        internal Matrix XHat { get => _xHat; set => _xHat = value; }
        internal Matrix V { get => _v; set => _v = value; }
        internal Matrix QXX { get => _qXX; set => _qXX = value; }
        public double PosteriorSigma0 { get => _posteriorSigma0; set => _posteriorSigma0 = value; }

        public Adjustment(int FunctionNum, int UnKnownNum)
        {
            B = new Matrix("B", FunctionNum, UnKnownNum);
            L = new Matrix("L", FunctionNum, 1);
            P = new Matrix("P", FunctionNum, FunctionNum);
            NBB = new Matrix("NBB = B[T]PB", B.Column, B.Column);
            S = new Matrix("S", NBB.Row, 1);
            WL = new Matrix("WL = B[T]PL", B.Column, 1);
            XHat = new Matrix("XHat = NBB[-1]WL", NBB.Row, 1);
            V = new Matrix("V = BXHat - L", FunctionNum, 1);
            QXX = new Matrix("QXX = NBB[-1]", NBB.Row, NBB.Row);
        }

        ~Adjustment()
        {

        }

        private void GetBMatrix(LevelingNetwork LN)
        {
            for (int i = 0; i < LN.LevelingLines.Length; i++)
            {
                for (int j = LN.KnownPointNum; j < LN.LevelingPoints.Length; j++)
                {
                    if (LN.LevelingLines[i].StartPoint.Name == LN.LevelingPoints[j].Name)
                    {
                        B.Values[i, j - LN.KnownPointNum] = -1;
                    }
                    else if (LN.LevelingLines[i].EndPoint.Name == LN.LevelingPoints[j].Name)
                    {
                        B.Values[i, j - LN.KnownPointNum] = 1;
                    }
                    else
                    {
                        B.Values[i, j - LN.KnownPointNum] = 0;
                    }
                }
            }
        }

        private void GetLMatrix(LevelingNetwork LN)
        {
            for (int i = 0; i < LN.LevelingLines.Length; i++)
            {
                L.Values[i, 0] = LN.LevelingLines[i].HeightDisparity - LN.LevelingLines[i].EndPoint.Height + LN.LevelingLines[i].StartPoint.Height;
            }
        }

        private void GetPMatrix(LevelingNetwork LN)
        {
            for (int i = 0; i < LN.LevelingLines.Length; i++)
            {
                P.Values[i, i] = 1 / LN.LevelingLines[i].RoadLength;
            }
        }

        private void GetNBBMatrix()
        {
            NBB.Values = (B.Transpose() * P * B).Values;
        }

        private void GetWLMatrix()
        {
            WL.Values = (B.Transpose() * P * L).Values;
        }

        private void GetXHatMatrix()
        {
            if (IsRankDeficitOrNot(NBB))
            {
                XHat.Values = (NBB.Inverse() * WL).Values;
            }
            else
            {
                for (int i = 0; i < NBB.Column; i++)
                {
                    S.Values[i, 0] = 1 / Math.Sqrt(B.Column);
                }

                XHat.Name = "XHat = (NBB + SS[T])[-1]WL";
                XHat.Values = ((NBB + S * S.Transpose()).Inverse() * WL).Values;
            }
        }

        private void GetVMatrix()
        {
            V.Values = (B * XHat - L).Values;
        }

        private void GetQXXMatrix()
        {
            if (IsRankDeficitOrNot(NBB))
            {
                QXX.Values = NBB.Inverse().Values;
            }
            else
            {
                QXX.Name = "QXX = (NBB + SS[T])[-1]";
                QXX.Values = ((NBB + S * S.Transpose()).Inverse() - S * S.Transpose()).Values;
            }
        }

        private void GetQVVMatrix()
        {
            QVV = new Matrix("QVV = Q - BNBBB[T]", V.Row, V.Column);
            QVV.Values = (P.Inverse() - B * NBB.Inverse() * B.Transpose()).Values;
        }

        private void GetPosteriorSigma0(Matrix V, Matrix P, int r)
        {
            Matrix VTPV = V.Transpose() * P * V;

            if (IsRankDeficitOrNot(NBB))
            {
                PosteriorSigma0 = Math.Sqrt(VTPV.Values[0, 0] / r);
            }
            else
            {
                PosteriorSigma0 = Math.Sqrt(VTPV.Values[0, 0] / (r + 1));
            }
        }

        private bool IsRankDeficitOrNot(Matrix NBB)
        {
            double beta = 1e-6;

            if (Matrix.GetDeterminant(NBB) < beta && Matrix.GetDeterminant(NBB) > -beta)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //水准点中误差
        private void GetPointSigma(LevelingNetwork LN)
        {
            for (int i = LN.KnownPointNum; i < LN.PointNum; i++)
            {
                LN.LevelingPoints[i].Sigma = PosteriorSigma0 * Math.Sqrt(QXX.Values[i - LN.KnownPointNum, i - LN.KnownPointNum]);
            }
        }

        //一条龙服务
        public void OneStopService(LevelingNetwork LN, DataGridView dgvKnow, bool CCTC)
        {
            double alpha = 1 - 0.003 / 2;
            r = LN.ObservationNum - (LN.PointNum - LN.KnownPointNum);

            if (r == 0)
            {
                r = dgvKnow.RowCount;
            }

            GetBMatrix(LN);
            GetLMatrix(LN);
            GetPMatrix(LN);

            if (CCTC == false)
            {
                LeastSquareMethod(LN);
            }
            else
            {
                double Ualpha2 = ReNorm(alpha);
                DataDetectionMethod(LN, Ualpha2);
            }          
        }

        //最小二乘法
        private void LeastSquareMethod(LevelingNetwork LN)
        {            
            GetNBBMatrix();
            GetWLMatrix();
            GetXHatMatrix();
            GetVMatrix();
            GetQXXMatrix();
            GetQVVMatrix();
            GetPosteriorSigma0(V, P, r);
            GetPointSigma(LN);
            LN.PosteriorSigma0 = PosteriorSigma0;

            for (int k = 0; k < LN.KnownPointNum; k++)
            {
                LN.LevelingPoints[k].AdjustHeight = LN.LevelingPoints[k].Height;
                LN.LevelingPoints[k].Qv = 0;
            }
            for (int i = LN.KnownPointNum; i < LN.LevelingPoints.Length; i++)
            {
                LN.LevelingPoints[i].AdjustHeight = LN.LevelingPoints[i].Height + XHat.Values[i - LN.KnownPointNum, 0];
                LN.LevelingPoints[i].Qv = QXX.Values[i - LN.KnownPointNum, i - LN.KnownPointNum];
            }
            for (int j = 0; j < LN.LevelingLines.Length; j++)
            {
                double qvi = QVV.Values[j, j];
                double vi = V.Values[j, 0];
                double mvi = LN.TranscendentalSigma0 * Math.Sqrt(qvi);  //单位统一

                LN.LevelingLines[j].AdjustHeightDisparity = LN.LevelingLines[j].HeightDisparity + V.Values[j, 0];
                LN.LevelingLines[j].U = vi / mvi;
            }
        }

        //数据探测法
        private void DataDetectionMethod(LevelingNetwork LN, double Ualpha2)
        {
            int indexMax = FindMaxU(LN);
            LeastSquareMethod(LN);
            double doubleMax = DoubleAbs(LN.LevelingLines[indexMax].U);

            while (doubleMax > Ualpha2)
            {
                LN.LevelingLines[indexMax].Error = true;
                P.Values[indexMax, indexMax] = 0;

                indexMax = FindMaxU(LN);
                LeastSquareMethod(LN);
                doubleMax = DoubleAbs(LN.LevelingLines[indexMax].U);
            }    
        }

        //正态分布反函数
        private double ReNorm(double alpha)
        {
            if (alpha == 0.5)
            {
                return 0;
            }
            if (alpha > 0.9999997)
            {
                return 5;
            }
            if (alpha < 0.0000003)
            {
                return -5;
            }
            if (alpha < 0.5)
            {
                return -ReNorm(1 - alpha);
            }

            double y = -Math.Log(4 * alpha * (1 - alpha));

            y = y * (1.570796288 + y * (0.3706987906e-1
                + y * (-0.8364353589e-3 + y * (-0.2250947176e-3
                + y * (0.6841218299e-5 + y * (0.5824238515e-5
                + y * (-0.1045274970e-5 + y * (0.8360937017e-7
                + y * (-0.3231081277e-8 + y * (0.3657763036e-10
                + y * 0.6936233982e-12))))))))));

            return Math.Sqrt(y);
        }

        //查找最大标准化残差
        private int FindMaxU(LevelingNetwork LN)
        {
            double Umax = -999999;
            int index = -1;

            for (int i = 0; i < LN.LevelingLines.Length; i++)
            {
                if (DoubleAbs(LN.LevelingLines[i].U) > Umax)
                {
                    Umax = DoubleAbs(LN.LevelingLines[i].U);
                    index = i;
                }
            }

            return index;
        }
        
        //小数的绝对值
        private double DoubleAbs(double number)
        {
            if (number >= 0)
            {
                return number;
            }
            else
            {
                return -number;
            }
        }
    }
}
