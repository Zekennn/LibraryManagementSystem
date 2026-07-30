using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class BookRegistrationForm : Form
    {
        private readonly LibraryService _service;

        private ComboBox _classificationBox;
        private TextBox _titleBox;
        private TextBox _authorBox;
        private TextBox _publisherBox;
        private NumericUpDown _copyCountBox;
        private CheckedListBox _copyTypesList;
        private ListBox _booksList;

        // Classification codes used by the library.
        private static readonly (char Code, string Name)[] Classifications =
        {
            ('F', "Fiction"),
            ('N', "Non-Fiction"),
            ('S', "Science"),
            ('T', "Technology"),
            ('H', "History"),
            ('C', "Children")
        };

        public BookRegistrationForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RefreshBookList();
        }

        private void BuildUi()
        {
            Text = "Book Registration";
            Width = 700;
            Height = 600;
            Font = new Font("Segoe UI", 10F);

            var layout = new TableLayoutPanel
            {
                Location = new Point(20, 20),
                Size = new Size(420, 220),
                ColumnCount = 2,
                RowCount = 5
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            layout.Controls.Add(CreateLabel("Classification:"), 0, 0);

            _classificationBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var c in Classifications)
                _classificationBox.Items.Add($"{c.Code} - {c.Name}");

            _classificationBox.SelectedIndex = 0;

            layout.Controls.Add(_classificationBox, 1, 0);

            layout.Controls.Add(CreateLabel("Title:"), 0, 1);

            _titleBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(_titleBox, 1, 1);

            layout.Controls.Add(CreateLabel("Author:"), 0, 2);

            _authorBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(_authorBox, 1, 2);

            layout.Controls.Add(CreateLabel("Publisher:"), 0, 3);

            _publisherBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(_publisherBox, 1, 3);

            layout.Controls.Add(CreateLabel("No. of Copies (max 10):"), 0, 4);

            _copyCountBox = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };

            _copyCountBox.ValueChanged += (s, e) => RebuildCopyTypeChecklist();

            layout.Controls.Add(_copyCountBox, 1, 4);

            Controls.Add(layout);

            var copyLabel = new Label
            {
                Text = "Tick each copy that is Borrowable (unticked = Reference only):",
                Location = new Point(20, 250),
                AutoSize = true
            };

            Controls.Add(copyLabel);

            _copyTypesList = new CheckedListBox
            {
                Location = new Point(20, 275),
                Size = new Size(420, 100),
                CheckOnClick = true
            };

            Controls.Add(_copyTypesList);

            RebuildCopyTypeChecklist();

            var registerButton = new Button
            {
                Text = "Register Book",
                Location = new Point(20, 390),
                Size = new Size(160, 40)
            };

            registerButton.Click += RegisterButton_Click;

            Controls.Add(registerButton);

            var listLabel = new Label
            {
                Text = "Registered Books:",
                Location = new Point(20, 440),
                AutoSize = true
            };

            Controls.Add(listLabel);

            _booksList = new ListBox
            {
                Location = new Point(20, 465),
                Size = new Size(640, 100)
            };

            Controls.Add(_booksList);
        }

        private static Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
        }

        private void RebuildCopyTypeChecklist()
        {
            _copyTypesList.Items.Clear();

            int count = (int)_copyCountBox.Value;

            for (int i = 1; i <= count; i++)
            {
                _copyTypesList.Items.Add($"Copy {i:D2}", true);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                char classification = ((string)_classificationBox.SelectedItem)[0];

                var copyTypes = new List<CopyType>();

                for (int i = 0; i < _copyTypesList.Items.Count; i++)
                {
                    bool isBorrowable = _copyTypesList.GetItemChecked(i);

                    copyTypes.Add(isBorrowable
                        ? CopyType.Borrowable
                        : CopyType.Reference);
                }

                var book = _service.RegisterBook(
                    classification,
                    _titleBox.Text,
                    _authorBox.Text,
                    _publisherBox.Text,
                    copyTypes);

                MessageBox.Show(
                    $"Book registered successfully.\nBook Number: {book.BookNumber}\nCopies created: {copyTypes.Count}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _titleBox.Clear();
                _authorBox.Clear();
                _publisherBox.Clear();
                _copyCountBox.Value = 1;

                RefreshBookList();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Register Book",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void RefreshBookList()
        {
            _booksList.Items.Clear();

            foreach (var b in _service.GetAllBooks())
            {
                var copies = _service.GetCopiesForBook(b.BookNumber);

                _booksList.Items.Add(
                    $"{b.BookNumber} | {b.Title} | {b.Author} | {b.Publisher} | Copies: {copies.Count}");
            }
        }
    }
}