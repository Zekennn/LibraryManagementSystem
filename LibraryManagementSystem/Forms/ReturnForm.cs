using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class ReturnForm : Form
    {
        private readonly LibraryService _service;

        private ComboBox _copyBox;
        private Label _resultLabel;
        private ListBox _outstandingLoansList;

        public ReturnForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RefreshOutstandingLoans();
        }

        private void BuildUi()
        {
            Text = "Return Process";
            Width = 620;
            Height = 480;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "Outstanding Loans:",
                Location = new Point(20, 20),
                AutoSize = true
            });

            _outstandingLoansList = new ListBox
            {
                Location = new Point(20, 45),
                Size = new Size(560, 220)
            };
            Controls.Add(_outstandingLoansList);

            Controls.Add(new Label
            {
                Text = "Copy Number to Return:",
                Location = new Point(20, 280),
                AutoSize = true
            });

            _copyBox = new ComboBox
            {
                Location = new Point(220, 277),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_copyBox);

            var returnButton = new Button
            {
                Text = "Confirm Return",
                Location = new Point(20, 320),
                Width = 160,
                Height = 35
            };

            returnButton.Click += ReturnButton_Click;
            Controls.Add(returnButton);

            _resultLabel = new Label
            {
                Location = new Point(20, 365),
                Size = new Size(560, 60),
                ForeColor = Color.DarkGreen
            };
            Controls.Add(_resultLabel);
        }

        private void RefreshOutstandingLoans()
        {
            _outstandingLoansList.Items.Clear();
            _copyBox.Items.Clear();

            var loans = _service.GetAllActiveLoans();

            if (loans.Count == 0)
            {
                _outstandingLoansList.Items.Add("(No active loans.)");
                return;
            }

            foreach (var loan in loans)
            {
                var overdueTag = loan.IsOverdue ? "  ** OVERDUE **" : "";

                _outstandingLoansList.Items.Add(
                    $"{loan.LoanId} | Copy: {loan.CopyNumber} | Member: {loan.UserNumber} | Due: {loan.DueDate:yyyy-MM-dd}{overdueTag}");

                _copyBox.Items.Add(loan.CopyNumber);
            }

            if (_copyBox.Items.Count > 0)
            {
                _copyBox.SelectedIndex = 0;
            }
        }

        private void ReturnButton_Click(object sender, EventArgs e)
        {
            if (_copyBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a copy to return.",
                    "Missing Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                var message = _service.ReturnBook((string)_copyBox.SelectedItem);

                _resultLabel.Text = message;

                RefreshOutstandingLoans();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Return Book",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ReturnForm
            // 
            this.ClientSize = new System.Drawing.Size(596, 334);
            this.Name = "ReturnForm";
            this.ResumeLayout(false);

        }
    }
}
