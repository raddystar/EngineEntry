using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;

namespace SuperiorPartsEngines
{
    public partial class MainForm : Form
    {
        private readonly string _appDir = AppDomain.CurrentDomain.BaseDirectory;
        private string _configPath => Path.Combine(_appDir, "db_config.txt");
        private string _passPath => Path.Combine(_appDir, "admin.hash");
        private string _dbPath = "";
        private string ConnStr => $"Data Source={_dbPath};Default Timeout=5;";

        private TabControl _tabs = new() { Dock = DockStyle.Fill };
        private ToolStripLabel _lblDb = new() { ForeColor = Color.DimGray };

        // Entries Tab
        private DataGridView _gridEntries = new();
        private TextBox _txtSearchEntries = new() { Width = 250, PlaceholderText = "Search Engine #, Container #, Entry #, Customer..." };
        private ComboBox _cmbStatusFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private long? _selectedEntryId = null;

        private TextBox _txtEngineNumber = new();
        private ComboBox _cmbEntryType = new() { Text = "C78" };
        private TextBox _txtEntryNumber = new();
        private DateTimePicker _dtpC78Date = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
        private TextBox _txtContainerNumber = new();
        private ComboBox _cmbEntrySupplier = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private DateTimePicker _dtpDateEntered = new() { Format = DateTimePickerFormat.Short };
        private TextBox _txtChassisNumber = new();
        private TextBox _txtPartNumber = new();
        private TextBox _txtDescription = new();
        private ComboBox _cmbEntryCustomer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private TextBox _txtCustomerName = new();
        private TextBox _txtInvoiceNumber = new();
        private TextBox _txtInvoiceAmount = new();
        private TextBox _txtDiscount = new();
        private DateTimePicker _dtpDateSold = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };

        // Suppliers Tab
        private DataGridView _gridSuppliers = new();
        private TextBox _txtSearchSuppliers = new() { Width = 240, PlaceholderText = "Search Supplier Name or Phone..." };
        private long? _selectedSupplierId = null;
        private TextBox _txtSupName = new();
        private TextBox _txtSupAddress = new();
        private TextBox _txtSupPhone = new();

        // Customers Tab
        private DataGridView _gridCustomers = new();
        private TextBox _txtSearchCustomers = new() { Width = 240, PlaceholderText = "Search Customer Name, TRN, Phone..." };
        private long? _selectedCustomerId = null;
        private TextBox _txtCustFirst = new();
        private TextBox _txtCustLast = new();
        private TextBox _txtCustTrn = new();
        private TextBox _txtCustAddr1 = new();
        private TextBox _txtCustParish = new();
        private TextBox _txtCustPhone1 = new();
        private TextBox _txtCustCell = new();
        private TextBox _txtCustEmail = new();
        private TextBox _txtCustCredit = new() { Text = "0" };
        private CheckBox _chkCustBlocked = new() { Text = "Account Blocked" };

        public MainForm()
        {
            Text = "Engine & Customs Inventory Manager (Multi-User)";
            Size = new Size(1380, 840);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);

            LoadDbPath();
            EnsureDefaultPassword();
            EnsureDatabase();
            BuildTopToolbar();
            BuildTabs();
            Controls.Add(_tabs);
            _tabs.BringToFront();

            RefreshAllData();
        }

        private void LoadDbPath()
        {
            if (File.Exists(_configPath))
            {
                string saved = File.ReadAllText(_configPath).Trim();
                if (!string.IsNullOrWhiteSpace(saved))
                {
                    _dbPath = saved;
                    return;
                }
            }
            _dbPath = Path.Combine(_appDir, "EngineEntry.db");
            File.WriteAllText(_configPath, _dbPath);
        }

        private void EnsureDatabase()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using var conn = new SqliteConnection(ConnStr);
                conn.Open();
                using (var pragma = new SqliteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000; PRAGMA foreign_keys=ON;", conn))
                    pragma.ExecuteNonQuery();

                string schemaFile = Path.Combine(_appDir, "schema.sql");
                string sql = File.Exists(schemaFile)
                    ? File.ReadAllText(schemaFile)
                    : @"CREATE TABLE IF NOT EXISTS Suppliers (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Address TEXT, Telephone TEXT);
                        CREATE TABLE IF NOT EXISTS Customer (CustomerID INTEGER PRIMARY KEY AUTOINCREMENT, First_Name TEXT, Last_Name TEXT NOT NULL, Tax_Registration_No TEXT, Address1 TEXT, Parish TEXT, Country TEXT DEFAULT 'Jamaica', Telephone_1 TEXT, Email TEXT, Cell TEXT, CreditLimit REAL DEFAULT 0, IsBlocked INTEGER DEFAULT 0);
                        CREATE TABLE IF NOT EXISTS Entries (ID INTEGER PRIMARY KEY AUTOINCREMENT, EngineNumber TEXT NOT NULL, EntryType TEXT DEFAULT 'C78', ChassisNumber TEXT, EntryNumber TEXT, SupplierID INTEGER, ContainerNumber TEXT, C78EntryDate TEXT, DateEntered TEXT DEFAULT (datetime('now','localtime')), InvoiceNumber INTEGER, PartNumber TEXT, Description TEXT, CustomerID INTEGER, InvoiceAmount REAL DEFAULT 0, CustomerName TEXT, DateSold TEXT, Discount REAL DEFAULT 0);";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                _lblDb.Text = $"  DB: {_dbPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error:\n{ex.Message}\n\nClick 'Network DB Path' to select a valid local or network folder.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshAllData()
        {
            LoadSuppliers();
            LoadCustomers();
            LoadEntries();
        }

        private void BuildTopToolbar()
        {
            var bar = new ToolStrip { Dock = DockStyle.Top, Padding = new Padding(6) };

            var btnRefresh = new ToolStripButton("Refresh Data") { BackColor = Color.FromArgb(255, 243, 205) };
            btnRefresh.Click += (s, e) => RefreshAllData();

            var btnImpEng = new ToolStripButton("Import Engines CSV") { BackColor = Color.FromArgb(230, 244, 234) };
            btnImpEng.Click += (s, e) => ImportEnginesCsv();

            var btnImpCust = new ToolStripButton("Import Customers CSV") { BackColor = Color.FromArgb(230, 244, 234) };
            btnImpCust.Click += (s, e) => ImportCustomersCsv();

            var btnExport = new ToolStripButton("Export Entries CSV");
            btnExport.Click += (s, e) => ExportEntriesCsv();

            var btnBackup = new ToolStripButton("Backup DB") { BackColor = Color.FromArgb(232, 240, 254) };
            btnBackup.Click += (s, e) => BackupDatabase();

            var btnNetDb = new ToolStripButton("Network DB Path");
            btnNetDb.Click += (s, e) => ChangeDatabaseLocation();

            var btnSetPass = new ToolStripButton("Set Delete Password");
            btnSetPass.Click += (s, e) => ChangeDeletePassword();

            bar.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, new ToolStripSeparator(),
                btnImpEng, btnImpCust, btnExport, new ToolStripSeparator(),
                btnBackup, btnNetDb, btnSetPass, _lblDb
            });
            Controls.Add(bar);
        }

        private void ChangeDatabaseLocation()
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Select or Create Shared Network Database File",
                Filter = "SQLite Database (*.db)|*.db",
                FileName = "EngineEntry.db",
                OverwritePrompt = false
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            _dbPath = sfd.FileName;
            File.WriteAllText(_configPath, _dbPath);
            EnsureDatabase();
            RefreshAllData();
            MessageBox.Show($"Connected to:\n{_dbPath}\n\nPoint other network PCs to this same shared file path.", "Network Database Connected");
        }

        private static string HashText(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private void EnsureDefaultPassword()
        {
            if (!File.Exists(_passPath))
                File.WriteAllText(_passPath, HashText("Superior2026"));
        }

        private bool VerifyDeletePassword()
        {
            string? entered = PromptPassword("Enter Admin Password to Delete (Default: Superior2026):");
            if (string.IsNullOrEmpty(entered)) return false;
            string stored = File.ReadAllText(_passPath).Trim();
            if (HashText(entered) == stored) return true;

            MessageBox.Show("Incorrect password. Delete cancelled.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return false;
        }

        private void ChangeDeletePassword()
{
if (!VerifyDeletePassword()) return;
string? newPass = PromptPassword("Enter NEW Delete Password:");
if (string.IsNullOrWhiteSpace(newPass)) return;
File.WriteAllText(_passPath, HashText(newPass.Trim()));
MessageBox.Show("Delete password updated!", "Password Saved");
}
private static string? PromptPassword(string title)
{
using var f = new Form { Width = 380, Height = 160, Text = "Security Check", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
var lbl = new Label { Left = 14, Top = 14, Width = 340, Text = title };
var txt = new TextBox { Left = 14, Top = 40, Width = 335, UseSystemPasswordChar = true };
var ok = new Button { Text = "OK", Left = 190, Top = 76, Width = 75, DialogResult = DialogResult.OK };
var cancel = new Button { Text = "Cancel", Left = 274, Top = 76, Width = 75, DialogResult = DialogResult.Cancel };
f.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
f.AcceptButton = ok;
return f.ShowDialog() == DialogResult.OK ? txt.Text : null;
}
private void BuildTabs()
{
var t1 = new TabPage("Engine Entries"); BuildEntriesTab(t1);
var t2 = new TabPage("Suppliers"); BuildSuppliersTab(t2);
var t3 = new TabPage("Customers"); BuildCustomersTab(t3);
_tabs.TabPages.AddRange(new[] { t1, t2, t3 });
}
private static void StyleGrid(DataGridView g)
{
g.Dock = DockStyle.Fill; g.ReadOnly = true; g.AllowUserToAddRows = false;
g.SelectionMode = DataGridViewSelectionMode.FullRowSelect; g.MultiSelect = false;
g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
g.BackgroundColor = Color.White; g.RowHeadersVisible = false;
}
private static void AddField(TableLayoutPanel tbl, int row, int col, string label, Control ctrl)
{
ctrl.Dock = DockStyle.Fill;
tbl.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 3, 3) }, col, row);
tbl.Controls.Add(ctrl, col + 1, row);
}
}
}


