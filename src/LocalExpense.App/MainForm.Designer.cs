namespace LocalExpense
{
    partial class MainForm
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            idColumn = new DataGridViewTextBoxColumn();
            dateColumn = new DataGridViewTextBoxColumn();
            categoryColumn = new DataGridViewTextBoxColumn();
            amountColumn = new DataGridViewTextBoxColumn();
            noteColumn = new DataGridViewTextBoxColumn();
            transactionBindingSource = new BindingSource(components);
            transactionsGrid = new DataGridView();
            toolbarPanel = new FlowLayoutPanel();
            addButton = new Button();
            editButton = new Button();
            deleteButton = new Button();
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).BeginInit();
            toolbarPanel.SuspendLayout();
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
            transactionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            transactionsGrid.Size = new Size(800, 450);
            transactionsGrid.TabIndex = 3;
            transactionsGrid.CellDoubleClick += transactionsGrid_CellDoubleClick;
            transactionsGrid.SelectionChanged += transactionsGrid_SelectionChanged;
            //
            // toolbarPanel
            //
            toolbarPanel.AutoSize = true;
            toolbarPanel.Controls.Add(addButton);
            toolbarPanel.Controls.Add(editButton);
            toolbarPanel.Controls.Add(deleteButton);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Location = new Point(0, 0);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.Padding = new Padding(6, 6, 6, 3);
            toolbarPanel.Size = new Size(800, 38);
            toolbarPanel.TabIndex = 0;
            //
            // addButton
            //
            addButton.AutoSize = true;
            addButton.Location = new Point(9, 9);
            addButton.Name = "addButton";
            addButton.Size = new Size(110, 26);
            addButton.TabIndex = 0;
            addButton.Text = "&Add transaction";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += addButton_Click;
            //
            // editButton
            //
            editButton.Enabled = false;
            editButton.Location = new Point(125, 9);
            editButton.Name = "editButton";
            editButton.Size = new Size(75, 26);
            editButton.TabIndex = 1;
            editButton.Text = "&Edit";
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += editButton_Click;
            //
            // deleteButton
            //
            deleteButton.Enabled = false;
            deleteButton.Location = new Point(206, 9);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(75, 26);
            deleteButton.TabIndex = 2;
            deleteButton.Text = "&Delete";
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += deleteButton_Click;
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
            // transactionBindingSource
            // 
            transactionBindingSource.DataSource = typeof(Models.Transaction);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(transactionsGrid);
            Controls.Add(toolbarPanel);
            Name = "MainForm";
            Text = "Local Expenses";
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).EndInit();
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView transactionsGrid;
        private BindingSource transactionBindingSource;
        private DataGridViewTextBoxColumn idColumn;
        private DataGridViewTextBoxColumn dateColumn;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn amountColumn;
        private DataGridViewTextBoxColumn noteColumn;
        private FlowLayoutPanel toolbarPanel;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
    }
}