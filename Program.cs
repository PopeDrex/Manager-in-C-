using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace NotesForm
{
    public class MainForm : Form
    {
        private List<Note> notes = new List<Note>();
        private TextBox txtTitle;
        private TextBox txtContent;
        private ListBox listBox;
        private Button btnView;

        public MainForm()
        {
            this.Text = "Password Application";
            this.Width = 600;
            this.Height = 600;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            txtTitle = new TextBox() { Top = 10, Left = 10, Width = 560, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtContent = new TextBox() { Top = 40, Left = 10, Width = 560, Height = 100, Multiline = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            listBox = new ListBox() { Top = 150, Left = 10, Width = 560, Height = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };

            Button btnAdd = new Button() { Text = "Add Password", Top = 370, Left = 10, Width = 105 };
            btnAdd.Click += (s, e) => AddNote();

            Button btnDelete = new Button() { Text = "Delete Password", Top = 370, Left = 120, Width = 105 };
            btnDelete.Click += (s, e) => DeleteNote();

            btnView = new Button() { Text = "View Password", Top = 370, Left = 230, Width = 105 };
            btnView.Click += (s, e) => ViewNote();

            Controls.Add(txtTitle);
            Controls.Add(txtContent);
            Controls.Add(listBox);
            Controls.Add(btnAdd);
            Controls.Add(btnDelete);
            Controls.Add(btnView);

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
                MessageBox.Show($"Title: {selectedNote.Title}\n\nContent:\n{selectedNote.Content}", "View Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select a Password to view.");
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
        private static readonly string Key = "1234567812345678"; // 16-char key for AES
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
