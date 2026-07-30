using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class LoanForm : Form
    {
        private readonly LibraryService _service;

        private TextBox _userNumberBox;
        private ComboBox _copyBox;
        private ListBox _currentLoansList;
        private Label _statusLabel;

        public LoanForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RefreshAvailableCopies();
        }

        private void BuildUi()
        {
            Text = "Loan Process";
            Width = 620;
            Height = 520;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "Member User Number:",
                Location = new Point(20, 25),
                AutoSize = true
            });

            _userNumberBox = new TextBox
            {
                Location = new Point(200, 22),
                Width = 160
            };
            Controls.Add(_userNumberBox);

            var checkButton = new Button
            {
                Text = "Check Loans",
                Location = new Point(370, 20),
                Width = 120
            };
            checkButton.Click += (s, e) => RefreshCurrentLoans();
            Controls.Add(checkButton);

            Controls.Add(new Label
            {
                Text = "Available Copy to Borrow:",
                Location = new Point(20, 65),
                AutoSize = true
            });

            _copyBox = new ComboBox
            {
                Location = new Point(200, 62),
                Width = 290,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_copyBox);

            var loanButton = new Button
            {
                Text = "Confirm Loan",
                Location = new Point(20, 105),
                Width = 160,
                Height = 35
            };
            loanButton.Click += LoanButton_Click;
            Controls.Add(loanButton);

            _statusLabel = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(560, 40),
                ForeColor = Color.DarkGreen
            };
            Controls.Add(_statusLabel);

            Controls.Add(new Label
            {
                Text = "Member's Current Loans:",
                Location = new Point(20, 200),
                AutoSize = true
            });

            _currentLoansList = new ListBox
            {
                Location = new Point(20, 225),
                Size = new Size(560, 220)
            };
            Controls.Add(_currentLoansList);
        }

        private void RefreshAvailableCopies()
        {
            _copyBox.Items.Clear();

            foreach (var copy in _service.GetAvailableBorrowableCopies())
            {
                _copyBox.Items.Add(copy.CopyNumber);
            }

            if (_copyBox.Items.Count > 0)
            {
                _copyBox.SelectedIndex = 0;
            }
        }

        private void LoanButton_Click(object sender, EventArgs e)
        {
            if (_copyBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a copy to borrow.",
                    "Missing Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                var loan = _service.LoanBook(
                    _userNumberBox.Text.Trim(),
                    (string)_copyBox.SelectedItem);

                _statusLabel.Text =
                    $"Loan confirmed. Loan ID: {loan.LoanId}. Due date: {loan.DueDate:yyyy-MM-dd}.";

                RefreshAvailableCopies();
                RefreshCurrentLoans();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Loan Book",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void RefreshCurrentLoans()
        {
            _currentLoansList.Items.Clear();

            var userNumber = _userNumberBox.Text.Trim();

            if (string.IsNullOrEmpty(userNumber))
                return;

            var loans = _service.GetActiveLoansForUser(userNumber);

            if (loans.Count == 0)
            {
                _currentLoansList.Items.Add("(No active loans for this member.)");
                return;
            }

            foreach (var loan in loans)
            {
                var overdueTag = loan.IsOverdue ? "  ** OVERDUE **" : "";

                _currentLoansList.Items.Add(
                    $"{loan.LoanId} | Copy: {loan.CopyNumber} | Borrowed: {loan.LoanDate:yyyy-MM-dd} | Due: {loan.DueDate:yyyy-MM-dd}{overdueTag}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LoanForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "LoanForm";
            this.Load += new System.EventHandler(this.LoanForm_Load);
            this.ResumeLayout(false);

        }

        private void LoanForm_Load(object sender, EventArgs e)
        {

        }
    }
}