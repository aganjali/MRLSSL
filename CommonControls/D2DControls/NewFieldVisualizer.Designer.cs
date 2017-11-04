namespace MRL.SSL.CommonControls.D2DControls
{
    partial class NewFieldVisualizer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addOurRobotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addOppRobotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.creatAllRobotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addOurRobotToolStripMenuItem,
            this.addOppRobotToolStripMenuItem,
            this.addBallToolStripMenuItem,
            this.creatAllRobotToolStripMenuItem});
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(169, 114);
            // 
            // addOurRobotToolStripMenuItem
            // 
            this.addOurRobotToolStripMenuItem.Name = "addOurRobotToolStripMenuItem";
            this.addOurRobotToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.addOurRobotToolStripMenuItem.Text = "AddBlueRobot";
            // 
            // addOppRobotToolStripMenuItem
            // 
            this.addOppRobotToolStripMenuItem.Name = "addOppRobotToolStripMenuItem";
            this.addOppRobotToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.addOppRobotToolStripMenuItem.Text = "AddYelowRobot";
            // 
            // addBallToolStripMenuItem
            // 
            this.addBallToolStripMenuItem.Name = "addBallToolStripMenuItem";
            this.addBallToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.addBallToolStripMenuItem.Text = "Move Ball to Here";
            // 
            // creatAllRobotToolStripMenuItem
            // 
            this.creatAllRobotToolStripMenuItem.Name = "creatAllRobotToolStripMenuItem";
            this.creatAllRobotToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.creatAllRobotToolStripMenuItem.Text = "Creat All Robot";
            // 
            // NewFieldVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "NewFieldVisualizer";
            this.Size = new System.Drawing.Size(291, 283);
            this.mainMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ContextMenuStrip mainMenuStrip;
        public System.Windows.Forms.ToolStripMenuItem addOurRobotToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem addOppRobotToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem addBallToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem creatAllRobotToolStripMenuItem;
    }
}
