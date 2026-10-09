namespace LocalExpense
{
    partial class AddTransactionForm
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
            layoutPanel = new TableLayoutPanel();
            dateLabel = new Label();
            datePicker = new DateTimePicker();
            typeLabel = new Label();
            typePanel = new FlowLayoutPanel();
            expenseRadio = new RadioButton();
            incomeRadio = new RadioButton();
            amountLabel = new Label();
            amountInput = new NumericUpDown();
            categoryLabel = new Label();
            categoryCombo = new ComboBox();
            noteLabel = new Label();
            noteText = new TextBox();
            buttonPanel = new FlowLayoutPanel();
            okButton = new Button();
            cancelButton = new Button();
            layoutPanel.SuspendLayout();
            typePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)amountInput).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // layoutPanel
            // 
            layoutPanel.ColumnCount = 2;
            layoutPanel.ColumnStyles.Add(new ColumnStyle());
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutPanel.Controls.Add(dateLabel, 0, 0);
            layoutPanel.Controls.Add(datePicker, 1, 0);
            layoutPanel.Controls.Add(typeLabel, 0, 1);
            layoutPanel.Controls.Add(typePanel, 1, 1);
            layoutPanel.Controls.Add(amountLabel, 0, 2);
            layoutPanel.Controls.Add(amountInput, 1, 2);
            layoutPanel.Controls.Add(categoryLabel, 0, 3);
            layoutPanel.Controls.Add(categoryCombo, 1, 3);
            layoutPanel.Controls.Add(noteLabel, 0, 4);
            layoutPanel.Controls.Add(noteText, 1, 4);
            layoutPanel.Controls.Add(buttonPanel, 0, 5);
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.Location = new Point(0, 0);
            layoutPanel.Name = "layoutPanel";
            layoutPanel.Padding = new Padding(12);
            layoutPanel.RowCount = 6;
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutPanel.RowStyles.Add(new RowStyle());
            layoutPanel.Size = new Size(384, 261);
            layoutPanel.TabIndex = 0;
            // 
            // dateLabel
            // 
            dateLabel.Anchor = AnchorStyles.Left;
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(15, 19);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(34, 15);
            dateLabel.TabIndex = 0;
            dateLabel.Text = "&Date:";
            // 
            // datePicker
            // 
            datePicker.Dock = DockStyle.Fill;
            datePicker.Format = DateTimePickerFormat.Short;
            datePicker.Location = new Point(79, 15);
            datePicker.Name = "datePicker";
            datePicker.Size = new Size(290, 23);
            datePicker.TabIndex = 1;
            // 
            // typeLabel
            // 
            typeLabel.Anchor = AnchorStyles.Left;
            typeLabel.AutoSize = true;
            typeLabel.Location = new Point(15, 49);
            typeLabel.Name = "typeLabel";
            typeLabel.Size = new Size(35, 15);
            typeLabel.TabIndex = 2;
            typeLabel.Text = "Type:";
            // 
            // typePanel
            // 
            typePanel.AutoSize = true;
            typePanel.Controls.Add(expenseRadio);
            typePanel.Controls.Add(incomeRadio);
            typePanel.Dock = DockStyle.Fill;
            typePanel.Location = new Point(79, 44);
            typePanel.Name = "typePanel";
            typePanel.Size = new Size(290, 25);
            typePanel.TabIndex = 3;
            // 
            // expenseRadio
            // 
            expenseRadio.AutoSize = true;
            expenseRadio.Checked = true;
            expenseRadio.Location = new Point(3, 3);
            expenseRadio.Name = "expenseRadio";
            expenseRadio.Size = new Size(67, 19);
            expenseRadio.TabIndex = 0;
            expenseRadio.TabStop = true;
            expenseRadio.Text = "&Expense";
            expenseRadio.UseVisualStyleBackColor = true;
            // 
            // incomeRadio
            // 
            incomeRadio.AutoSize = true;
            incomeRadio.Location = new Point(76, 3);
            incomeRadio.Name = "incomeRadio";
            incomeRadio.Size = new Size(65, 19);
            incomeRadio.TabIndex = 1;
            incomeRadio.Text = "&Income";
            incomeRadio.UseVisualStyleBackColor = true;
            // 
            // amountLabel
            // 
            amountLabel.Anchor = AnchorStyles.Left;
            amountLabel.AutoSize = true;
            amountLabel.Location = new Point(15, 79);
            amountLabel.Name = "amountLabel";
            amountLabel.Size = new Size(54, 15);
            amountLabel.TabIndex = 4;
            amountLabel.Text = "&Amount:";
            // 
            // amountInput
            // 
            amountInput.Dock = DockStyle.Fill;
            amountInput.Location = new Point(79, 75);
            amountInput.Name = "amountInput";
            amountInput.Size = new Size(290, 23);
            amountInput.TabIndex = 5;
            amountInput.TextAlign = HorizontalAlignment.Right;
            // 
            // categoryLabel
            // 
            categoryLabel.Anchor = AnchorStyles.Left;
            categoryLabel.AutoSize = true;
            categoryLabel.Location = new Point(15, 108);
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Size = new Size(58, 15);
            categoryLabel.TabIndex = 6;
            categoryLabel.Text = "&Category:";
            // 
            // categoryCombo
            // 
            categoryCombo.Dock = DockStyle.Fill;
            categoryCombo.Location = new Point(79, 104);
            categoryCombo.Name = "categoryCombo";
            categoryCombo.Size = new Size(290, 23);
            categoryCombo.TabIndex = 7;
            // 
            // noteLabel
            // 
            noteLabel.AutoSize = true;
            noteLabel.Location = new Point(15, 130);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new Size(36, 15);
            noteLabel.TabIndex = 8;
            noteLabel.Text = "&Note:";
            // 
            // noteText
            // 
            noteText.AcceptsReturn = true;
            noteText.Dock = DockStyle.Fill;
            noteText.Location = new Point(79, 133);
            noteText.Multiline = true;
            noteText.Name = "noteText";
            noteText.ScrollBars = ScrollBars.Vertical;
            noteText.Size = new Size(290, 78);
            noteText.TabIndex = 9;
            // 
            // buttonPanel
            // 
            buttonPanel.AutoSize = true;
            layoutPanel.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(15, 217);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(354, 29);
            buttonPanel.TabIndex = 10;
            // 
            // okButton
            // 
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(276, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 0;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(195, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // AddTransactionForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(384, 261);
            Controls.Add(layoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddTransactionForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add transaction";
            layoutPanel.ResumeLayout(false);
            layoutPanel.PerformLayout();
            typePanel.ResumeLayout(false);
            typePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)amountInput).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutPanel;
        private Label dateLabel;
        private DateTimePicker datePicker;
        private Label typeLabel;
        private FlowLayoutPanel typePanel;
        private RadioButton expenseRadio;
        private RadioButton incomeRadio;
        private Label amountLabel;
        private NumericUpDown amountInput;
        private Label categoryLabel;
        private ComboBox categoryCombo;
        private Label noteLabel;
        private TextBox noteText;
        private FlowLayoutPanel buttonPanel;
        private Button okButton;
        private Button cancelButton;
    }
}
