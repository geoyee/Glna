using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LineControl
{
    /// <summary>
    /// 画线基类。可以延伸出半圆线等不规则线。暂时就延伸一个直线类
    /// </summary>
    public abstract class LineBase : Control
    {
        private int thickness;
        private bool antiAlias;
        protected Pen pen;

        public LineBase()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);
            Thickness = 1;
            antiAlias = true;
            BackColor = Color.Transparent;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && pen != null)
            {
                pen.Dispose();
            }
            base.Dispose(disposing);
        }

        [
        Category("Line Properties"),
        DefaultValue(true)
        ]
        public bool AntiAlias
        {
            get
            {
                return antiAlias;
            }
            set
            {
                antiAlias = value;
                Invalidate();
            }
        }

        [
        Category("Line Properties"),
        DefaultValue(1)
        ]
        public int Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                thickness = value;
                Invalidate();
            }
        }

        #region Component Designer generated code
        ///   
        /// Required method for Designer support - do not modify  
        /// the contents of this method with the code editor. 
        ///  
        private void InitializeComponent()
        {
            //  
            // LineBase 
            //  
            this.Name = "LineBase";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LineBase_Paint);

        }
        #endregion

        protected abstract void LineBase_Paint(object sender, System.Windows.Forms.PaintEventArgs e);

    } 
}
