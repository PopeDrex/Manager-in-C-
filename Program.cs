using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace NotesForm
{
    public class MainForm : Form
    {
        private List<Note> notes = new List<Note>();
        private TextBox txtTitle;
        private TextBox txtContent;
        private ListBox listBox;
        private Button btnView, btnAdd, btnDelete, btnGenerate;
        private CheckBox chkShowPassword;
        private NumericUpDown numPasswordLength;
        private PictureBox logoPictureBox;

        public MainForm()
        {
            this.Text = "Password Manager";
            this.Icon = new Icon("icon.ico");
            this.Width = 600;
            this.Height = 500;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            logoPictureBox = new PictureBox()
            {
                Size = new Size(167, 115),
                Location = new Point(1, 1),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            try
            {
                logoPictureBox.Image = Image.FromFile("logo.png"); 
            }
            catch (Exception)
            {
                MessageBox.Show("Logo image not found.");
            }
            this.Controls.Add(logoPictureBox);

            TableLayoutPanel layout = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 8,
                AutoSize = true,
                BackColor = Color.FromArgb(30, 30, 30)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            Label lblTitle = new Label() { Text = "Title:", Anchor = AnchorStyles.Left, ForeColor = Color.White };
            txtTitle = new TextBox() { Dock = DockStyle.Fill, BackColor = Color.Black, ForeColor = Color.White };
            Label lblContent = new Label() { Text = "Password:", Anchor = AnchorStyles.Left, ForeColor = Color.White };
            txtContent = new TextBox() { Dock = DockStyle.Fill, PasswordChar = '*', BackColor = Color.Black, ForeColor = Color.White };

            chkShowPassword = new CheckBox() { Text = "Show Password", ForeColor = Color.White, AutoSize = true };
            chkShowPassword.CheckedChanged += (s, e) => txtContent.PasswordChar = chkShowPassword.Checked ? '\0' : '*';

            btnGenerate = new Button() { Text = "Generate Password", BackColor = Color.FromArgb(255, 165, 0), ForeColor = Color.White, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            btnGenerate.Click += (s, e) => txtContent.Text = GeneratePassword((int)numPasswordLength.Value);

            numPasswordLength = new NumericUpDown() { Minimum = 4, Maximum = 32, Value = 12, ForeColor = Color.White, BackColor = Color.Black, Width = 50 };

            listBox = new ListBox() { Dock = DockStyle.Fill, Height = 150, BackColor = Color.Black, ForeColor = Color.White };

            btnAdd = new Button() { Text = "Add Password", BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            btnAdd.Click += (s, e) => AddNote();

            btnDelete = new Button() { Text = "Delete Password", BackColor = Color.FromArgb(178, 34, 34), ForeColor = Color.White, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            btnDelete.Click += (s, e) => DeleteNote();

            btnView = new Button() { Text = "View Password", BackColor = Color.FromArgb(34, 139, 34), ForeColor = Color.White, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            btnView.Click += (s, e) => ViewNote();

            layout.Controls.Add(lblTitle, 0, 0);
            layout.Controls.Add(txtTitle, 1, 0);
            layout.Controls.Add(lblContent, 0, 1);
            layout.Controls.Add(txtContent, 1, 1);
            layout.Controls.Add(chkShowPassword, 1, 2);
            layout.Controls.Add(btnGenerate, 1, 3);
            layout.Controls.Add(numPasswordLength, 2, 3);
            layout.Controls.Add(listBox, 0, 4);
            layout.SetColumnSpan(listBox, 3);
            layout.Controls.Add(btnAdd, 0, 5);
            layout.Controls.Add(btnDelete, 1, 5);
            layout.Controls.Add(btnView, 0, 6);

            Controls.Add(layout);
            LoadNotes();
        }
        public class Note
        {
            public string Title { get; set; }
            public string Content { get; set; }

            public override string ToString() => Title;
        }

        private void LoadNotes()
        {
            Dictionary<string, string> remainders = JSONUtility.LoadDictionary("data");
            if (remainders != null)
            {
                foreach (var kvp in remainders)
                {
                    notes.Add(new Note { Title = JSONUtility.Decrypt(kvp.Key), Content = JSONUtility.Decrypt(kvp.Value) });
                }
            }
            UpdateListbox();
        }

        private void SaveNotes()
        {
            var remainders = new Dictionary<string, string>();
            foreach (var note in notes)
            {
                remainders[JSONUtility.Encrypt(note.Title)] = JSONUtility.Encrypt(note.Content);
            }
            JSONUtility.SaveDictionary(remainders, "data");
        }

        private void AddNote()
        {
            if (!string.IsNullOrWhiteSpace(txtTitle.Text) && !string.IsNullOrWhiteSpace(txtContent.Text))
            {
                notes.Add(new Note { Title = txtTitle.Text, Content = txtContent.Text });
                UpdateListbox();
                SaveNotes();
            }
            else
            {
                MessageBox.Show("Please enter both a title and content for the Password.");
            }
        }

        private string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            StringBuilder password = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[length];
                rng.GetBytes(buffer);
                for (int i = 0; i < length; i++)
                {
                    password.Append(chars[buffer[i] % chars.Length]);
                }
            }
            return password.ToString();
        }

        private void DeleteNote()
        {
            if (listBox.SelectedItem is Note selectedNote)
            {
                notes.RemoveAll(note => note.Title == selectedNote.Title);
                UpdateListbox();
                SaveNotes();
            }
            else
            {
                MessageBox.Show("Please select a Password to delete.");
            }
        }

        private void ViewNote()
        {
            if (listBox.SelectedItem is Note selectedNote)
            {
                DialogResult result = MessageBox.Show($"Title: {selectedNote.Title}\n\nPassword: {selectedNote.Content}\n\nCopy to clipboard?", "View Password", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    Clipboard.SetText(selectedNote.Content);
                }
            }
            else
            {
                MessageBox.Show("Please select a password to view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateListbox()
        {
            listBox.Items.Clear();
            foreach (var note in notes)
            {
                listBox.Items.Add(note);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }

    public static class JSONUtility
    {
        private static readonly string Key = "WnwEdpCqxkw/xQTPn4fhLg=="; // 16-char key for AES
        private static string GetFilePath(string fileName) => $"{fileName}.json";

        public static Dictionary<string, string> LoadDictionary(string fileName)
        {
            string path = GetFilePath(fileName);
            if (!File.Exists(path)) return new Dictionary<string, string>();

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public static void SaveDictionary(Dictionary<string, string> data, string fileName)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                File.WriteAllText(GetFilePath(fileName), json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        public static string Encrypt(string text)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = new byte[16];
            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = new byte[16];
            using var decryptor = aes.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
