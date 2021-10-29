
namespace Tension.LineLength
{
    partial class xFrmLineLength
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.gdLineLen = new DevExpress.XtraGrid.GridControl();
            this.gvLineLen = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gcLineName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcDeviceName = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcLeftCnt = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRightCnt = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gdLineLen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLineLen)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.gdLineLen);
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(629, 333);
            this.groupControl1.TabIndex = 3;
            this.groupControl1.Text = "线长采集信息统计";
            // 
            // gdLineLen
            // 
            this.gdLineLen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gdLineLen.Location = new System.Drawing.Point(2, 23);
            this.gdLineLen.MainView = this.gvLineLen;
            this.gdLineLen.Name = "gdLineLen";
            this.gdLineLen.Size = new System.Drawing.Size(625, 308);
            this.gdLineLen.TabIndex = 0;
            this.gdLineLen.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvLineLen});
            // 
            // gvLineLen
            // 
            this.gvLineLen.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gcLineName,
            this.gcDeviceName,
            this.gcLeftCnt,
            this.gcRightCnt});
            this.gvLineLen.GridControl = this.gdLineLen;
            this.gvLineLen.Name = "gvLineLen";
            // 
            // gcLineName
            // 
            this.gcLineName.Caption = "线体名字";
            this.gcLineName.FieldName = "LineName";
            this.gcLineName.Name = "gcLineName";
            this.gcLineName.Visible = true;
            this.gcLineName.VisibleIndex = 0;
            // 
            // gcDeviceName
            // 
            this.gcDeviceName.Caption = "设备名字";
            this.gcDeviceName.FieldName = "DeviceName";
            this.gcDeviceName.Name = "gcDeviceName";
            this.gcDeviceName.Visible = true;
            this.gcDeviceName.VisibleIndex = 1;
            // 
            // gcLeftCnt
            // 
            this.gcLeftCnt.Caption = "左边计数";
            this.gcLeftCnt.FieldName = "LeftCnt";
            this.gcLeftCnt.Name = "gcLeftCnt";
            this.gcLeftCnt.Visible = true;
            this.gcLeftCnt.VisibleIndex = 2;
            // 
            // gcRightCnt
            // 
            this.gcRightCnt.Caption = "右边计数";
            this.gcRightCnt.FieldName = "RightCnt";
            this.gcRightCnt.Name = "gcRightCnt";
            this.gcRightCnt.Visible = true;
            this.gcRightCnt.VisibleIndex = 3;
            // 
            // xFrmLineLength
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 359);
            this.Controls.Add(this.groupControl1);
            this.Name = "xFrmLineLength";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.xFrmLineLength_FormClosing);
            this.Load += new System.EventHandler(this.xFrmLineLength_Load);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gdLineLen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvLineLen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraGrid.GridControl gdLineLen;
        private DevExpress.XtraGrid.Views.Grid.GridView gvLineLen;
        private DevExpress.XtraGrid.Columns.GridColumn gcLineName;
        private DevExpress.XtraGrid.Columns.GridColumn gcDeviceName;
        private DevExpress.XtraGrid.Columns.GridColumn gcLeftCnt;
        private DevExpress.XtraGrid.Columns.GridColumn gcRightCnt;
    }
}

