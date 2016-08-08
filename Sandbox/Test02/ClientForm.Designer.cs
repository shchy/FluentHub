namespace Sandbox.Test02
{
    partial class ClientForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rooms = new System.Windows.Forms.ListBox();
            this.makeRoom = new System.Windows.Forms.Button();
            this.logs = new System.Windows.Forms.ListBox();
            this.send = new System.Windows.Forms.Button();
            this.comment = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.rooms, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.makeRoom, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.logs, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.send, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.comment, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(505, 386);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // rooms
            // 
            this.rooms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rooms.FormattingEnabled = true;
            this.rooms.ItemHeight = 12;
            this.rooms.Location = new System.Drawing.Point(3, 3);
            this.rooms.Name = "rooms";
            this.rooms.Size = new System.Drawing.Size(174, 351);
            this.rooms.TabIndex = 0;
            // 
            // makeRoom
            // 
            this.makeRoom.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.makeRoom.Location = new System.Drawing.Point(52, 360);
            this.makeRoom.Name = "makeRoom";
            this.makeRoom.Size = new System.Drawing.Size(75, 23);
            this.makeRoom.TabIndex = 1;
            this.makeRoom.Text = "makeRoom";
            this.makeRoom.UseVisualStyleBackColor = true;
            this.makeRoom.Click += new System.EventHandler(this.makeRoom_Click);
            // 
            // logs
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.logs, 2);
            this.logs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logs.FormattingEnabled = true;
            this.logs.ItemHeight = 12;
            this.logs.Location = new System.Drawing.Point(183, 3);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(319, 351);
            this.logs.TabIndex = 2;
            // 
            // send
            // 
            this.send.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.send.Location = new System.Drawing.Point(427, 360);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(75, 23);
            this.send.TabIndex = 3;
            this.send.Text = "button2";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // comment
            // 
            this.comment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comment.Location = new System.Drawing.Point(183, 362);
            this.comment.Name = "comment";
            this.comment.Size = new System.Drawing.Size(238, 19);
            this.comment.TabIndex = 4;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 386);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ClientForm";
            this.Text = "ClientForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox rooms;
        private System.Windows.Forms.Button makeRoom;
        private System.Windows.Forms.ListBox logs;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.TextBox comment;
    }
}