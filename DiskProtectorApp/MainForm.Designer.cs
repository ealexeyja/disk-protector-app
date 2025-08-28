namespace DiskProtectorApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ListView listViewDrives;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button btnProtect;
        private System.Windows.Forms.Button btnUnprotect;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnHelp;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listViewDrives = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnProtect = new System.Windows.Forms.Button();
            this.btnUnprotect = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            // listViewDrives
            this.listViewDrives.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewDrives.FullRowSelect = true;
            this.listViewDrives.Location = new System.Drawing.Point(12, 12);
            this.listViewDrives.Name = "listViewDrives";
            this.listViewDrives.Size = new System.Drawing.Size(660, 300);
            this.listViewDrives.TabIndex = 0;
            this.listViewDrives.UseCompatibleStateImageBehavior = false;
            this.listViewDrives.View = System.Windows.Forms.View.Details;
            
            // columnHeader1
            this.columnHeader1.Text = "Unidad";
            this.columnHeader1.Width = 80;
            
            // columnHeader2
            this.columnHeader2.Text = "Etiqueta";
            this.columnHeader2.Width = 120;
            
            // columnHeader3
            this.columnHeader3.Text = "Tipo";
            this.columnHeader3.Width = 80;
            
            // columnHeader4
            this.columnHeader4.Text = "Tamaño";
            this.columnHeader4.Width = 100;
            
            // columnHeader5
            this.columnHeader5.Text = "Estado";
            this.columnHeader5.Width = 100;
            
            // btnProtect
            this.btnProtect.Location = new System.Drawing.Point(12, 325);
            this.btnProtect.Name = "btnProtect";
            this.btnProtect.Size = new System.Drawing.Size(150, 35);
            this.btnProtect.TabIndex = 1;
            this.btnProtect.Text = "🔒 Proteger";
            this.btnProtect.UseVisualStyleBackColor = true;
            this.btnProtect.Click += new System.EventHandler(this.btnProtect_Click);
            
            // btnUnprotect
            this.btnUnprotect.Location = new System.Drawing.Point(168, 325);
            this.btnUnprotect.Name = "btnUnprotect";
            this.btnUnprotect.Size = new System.Drawing.Size(150, 35);
            this.btnUnprotect.TabIndex = 2;
            this.btnUnprotect.Text = "🔓 Desproteger";
            this.btnUnprotect.UseVisualStyleBackColor = true;
            this.btnUnprotect.Click += new System.EventHandler(this.btnUnprotect_Click);
            
            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(324, 325);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(150, 35);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "🔄 Actualizar";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // btnHelp
            this.btnHelp.Location = new System.Drawing.Point(480, 325);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(150, 35);
            this.btnHelp.TabIndex = 4;
            this.btnHelp.Text = "❓ Ayuda";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            
            // MainForm
            this.ClientSize = new System.Drawing.Size(684, 371);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnUnprotect);
            this.Controls.Add(this.btnProtect);
            this.Controls.Add(this.listViewDrives);
            this.Name = "MainForm";
            this.Text = "Protector de Discos Internos";
            this.ResumeLayout(false);
        }
    }
}
