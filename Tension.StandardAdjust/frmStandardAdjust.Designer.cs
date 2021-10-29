
namespace Tension.StandardAdjust
{
    partial class frmStandardAdjust
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
            this.gridTenAdj = new DevExpress.XtraGrid.GridControl();
            this.gvTenAdj = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gcNameDev = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcStsRightAdj = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcTargetTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcActTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcStsLeftAdj = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcReadCur = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcAverTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcLock = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcTypeNo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.grpTenAdj = new DevExpress.XtraEditors.GroupControl();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.gridRiTenAdj = new DevExpress.XtraGrid.GridControl();
            this.gvRiTenAdj = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gcRiNameTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRiNameDev = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRiValReadCur = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRiValAverTen = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gcRiTimeCreate = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridTenAdj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTenAdj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpTenAdj)).BeginInit();
            this.grpTenAdj.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRiTenAdj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRiTenAdj)).BeginInit();
            this.SuspendLayout();
            // 
            // gridTenAdj
            // 
            this.gridTenAdj.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTenAdj.Location = new System.Drawing.Point(2, 23);
            this.gridTenAdj.MainView = this.gvTenAdj;
            this.gridTenAdj.Name = "gridTenAdj";
            this.gridTenAdj.Size = new System.Drawing.Size(693, 160);
            this.gridTenAdj.TabIndex = 0;
            this.gridTenAdj.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvTenAdj});
            // 
            // gvTenAdj
            // 
            this.gvTenAdj.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gcNameDev,
            this.gcStsRightAdj,
            this.gcTargetTen,
            this.gcActTen,
            this.gcStsLeftAdj,
            this.gcReadCur,
            this.gcAverTen,
            this.gcLock,
            this.gcTypeNo});
            this.gvTenAdj.GridControl = this.gridTenAdj;
            this.gvTenAdj.Name = "gvTenAdj";
            // 
            // gcNameDev
            // 
            this.gcNameDev.Caption = "设备";
            this.gcNameDev.FieldName = "NameDev";
            this.gcNameDev.Name = "gcNameDev";
            this.gcNameDev.Visible = true;
            this.gcNameDev.VisibleIndex = 0;
            this.gcNameDev.Width = 103;
            // 
            // gcStsRightAdj
            // 
            this.gcStsRightAdj.Caption = "右调整";
            this.gcStsRightAdj.FieldName = "StsRightAdj";
            this.gcStsRightAdj.Name = "gcStsRightAdj";
            this.gcStsRightAdj.Visible = true;
            this.gcStsRightAdj.VisibleIndex = 3;
            this.gcStsRightAdj.Width = 51;
            // 
            // gcTargetTen
            // 
            this.gcTargetTen.Caption = "目标张力";
            this.gcTargetTen.DisplayFormat.FormatString = "\"0.00\"";
            this.gcTargetTen.FieldName = "ValTargetTen";
            this.gcTargetTen.Name = "gcTargetTen";
            this.gcTargetTen.Visible = true;
            this.gcTargetTen.VisibleIndex = 5;
            this.gcTargetTen.Width = 62;
            // 
            // gcActTen
            // 
            this.gcActTen.Caption = "实时张力";
            this.gcActTen.DisplayFormat.FormatString = "\"0.00\"";
            this.gcActTen.FieldName = "ValActTen";
            this.gcActTen.Name = "gcActTen";
            this.gcActTen.Visible = true;
            this.gcActTen.VisibleIndex = 7;
            this.gcActTen.Width = 60;
            // 
            // gcStsLeftAdj
            // 
            this.gcStsLeftAdj.Caption = "左调整";
            this.gcStsLeftAdj.FieldName = "StsLeftAdj";
            this.gcStsLeftAdj.Name = "gcStsLeftAdj";
            this.gcStsLeftAdj.Visible = true;
            this.gcStsLeftAdj.VisibleIndex = 2;
            this.gcStsLeftAdj.Width = 51;
            // 
            // gcReadCur
            // 
            this.gcReadCur.Caption = "当前电流";
            this.gcReadCur.DisplayFormat.FormatString = "\"0.0\"";
            this.gcReadCur.FieldName = "ValReadCur";
            this.gcReadCur.Name = "gcReadCur";
            this.gcReadCur.Visible = true;
            this.gcReadCur.VisibleIndex = 8;
            this.gcReadCur.Width = 60;
            // 
            // gcAverTen
            // 
            this.gcAverTen.Caption = "稳定张力";
            this.gcAverTen.DisplayFormat.FormatString = "\"0.00\"";
            this.gcAverTen.FieldName = "ValAverTen";
            this.gcAverTen.Name = "gcAverTen";
            this.gcAverTen.Visible = true;
            this.gcAverTen.VisibleIndex = 6;
            this.gcAverTen.Width = 60;
            // 
            // gcLock
            // 
            this.gcLock.Caption = "飞叉";
            this.gcLock.FieldName = "StsLock";
            this.gcLock.Name = "gcLock";
            this.gcLock.Visible = true;
            this.gcLock.VisibleIndex = 4;
            this.gcLock.Width = 50;
            // 
            // gcTypeNo
            // 
            this.gcTypeNo.Caption = "型号";
            this.gcTypeNo.FieldName = "TypeNo";
            this.gcTypeNo.Name = "gcTypeNo";
            this.gcTypeNo.Visible = true;
            this.gcTypeNo.VisibleIndex = 1;
            this.gcTypeNo.Width = 158;
            // 
            // grpTenAdj
            // 
            this.grpTenAdj.Controls.Add(this.gridTenAdj);
            this.grpTenAdj.Location = new System.Drawing.Point(12, 12);
            this.grpTenAdj.Name = "grpTenAdj";
            this.grpTenAdj.Size = new System.Drawing.Size(697, 185);
            this.grpTenAdj.TabIndex = 1;
            this.grpTenAdj.Text = "张力计调整状态信息";
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.gridRiTenAdj);
            this.groupControl1.Location = new System.Drawing.Point(12, 203);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(699, 265);
            this.groupControl1.TabIndex = 2;
            this.groupControl1.Text = "张力调整结果信息";
            // 
            // gridRiTenAdj
            // 
            this.gridRiTenAdj.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRiTenAdj.Location = new System.Drawing.Point(2, 23);
            this.gridRiTenAdj.MainView = this.gvRiTenAdj;
            this.gridRiTenAdj.Name = "gridRiTenAdj";
            this.gridRiTenAdj.Size = new System.Drawing.Size(695, 240);
            this.gridRiTenAdj.TabIndex = 0;
            this.gridRiTenAdj.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvRiTenAdj});
            // 
            // gvRiTenAdj
            // 
            this.gvRiTenAdj.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gcRiNameTen,
            this.gcRiNameDev,
            this.gcRiValReadCur,
            this.gcRiValAverTen,
            this.gcRiTimeCreate});
            this.gvRiTenAdj.GridControl = this.gridRiTenAdj;
            this.gvRiTenAdj.Name = "gvRiTenAdj";
            // 
            // gcRiNameTen
            // 
            this.gcRiNameTen.Caption = "张力计名字";
            this.gcRiNameTen.FieldName = "NameTen";
            this.gcRiNameTen.Name = "gcRiNameTen";
            this.gcRiNameTen.Visible = true;
            this.gcRiNameTen.VisibleIndex = 0;
            this.gcRiNameTen.Width = 100;
            // 
            // gcRiNameDev
            // 
            this.gcRiNameDev.Caption = "设备名字";
            this.gcRiNameDev.FieldName = "NameDev";
            this.gcRiNameDev.Name = "gcRiNameDev";
            this.gcRiNameDev.Visible = true;
            this.gcRiNameDev.VisibleIndex = 1;
            this.gcRiNameDev.Width = 100;
            // 
            // gcRiValReadCur
            // 
            this.gcRiValReadCur.Caption = "电流";
            this.gcRiValReadCur.FieldName = "ValReadCur";
            this.gcRiValReadCur.Name = "gcRiValReadCur";
            this.gcRiValReadCur.Visible = true;
            this.gcRiValReadCur.VisibleIndex = 3;
            this.gcRiValReadCur.Width = 45;
            // 
            // gcRiValAverTen
            // 
            this.gcRiValAverTen.Caption = "结果张力";
            this.gcRiValAverTen.DisplayFormat.FormatString = "\"0.00\"";
            this.gcRiValAverTen.FieldName = "ValResultTen";
            this.gcRiValAverTen.Name = "gcRiValAverTen";
            this.gcRiValAverTen.Visible = true;
            this.gcRiValAverTen.VisibleIndex = 2;
            this.gcRiValAverTen.Width = 46;
            // 
            // gcRiTimeCreate
            // 
            this.gcRiTimeCreate.Caption = "时间";
            this.gcRiTimeCreate.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            this.gcRiTimeCreate.FieldName = "TimeCreate";
            this.gcRiTimeCreate.Name = "gcRiTimeCreate";
            this.gcRiTimeCreate.Visible = true;
            this.gcRiTimeCreate.VisibleIndex = 4;
            this.gcRiTimeCreate.Width = 120;
            // 
            // frmStandardAdjust
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 479);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.grpTenAdj);
            this.Name = "frmStandardAdjust";
            this.Text = "张力自动调整系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmStandardAdjust_FormClosing);
            this.Load += new System.EventHandler(this.frmStandardAdjust_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridTenAdj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvTenAdj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpTenAdj)).EndInit();
            this.grpTenAdj.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridRiTenAdj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvRiTenAdj)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridTenAdj;
        private DevExpress.XtraGrid.Views.Grid.GridView gvTenAdj;
        private DevExpress.XtraEditors.GroupControl grpTenAdj;
        private DevExpress.XtraGrid.Columns.GridColumn gcTargetTen;
        private DevExpress.XtraGrid.Columns.GridColumn gcActTen;
        private DevExpress.XtraGrid.Columns.GridColumn gcStsLeftAdj;
        private DevExpress.XtraGrid.Columns.GridColumn gcReadCur;
        private DevExpress.XtraGrid.Columns.GridColumn gcAverTen;
        private DevExpress.XtraGrid.Columns.GridColumn gcNameDev;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraGrid.GridControl gridRiTenAdj;
        private DevExpress.XtraGrid.Views.Grid.GridView gvRiTenAdj;
        private DevExpress.XtraGrid.Columns.GridColumn gcRiNameDev;
        private DevExpress.XtraGrid.Columns.GridColumn gcRiValAverTen;
        private DevExpress.XtraGrid.Columns.GridColumn gcRiTimeCreate;
        private DevExpress.XtraGrid.Columns.GridColumn gcRiValReadCur;
        private DevExpress.XtraGrid.Columns.GridColumn gcLock;
        private DevExpress.XtraGrid.Columns.GridColumn gcRiNameTen;
        private DevExpress.XtraGrid.Columns.GridColumn gcStsRightAdj;
        private DevExpress.XtraGrid.Columns.GridColumn gcTypeNo;
    }
}

