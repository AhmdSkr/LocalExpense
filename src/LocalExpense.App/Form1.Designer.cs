namespace LocalExpense
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            DataGridView transactionsGrid;
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            transactionBindingSource = new BindingSource(components);
            idColumn = new DataGridViewTextBoxColumn();
            dateColumn = new DataGridViewTextBoxColumn();
            categoryColumn = new DataGridViewTextBoxColumn();
            amountColumn = new DataGridViewTextBoxColumn();
            noteColumn = new DataGridViewTextBoxColumn();
            transactionsGrid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).BeginInit();
            SuspendLayout();
            // 
            // transactionsGrid
            // 
            transactionsGrid.AllowUserToAddRows = false;
            transactionsGrid.AllowUserToDeleteRows = false;
            transactionsGrid.AutoGenerateColumns = false;
            transactionsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            transactionsGrid.Columns.AddRange(new DataGridViewColumn[] { idColumn, dateColumn, categoryColumn, amountColumn, noteColumn });
            transactionsGrid.DataSource = transactionBindingSource;
            transactionsGrid.Dock = DockStyle.Fill;
            transactionsGrid.Location = new Point(0, 0);
            transactionsGrid.Name = "transactionsGrid";
            transactionsGrid.ReadOnly = true;
            transactionsGrid.Size = new Size(800, 450);
            transactionsGrid.TabIndex = 0;
            // 
            // transactionBindingSource
            // 
            transactionBindingSource.DataSource = typeof(Models.Transaction);
            // 
            // idColumn
            // 
            idColumn.DataPropertyName = "Id";
            idColumn.HeaderText = "Id";
            idColumn.Name = "idColumn";
            idColumn.ReadOnly = true;
            idColumn.Visible = false;
            // 
            // dateColumn
            // 
            dateColumn.DataPropertyName = "Date";
            dataGridViewCellStyle1.Format = "yyyy-MM-dd";
            dateColumn.DefaultCellStyle = dataGridViewCellStyle1;
            dateColumn.HeaderText = "Date";
            dateColumn.Name = "dateColumn";
            dateColumn.ReadOnly = true;
            // 
            // categoryColumn
            // 
            categoryColumn.DataPropertyName = "Category";
            categoryColumn.HeaderText = "Category";
            categoryColumn.Name = "categoryColumn";
            categoryColumn.ReadOnly = true;
            categoryColumn.Width = 150;
            // 
            // amountColumn
            // 
            amountColumn.DataPropertyName = "AmountMinor";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            amountColumn.DefaultCellStyle = dataGridViewCellStyle2;
            amountColumn.HeaderText = "Amount";
            amountColumn.Name = "amountColumn";
            amountColumn.ReadOnly = true;
            amountColumn.Width = 120;
            // 
            // noteColumn
            // 
            noteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            noteColumn.DataPropertyName = "Note";
            noteColumn.HeaderText = "Note";
            noteColumn.Name = "noteColumn";
            noteColumn.ReadOnly = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(transactionsGrid);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView transactionsGrid;
        private BindingSource transactionBindingSource;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewTextBoxColumn dateColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn amountColumn;
        private DataGridViewTextBoxColumn noteColumn;
    }
}