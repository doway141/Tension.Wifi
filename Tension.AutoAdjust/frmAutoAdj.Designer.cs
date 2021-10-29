
namespace Tension.AutoAdjust
{
    partial class frmAutoAdj
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrScan = new System.Windows.Forms.Timer(this.components);
            this.lbGroupId = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lbTypeNo = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lbActCurL = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ltbStsL = new System.Windows.Forms.ListBox();
            this.lbAverL = new System.Windows.Forms.Label();
            this.lbStsL = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnStartL = new System.Windows.Forms.Button();
            this.lbActL = new System.Windows.Forms.Label();
            this.tbTargetL = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbGroupId
            // 
            this.lbGroupId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbGroupId.Location = new System.Drawing.Point(309, 4);
            this.lbGroupId.Name = "lbGroupId";
            this.lbGroupId.Size = new System.Drawing.Size(114, 22);
            this.lbGroupId.TabIndex = 23;
            this.lbGroupId.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(256, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 12);
            this.label11.TabIndex = 22;
            this.label11.Text = "GroupId";
            // 
            // lbTypeNo
            // 
            this.lbTypeNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbTypeNo.Location = new System.Drawing.Point(71, 4);
            this.lbTypeNo.Name = "lbTypeNo";
            this.lbTypeNo.Size = new System.Drawing.Size(165, 22);
            this.lbTypeNo.TabIndex = 21;
            this.lbTypeNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 20;
            this.label8.Text = "当前型号";
            // 
            // lbActCurL
            // 
            this.lbActCurL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbActCurL.Location = new System.Drawing.Point(77, 119);
            this.lbActCurL.Name = "lbActCurL";
            this.lbActCurL.Size = new System.Drawing.Size(59, 22);
            this.lbActCurL.TabIndex = 3;
            this.lbActCurL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "当前电流";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "目标值";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbTargetL);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.ltbStsL);
            this.groupBox1.Controls.Add(this.lbAverL);
            this.groupBox1.Controls.Add(this.lbStsL);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnStartL);
            this.groupBox1.Controls.Add(this.lbActL);
            this.groupBox1.Controls.Add(this.lbActCurL);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(14, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(547, 194);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "张力";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 17;
            this.label5.Text = "平均张力";
            // 
            // ltbStsL
            // 
            this.ltbStsL.FormattingEnabled = true;
            this.ltbStsL.ItemHeight = 12;
            this.ltbStsL.Location = new System.Drawing.Point(163, 22);
            this.ltbStsL.Name = "ltbStsL";
            this.ltbStsL.Size = new System.Drawing.Size(373, 160);
            this.ltbStsL.TabIndex = 16;
            // 
            // lbAverL
            // 
            this.lbAverL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbAverL.Location = new System.Drawing.Point(77, 86);
            this.lbAverL.Name = "lbAverL";
            this.lbAverL.Size = new System.Drawing.Size(59, 22);
            this.lbAverL.TabIndex = 15;
            this.lbAverL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStsL
            // 
            this.lbStsL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbStsL.Location = new System.Drawing.Point(11, 158);
            this.lbStsL.Name = "lbStsL";
            this.lbStsL.Size = new System.Drawing.Size(59, 22);
            this.lbStsL.TabIndex = 13;
            this.lbStsL.Text = "Start";
            this.lbStsL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "当前张力";
            // 
            // btnStartL
            // 
            this.btnStartL.Location = new System.Drawing.Point(78, 152);
            this.btnStartL.Name = "btnStartL";
            this.btnStartL.Size = new System.Drawing.Size(58, 30);
            this.btnStartL.TabIndex = 8;
            this.btnStartL.Text = "启动";
            this.btnStartL.UseVisualStyleBackColor = true;
            this.btnStartL.Click += new System.EventHandler(this.btnStartL_Click);
            // 
            // lbActL
            // 
            this.lbActL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbActL.Location = new System.Drawing.Point(77, 58);
            this.lbActL.Name = "lbActL";
            this.lbActL.Size = new System.Drawing.Size(59, 22);
            this.lbActL.TabIndex = 9;
            this.lbActL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbTargetL
            // 
            this.tbTargetL.Location = new System.Drawing.Point(77, 28);
            this.tbTargetL.Name = "tbTargetL";
            this.tbTargetL.Size = new System.Drawing.Size(59, 21);
            this.tbTargetL.TabIndex = 24;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 235);
            this.Controls.Add(this.lbGroupId);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lbTypeNo);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "frmMain";
            this.Text = "张力自动调整系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tmrScan;
        private System.Windows.Forms.Label lbGroupId;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lbTypeNo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbActCurL;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox ltbStsL;
        private System.Windows.Forms.Label lbAverL;
        private System.Windows.Forms.Label lbStsL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnStartL;
        private System.Windows.Forms.Label lbActL;
        private System.Windows.Forms.TextBox tbTargetL;
    }
}

