using System.Linq.Expressions;

namespace MODBUS_Master;

partial class ModbusController
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
        COMPort_gb = new System.Windows.Forms.GroupBox();
        COMPort_Status = new System.Windows.Forms.PictureBox();
        COMPort_Baudrate = new System.Windows.Forms.ComboBox();
        COMPort_Comselect = new System.Windows.Forms.ComboBox();
        COMPort_Close = new System.Windows.Forms.Button();
        COMPort_Open = new System.Windows.Forms.Button();
        FunctionSelectcb = new System.Windows.Forms.ComboBox();
        Master_grid = new System.Windows.Forms.DataGridView();
        Slave_grid = new System.Windows.Forms.DataGridView();
        Function_gb = new System.Windows.Forms.GroupBox();
        FunctionSend_bt = new System.Windows.Forms.Button();
        Oper_Status = new System.Windows.Forms.Label();
        COMPort_gb.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)COMPort_Status).BeginInit();
        ((System.ComponentModel.ISupportInitialize)Master_grid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)Slave_grid).BeginInit();
        Function_gb.SuspendLayout();
        SuspendLayout();
        // 
        // COMPort_gb
        // 
        COMPort_gb.Controls.Add(COMPort_Status);
        COMPort_gb.Controls.Add(COMPort_Baudrate);
        COMPort_gb.Controls.Add(COMPort_Comselect);
        COMPort_gb.Controls.Add(COMPort_Close);
        COMPort_gb.Controls.Add(COMPort_Open);
        COMPort_gb.Location = new System.Drawing.Point(17, 17);
        COMPort_gb.Name = "COMPort_gb";
        COMPort_gb.Size = new System.Drawing.Size(266, 93);
        COMPort_gb.TabIndex = 0;
        COMPort_gb.TabStop = false;
        COMPort_gb.Text = "COM Port Manager";
        // 
        // COMPort_Status
        // 
        COMPort_Status.BackColor = System.Drawing.Color.Red;
        COMPort_Status.Location = new System.Drawing.Point(199, 26);
        COMPort_Status.Name = "COMPort_Status";
        COMPort_Status.Size = new System.Drawing.Size(54, 50);
        COMPort_Status.TabIndex = 4;
        COMPort_Status.TabStop = false;
        // 
        // COMPort_Baudrate
        // 
        COMPort_Baudrate.AllowDrop = true;
        COMPort_Baudrate.FormattingEnabled = true;
        COMPort_Baudrate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
        COMPort_Baudrate.Location = new System.Drawing.Point(9, 55);
        COMPort_Baudrate.Name = "COMPort_Baudrate";
        COMPort_Baudrate.Size = new System.Drawing.Size(100, 23);
        COMPort_Baudrate.TabIndex = 3;
        // 
        // COMPort_Comselect
        // 
        COMPort_Comselect.AllowDrop = true;
        COMPort_Comselect.FormattingEnabled = true;
        COMPort_Comselect.Location = new System.Drawing.Point(9, 26);
        COMPort_Comselect.Name = "COMPort_Comselect";
        COMPort_Comselect.Size = new System.Drawing.Size(100, 23);
        COMPort_Comselect.TabIndex = 2;
        COMPort_Comselect.DropDown += COMPort_Comselect_DropDown;
        COMPort_Comselect.SelectedIndexChanged += _auto_select_baudrate;
        // 
        // COMPort_Close
        // 
        COMPort_Close.Location = new System.Drawing.Point(115, 55);
        COMPort_Close.Name = "COMPort_Close";
        COMPort_Close.Size = new System.Drawing.Size(78, 23);
        COMPort_Close.TabIndex = 1;
        COMPort_Close.Text = "Close";
        COMPort_Close.UseVisualStyleBackColor = true;
        COMPort_Close.Click += COMPort_Close_Click;
        // 
        // COMPort_Open
        // 
        COMPort_Open.Location = new System.Drawing.Point(115, 26);
        COMPort_Open.Name = "COMPort_Open";
        COMPort_Open.Size = new System.Drawing.Size(78, 23);
        COMPort_Open.TabIndex = 0;
        COMPort_Open.Text = "Open";
        COMPort_Open.UseVisualStyleBackColor = true;
        COMPort_Open.Click += COMPort_Open_Click;
        // 
        // FunctionSelectcb
        // 
        FunctionSelectcb.FormattingEnabled = true;
        FunctionSelectcb.Location = new System.Drawing.Point(25, 27);
        FunctionSelectcb.Name = "FunctionSelectcb";
        FunctionSelectcb.Size = new System.Drawing.Size(210, 23);
        FunctionSelectcb.TabIndex = 5;
        FunctionSelectcb.SelectedIndexChanged += FunctionSelectcb_SelectedIndexChanged;
        // 
        // Master_grid
        // 
        Master_grid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
        Master_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        Master_grid.Location = new System.Drawing.Point(17, 116);
        Master_grid.Name = "Master_grid";
        Master_grid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
        Master_grid.Size = new System.Drawing.Size(266, 377);
        Master_grid.TabIndex = 1;
        Master_grid.Text = "Master Grid";
        Master_grid.CellValueChanged += Master_Grid_CellValueChanged;
        // 
        // Slave_grid
        // 
        Slave_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        Slave_grid.Location = new System.Drawing.Point(289, 17);
        Slave_grid.Name = "Slave_grid";
        Slave_grid.Size = new System.Drawing.Size(260, 348);
        Slave_grid.TabIndex = 2;
        Slave_grid.Text = "Slave Grid";
        // 
        // Function_gb
        // 
        Function_gb.Controls.Add(Oper_Status);
        Function_gb.Controls.Add(FunctionSend_bt);
        Function_gb.Controls.Add(FunctionSelectcb);
        Function_gb.Location = new System.Drawing.Point(289, 371);
        Function_gb.Name = "Function_gb";
        Function_gb.Size = new System.Drawing.Size(260, 122);
        Function_gb.TabIndex = 3;
        Function_gb.TabStop = false;
        Function_gb.Text = "Function Selector";
        // 
        // FunctionSend_bt
        // 
        FunctionSend_bt.Location = new System.Drawing.Point(25, 56);
        FunctionSend_bt.Name = "FunctionSend_bt";
        FunctionSend_bt.Size = new System.Drawing.Size(100, 40);
        FunctionSend_bt.TabIndex = 6;
        FunctionSend_bt.Text = "Send";
        FunctionSend_bt.UseVisualStyleBackColor = true;
        FunctionSend_bt.Click += Send_bt_Click;
        // 
        // Oper_Status
        // 
        Oper_Status.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)0));
        Oper_Status.Location = new System.Drawing.Point(135, 56);
        Oper_Status.Name = "Oper_Status";
        Oper_Status.Size = new System.Drawing.Size(100, 40);
        Oper_Status.TabIndex = 7;
        Oper_Status.Text = "";
        Oper_Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ModbusController
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(557, 509);
        Controls.Add(Function_gb);
        Controls.Add(Slave_grid);
        Controls.Add(Master_grid);
        Controls.Add(COMPort_gb);
        Text = "Master Interface";
        COMPort_gb.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)COMPort_Status).EndInit();
        ((System.ComponentModel.ISupportInitialize)Master_grid).EndInit();
        ((System.ComponentModel.ISupportInitialize)Slave_grid).EndInit();
        Function_gb.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Label Oper_Status;

    private System.Windows.Forms.GroupBox Function_gb;
    private System.Windows.Forms.Button FunctionSend_bt;

    private System.Windows.Forms.DataGridView Slave_grid;
    private System.Windows.Forms.ComboBox FunctionSelectcb;

    private System.Windows.Forms.DataGridView Master_grid;
    private System.Windows.Forms.PictureBox COMPort_Status;

    private System.Windows.Forms.GroupBox COMPort_gb;
    private System.Windows.Forms.Button COMPort_Open;
    private System.Windows.Forms.Button COMPort_Close;
    private System.Windows.Forms.ComboBox COMPort_Comselect;
    private System.Windows.Forms.ComboBox COMPort_Baudrate;

    #endregion
}