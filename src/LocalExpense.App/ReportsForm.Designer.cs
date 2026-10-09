namespace LocalExpense
{
    partial class ReportsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportsForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            categoryColumn = new DataGridViewTextBoxColumn();
            totalColumn = new DataGridViewTextBoxColumn();
            categoryGrid = new DataGridView();
            viewPanel = new Panel();
            linePlot = new ScottPlot.WinForms.FormsPlot();
            barPlot = new ScottPlot.WinForms.FormsPlot();
            layoutPanel = new TableLayoutPanel();
            selectorPanel = new FlowLayoutPanel();
            periodLabel = new Label();
            kindCombo = new ComboBox();
            yearLabel = new Label();
            yearCombo = new ComboBox();
            monthLabel = new Label();
            monthCombo = new ComboBox();
            viewLabel = new Label();
            viewCombo = new ComboBox();
            rangeLabel = new Label();
            summaryPanel = new TableLayoutPanel();
            incomeLabel = new Label();
            expensesLabel = new Label();
            netLabel = new Label();
            incomeValue = new Label();
            minusLabel = new Label();
            expensesValue = new Label();
            equalsLabel = new Label();
            netValue = new Label();
            noDataLabel = new Label();
            buttonPanel = new FlowLayoutPanel();
            closeButton = new Button();
            ((System.ComponentModel.ISupportInitialize)categoryGrid).BeginInit();
            layoutPanel.SuspendLayout();
            selectorPanel.SuspendLayout();
            summaryPanel.SuspendLayout();
            buttonPanel.SuspendLayout();
            viewPanel.SuspendLayout();
            SuspendLayout();
            //
            // layoutPanel
            //
            layoutPanel.ColumnCount = 1;
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutPanel.Controls.Add(selectorPanel, 0, 0);
            layoutPanel.Controls.Add(rangeLabel, 0, 1);
            layoutPanel.Controls.Add(summaryPanel, 0, 2);
            layoutPanel.Controls.Add(noDataLabel, 0, 3);
            layoutPanel.Controls.Add(viewPanel, 0, 4);
            layoutPanel.Controls.Add(buttonPanel, 0, 5);
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.Name = "layoutPanel";
            layoutPanel.Padding = new Padding(12);
            layoutPanel.RowCount = 6;
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.TabIndex = 0;
            //
            // selectorPanel
            //
            selectorPanel.AutoSize = true;
            selectorPanel.Controls.Add(periodLabel);
            selectorPanel.Controls.Add(kindCombo);
            selectorPanel.Controls.Add(yearLabel);
            selectorPanel.Controls.Add(yearCombo);
            selectorPanel.Controls.Add(monthLabel);
            selectorPanel.Controls.Add(monthCombo);
            selectorPanel.Controls.Add(viewLabel);
            selectorPanel.Controls.Add(viewCombo);
            selectorPanel.Dock = DockStyle.Fill;
            selectorPanel.Name = "selectorPanel";
            selectorPanel.TabIndex = 0;
            //
            // periodLabel
            //
            periodLabel.Anchor = AnchorStyles.Left;
            periodLabel.AutoSize = true;
            periodLabel.Name = "periodLabel";
            periodLabel.TabIndex = 0;
            resources.ApplyResources(periodLabel, "periodLabel");
            //
            // kindCombo
            //
            kindCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            kindCombo.FormattingEnabled = true;
            kindCombo.Margin = new Padding(3, 3, 12, 3);
            kindCombo.Name = "kindCombo";
            kindCombo.TabIndex = 1;
            kindCombo.SelectedIndexChanged += kindCombo_SelectedIndexChanged;
            //
            // yearLabel
            //
            yearLabel.Anchor = AnchorStyles.Left;
            yearLabel.AutoSize = true;
            yearLabel.Name = "yearLabel";
            yearLabel.TabIndex = 2;
            resources.ApplyResources(yearLabel, "yearLabel");
            //
            // yearCombo
            //
            yearCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            yearCombo.FormattingEnabled = true;
            yearCombo.Margin = new Padding(3, 3, 12, 3);
            yearCombo.Name = "yearCombo";
            yearCombo.TabIndex = 3;
            yearCombo.SelectedIndexChanged += yearCombo_SelectedIndexChanged;
            //
            // monthLabel
            //
            monthLabel.Anchor = AnchorStyles.Left;
            monthLabel.AutoSize = true;
            monthLabel.Name = "monthLabel";
            monthLabel.TabIndex = 4;
            resources.ApplyResources(monthLabel, "monthLabel");
            //
            // monthCombo
            //
            monthCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            monthCombo.FormattingEnabled = true;
            monthCombo.Margin = new Padding(3, 3, 12, 3);
            monthCombo.Name = "monthCombo";
            monthCombo.TabIndex = 5;
            monthCombo.SelectedIndexChanged += monthCombo_SelectedIndexChanged;
            // 
            // viewLabel
            // 
            viewLabel.Anchor = AnchorStyles.Left;
            viewLabel.AutoSize = true;
            viewLabel.Name = "viewLabel";
            viewLabel.TabIndex = 6;
            resources.ApplyResources(viewLabel, "viewLabel");
            // 
            // viewCombo
            // 
            viewCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            viewCombo.FormattingEnabled = true;
            viewCombo.Name = "viewCombo";
            viewCombo.TabIndex = 7;
            viewCombo.SelectedIndexChanged += viewCombo_SelectedIndexChanged;
            //
            // rangeLabel
            //
            rangeLabel.AutoSize = true;
            rangeLabel.Margin = new Padding(3, 12, 3, 12);
            rangeLabel.Name = "rangeLabel";
            rangeLabel.TabIndex = 1;
            // 
            // summaryPanel
            // 
            summaryPanel.Anchor = AnchorStyles.None;
            summaryPanel.AutoSize = true;
            summaryPanel.ColumnCount = 5;
            summaryPanel.ColumnStyles.Add(new ColumnStyle());
            summaryPanel.ColumnStyles.Add(new ColumnStyle());
            summaryPanel.ColumnStyles.Add(new ColumnStyle());
            summaryPanel.ColumnStyles.Add(new ColumnStyle());
            summaryPanel.ColumnStyles.Add(new ColumnStyle());
            summaryPanel.Controls.Add(incomeLabel, 0, 0);
            summaryPanel.Controls.Add(expensesLabel, 2, 0);
            summaryPanel.Controls.Add(netLabel, 4, 0);
            summaryPanel.Controls.Add(incomeValue, 0, 1);
            summaryPanel.Controls.Add(minusLabel, 1, 1);
            summaryPanel.Controls.Add(expensesValue, 2, 1);
            summaryPanel.Controls.Add(equalsLabel, 3, 1);
            summaryPanel.Controls.Add(netValue, 4, 1);
            summaryPanel.Name = "summaryPanel";
            summaryPanel.RowCount = 2;
            summaryPanel.RowStyles.Add(new RowStyle());
            summaryPanel.RowStyles.Add(new RowStyle());
            summaryPanel.TabIndex = 2;
            // 
            // incomeLabel
            // 
            incomeLabel.Anchor = AnchorStyles.None;
            incomeLabel.AutoSize = true;
            incomeLabel.Name = "incomeLabel";
            incomeLabel.TabIndex = 0;
            resources.ApplyResources(incomeLabel, "incomeLabel");
            // 
            // expensesLabel
            // 
            expensesLabel.Anchor = AnchorStyles.None;
            expensesLabel.AutoSize = true;
            expensesLabel.Name = "expensesLabel";
            expensesLabel.TabIndex = 1;
            resources.ApplyResources(expensesLabel, "expensesLabel");
            // 
            // netLabel
            // 
            netLabel.Anchor = AnchorStyles.None;
            netLabel.AutoSize = true;
            netLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            netLabel.Name = "netLabel";
            netLabel.TabIndex = 2;
            resources.ApplyResources(netLabel, "netLabel");
            // 
            // incomeValue
            // 
            incomeValue.Anchor = AnchorStyles.None;
            incomeValue.AutoSize = true;
            incomeValue.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            incomeValue.Margin = new Padding(12, 3, 12, 3);
            incomeValue.Name = "incomeValue";
            incomeValue.TabIndex = 3;
            // 
            // minusLabel
            // 
            minusLabel.Anchor = AnchorStyles.None;
            minusLabel.AutoSize = true;
            minusLabel.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            minusLabel.Name = "minusLabel";
            minusLabel.TabIndex = 4;
            resources.ApplyResources(minusLabel, "minusLabel");
            // 
            // expensesValue
            // 
            expensesValue.Anchor = AnchorStyles.None;
            expensesValue.AutoSize = true;
            expensesValue.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            expensesValue.Margin = new Padding(12, 3, 12, 3);
            expensesValue.Name = "expensesValue";
            expensesValue.TabIndex = 5;
            // 
            // equalsLabel
            // 
            equalsLabel.Anchor = AnchorStyles.None;
            equalsLabel.AutoSize = true;
            equalsLabel.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            equalsLabel.Name = "equalsLabel";
            equalsLabel.TabIndex = 6;
            resources.ApplyResources(equalsLabel, "equalsLabel");
            // 
            // netValue
            // 
            netValue.Anchor = AnchorStyles.None;
            netValue.AutoSize = true;
            netValue.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            netValue.Margin = new Padding(12, 3, 12, 3);
            netValue.Name = "netValue";
            netValue.TabIndex = 7;
            //
            // noDataLabel
            //
            noDataLabel.AutoSize = true;
            noDataLabel.Name = "noDataLabel";
            noDataLabel.TabIndex = 3;
            resources.ApplyResources(noDataLabel, "noDataLabel");
            // 
            // viewPanel
            // 
            viewPanel.Controls.Add(categoryGrid);
            viewPanel.Controls.Add(linePlot);
            viewPanel.Controls.Add(barPlot);
            viewPanel.Dock = DockStyle.Fill;
            viewPanel.Name = "viewPanel";
            viewPanel.TabIndex = 4;
            // 
            // categoryGrid
            // 
            categoryGrid.AllowUserToAddRows = false;
            categoryGrid.AllowUserToDeleteRows = false;
            categoryGrid.AllowUserToResizeRows = false;
            categoryGrid.BackgroundColor = SystemColors.Window;
            categoryGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            categoryGrid.Columns.AddRange(new DataGridViewColumn[] { categoryColumn, totalColumn });
            categoryGrid.Dock = DockStyle.Fill;
            categoryGrid.Name = "categoryGrid";
            categoryGrid.ReadOnly = true;
            categoryGrid.RowHeadersVisible = false;
            categoryGrid.TabIndex = 0;
            // 
            // categoryColumn
            // 
            categoryColumn.Frozen = true;
            resources.ApplyResources(categoryColumn, "categoryColumn");
            categoryColumn.Name = "categoryColumn";
            categoryColumn.ReadOnly = true;
            categoryColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            categoryColumn.Width = 150;
            // 
            // totalColumn
            // 
            totalColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight;
            totalColumn.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(totalColumn, "totalColumn");
            totalColumn.Name = "totalColumn";
            totalColumn.ReadOnly = true;
            totalColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // linePlot
            // 
            linePlot.Dock = DockStyle.Fill;
            linePlot.Name = "linePlot";
            linePlot.TabIndex = 1;
            // 
            // barPlot
            // 
            barPlot.Dock = DockStyle.Fill;
            barPlot.Name = "barPlot";
            barPlot.TabIndex = 2;
            //
            // buttonPanel
            //
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Name = "buttonPanel";
            buttonPanel.TabIndex = 5;
            //
            // closeButton
            //
            closeButton.DialogResult = DialogResult.Cancel;
            closeButton.Name = "closeButton";
            closeButton.TabIndex = 0;
            resources.ApplyResources(closeButton, "closeButton");
            closeButton.UseVisualStyleBackColor = true;
            //
            // ReportsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(860, 540);
            Controls.Add(layoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ReportsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)categoryGrid).EndInit();
            layoutPanel.ResumeLayout(false);
            layoutPanel.PerformLayout();
            selectorPanel.ResumeLayout(false);
            selectorPanel.PerformLayout();
            summaryPanel.ResumeLayout(false);
            summaryPanel.PerformLayout();
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            viewPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutPanel;
        private FlowLayoutPanel selectorPanel;
        private Label periodLabel;
        private ComboBox kindCombo;
        private Label yearLabel;
        private ComboBox yearCombo;
        private Label monthLabel;
        private ComboBox monthCombo;
        private Label viewLabel;
        private ComboBox viewCombo;
        private Label rangeLabel;
        private TableLayoutPanel summaryPanel;
        private Label incomeLabel;
        private Label expensesLabel;
        private Label netLabel;
        private Label incomeValue;
        private Label minusLabel;
        private Label expensesValue;
        private Label equalsLabel;
        private Label netValue;
        private Label noDataLabel;
        private Panel viewPanel;
        private DataGridView categoryGrid;
        private DataGridViewTextBoxColumn categoryColumn;
        private DataGridViewTextBoxColumn totalColumn;
        private ScottPlot.WinForms.FormsPlot linePlot;
        private ScottPlot.WinForms.FormsPlot barPlot;
        private FlowLayoutPanel buttonPanel;
        private Button closeButton;
    }
}
