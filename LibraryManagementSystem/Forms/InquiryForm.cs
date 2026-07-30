using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class InquiryForm : Form
    {
        private readonly LibraryService _service;

        private TextBox _queryBox;
        private ListBox _resultsList;

        public InquiryForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RunSearch(string.Empty);
        }

        private void BuildUi()
        {
            Text = "Inquiry Process";
            Width = 700;
            Height = 500;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "Search by Book Number, Copy (Accession) Number, or part of Title/Author:",
                Location = new Point(20, 20),
                AutoSize = true
            });

            _queryBox = new TextBox
            {
                Location = new Point(20, 45),
                Width = 480
            };

            _queryBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    RunSearch(_queryBox.Text);
                    e.SuppressKeyPress = true;
                }
            };

            Controls.Add(_queryBox);

            var searchButton = new Button
            {
                Text = "Search",
                Location = new Point(510, 43),
                Width = 130
            };

            searchButton.Click += (s, e) => RunSearch(_queryBox.Text);
            Controls.Add(searchButton);

            _resultsList = new ListBox
            {
                Location = new Point(20, 85),
                Size = new Size(640, 360),
                HorizontalScrollbar = true
            };

            Controls.Add(_resultsList);
        }

        private void RunSearch(string query)
        {
            _resultsList.Items.Clear();

            var results = _service.Inquire(query);

            if (results.Count == 0)
            {
                _resultsList.Items.Add("(No matching books found.)");
                return;
            }

            foreach (var r in results)
            {
                string availability;

                if (r.ReferenceOnlyCopies == r.TotalCopies)
                {
                    availability = "Reference only - not borrowable";
                }
                else if (r.AvailableCopies > 0)
                {
                    availability = $"Available ({r.AvailableCopies} of {r.TotalCopies} copies free)";
                }
                else if (r.ReservedCopies > 0)
                {
                    availability = "All copies on loan or reserved";
                }
                else
                {
                    availability = "All copies currently on loan";
                }

                _resultsList.Items.Add(
                    $"{r.Book.BookNumber} | {r.Book.Title} by {r.Book.Author} | Publisher: {r.Book.Publisher} | " +
                    $"Total: {r.TotalCopies}, On Loan: {r.OnLoanCopies}, Reserved: {r.ReservedCopies} | {availability}");
            }
        }
    }
}