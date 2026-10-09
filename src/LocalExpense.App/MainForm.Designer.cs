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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            filterPanel = new FlowLayoutPanel();
            fromLabel = new Label();
            fromPicker = new DateTimePicker();
            toLabel = new Label();
            toPicker = new DateTimePicker();
            categoryFilterLabel = new Label();
            categoryFilter = new ComboBox();
            clearFilterButton = new Button();
            errorProvider = new ErrorProvider(components);
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).BeginInit();
            toolbarPanel.SuspendLayout();
            filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
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
            transactionsGrid.TabIndex = 2;
            transactionsGrid.CellDoubleClick += transactionsGrid_CellDoubleClick;
            transactionsGrid.DataError += transactionsGrid_DataError;
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
            resources.ApplyResources(addButton, "addButton");
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
            resources.ApplyResources(editButton, "editButton");
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
            resources.ApplyResources(deleteButton, "deleteButton");
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += deleteButton_Click;
            //
            // filterPanel
            //
            filterPanel.AutoSize = true;
            filterPanel.Controls.Add(fromLabel);
            filterPanel.Controls.Add(fromPicker);
            filterPanel.Controls.Add(toLabel);
            filterPanel.Controls.Add(toPicker);
            filterPanel.Controls.Add(categoryFilterLabel);
            filterPanel.Controls.Add(categoryFilter);
            filterPanel.Controls.Add(clearFilterButton);
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Location = new Point(0, 38);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(6, 0, 6, 3);
            filterPanel.Size = new Size(800, 35);
            filterPanel.TabIndex = 1;
            //
            // fromLabel
            //
            fromLabel.Anchor = AnchorStyles.Left;
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(9, 9);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(38, 15);
            fromLabel.TabIndex = 0;
            resources.ApplyResources(fromLabel, "fromLabel");
            //
            // fromPicker
            //
            fromPicker.Checked = false;
            fromPicker.Format = DateTimePickerFormat.Short;
            fromPicker.Location = new Point(53, 3);
            fromPicker.Name = "fromPicker";
            fromPicker.ShowCheckBox = true;
            fromPicker.Size = new Size(125, 23);
            fromPicker.TabIndex = 1;
            fromPicker.ValueChanged += fromPicker_ValueChanged;
            //
            // toLabel
            //
            toLabel.Anchor = AnchorStyles.Left;
            toLabel.AutoSize = true;
            toLabel.Location = new Point(184, 9);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(23, 15);
            toLabel.TabIndex = 2;
            resources.ApplyResources(toLabel, "toLabel");
            //
            // toPicker
            //
            toPicker.Checked = false;
            toPicker.Format = DateTimePickerFormat.Short;
            toPicker.Location = new Point(213, 3);
            toPicker.Margin = new Padding(3, 3, 22, 3);
            toPicker.Name = "toPicker";
            toPicker.ShowCheckBox = true;
            toPicker.Size = new Size(125, 23);
            toPicker.TabIndex = 3;
            toPicker.ValueChanged += toPicker_ValueChanged;
            //
            // categoryFilterLabel
            //
            categoryFilterLabel.Anchor = AnchorStyles.Left;
            categoryFilterLabel.AutoSize = true;
            categoryFilterLabel.Location = new Point(363, 9);
            categoryFilterLabel.Name = "categoryFilterLabel";
            categoryFilterLabel.Size = new Size(58, 15);
            categoryFilterLabel.TabIndex = 4;
            resources.ApplyResources(categoryFilterLabel, "categoryFilterLabel");
            //
            // categoryFilter
            //
            categoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryFilter.FormattingEnabled = true;
            categoryFilter.Location = new Point(427, 3);
            categoryFilter.Name = "categoryFilter";
            categoryFilter.Size = new Size(150, 23);
            categoryFilter.TabIndex = 5;
            categoryFilter.SelectedIndexChanged += categoryFilter_SelectedIndexChanged;
            //
            // clearFilterButton
            //
            clearFilterButton.AutoSize = true;
            clearFilterButton.Location = new Point(583, 3);
            clearFilterButton.Name = "clearFilterButton";
            clearFilterButton.Size = new Size(75, 25);
            clearFilterButton.TabIndex = 6;
            resources.ApplyResources(clearFilterButton, "clearFilterButton");
            clearFilterButton.UseVisualStyleBackColor = true;
            clearFilterButton.Click += clearFilterButton_Click;
            //
            // errorProvider
            //
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
            //
            // idColumn
            // 
            idColumn.DataPropertyName = "Id";
            resources.ApplyResources(idColumn, "idColumn");
            idColumn.Name = "idColumn";
            idColumn.ReadOnly = true;
            idColumn.Visible = false;
            // 
            // dateColumn
            // 
            dateColumn.DataPropertyName = "Date";
            dataGridViewCellStyle1.Format = "yyyy-MM-dd";
            dateColumn.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(dateColumn, "dateColumn");
            dateColumn.Name = "dateColumn";
            dateColumn.ReadOnly = true;
            // 
            // categoryColumn
            // 
            categoryColumn.DataPropertyName = "Category";
            resources.ApplyResources(categoryColumn, "categoryColumn");
            categoryColumn.Name = "categoryColumn";
            categoryColumn.ReadOnly = true;
            categoryColumn.Width = 150;
            // 
            // amountColumn
            // 
            amountColumn.DataPropertyName = "AmountMinor";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            amountColumn.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(amountColumn, "amountColumn");
            amountColumn.Name = "amountColumn";
            amountColumn.ReadOnly = true;
            amountColumn.Width = 120;
            // 
            // noteColumn
            // 
            noteColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            noteColumn.DataPropertyName = "Note";
            resources.ApplyResources(noteColumn, "noteColumn");
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
            Controls.Add(filterPanel);
            Controls.Add(toolbarPanel);
            Name = "MainForm";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)transactionsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)transactionBindingSource).EndInit();
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
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
        private FlowLayoutPanel filterPanel;
        private Label fromLabel;
        private DateTimePicker fromPicker;
        private Label toLabel;
        private DateTimePicker toPicker;
        private Label categoryFilterLabel;
        private ComboBox categoryFilter;
        private Button clearFilterButton;
        private ErrorProvider errorProvider;
    }
}