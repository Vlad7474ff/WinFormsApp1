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
            // Настройка формы
            this.Text = "Система табельного учета";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создание ComboBox для выбора сущности
            Label lblEntity = new Label
            {
                Text = "Выберите сущность:",
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
            cmbEntities.Items.AddRange(new string[] { "Сотрудник", "Статус_дня", "Табель", "Отклонение" });
            cmbEntities.SelectedIndex = 0;
            cmbEntities.SelectedIndexChanged += (s, e) => LoadData();
            Controls.Add(cmbEntities);

            // Создание DataGridView
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

            // Создание кнопок
            var btnAdd = new Button { Text = "Ввод", Size = new Size(150, 40), Location = new Point(20, 480) };
            var btnEdit = new Button { Text = "Редактирование", Size = new Size(150, 40), Location = new Point(190, 480) };
            var btnDelete = new Button { Text = "Удаление данных", Size = new Size(150, 40), Location = new Point(360, 480) };
            var btnSearch = new Button { Text = "Поиск и сортировка", Size = new Size(150, 40), Location = new Point(530, 480) };
            var btnRelated = new Button { Text = "Связанные таблицы", Size = new Size(150, 40), Location = new Point(700, 480) };

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
                        case "Сотрудник":
                            var employees = context.Сотрудники
                                .Select(s => new
                                {
                                    s.Сотрудник_id,
                                    s.Фамилия,
                                    s.Имя,
                                    s.Отчество,
                                    s.Табельный_номер,
                                    s.Дата_приема
                                })
                                .ToList();
                            bindingSource.DataSource = employees;
                            break;

                        case "Статус_дня":
                            var statuses = context.Статусы_дней
                                .Select(s => new
                                {
                                    s.Статус_id,
                                    s.Название_Рабочий_Отпуск_Больни
                                })
                                .ToList();
                            bindingSource.DataSource = statuses;
                            break;

                        case "Табель":
                            var timesheets = context.Табели
                                .Include(t => t.Сотрудник)
                                .Include(t => t.Статус)
                                .Select(t => new
                                {
                                    t.Табель_id,
                                    t.Дата,
                                    t.Часы_работы,
                                    t.Сотрудник_id,
                                    ФИО_сотрудника = t.Сотрудник.Фамилия + " " + t.Сотрудник.Имя,
                                    t.Статус_id,
                                    Статус = t.Статус.Название_Рабочий_Отпуск_Больни
                                })
                                .ToList();
                            bindingSource.DataSource = timesheets;
                            break;

                        case "Отклонение":
                            var deviations = context.Отклонения
                                .Include(o => o.Табель)
                                .ThenInclude(t => t.Сотрудник)
                                .Include(o => o.Табель)
                                .ThenInclude(t => t.Статус)
                                .Select(o => new
                                {
                                    o.Отклонение_id,
                                    o.Тип_Переработка_Недоработка,
                                    o.Часы,
                                    o.Табель_id,
                                    o.Сотрудник_id,
                                    ФИО_сотрудника = o.Табель.Сотрудник.Фамилия + " " + o.Табель.Сотрудник.Имя,
                                    o.Статус_id,
                                    Статус = o.Табель.Статус.Название_Рабочий_Отпуск_Больни
                                })
                                .ToList();
                            bindingSource.DataSource = deviations;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var entity = cmbEntities.SelectedItem.ToString();
            Form form = null;

            switch (entity)
            {
                case "Сотрудник":
                    form = new СотрудникForm();
                    break;
                case "Статус_дня":
                    form = new СтатусДняForm();
                    break;
                case "Табель":
                    form = new ТабельForm();
                    break;
                case "Отклонение":
                    form = new ОтклонениеForm();
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
                case "Сотрудник":
                    int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                    form = new СотрудникForm(employeeId);
                    break;
                case "Статус_дня":
                    int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                    form = new СтатусДняForm(statusId);
                    break;
                case "Табель":
                    int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Табель_id"].Value);
                    int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                    int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                    form = new ТабельForm(timesheetId, empId, statId);
                    break;
                case "Отклонение":
                    int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Отклонение_id"].Value);
                    int tabId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Табель_id"].Value);
                    int sotrudnikId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                    int statusDayId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                    form = new ОтклонениеForm(deviationId, tabId, sotrudnikId, statusDayId);
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

            string message = $"Вы уверены, что хотите удалить выбранную запись?";
            if (MessageBox.Show(message, "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var context = new AppDbContext())
                    {
                        switch (entity)
                        {
                            case "Сотрудник":
                                int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                                var employee = context.Сотрудники.Find(employeeId);
                                if (employee != null)
                                {
                                    context.Сотрудники.Remove(employee);
                                }
                                break;

                            case "Статус_дня":
                                int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                                var status = context.Статусы_дней.Find(statusId);
                                if (status != null)
                                {
                                    context.Статусы_дней.Remove(status);
                                }
                                break;

                            case "Табель":
                                int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Табель_id"].Value);
                                int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                                int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                                var timesheet = context.Табели.Find(timesheetId, empId, statId);
                                if (timesheet != null)
                                {
                                    context.Табели.Remove(timesheet);
                                }
                                break;

                            case "Отклонение":
                                int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Отклонение_id"].Value);
                                int tabId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Табель_id"].Value);
                                int sotrudnikId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                                int statusDayId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                                var deviation = context.Отклонения.Find(deviationId, tabId, sotrudnikId, statusDayId);
                                if (deviation != null)
                                {
                                    context.Отклонения.Remove(deviation);
                                }
                                break;
                        }

                        context.SaveChanges();
                        LoadData();
                        MessageBox.Show("Запись успешно удалена", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
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
                case "Сотрудник":
                    form = new SearchSortForm("Сотрудник");
                    break;
                case "Статус_дня":
                    form = new SearchSortForm("Статус_дня");
                    break;
                case "Табель":
                    form = new SearchSortForm("Табель");
                    break;
                case "Отклонение":
                    form = new SearchSortForm("Отклонение");
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
                case "Сотрудник":
                    int employeeId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                    form = new RelatedTablesForm("Сотрудник", employeeId);
                    break;
                case "Статус_дня":
                    int statusId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                    form = new RelatedTablesForm("Статус_дня", statusId);
                    break;
                case "Табель":
                    int timesheetId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Табель_id"].Value);
                    int empId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Сотрудник_id"].Value);
                    int statId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Статус_id"].Value);
                    form = new RelatedTablesForm("Табель", timesheetId, empId, statId);
                    break;
                case "Отклонение":
                    int deviationId = Convert.ToInt32(dataGridView.CurrentRow.Cells["Отклонение_id"].Value);
                    form = new RelatedTablesForm("Отклонение", deviationId);
                    break;
            }

            if (form != null)
            {
                form.ShowDialog();
            }
        }
    }
}
