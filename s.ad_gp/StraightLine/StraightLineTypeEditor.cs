using System.Runtime.InteropServices;
using System.ComponentModel;
using System;
using System.ComponentModel.Design;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel;
using System.Windows.Forms.Design;


namespace LineControl
{
    [Editor("StraightLineTypeEditor", typeof(UITypeEditor))]
    public enum StraightLineTypes
    {
        //Auto, 
        Horizontal,
        Vertical,
        DiagonalDescending,
        DiagonalAscending,
    }

    public class StraightLineTypeEditor : UITypeEditor
    {
        private StraightLineTypeUI lineTypeUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object returnValue = value;
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    if (lineTypeUI == null)
                    {
                        lineTypeUI = new StraightLineTypeUI(this);
                    }
                    lineTypeUI.Start(edSvc, value);
                    edSvc.DropDownControl(lineTypeUI);
                    value = lineTypeUI.Value;
                    lineTypeUI.End();
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class StraightLineTypeUI : Control
        {
            private IWindowsFormsEditorService edSvc;
            private StraightLineTypeEditor editor = null;
            private StraightLineTypes oldLineType;
            private object value;

            public StraightLineTypeUI(StraightLineTypeEditor editor)
            {
                this.editor = editor;
                InitializeComponent();
            }

            public object Value
            {
                get
                {
                    return value;
                }
            }
            
            public void End()
            {
                value = null;
                edSvc = null;
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                this.oldLineType = (StraightLineTypes)value;

                Panel panel;
                foreach (Control c in Controls)
                {
                    panel = c as Panel;
                    if (c != null)
                    {
                        c.BackColor = SystemColors.Control;
                        c.ForeColor = SystemColors.ControlText;
                    }
                    panel = null;
                }

                switch ((StraightLineTypes)value)
                {
                    case StraightLineTypes.Horizontal:
                        this.horizontalPanel.BackColor = SystemColors.ControlText;
                        this.horizontalPanel.ForeColor = SystemColors.Control;
                        break;
                    case StraightLineTypes.Vertical:
                        this.verticalPanel.BackColor = SystemColors.ControlText;
                        this.verticalPanel.ForeColor = SystemColors.Control;
                        break;
                    case StraightLineTypes.DiagonalAscending:
                        this.diagonalAscendingPanel.BackColor = SystemColors.ControlText;
                        this.diagonalAscendingPanel.ForeColor = SystemColors.Control;
                        break;
                    case StraightLineTypes.DiagonalDescending:
                        this.diagonalDescendingPanel.BackColor = SystemColors.ControlText;
                        this.diagonalDescendingPanel.ForeColor = SystemColors.Control;
                        break;
                    default: break;
                }
            }

            private void Teardown(bool save)
            {
                if (!save)
                {
                    value = oldLineType;
                }
                edSvc.CloseDropDown();
            }

            #region Component Designer generated code

            private System.Windows.Forms.Panel horizontalPanel;
            private System.Windows.Forms.Panel verticalPanel;
            private System.Windows.Forms.Panel diagonalDescendingPanel;
            private System.Windows.Forms.Panel diagonalAscendingPanel;
            private StraightLine horizontalLine;
            private StraightLine verticalLine;
            private StraightLine diagonalDescendingLine;
            private StraightLine diagonalAscendingLine;

            internal virtual void InitializeComponent()
            {
                this.horizontalPanel = new System.Windows.Forms.Panel();
                this.verticalPanel = new System.Windows.Forms.Panel();
                this.diagonalDescendingPanel = new System.Windows.Forms.Panel();
                this.diagonalAscendingPanel = new System.Windows.Forms.Panel();
                this.horizontalLine = new StraightLine();
                this.verticalLine = new StraightLine();
                this.diagonalDescendingLine = new StraightLine();
                this.diagonalAscendingLine = new StraightLine();
                this.horizontalPanel.SuspendLayout();
                this.verticalPanel.SuspendLayout();
                this.diagonalDescendingPanel.SuspendLayout();
                this.diagonalAscendingPanel.SuspendLayout();
                this.SuspendLayout();
                //  
                // horizontalPanel 
                //  
                this.horizontalPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.horizontalPanel.Controls.AddRange(new System.Windows.Forms.Control[] { 
																					 this.horizontalLine});
                this.horizontalPanel.Location = new System.Drawing.Point(2, 2);
                this.horizontalPanel.Name = "horizontalPanel";
                this.horizontalPanel.Size = new System.Drawing.Size(40, 40);
                this.horizontalPanel.TabIndex = 0;
                //  
                // verticalPanel 
                //  
                this.verticalPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.verticalPanel.Controls.AddRange(new System.Windows.Forms.Control[] { 
																					 this.verticalLine});
                this.verticalPanel.Location = new System.Drawing.Point(44, 2);
                this.verticalPanel.Name = "verticalPanel";
                this.verticalPanel.Size = new System.Drawing.Size(40, 40);
                this.verticalPanel.TabIndex = 1;
                //  
                // diagonalDescendingPanel 
                //  
                this.diagonalDescendingPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.diagonalDescendingPanel.Controls.AddRange(new System.Windows.Forms.Control[] { 
																					 this.diagonalDescendingLine});
                this.diagonalDescendingPanel.Location = new System.Drawing.Point(86, 2);
                this.diagonalDescendingPanel.Name = "diagonalDescendingPanel";
                this.diagonalDescendingPanel.Size = new System.Drawing.Size(40, 40);
                this.diagonalDescendingPanel.TabIndex = 2;
                //  
                // diagonalAscendingPanel 
                //  
                this.diagonalAscendingPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.diagonalAscendingPanel.Controls.AddRange(new System.Windows.Forms.Control[] { 
																					 this.diagonalAscendingLine});
                this.diagonalAscendingPanel.Location = new System.Drawing.Point(128, 2);
                this.diagonalAscendingPanel.Name = "diagonalAscendingPanel";
                this.diagonalAscendingPanel.Size = new System.Drawing.Size(40, 40);
                this.diagonalAscendingPanel.TabIndex = 3;
                //  
                // horizontalLine 
                //  
                this.horizontalLine.Dock = System.Windows.Forms.DockStyle.Fill;
                this.horizontalLine.LineType = StraightLineTypes.Horizontal;
                this.horizontalLine.Name = "horizontalLine";
                this.horizontalLine.Size = new System.Drawing.Size(38, 38);
                this.horizontalLine.TabIndex = 0;
                this.horizontalLine.Text = "straightLine1";
                this.horizontalLine.Click += new EventHandler(this.button_Click);
                //  
                // verticalLine 
                //  
                this.verticalLine.Dock = System.Windows.Forms.DockStyle.Fill;
                this.verticalLine.LineType = StraightLineTypes.Vertical;
                this.verticalLine.Name = "verticalLine";
                this.verticalLine.Size = new System.Drawing.Size(38, 38);
                this.verticalLine.TabIndex = 0;
                this.verticalLine.Text = "straightLine2";
                this.verticalLine.Click += new EventHandler(this.button_Click);
                //  
                // diagonalDescendingLine 
                //  
                this.diagonalDescendingLine.Dock = System.Windows.Forms.DockStyle.Fill;
                this.diagonalDescendingLine.LineType = StraightLineTypes.DiagonalDescending;
                this.diagonalDescendingLine.Name = "diagonalDescendingLine";
                this.diagonalDescendingLine.Size = new System.Drawing.Size(38, 38);
                this.diagonalDescendingLine.TabIndex = 0;
                this.diagonalDescendingLine.Text = "straightLine3";
                this.diagonalDescendingLine.Click += new EventHandler(this.button_Click);
                //  
                // diagonalAscendingLine 
                //  
                this.diagonalAscendingLine.Dock = System.Windows.Forms.DockStyle.Fill;
                this.diagonalAscendingLine.LineType = StraightLineTypes.DiagonalAscending;
                this.diagonalAscendingLine.Name = "diagonalAscendingLine";
                this.diagonalAscendingLine.Size = new System.Drawing.Size(38, 38);
                this.diagonalAscendingLine.TabIndex = 0;
                this.diagonalAscendingLine.Text = "straightLine4";
                this.diagonalAscendingLine.Click += new EventHandler(this.button_Click);
                //  
                // UserControl1 
                //  
                this.Controls.AddRange(new System.Windows.Forms.Control[] { 
																			  this.diagonalAscendingPanel, 
																			  this.diagonalDescendingPanel, 
																			  this.verticalPanel, 
																			  this.horizontalPanel});
                this.Name = "UserControl1";
                this.Size = new System.Drawing.Size(164, 44);
                this.BackColor = System.Drawing.SystemColors.InactiveBorder;
                this.horizontalPanel.ResumeLayout(false);
                this.verticalPanel.ResumeLayout(false);
                this.diagonalDescendingPanel.ResumeLayout(false);
                this.diagonalAscendingPanel.ResumeLayout(false);
                this.ResumeLayout(false);
            }
            #endregion

            private void button_Click(object sender, System.EventArgs e)
            {
                StraightLine line = sender as StraightLine;
                if (line != null)
                {
                    this.value = line.LineType;
                    Teardown(true);
                }
            }
        }
    } 
 
}
