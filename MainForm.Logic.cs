using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;

namespace SuperiorPartsEngines
{
    public partial class MainForm
    {
        private void BuildEntriesTab(TabPage page)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 360 };
            var topPanel = new Panel { Dock = DockStyle.Fill };
            var searchBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
            _cmbStatusFilter.Items.AddRange(new object[] { "All Engines", "In Stock (Unsold)", "Sold" });
            _cmbStatusFilter.SelectedIndex = 0;

            var btnReset = new Button { Text = "Reset", AutoSize = true };
            _txtSearchEntries.TextChanged += (s, e) => LoadEntries();
            _cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadEntries();
            btnReset.Click += (s, e) => { _txtSearchEntries.Clear(); _cmbStatusFilter.SelectedIndex = 0; };

            searchBar.Controls.AddRange(new Control[] { new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, _txtSearchEntries, _cmbStatusFilter, btnReset });
            StyleGrid(_gridEntries);
            _gridEntries.CellClick += GridEntries_CellClick;
            topPanel.Controls.Add(_gridEntries);
            topPanel.Controls.Add(searchBar);
            split.Panel1.Controls.Add(topPanel);

            var formPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6 };
            for (int i = 0; i < 6; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i % 2 == 0 ? 12f : 21.3f));

            _cmbEntryType.Items.AddRange(new object[] { "C78", "IM4", "Other" });
            _cmbEntryCustomer.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbEntryCustomer.SelectedItem is ComboItem item && item.Id > 0)
                    _txtCustomerName.Text = item.Text;
            };

            AddField(tbl, 0, 0, "Engine Number *:", _txtEngineNumber);
            AddField(tbl, 0, 2, "Container #:", _txtContainerNumber);
            AddField(tbl, 0, 4, "Import Entry #:", _txtEntryNumber);
            AddField(tbl, 1, 0, "Entry Type:", _cmbEntryType);
            AddField(tbl, 1, 2, "C78 Entry Date:", _dtpC78Date);
            AddField(tbl, 1, 4, "Date Added:", _dtpDateEntered);
            AddField(tbl, 2, 0, "Supplier:", _cmbEntrySupplier);
            AddField(tbl, 2, 2, "Chassis Number:", _txtChassisNumber);
            AddField(tbl, 2, 4, "Part Number:", _txtPartNumber);
            AddField(tbl, 3, 0, "Description:", _txtDescription);
            AddField(tbl, 3, 2, "Customer (List):", _cmbEntryCustomer);
            AddField(tbl, 3, 4, "Customer Name:", _txtCustomerName);
            AddField(tbl, 4, 0, "Invoice #:", _txtInvoiceNumber);
            AddField(tbl, 4, 2, "Invoice Amount:", _txtInvoiceAmount);
            AddField(tbl, 4, 4, "Date Sold:", _dtpDateSold);
            AddField(tbl, 5, 0, "Discount:", _txtDiscount);

            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 8, 0, 0) };
            var btnAdd = new Button { Text = "Add New Engine", Width = 135, Height = 34, BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnUpd = new Button { Text = "Update Selected", Width = 135, Height = 34, BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnDel = new Button { Text = "Delete (Password)", Width = 140, Height = 34, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnLabel = new Button { Text = "Print Label", Width = 115, Height = 34, BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnReceipt = new Button { Text = "Print Receipt", Width = 120, Height = 34, BackColor = Color.FromArgb(102, 16, 242), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnClr = new Button { Text = "Clear Form", Width = 100, Height = 34 };

            btnAdd.Click += (s, e) => SaveEntry(false);
            btnUpd.Click += (s, e) => SaveEntry(true);
            btnDel.Click += (s, e) => DeleteEntry();
            btnLabel.Click += (s, e) => PrintDocumentPreview(isReceipt: false);
            btnReceipt.Click += (s, e) => PrintDocumentPreview(isReceipt: true);
            btnClr.Click += (s, e) => ClearEntryForm();

            btnRow.Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnLabel, btnReceipt, btnClr });
            formPanel.Controls.Add(btnRow);
            formPanel.Controls.Add(tbl);
            split.Panel2.Controls.Add(formPanel);
            page.Controls.Add(split);
        }

        private void BuildSuppliersTab(TabPage page)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 420 };
            var top = new Panel { Dock = DockStyle.Fill };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
            _txtSearchSuppliers.TextChanged += (s, e) => LoadSuppliers();
            bar.Controls.AddRange(new Control[] { new Label { Text = "Search Suppliers:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, _txtSearchSuppliers });
            StyleGrid(_gridSuppliers);
            _gridSuppliers.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var r = _gridSuppliers.Rows[e.RowIndex];
                _selectedSupplierId = Convert.ToInt64(r.Cells["ID"].Value);
                _txtSupName.Text = r.Cells["Name"].Value?.ToString() ?? "";
                _txtSupAddress.Text = r.Cells["Address"].Value?.ToString() ?? "";
                _txtSupPhone.Text = r.Cells["Telephone"].Value?.ToString() ?? "";
            };
            top.Controls.Add(_gridSuppliers);
            top.Controls.Add(bar);
            split.Panel1.Controls.Add(top);

            var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6 };
            for (int i = 0; i < 6; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i % 2 == 0 ? 11f : 22.3f));
            AddField(tbl, 0, 0, "Supplier Name *:", _txtSupName);
            AddField(tbl, 0, 2, "Telephone:", _txtSupPhone);
            AddField(tbl, 0, 4, "Address:", _txtSupAddress);

            var btns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 10, 0, 0) };
            var bAdd = new Button { Text = "Add Supplier", Width = 130, Height = 34, BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var bUpd = new Button { Text = "Update Supplier", Width = 130, Height = 34, BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var bDel = new Button { Text = "Delete (Password)", Width = 140, Height = 34, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var bClr = new Button { Text = "Clear", Width = 90, Height = 34 };

            bAdd.Click += (s, e) => SaveSupplier(false);
            bUpd.Click += (s, e) => SaveSupplier(true);
            bDel.Click += (s, e) => DeleteSupplier();
            bClr.Click += (s, e) => { _selectedSupplierId = null; _txtSupName.Clear(); _txtSupAddress.Clear(); _txtSupPhone.Clear(); };

            btns.Controls.AddRange(new Control[] { bAdd, bUpd, bDel, bClr });
            bottom.Controls.Add(btns);
            bottom.Controls.Add(tbl);
            split.Panel2.Controls.Add(bottom);
            page.Controls.Add(split);
        }

        private void BuildCustomersTab(TabPage page)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 390 };
            var top = new Panel { Dock = DockStyle.Fill };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
            _txtSearchCustomers.TextChanged += (s, e) => LoadCustomers();
            bar.Controls.AddRange(new Control[] { new Label { Text = "Search Customers:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) }, _txtSearchCustomers });
            StyleGrid(_gridCustomers);
            _gridCustomers.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var r = _gridCustomers.Rows[e.RowIndex];
                _selectedCustomerId = Convert.ToInt64(r.Cells["CustomerID"].Value);
                _txtCustFirst.Text = r.Cells["First_Name"].Value?.ToString() ?? "";
                _txtCustLast.Text = r.Cells["Last_Name"].Value?.ToString() ?? "";
                _txtCustTrn.Text = r.Cells["Tax_Registration_No"].Value?.ToString() ?? "";
                _txtCustAddr1.Text = r.Cells["Address1"].Value?.ToString() ?? "";
                _txtCustParish.Text = r.Cells["Parish"].Value?.ToString() ?? "";
                _txtCustPhone1.Text = r.Cells["Telephone_1"].Value?.ToString() ?? "";
                _txtCustCell.Text = r.Cells["Cell"].Value?.ToString() ?? "";_txtCustEmail.Text = r.Cells["Email"].Value?.ToString() ?? "";
_txtCustCredit.Text = r.Cells["CreditLimit"].Value?.ToString() ?? "0";
_chkCustBlocked.Checked = Convert.ToInt32(r.Cells["IsBlocked"].Value ?? 0) == 1;
};
top.Controls.Add(_gridCustomers);
top.Controls.Add(bar);
split.Panel1.Controls.Add(top);
var bottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
var tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6 };
for (int i = 0; i < 6; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i % 2 == 0 ? 11f : 22.3f));
AddField(tbl, 0, 0, "First Name:", _txtCustFirst);
AddField(tbl, 0, 2, "Last / Company *:", _txtCustLast);
AddField(tbl, 0, 4, "TRN:", _txtCustTrn);
AddField(tbl, 1, 0, "Address:", _txtCustAddr1);
AddField(tbl, 1, 2, "Parish:", _txtCustParish);
AddField(tbl, 1, 4, "Telephone 1:", _txtCustPhone1);
AddField(tbl, 2, 0, "Cell:", _txtCustCell);
AddField(tbl, 2, 2, "Email:", _txtCustEmail);
AddField(tbl, 2, 4, "Credit Limit:", _txtCustCredit);
tbl.Controls.Add(_chkCustBlocked, 1, 3);
var btns = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 10, 0, 0) };
var bAdd = new Button { Text = "Add Customer", Width = 130, Height = 34, BackColor = Color.FromArgb(25, 135, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
var bUpd = new Button { Text = "Update Customer", Width = 130, Height = 34, BackColor = Color.FromArgb(13, 110, 253), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
var bDel = new Button { Text = "Delete (Password)", Width = 140, Height = 34, BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
var bClr = new Button { Text = "Clear", Width = 90, Height = 34 };
bAdd.Click += (s, e) => SaveCustomer(false);
bUpd.Click += (s, e) => SaveCustomer(true);
bDel.Click += (s, e) => DeleteCustomer();
bClr.Click += (s, e) => { _selectedCustomerId = null; _txtCustFirst.Clear(); _txtCustLast.Clear(); _txtCustTrn.Clear(); _txtCustAddr1.Clear(); _txtCustParish.Clear(); _txtCustPhone1.Clear(); _txtCustCell.Clear(); _txtCustEmail.Clear(); _txtCustCredit.Text = "0"; _chkCustBlocked.Checked = false; };
btns.Controls.AddRange(new Control[] { bAdd, bUpd, bDel, bClr });
bottom.Controls.Add(btns);
bottom.Controls.Add(tbl);
split.Panel2.Controls.Add(bottom);
page.Controls.Add(split);
}
private SqliteConnection OpenConn()
{
var conn = new SqliteConnection(ConnStr);
conn.Open();
using var cmd = new SqliteCommand("PRAGMA busy_timeout=5000;", conn);
cmd.ExecuteNonQuery();
return conn;
}
private DataTable QueryTable(string sql, params SqliteParameter[] parameters)
{
using var conn = OpenConn();
using var cmd = new SqliteCommand(sql, conn);
cmd.Parameters.AddRange(parameters);
using var reader = cmd.ExecuteReader();
var dt = new DataTable();
dt.Load(reader);
return dt;
}
private void ExecNonQuery(string sql, params SqliteParameter[] parameters)
{
using var conn = OpenConn();
using var cmd = new SqliteCommand(sql, conn);
cmd.Parameters.AddRange(parameters);
cmd.ExecuteNonQuery();
}
private void LoadEntries()
{
string q = $"%{_txtSearchEntries.Text.Trim()}%";
string statusClause = _cmbStatusFilter.SelectedIndex switch
{
1 => "AND (e.DateSold IS NULL OR e.DateSold = '')",
2 => "AND (e.DateSold IS NOT NULL AND e.DateSold <> '')",
_ => ""
};
string sql = $@"
SELECT e.ID, e.EngineNumber, e.ContainerNumber, e.EntryNumber, e.EntryType,
e.C78EntryDate, e.DateEntered, s.Name AS SupplierName, s.Telephone AS SupplierPhone,
e.SupplierID, e.ChassisNumber, e.PartNumber, e.Description,
e.CustomerID, e.CustomerName, e.InvoiceNumber, e.InvoiceAmount, e.Discount, e.DateSold
FROM Entries e
LEFT JOIN Suppliers s ON e.SupplierID = s.ID
WHERE (e.EngineNumber LIKE @q OR IFNULL(e.ContainerNumber,'') LIKE @q
OR IFNULL(e.EntryNumber,'') LIKE @q OR IFNULL(e.ChassisNumber,'') LIKE @q
OR IFNULL(e.CustomerName,'') LIKE @q OR IFNULL(s.Name,'') LIKE @q)
{statusClause}
ORDER BY e.ID DESC";
_gridEntries.DataSource = QueryTable(sql, new SqliteParameter("@q", q));
if (_gridEntries.Columns.Contains("SupplierID")) _gridEntries.Columns["SupplierID"].Visible = false;
if (_gridEntries.Columns.Contains("CustomerID")) _gridEntries.Columns["CustomerID"].Visible = false;
}
private void GridEntries_CellClick(object? sender, DataGridViewCellEventArgs e)
{
if (e.RowIndex < 0) return;
var r = _gridEntries.Rows[e.RowIndex];
_selectedEntryId = Convert.ToInt64(r.Cells["ID"].Value);
_txtEngineNumber.Text = r.Cells["EngineNumber"].Value?.ToString() ?? "";
_txtContainerNumber.Text = r.Cells["ContainerNumber"].Value?.ToString() ?? "";
_txtEntryNumber.Text = r.Cells["EntryNumber"].Value?.ToString() ?? "";
_cmbEntryType.Text = r.Cells["EntryType"].Value?.ToString() ?? "C78";
_txtChassisNumber.Text = r.Cells["ChassisNumber"].Value?.ToString() ?? "";
_txtPartNumber.Text = r.Cells["PartNumber"].Value?.ToString() ?? "";
_txtDescription.Text = r.Cells["Description"].Value?.ToString() ?? "";
_txtCustomerName.Text = r.Cells["CustomerName"].Value?.ToString() ?? "";
_txtInvoiceNumber.Text = r.Cells["InvoiceNumber"].Value?.ToString() ?? "";
_txtInvoiceAmount.Text = r.Cells["InvoiceAmount"].Value?.ToString() ?? "";
_txtDiscount.Text = r.Cells["Discount"].Value?.ToString() ?? "";
if (DateTime.TryParse(r.Cells["C78EntryDate"].Value?.ToString(), out var c78)) { _dtpC78Date.Checked = true; _dtpC78Date.Value = c78; } else _dtpC78Date.Checked = false;
if (DateTime.TryParse(r.Cells["DateEntered"].Value?.ToString(), out var added)) _dtpDateEntered.Value = added;
if (DateTime.TryParse(r.Cells["DateSold"].Value?.ToString(), out var sold)) { _dtpDateSold.Checked = true; _dtpDateSold.Value = sold; } else _dtpDateSold.Checked = false;
long supId = r.Cells["SupplierID"].Value == DBNull.Value ? 0 : Convert.ToInt64(r.Cells["SupplierID"].Value);
SelectComboById(_cmbEntrySupplier, supId);
long custId = r.Cells["CustomerID"].Value == DBNull.Value ? 0 : Convert.ToInt64(r.Cells["CustomerID"].Value);
SelectComboById(_cmbEntryCustomer, custId);
}
private void SaveEntry(bool isUpdate)
{
if (string.IsNullOrWhiteSpace(_txtEngineNumber.Text)) { MessageBox.Show("Engine Number is required."); return; }
if (isUpdate && _selectedEntryId == null) { MessageBox.Show("Select an entry first to update."); return; }
long supId = (_cmbEntrySupplier.SelectedItem as ComboItem)?.Id ?? 0;
long custId = (_cmbEntryCustomer.SelectedItem as ComboItem)?.Id ?? 0;
int.TryParse(_txtInvoiceNumber.Text, out int invNo);
decimal.TryParse(_txtInvoiceAmount.Text, out decimal invAmt);
decimal.TryParse(_txtDiscount.Text, out decimal disc);
var p = new[]
{
new SqliteParameter("@eng", _txtEngineNumber.Text.Trim()),
new SqliteParameter("@etype", _cmbEntryType.Text.Trim()),
new SqliteParameter("@enum", _txtEntryNumber.Text.Trim()),
new SqliteParameter("@cont", _txtContainerNumber.Text.Trim()),
new SqliteParameter("@c78", _dtpC78Date.Checked ? _dtpC78Date.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value),
new SqliteParameter("@added", _dtpDateEntered.Value.ToString("yyyy-MM-dd")),
new SqliteParameter("@sup", supId > 0 ? supId : (object)DBNull.Value),
new SqliteParameter("@chas", _txtChassisNumber.Text.Trim()),
new SqliteParameter("@part", _txtPartNumber.Text.Trim()),
new SqliteParameter("@desc", _txtDescription.Text.Trim()),
new SqliteParameter("@cid", custId > 0 ? custId : (object)DBNull.Value),
new SqliteParameter("@cname", _txtCustomerName.Text.Trim()),
new SqliteParameter("@inv", invNo > 0 ? invNo : (object)DBNull.Value),
new SqliteParameter("@amt", invAmt),
new SqliteParameter("@disc", disc),
new SqliteParameter("@sold", _dtpDateSold.Checked ? _dtpDateSold.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value),
new SqliteParameter("@id", _selectedEntryId ?? 0)
};
if (!isUpdate)
ExecNonQuery(@"INSERT INTO Entries (EngineNumber, EntryType, EntryNumber, ContainerNumber, C78EntryDate, DateEntered, SupplierID, ChassisNumber, PartNumber, Description, CustomerID, CustomerName, InvoiceNumber, InvoiceAmount, Discount, DateSold)
VALUES (@eng, @etype, @enum, @cont, @c78, @added, @sup, @chas, @part, @desc, @cid, @cname, @inv, @amt, @disc, @sold)", p);
else
ExecNonQuery(@"UPDATE Entries SET EngineNumber=@eng, EntryType=@etype, EntryNumber=@enum, ContainerNumber=@cont, C78EntryDate=@c78, DateEntered=@added, SupplierID=@sup, ChassisNumber=@chas, PartNumber=@part, Description=@desc, CustomerID=@cid, CustomerName=@cname, InvoiceNumber=@inv, InvoiceAmount=@amt, Discount=@disc, DateSold=@sold
WHERE ID=@id", p);
ClearEntryForm();
LoadEntries();
}
private void DeleteEntry()
{
if (_selectedEntryId == null) { MessageBox.Show("Select an engine record first."); return; }
if (!VerifyDeletePassword()) return;
if (MessageBox.Show("Permanently delete the selected engine record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
{
ExecNonQuery("DELETE FROM Entries WHERE ID=@id", new SqliteParameter("@id", _selectedEntryId));
ClearEntryForm();
LoadEntries();
}
}
private void ClearEntryForm()
{
_selectedEntryId = null;
_txtEngineNumber.Clear(); _txtContainerNumber.Clear(); _txtEntryNumber.Clear();
_txtChassisNumber.Clear(); _txtPartNumber.Clear(); _txtDescription.Clear();
_txtCustomerName.Clear(); _txtInvoiceNumber.Clear(); _txtInvoiceAmount.Clear(); _txtDiscount.Clear();
_dtpDateSold.Checked = false;
if (_cmbEntrySupplier.Items.Count > 0) _cmbEntrySupplier.SelectedIndex = 0;
if (_cmbEntryCustomer.Items.Count > 0) _cmbEntryCustomer.SelectedIndex = 0;
}
private void LoadSuppliers()
{
string q = $"%{_txtSearchSuppliers.Text.Trim()}%";
_gridSuppliers.DataSource = QueryTable("SELECT ID, Name, Address, Telephone FROM Suppliers WHERE Name LIKE @q OR IFNULL(Telephone,'') LIKE @q ORDER BY Name", new SqliteParameter("@q", q));
var all = QueryTable("SELECT ID, Name FROM Suppliers ORDER BY Name");
_cmbEntrySupplier.Items.Clear();
_cmbEntrySupplier.Items.Add(new ComboItem(0, "-- Select Supplier --"));
foreach (DataRow r in all.Rows)
_cmbEntrySupplier.Items.Add(new ComboItem(Convert.ToInt64(r["ID"]), r["Name"].ToString() ?? ""));
_cmbEntrySupplier.SelectedIndex = 0;
}
private void SaveSupplier(bool isUpdate)
{
if (string.IsNullOrWhiteSpace(_txtSupName.Text)) { MessageBox.Show("Supplier Name is required."); return; }
if (isUpdate && _selectedSupplierId == null) { MessageBox.Show("Select a supplier first."); return; }
var p = new[]
{
new SqliteParameter("@n", _txtSupName.Text.Trim()),
new SqliteParameter("@a", _txtSupAddress.Text.Trim()),
new SqliteParameter("@t", _txtSupPhone.Text.Trim()),
new SqliteParameter("@id", _selectedSupplierId ?? 0)
};
if (!isUpdate) ExecNonQuery("INSERT INTO Suppliers (Name, Address, Telephone) VALUES (@n, @a, @t)", p);
else ExecNonQuery("UPDATE Suppliers SET Name=@n, Address=@a, Telephone=@t WHERE ID=@id", p);
_selectedSupplierId = null; _txtSupName.Clear(); _txtSupAddress.Clear(); _txtSupPhone.Clear();
LoadSuppliers(); LoadEntries();
}
private void DeleteSupplier()
{
if (_selectedSupplierId == null) { MessageBox.Show("Select a supplier first."); return; }
if (!VerifyDeletePassword()) return;
if (MessageBox.Show("Delete selected supplier?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
{
ExecNonQuery("DELETE FROM Suppliers WHERE ID=@id", new SqliteParameter("@id", _selectedSupplierId));
_selectedSupplierId = null;
LoadSuppliers();
}
}
private void LoadCustomers()
{
string q = $"%{_txtSearchCustomers.Text.Trim()}%";
_gridCustomers.DataSource = QueryTable(@"SELECT CustomerID, First_Name, Last_Name, Tax_Registration_No, Address1, Parish, Telephone_1, Cell, Email, CreditLimit, IsBlocked
FROM Customer
WHERE Last_Name LIKE @q OR IFNULL(First_Name,'') LIKE @q OR IFNULL(Tax_Registration_No,'') LIKE @q OR IFNULL(Telephone_1,'') LIKE @q OR IFNULL(Cell,'') LIKE @q
ORDER BY Last_Name", new SqliteParameter("@q", q));
var all = QueryTable("SELECT CustomerID, TRIM(IFNULL(First_Name,'') || ' ' || Last_Name) AS FullName FROM Customer ORDER BY Last_Name");
_cmbEntryCustomer.Items.Clear();
_cmbEntryCustomer.Items.Add(new ComboItem(0, "-- Select Customer --"));
foreach (DataRow r in all.Rows)
_cmbEntryCustomer.Items.Add(new ComboItem(Convert.ToInt64(r["CustomerID"]), r["FullName"].ToString() ?? ""));
_cmbEntryCustomer.SelectedIndex = 0;
}
private void SaveCustomer(bool isUpdate)
{
if (string.IsNullOrWhiteSpace(_txtCustLast.Text)) { MessageBox.Show("Last / Company Name is required."); return; }
if (isUpdate && _selectedCustomerId == null) { MessageBox.Show("Select a customer first."); return; }
decimal.TryParse(_txtCustCredit.Text, out decimal credit);
var p = new[]
{
new SqliteParameter("@fn", _txtCustFirst.Text.Trim()),
new SqliteParameter("@ln", _txtCustLast.Text.Trim()),
new SqliteParameter("@trn", _txtCustTrn.Text.Trim()),
new SqliteParameter("@a1", _txtCustAddr1.Text.Trim()),
new SqliteParameter("@par", _txtCustParish.Text.Trim()),
new SqliteParameter("@p1", _txtCustPhone1.Text.Trim()),
new SqliteParameter("@cell", _txtCustCell.Text.Trim()),
new SqliteParameter("@em", _txtCustEmail.Text.Trim()),
new SqliteParameter("@cr", credit),
new SqliteParameter("@blk", _chkCustBlocked.Checked ? 1 : 0),
new SqliteParameter("@id", _selectedCustomerId ?? 0)
};
if (!isUpdate)
ExecNonQuery(@"INSERT INTO Customer (First_Name, Last_Name, Tax_Registration_No, Address1, Parish, Telephone_1, Cell, Email, CreditLimit, IsBlocked, Date_Open)
VALUES (@fn, @ln, @trn, @a1, @par, @p1, @cell, @em, @cr, @blk, datetime('now','localtime'))", p);
else
ExecNonQuery(@"UPDATE Customer SET First_Name=@fn, Last_Name=@ln, Tax_Registration_No=@trn, Address1=@a1, Parish=@par, Telephone_1=@p1, Cell=@cell, Email=@em, CreditLimit=@cr, IsBlocked=@blk, Last_Updated=datetime('now','localtime')
WHERE CustomerID=@id", p);
_selectedCustomerId = null;
LoadCustomers();
}
private void DeleteCustomer()
{
if (_selectedCustomerId == null) { MessageBox.Show("Select a customer first."); return; }
if (!VerifyDeletePassword()) return;
if (MessageBox.Show("Delete selected customer?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
{
ExecNonQuery("DELETE FROM Customer WHERE CustomerID=@id", new SqliteParameter("@id", _selectedCustomerId));
_selectedCustomerId = null;
LoadCustomers();
}
}
private void ImportEnginesCsv()
{
using var ofd = new OpenFileDialog { Filter = "CSV File (.csv)|.csv", Title = "Select Engine CSV File" };
if (ofd.ShowDialog() != DialogResult.OK) return;
int imported = 0;
using var parser = new TextFieldParser(ofd.FileName);
parser.SetDelimiters(",");
parser.HasFieldsEnclosedInQuotes = true;
if (parser.EndOfData) return;
string[] headers = (parser.ReadFields() ?? Array.Empty<string>()).Select(h => h.Trim()).ToArray();
int Col(string name) => Array.FindIndex(headers, h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
int iEng = Col("EngineNumber");
if (iEng < 0) { MessageBox.Show("CSV must contain an 'EngineNumber' column header.", "Invalid CSV"); return; }
int iCont = Col("ContainerNumber"), iEntry = Col("EntryNumber"), iType = Col("EntryType"),
iC78 = Col("C78EntryDate"), iAdded = Col("DateEntered"), iSup = Col("SupplierName"),
iChas = Col("ChassisNumber"), iPart = Col("PartNumber"), iDesc = Col("Description"),
iCust = Col("CustomerName"), iInv = Col("InvoiceNumber"), iAmt = Col("InvoiceAmount"),
iDisc = Col("Discount"), iSold = Col("DateSold");
string Val(string[] f, int idx) => (idx >= 0 && idx < f.Length) ? f[idx].Trim() : "";
using var conn = OpenConn();
using var tx = conn.BeginTransaction();
while (!parser.EndOfData)
{
string[] f = parser.ReadFields() ?? Array.Empty<string>();
string eng = Val(f, iEng);
if (string.IsNullOrWhiteSpace(eng)) continue;
long? supId = null;
string supName = Val(f, iSup);
if (!string.IsNullOrWhiteSpace(supName))
{
using var findCmd = new SqliteCommand("SELECT ID FROM Suppliers WHERE Name=@n LIMIT 1", conn, tx);
findCmd.Parameters.AddWithValue("@n", supName);
var res = findCmd.ExecuteScalar();
if (res != null && res != DBNull.Value) supId = Convert.ToInt64(res);
else
{
using var insSup = new SqliteCommand("INSERT INTO Suppliers (Name) VALUES (@n); SELECT last_insert_rowid();", conn, tx);
insSup.Parameters.AddWithValue("@n", supName);
supId = Convert.ToInt64(insSup.ExecuteScalar());
}
}
int.TryParse(Val(f, iInv), out int invNo);
decimal.TryParse(Val(f, iAmt), out decimal amt);
decimal.TryParse(Val(f, iDisc), out decimal disc);
string added = string.IsNullOrWhiteSpace(Val(f, iAdded)) ? DateTime.Now.ToString("yyyy-MM-dd") : Val(f, iAdded);
using var cmd = new SqliteCommand(@"INSERT INTO Entries (EngineNumber, EntryType, EntryNumber, ContainerNumber, C78EntryDate, DateEntered, SupplierID, ChassisNumber, PartNumber, Description, CustomerName, InvoiceNumber, InvoiceAmount, Discount, DateSold)
VALUES (@eng, @et, @en, @cn, @c78, @ad, @sid, @ch, @pt, @ds, @cu, @iv, @am, @dc, @sd)", conn, tx);
cmd.Parameters.AddWithValue("@eng", eng);
cmd.Parameters.AddWithValue("@et", string.IsNullOrWhiteSpace(Val(f, iType)) ? "C78" : Val(f, iType));
cmd.Parameters.AddWithValue("@en", Val(f, iEntry));
cmd.Parameters.AddWithValue("@cn", Val(f, iCont));
cmd.Parameters.AddWithValue("@c78", string.IsNullOrWhiteSpace(Val(f, iC78)) ? DBNull.Value : Val(f, iC78));
cmd.Parameters.AddWithValue("@ad", added);
cmd.Parameters.AddWithValue("@sid", supId.HasValue ? supId.Value : DBNull.Value);
cmd.Parameters.AddWithValue("@ch", Val(f, iChas));
cmd.Parameters.AddWithValue("@pt", Val(f, iPart));
cmd.Parameters.AddWithValue("@ds", Val(f, iDesc));
cmd.Parameters.AddWithValue("@cu", Val(f, iCust));
cmd.Parameters.AddWithValue("@iv", invNo > 0 ? invNo : DBNull.Value);
cmd.Parameters.AddWithValue("@am", amt);
cmd.Parameters.AddWithValue("@dc", disc);
cmd.Parameters.AddWithValue("@sd", string.IsNullOrWhiteSpace(Val(f, iSold)) ? DBNull.Value : Val(f, iSold));
cmd.ExecuteNonQuery();
imported++;
}
tx.Commit();
RefreshAllData();
MessageBox.Show($"Imported {imported} engine records successfully!", "Engine Import Complete");
}
private void ImportCustomersCsv()
{
using var ofd = new OpenFileDialog { Filter = "CSV File (.csv)|.csv", Title = "Select Customer CSV File" };
if (ofd.ShowDialog() != DialogResult.OK) return;
int imported = 0;
using var parser = new TextFieldParser(ofd.FileName);
parser.SetDelimiters(",");
parser.HasFieldsEnclosedInQuotes = true;
if (parser.EndOfData) return;
string[] headers = (parser.ReadFields() ?? Array.Empty<string>()).Select(h => h.Trim()).ToArray();
int Col(string name) => Array.FindIndex(headers, h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
int iLast = Col("Last_Name");
if (iLast < 0) { MessageBox.Show("CSV must contain a 'Last_Name' column header.", "Invalid CSV"); return; }
int iFirst = Col("First_Name"), iTrn = Col("Tax_Registration_No"), iAddr = Col("Address1"),
iPar = Col("Parish"), iPh1 = Col("Telephone_1"), iCell = Col("Cell"),
iEm = Col("Email"), iCr = Col("CreditLimit"), iBlk = Col("IsBlocked");
string Val(string[] f, int idx) => (idx >= 0 && idx < f.Length) ? f[idx].Trim() : "";
using var conn = OpenConn();
using var tx = conn.BeginTransaction();
while (!parser.EndOfData)
{
string[] f = parser.ReadFields() ?? Array.Empty<string>();
string ln = Val(f, iLast);
if (string.IsNullOrWhiteSpace(ln)) continue;
decimal.TryParse(Val(f, iCr), out decimal credit);
int.TryParse(Val(f, iBlk), out int blocked);
using var cmd = new SqliteCommand(@"INSERT INTO Customer (First_Name, Last_Name, Tax_Registration_No, Address1, Parish, Telephone_1, Cell, Email, CreditLimit, IsBlocked, Date_Open)
VALUES (@fn, @ln, @trn, @a1, @par, @p1, @cell, @em, @cr, @blk, datetime('now','localtime'))", conn, tx);
cmd.Parameters.AddWithValue("@fn", Val(f, iFirst));
cmd.Parameters.AddWithValue("@ln", ln);
cmd.Parameters.AddWithValue("@trn", Val(f, iTrn));
cmd.Parameters.AddWithValue("@a1", Val(f, iAddr));
cmd.Parameters.AddWithValue("@par", Val(f, iPar));
cmd.Parameters.AddWithValue("@p1", Val(f, iPh1));
cmd.Parameters.AddWithValue("@cell", Val(f, iCell));
cmd.Parameters.AddWithValue("@em", Val(f, iEm));
cmd.Parameters.AddWithValue("@cr", credit);
cmd.Parameters.AddWithValue("@blk", blocked);
cmd.ExecuteNonQuery();
imported++;
}
tx.Commit();
LoadCustomers();
MessageBox.Show($"Imported {imported} customers successfully!", "Customer Import Complete");
}
private void PrintDocumentPreview(bool isReceipt)
{
if (string.IsNullOrWhiteSpace(_txtEngineNumber.Text))
{
MessageBox.Show("Select or enter an engine record first to print.", "Nothing to Print");
return;
}
var doc = new PrintDocument();
doc.PrintPage += (s, e) =>
{
var g = e.Graphics!;
using var fTitle = new Font("Segoe UI", 14, FontStyle.Bold);
using var fSub = new Font("Segoe UI", 10, FontStyle.Bold);
using var fBody = new Font("Consolas", 10);
float y = 30;
g.DrawString("EngineEntry", fTitle, Brushes.Black, 30, y); y += 28;
g.DrawString(isReceipt ? "OFFICIAL ENGINE SALES RECEIPT" : "WAREHOUSE ENGINE INVENTORY LABEL", fSub, Brushes.Black, 30, y); y += 24;
g.DrawLine(Pens.Black, 30, y, 520, y); y += 12;
void Line(string k, string v) { g.DrawString($"{k,-18}: {v}", fBody, Brushes.Black, 30, y); y += 20; }
Line("Engine Number", _txtEngineNumber.Text.Trim());
Line("Part / Desc", $"{_txtPartNumber.Text} {_txtDescription.Text}".Trim());
Line("Chassis Number", _txtChassisNumber.Text.Trim());
Line("Container #", _txtContainerNumber.Text.Trim());
Line("Import Entry #", $"{_cmbEntryType.Text} - {_txtEntryNumber.Text}".Trim());
Line("C78 Landing Date", _dtpC78Date.Checked ? _dtpC78Date.Value.ToString("yyyy-MM-dd") : "N/A");
Line("Supplier", _cmbEntrySupplier.Text);
if (isReceipt)
{
y += 8; g.DrawLine(Pens.Black, 30, y, 520, y); y += 12;
decimal.TryParse(_txtInvoiceAmount.Text, out decimal amt);
decimal.TryParse(_txtDiscount.Text, out decimal disc);
Line("Customer", _txtCustomerName.Text.Trim());
Line("Invoice #", _txtInvoiceNumber.Text.Trim());
Line("Date Sold", _dtpDateSold.Checked ? _dtpDateSold.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd"));
Line("Invoice Amount", $"${amt:N2}");
Line("Discount", $"${disc:N2}");
Line("NET TOTAL", $"${(amt - disc):N2}");
y += 20;
g.DrawString("Customer Signature: _________________________", fBody, Brushes.Black, 30, y);
}
else
{
y += 8; g.DrawLine(Pens.Black, 30, y, 520, y); y += 12;
Line("Date Logged", _dtpDateEntered.Value.ToString("yyyy-MM-dd"));
Line("Printed", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
}
};
using var preview = new PrintPreviewDialog { Document = doc, Width = 800, Height = 650 };
preview.ShowDialog();
}
private static void SelectComboById(ComboBox cmb, long id)
{
for (int i = 0; i < cmb.Items.Count; i++)
if (cmb.Items[i] is ComboItem item && item.Id == id) { cmb.SelectedIndex = i; return; }
if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
}
private void ExportEntriesCsv()
{
using var sfd = new SaveFileDialog { Filter = "CSV File (.csv)|.csv", FileName = $"EngineEntries_{DateTime.Now:yyyyMMdd}.csv" };
if (sfd.ShowDialog() != DialogResult.OK) return;
var dt = (DataTable)_gridEntries.DataSource;
var sb = new StringBuilder();
string q = ((char)34).ToString();
var cols = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName != "SupplierID" && c.ColumnName != "CustomerID").ToList();
sb.AppendLine(string.Join(",", cols.Select(c => q + c.ColumnName + q)));
foreach (DataRow row in dt.Rows)
sb.AppendLine(string.Join(",", cols.Select(c => q + (row[c]?.ToString() ?? "").Replace(q, q + q) + q)));
File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
MessageBox.Show("CSV exported successfully!", "Export Complete");
}
private void BackupDatabase()
{
using var sfd = new SaveFileDialog { Filter = "SQLite Database (.db)|.db", FileName = $"EngineEntry_Backup_{DateTime.Now:yyyyMMdd_HHmm}.db" };
if (sfd.ShowDialog() != DialogResult.OK) return;
SqliteConnection.ClearAllPools();
File.Copy(_dbPath, sfd.FileName, overwrite: true);
MessageBox.Show("Database backup saved!", "Backup Complete");
}
private sealed record ComboItem(long Id, string Text) { public override string ToString() => Text; }
}
}



