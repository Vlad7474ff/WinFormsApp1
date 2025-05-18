using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private ComboBox cmbEntities;
        private DataGridView dataGridView;
        private BindingSource bindingSource = new BindingSource();

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            // ��������� �����
            this.Text = "������� ���������� �����";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // �������� ComboBox ��� ������ ��������
            Label lblEntity = new Label
            {
                Text = "�������� ��������:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(lblEntity);

            cmbEntities = new ComboBox
            {
                Location = new Point(150, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEntities.Items.AddRange(new string[] { "���������", "������_���", "������", "����������" });
            cmbEntities.SelectedIndex = 0;
            cmbEntities.SelectedIndexChanged += (s, e) => LoadData();
            Controls.Add(cmbEntities);

            // �������� DataGridView
            dataGridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(940, 400),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                DataSource = bindingSource
            };
            Controls.Add(dataGridView);

            // �������� ������
            var btnAdd = new Button { Text = "����", Size = new Size(150, 40), Location = new Point(20, 480) };
            var btnEdit = new Button { Text = "��������������", Size = new Size(150, 40), Location = new Point(190, 480) };
            var btnDelete = new Button { Text = "�������� ������", Size = new Size(150, 40), Location = new Point(360, 480) };
            var btnSearch = new Button { Text = "����� � ����������", Size = new Size(150, 40), Location = new Point(530, 480) };
            var btnRelated = new Button { Text = "��������� �������", Size = new Size(150, 40), Location = new Point(700, 480) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRelated.Click += BtnRelated_Click;

            Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnSearch, btnRelated });
        }

        private void LoadData()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    switch (cmbEntities.SelectedItem.ToString())
                    {
                        case "���������":
                            var employees = context.����������
                                .Select(s => new
                                {
                                    s.���������_id,
                                    s.�������,
                                    s.���,
                                    s.��������,
                                    s.���������_�����,
                                    s.����_������
                                })
                                .ToList();
                            bindingSource.DataSource = employees;
                            break;

                        case "������_���":
                            var statuses = context.�������_����
                                .Select(s => new
                                {
                                    s.������_id,
                                    s.��������_�������_������_������
                                })
                                .ToList();
                            bindingSource.DataSource = statuses;
                            break;

                        case "������":
                            var timesheets = context.������
                                .Include(t => t.���������)
                                .Include(t => t.������)
                                .Select(t => new
                                {
                                    t.������_id,
                                    t.����,
                                    t.����_������,
                                    t.���������_id,
                                    ���_���������� = t.���������.������� + " " + t.���������.���,
                                    t.������_id,
                                    ������ = t.������.��������_�������_������_������
                                })
                                .ToList();
                            bindingSource.DataSource = timesheets;
                            break;

                        case "����������":
                            var deviations = context.����������
                                .Include(o => o.������)
                                .ThenInclude(t => t.���������)
                                .Include(o => o.������)
                                .ThenInclude(t => t.������)
                                .Select(o => new
                                {
                                    o.����������_id,
                                    o.���_�����������_�����������,
                                    o.����,
                                    o.������_id,
                                    o.���������_id,
                                    ���_���������� = o.������.���������.������� + " " + o.������.���������.���,
                                    o.������_id,
                                    ������ = o.������.������.��������_�������_������_������
                                })
                                .ToList();
                            bindingSource.DataSource = deviations;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ �������� ������: {ex.Message}", "������",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var entity = cmbEntities.SelectedItem.ToString();
            Form form = null;

            switch (entity)
            {
                case "���������":
                    form = new ���������Form();
                    break;
                case "������_���":
                    form = new ���������Form();
                    break;
                case "������":
                    form = new ������Form();
                    break;
                case "����������":
                    form = new ����������Form();
                    break;
            }

            if (form != null && form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;

            var entity = cmbEntities.SelectedItem.ToString();
            Form form = null;

            switch (entity)
            {
                case "���������":
                    int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                    form = new ���������Form(employeeId);
                    break;
                case "������_���":
                    int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    form = new ���������Form(statusId);
                    break;
                case "������":
                    int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                    int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    form = new ������Form(timesheetId, empId, statId);
                    break;
                case "����������":
                    int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["����������_id"].Value);
                    int tabId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    int sotrudnikId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                    int statusDayId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    form = new ����������Form(deviationId, tabId, sotrudnikId, statusDayId);
                    break;
            }

            if (form != null && form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;

            var entity = cmbEntities.SelectedItem.ToString();

            string message = $"�� �������, ��� ������ ������� ��������� ������?";
            if (MessageBox.Show(message, "������������� ��������",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var context = new AppDbContext())
                    {
                        switch (entity)
                        {
                            case "���������":
                                int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                                var employee = context.����������.Find(employeeId);
                                if (employee != null)
                                {
                                    context.����������.Remove(employee);
                                }
                                break;

                            case "������_���":
                                int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                                var status = context.�������_����.Find(statusId);
                                if (status != null)
                                {
                                    context.�������_����.Remove(status);
                                }
                                break;

                            case "������":
                                int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                                int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                                int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                                var timesheet = context.������.Find(timesheetId, empId, statId);
                                if (timesheet != null)
                                {
                                    context.������.Remove(timesheet);
                                }
                                break;

                            case "����������":
                                int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["����������_id"].Value);
                                int tabId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                                int sotrudnikId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                                int statusDayId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                                var deviation = context.����������.Find(deviationId, tabId, sotrudnikId, statusDayId);
                                if (deviation != null)
                                {
                                    context.����������.Remove(deviation);
                                }
                                break;
                        }

                        context.SaveChanges();
                        LoadData();
                        MessageBox.Show("������ ������� �������", "����������",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��������: {ex.Message}", "������",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var entity = cmbEntities.SelectedItem.ToString();
            Form form = null;

            switch (entity)
            {
                case "���������":
                    form = new SearchSortForm("���������");
                    break;
                case "������_���":
                    form = new SearchSortForm("������_���");
                    break;
                case "������":
                    form = new SearchSortForm("������");
                    break;
                case "����������":
                    form = new SearchSortForm("����������");
                    break;
            }

            if (form != null)
            {
                form.ShowDialog();
            }
        }

        private void BtnRelated_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null) return;

            var entity = cmbEntities.SelectedItem.ToString();
            Form form = null;

            switch (entity)
            {
                case "���������":
                    int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                    form = new RelatedTablesForm("���������", employeeId);
                    break;
                case "������_���":
                    int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    form = new RelatedTablesForm("������_���", statusId);
                    break;
                case "������":
                    int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["���������_id"].Value);
                    int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["������_id"].Value);
                    form = new RelatedTablesForm("������", timesheetId, empId, statId);
                    break;
                case "����������":
                    int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["����������_id"].Value);
                    form = new RelatedTablesForm("����������", deviationId);
                    break;
            }

            if (form != null)
            {
                form.ShowDialog();
            }
        }
    }
}
