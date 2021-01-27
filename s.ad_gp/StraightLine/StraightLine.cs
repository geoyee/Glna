using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace LineControl
{
    /// <summary>
    /// 直线类。继承连基类
    /// </summary>
    public class StraightLine : LineBase
    {
        public string startP;
        public string endP;
        public double Lenght;

        private StraightLineTypes lineType;

        public StraightLine(): base()
        {
            lineType = StraightLineTypes.Horizontal;
        }

        [Category("Line Properties"),
         DefaultValue(typeof(StraightLineTypes), "StraightLineTypes.Horizontal")]
        public StraightLineTypes LineType
        {
            get
            {
                return lineType;
            }
            set
            {
                lineType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 重写画线部分GDI实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void LineBase_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            pen = new Pen(ForeColor, Thickness);

            if (AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            switch (lineType)
            {
                case StraightLineTypes.Horizontal:
                    DrawCenteredHorizontalLine(e.Graphics);
                    break;
                case StraightLineTypes.Vertical:
                    DrawCenteredVerticalLine(e.Graphics);
                    break;
                case StraightLineTypes.DiagonalAscending:
                    DrawCenteredDiagonalAscendingLine(e.Graphics);
                    break;
                case StraightLineTypes.DiagonalDescending:
                    DrawCenteredDiagonalDescendingLine(e.Graphics);
                    break;
                default: break;
            }
        }

        #region 画线函数

        private void DrawCenteredHorizontalLine(Graphics g)
        {
            g.DrawLine(pen, 0, Height / 2, Width, Height / 2);
            Graphics graphics = this.CreateGraphics();
            Point[] mypoints = { new Point(0, Height / 2 + 1), new Point(0, Height / 2 - 1), new Point(Width, Height / 2 - 1), new Point(Width, Height / 2 + 1) };
            RegionControl(mypoints);
        }

        private void DrawCenteredVerticalLine(Graphics g)
        {
            g.DrawLine(pen, Width / 2, 0, Width / 2, Height);
            Point[] mypoints = { new Point(Width / 2 - 1, 0), new Point(Width / 2 + 1, 0), new Point(Width / 2 + 1, Height), new Point(Width / 2 - 1, Height) };
            RegionControl(mypoints);
        }

        private void DrawCenteredDiagonalAscendingLine(Graphics g)
        {
            g.DrawLine(pen, 0, Height, Width, 0);
            Point[] mypoints = { new Point(1, Height), new Point(0, Height - 1), new Point(Width - 1, 0), new Point(Width, 1) };
            RegionControl(mypoints);
        }

        private void DrawCenteredDiagonalDescendingLine(Graphics g)
        {
            g.DrawLine(pen, 0, 0, Width, Height);
            Point[] mypoints = { new Point(0, 1), new Point(1, 0), new Point(Width, Height - 1), new Point(Width - 1, Height) };
            RegionControl(mypoints);
        } 
        
        /// <summary>
        /// 按照点集合生成异形窗体
        /// </summary>
        /// <param name="points"></param>
        private void RegionControl(Point[] points)
        {
            GraphicsPath mygraphicsPath = new GraphicsPath();
            mygraphicsPath.AddPolygon(points);
            Region myregion = new Region(mygraphicsPath);
            this.Region = myregion;
        }
        #endregion
    } 

}
