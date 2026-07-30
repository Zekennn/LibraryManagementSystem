using LibraryManagementSystem.Data;
using LibraryManagementSystem.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryManagementSystem.Forms
{
    public class MainForm : Form
    {
        private readonly LibraryService _service;

        public MainForm()
        {
            _service = new LibraryService(DataStore.Instance);
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Sarasavi Library System";
            Width = 640;
            Height = 480;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "Sarasavi Library",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(40, 30)
            };

            var subtitle = new Label
            {
                Text = "Loan • Return • Reservation • Inquiry • Book Registration • User Registration",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(42, 75)
            };

            Controls.Add(title);
            Controls.Add(subtitle);

            var buttonsPanel = new FlowLayoutPanel
            {
                Location = new Point(40, 120),
                Size = new Size(540, 300),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            Controls.Add(buttonsPanel);

            AddMenuButton(buttonsPanel, "1. Loan Process", (s, e) => OpenForm(new LoanForm(_service)));
            AddMenuButton(buttonsPanel, "2. Return Process", (s, e) => OpenForm(new ReturnForm(_service)));
            AddMenuButton(buttonsPanel, "3. Reservation Process", (s, e) => OpenForm(new ReservationForm(_service)));
            AddMenuButton(buttonsPanel, "4. Inquiry Process", (s, e) => OpenForm(new InquiryForm(_service)));
            AddMenuButton(buttonsPanel, "5. Book Registration", (s, e) => OpenForm(new BookRegistrationForm(_service)));
            AddMenuButton(buttonsPanel, "6. User Registration", (s, e) => OpenForm(new UserRegistrationForm(_service)));

            var exitButton = new Button
            {
                Text = "Exit",
                Size = new Size(160, 45),
                Location = new Point(40, 400)
            };

            exitButton.Click += (s, e) => Close();
            Controls.Add(exitButton);
        }

        private static void AddMenuButton(FlowLayoutPanel panel, string text, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(250, 60),
                Margin = new Padding(10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.Click += onClick;
            panel.Controls.Add(button);
        }

        private static void OpenForm(Form form)
        {
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(637, 331);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}