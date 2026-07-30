using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class ReservationForm : Form
    {
        private readonly LibraryService _service;

        private TextBox _userNumberBox;
        private ComboBox _bookBox;
        private ListBox _reservationsList;
        private Label _statusLabel;

        public ReservationForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RefreshBooks();
            RefreshReservations();
        }

        private void BuildUi()
        {
            Text = "Reservation Process";
            Width = 640;
            Height = 500;
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

            Controls.Add(new Label
            {
                Text = "Book Title to Reserve:",
                Location = new Point(20, 65),
                AutoSize = true
            });

            _bookBox = new ComboBox
            {
                Location = new Point(200, 62),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_bookBox);

            var reserveButton = new Button
            {
                Text = "Make Reservation",
                Location = new Point(20, 105),
                Width = 160,
                Height = 35
            };
            reserveButton.Click += ReserveButton_Click;
            Controls.Add(reserveButton);

            _statusLabel = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(580, 40),
                ForeColor = Color.DarkGreen
            };
            Controls.Add(_statusLabel);

            Controls.Add(new Label
            {
                Text = "Active Reservations (oldest first):",
                Location = new Point(20, 200),
                AutoSize = true
            });

            _reservationsList = new ListBox
            {
                Location = new Point(20, 225),
                Size = new Size(580, 180)
            };
            Controls.Add(_reservationsList);

            var cancelButton = new Button
            {
                Text = "Cancel Selected Reservation",
                Location = new Point(20, 415),
                Width = 220,
                Height = 32
            };
            cancelButton.Click += CancelButton_Click;
            Controls.Add(cancelButton);
        }

        private void RefreshBooks()
        {
            _bookBox.Items.Clear();

            foreach (var book in _service.GetAllBooks())
            {
                _bookBox.Items.Add($"{book.BookNumber} - {book.Title}");
            }

            if (_bookBox.Items.Count > 0)
            {
                _bookBox.SelectedIndex = 0;
            }
        }

        private void ReserveButton_Click(object sender, EventArgs e)
        {
            if (_bookBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a book title.",
                    "Missing Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var bookNumber = ((string)_bookBox.SelectedItem).Split(new[] { " - " }, StringSplitOptions.None)[0];

            try
            {
                var reservation = _service.ReserveBook(
                    _userNumberBox.Text.Trim(),
                    bookNumber);

                _statusLabel.Text =
                    $"Reservation confirmed. Reservation ID: {reservation.ReservationId}.";

                RefreshReservations();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Make Reservation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (_reservationsList.SelectedItem == null)
                return;

            var line = (string)_reservationsList.SelectedItem;
            var reservationId = line.Split('|')[0].Trim();

            try
            {
                _service.CancelReservation(reservationId);
                RefreshReservations();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Cancel Reservation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void RefreshReservations()
        {
            _reservationsList.Items.Clear();

            var reservations = _service.GetActiveReservations();

            if (reservations.Count == 0)
            {
                _reservationsList.Items.Add("(No active reservations.)");
                return;
            }

            foreach (var r in reservations)
            {
                _reservationsList.Items.Add(
                    $"{r.ReservationId} | Book: {r.BookNumber} | Member: {r.UserNumber} | Reserved on: {r.ReservationDate:yyyy-MM-dd}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ReservationForm
            // 
            this.ClientSize = new System.Drawing.Size(672, 322);
            this.Name = "ReservationForm";
            this.ResumeLayout(false);

        }
    }
}