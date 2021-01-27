using LineControl;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace s.ad_gp
{
    class LevelingPicture
    {
        private static int _knowPointNum;
        private static int _unKnowPointNum;
        private static int _lineNum;
        private static int pN;  //避免顺序出错
        private static int lN;

        public static int KnowPointNum { get => _knowPointNum; set => _knowPointNum = value; }
        public static int UnKnowPointNum { get => _unKnowPointNum; set => _unKnowPointNum = value; }
        public static int LineNum { get => _lineNum; set => _lineNum = value; }

        public static void InitPic()
        {
            KnowPointNum = 0;
            UnKnowPointNum = 0;
            LineNum = 0;
            pN = 0;
            lN = 0;
        }

        //绘点
        private static PictureBox NewPic(Point point, Panel drawingBoard, string name)
        {
            PictureBox pointPic = new PictureBox();
            pointPic.Name = name;
            pointPic.Size = new Size(20, 20);
            pointPic.Location = point;
            pointPic.Parent = drawingBoard;
            pointPic.BackColor = Color.Transparent;
            pointPic.SizeMode = PictureBoxSizeMode.StretchImage;

            return pointPic;
        }

        private static Label NewLab(Point point, Panel drawingBoard, PictureBox picture)
        {
            Label pointLab = new Label();
            pointLab.AutoSize = true;
            pointLab.Location = new Point(point.X - 10, point.Y - 10);
            pointLab.Parent = picture;
            pointLab.BackColor = Color.Transparent;
            pointLab.Text = picture.Name;

            return pointLab;
        }

        public static PictureBox DrawPoint(Point point, Panel drawingBoard)
        {
            PictureBox pointPic = NewPic(point, drawingBoard, "P" + pN++);
            Label pointLab = NewLab(point, drawingBoard, pointPic);

            pointPic.Load(Application.StartupPath + "/tools_img/WZD_512px.png");
            drawingBoard.Controls.Add(pointPic);
            drawingBoard.Controls.Add(pointLab);
            UnKnowPointNum += 1;

            return pointPic;
        }

        public static PictureBox DrawPoint(Point point, Panel drawingBoard, DataGridView data)
        {
            PictureBox pointPic = NewPic(point, drawingBoard, "K" + pN++);
            Label pointLab = NewLab(point, drawingBoard, pointPic);

            pointPic.Load(Application.StartupPath + "/tools_img/YZD_512px.png");
            data.Rows.Add();
            data[0, KnowPointNum].Value = "K" + (pN - 1); 
            drawingBoard.Controls.Add(pointPic);
            drawingBoard.Controls.Add(pointLab);
            KnowPointNum += 1;

            return pointPic;
        }

        //绘线
        private static void StraighLineDraw(PictureBox P1, PictureBox P2, Panel drawingBoard)
        {
            Point PO1 = new Point(P1.Location.X + 10, P1.Location.Y + 10);
            Point PO2 = new Point(P2.Location.X + 10, P2.Location.Y + 10);
            StraightLine line = new StraightLine();

            line.Name = "L" + lN++;
            line.Parent = drawingBoard;
            line.ForeColor = Color.Blue;
            line.Thickness = 2;  //线宽
            line.startP = P1.Name;  //识别码
            line.endP = P2.Name;
            line.Lenght = Math.Round(Math.Sqrt((PO1.X - PO2.X) * (PO1.X - PO2.X) + (PO1.Y - PO2.Y) * (PO1.Y - PO2.Y)), 2);

            if (PO1.X > PO2.X && PO1.Y < PO2.Y)
            {
                line.Location = new Point(PO2.X, PO1.Y);
                line.Size = new Size(PO1.X - PO2.X, PO2.Y - PO1.Y);
                line.LineType = StraightLineTypes.DiagonalAscending;
            }
            else if (PO1.X > PO2.X && PO1.Y > PO2.Y)
            {
                line.Location = PO2;
                line.Size = new Size(PO1.X - PO2.X, PO1.Y - PO2.Y);
                line.LineType = StraightLineTypes.DiagonalDescending;
            }
            else if(PO1.X < PO2.X && PO1.Y > PO2.Y)
            {
                line.Location = new Point(PO1.X, PO2.Y);
                line.Size = new Size(PO2.X - PO1.X, PO1.Y - PO2.Y);
                line.LineType = StraightLineTypes.DiagonalAscending;
            }
            else if (PO1.X < PO2.X && PO1.Y < PO2.Y)
            {
                line.Location = PO1;
                line.Size = new Size(PO2.X - PO1.X, PO2.Y - PO1.Y);
                line.LineType = StraightLineTypes.DiagonalDescending;
            }
            else if (PO1.X == PO2.X)
            {
                if (PO1.Y < PO2.Y)
                {
                    line.Location = new Point(PO1.X, PO1.Y - 10);
                    line.Size = new Size(20, PO2.Y - PO1.Y);
                    
                }
                else
                {
                    line.Location = new Point(PO2.X, PO2.Y - 10);
                    line.Size = new Size(20, PO1.Y - PO2.Y);
                }

                line.LineType = StraightLineTypes.Vertical;
            }
            else if (PO1.Y == PO2.Y)
            {
                if (PO1.X < PO2.X)
                {
                    line.Location = new Point(PO1.X - 10, PO1.Y);
                    line.Size = new Size(PO2.X - PO1.X, 20);
                }
                else
                {
                    line.Location = new Point(PO2.X - 10, PO2.Y);
                    line.Size = new Size(PO1.X - PO2.X, 20);
                }

                line.LineType = StraightLineTypes.Horizontal;
            }

            drawingBoard.Controls.Add(line);
        }

        public static void DrawLine(PictureBox P1, PictureBox P2, Panel drawingBoard, DataGridView data)
        {
            //Graphics graphics = drawingBoard.CreateGraphics();
            //graphics.DrawLine(new Pen(Color.Blue, 2), PO1, PO2);

            StraighLineDraw(P1, P2, drawingBoard);

            data.Rows.Add();
            data[0, LineNum].Value = P1.Name;
            data[1, LineNum].Value = P2.Name;
            LineNum += 1;
        }

        //擦除
        private static Label RelationPicAndLab(PictureBox P, Panel drawingBoard)
        {
            Label rLab = new Label();

            foreach (Control ctl in drawingBoard.Controls)
            {
                if (ctl is Label && ctl.Text == P.Name)
                {
                    rLab = (Label)ctl;
                }
            }

            return rLab;
        }

        private static void ReMoveRelationLine(PictureBox P, Panel drawingBoard, DataGridView data)
        {
            for (int i = drawingBoard.Controls.Count - 1; i >= 0; i--)
            {
                if (drawingBoard.Controls[i] is StraightLine)
                {
                    StraightLine line = (StraightLine)drawingBoard.Controls[i];

                    if (line.startP == P.Name || line.endP == P.Name)
                    {
                        for (int j = 0; j < data.RowCount; j++)
                        {
                            if (data[0, j].Value.ToString() == line.startP && data[1, j].Value.ToString() == line.endP)
                            {
                                data.Rows.RemoveAt(j);
                            }
                        }

                        drawingBoard.Controls.Remove(drawingBoard.Controls[i]);
                        LineNum -= 1;
                    }
                }
            }
        }

        public static void Erase(PictureBox P, Panel drawingBoard, DataGridView dataL)
        {
            ReMoveRelationLine(P, drawingBoard, dataL);
            drawingBoard.Controls.Remove(P);
            drawingBoard.Controls.Remove(RelationPicAndLab(P, drawingBoard));
            UnKnowPointNum -= 1;
        }

        public static void Erase(PictureBox P, Panel drawingBoard, DataGridView dataP, DataGridView dataL)
        {
            for (int i = 0; i < dataP.RowCount; i++)
            {
                if (dataP[0, i].Value.ToString() == P.Name)
                {
                    dataP.Rows.RemoveAt(i);
                    break;
                }
            }

            ReMoveRelationLine(P, drawingBoard, dataL);
            drawingBoard.Controls.Remove(P);
            drawingBoard.Controls.Remove(RelationPicAndLab(P, drawingBoard));      
            KnowPointNum -= 1;
        }
    }
}
