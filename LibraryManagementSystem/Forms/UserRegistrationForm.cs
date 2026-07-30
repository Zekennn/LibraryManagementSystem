using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Forms
{
    public class UserRegistrationForm : Form
    {
        private readonly LibraryService _service;

        private TextBox _nameBox;
        private ComboBox _sexBox;
        private TextBox _nicBox;
        private TextBox _addressBox;
        private ComboBox _typeBox;
        private ListBox _usersList;

        public UserRegistrationForm(LibraryService service)
        {
            _service = service;
            BuildUi();
            RefreshUserList();
        }

        private void BuildUi()
        {
            Text = "User Registration";
            Width = 640;
            Height = 520;
            Font = new Font("Segoe UI", 10F);

            var layout = new TableLayoutPanel
            {
                Location = new Point(20, 20),
                Size = new Size(360, 260),
                ColumnCount = 2,
                RowCount = 5
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < layout.RowCount; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            }

            layout.Controls.Add(new Label
            {
                Text = "Name:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 0);

            _nameBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(_nameBox, 1, 0);

            layout.Controls.Add(new Label
            {
                Text = "Sex:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 1);

            _sexBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _sexBox.Items.AddRange(new object[]
            {
                Sex.Male,
                Sex.Female
            });
            _sexBox.SelectedIndex = 0;
            layout.Controls.Add(_sexBox, 1, 1);

            layout.Controls.Add(new Label
            {
                Text = "NIC Number:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 2);

            _nicBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(_nicBox, 1, 2);

            layout.Controls.Add(new Label
            {
                Text = "Address:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 3);

            _addressBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            layout.Controls.Add(_addressBox, 1, 3);

            layout.Controls.Add(new Label
            {
                Text = "User Type:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 4);

            _typeBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _typeBox.Items.AddRange(new object[]
            {
                UserType.Member,
                UserType.Visitor
            });
            _typeBox.SelectedIndex = 0;
            layout.Controls.Add(_typeBox, 1, 4);

            Controls.Add(layout);

            var registerButton = new Button
            {
                Text = "Register User",
                Location = new Point(20, 300),
                Size = new Size(160, 40)
            };

            registerButton.Click += RegisterButton_Click;
            Controls.Add(registerButton);

            var listLabel = new Label
            {
                Text = "Registered Users:",
                Location = new Point(20, 350),
                AutoSize = true
            };
            Controls.Add(listLabel);

            _usersList = new ListBox
            {
                Location = new Point(20, 375),
                Size = new Size(580, 100)
            };
            Controls.Add(_usersList);
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                var sex = (Sex)_sexBox.SelectedItem;
                var type = (UserType)_typeBox.SelectedItem;

                var user = _service.RegisterUser(
                    _nameBox.Text,
                    sex,
                    _nicBox.Text,
                    _addressBox.Text,
                    type);

                MessageBox.Show(
                    "User registered successfully.\nUser Number: " + user.UserNumber,
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _nameBox.Clear();
                _nicBox.Clear();
                _addressBox.Clear();

                RefreshUserList();
            }
            catch (LibraryRuleException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Cannot Register User",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void RefreshUserList()
        {
            _usersList.Items.Clear();

            foreach (var u in _service.GetAllUsers())
            {
                _usersList.Items.Add(
                    $"{u.UserNumber} | {u.Name} | {u.Sex} | {u.Type} | NIC: {u.NationalIdentityCardNumber} | {u.Address}");
            }
        }
    }
}