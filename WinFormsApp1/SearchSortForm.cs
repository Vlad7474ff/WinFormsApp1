using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class SearchSortForm : Form
    {
        private string _entityType;
        private TextBox txtSearch;
        private ComboBox cmbSearchField;
        private ComboBox cmbSortField;
        private RadioButton rbAscending;
        private RadioButton rbDescending;
        private DataGridView dgvResults;
        private Button btnSearch;

        public SearchSortForm(string entityType)
        {
            _entityType = entityType;

            // Базовая настройка формы
            this.Text = $"Поиск и сортировка - {_entityType}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();
        }

        private void SetupUI()
        {
            // Создание элементов управления
            int labelX = 20;
            int controlX = 160;
            int y = 20;
            int padding = 40;
            int controlWidth = 200;

            // Строка поиска
            Controls.Add(new Label { Text = "Поиск:", Location = new Point(labelX, y), AutoSize = true });
            txtSearch = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth
            };
            Controls.Add(txtSearch);
            y += padding;

            // Поле для поиска
            Controls.Add(new Label { Text = "Поле для поиска:", Location = new Point(labelX, y), AutoSize = true });
            cmbSearchField = new ComboBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Заполняем поля для поиска в зависимости от типа сущности
            switch (_entityType)
            {
                case "Сотрудник":
                    cmbSearchField.Items.AddRange(new string[] { "Фамилия", "Имя", "Отчество", "Табельный номер" });
                    break;
                case "Статус_дня":
                    cmbSearchField.Items.AddRange(new string[] { "Название" });
                    break;
                case "Табель":
                    cmbSearchField.Items.AddRange(new string[] { "Дата", "Часы работы", "Сотрудник", "Статус" });
                    break;
                case "Отклонение":
                    cmbSearchField.Items.AddRange(new string[] { "Тип", "Часы", "Сотрудник", "Статус" });
                    break;
            }

            if (cmbSearchField.Items.Count > 0)
                cmbSearchField.SelectedIndex = 0;

            Controls.Add(cmbSearchField);
            y += padding;

            // Поле для сортировки
            Controls.Add(new Label { Text = "Сортировать по:", Location = new Point(labelX, y), AutoSize = true });
            cmbSortField = new ComboBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Заполняем поля для сортировки в зависимости от типа сущности
            switch (_entityType)
            {
                case "Сотрудник":
                    cmbSortField.Items.AddRange(new string[] { "ID", "Фамилия", "Имя", "Табельный номер", "Дата приема" });
                    break;
                case "Статус_дня":
                    cmbSortField.Items.AddRange(new string[] { "ID", "Название" });
                    break;
                case "Табель":
                    cmbSortField.Items.AddRange(new string[] { "ID", "Дата", "Часы работы", "Сотрудник", "Статус" });
                    break;
                case "Отклонение":
                    cmbSortField.Items.AddRange(new string[] { "ID", "Тип", "Часы", "Табель", "Сотрудник", "Статус" });
                    break;
            }

            if (cmbSortField.Items.Count > 0)
                cmbSortField.SelectedIndex = 0;

            Controls.Add(cmbSortField);
            y += padding;

            // Направление сортировки
            Controls.Add(new Label { Text = "Направление:", Location = new Point(labelX, y), AutoSize = true });
            rbAscending = new RadioButton
            {
                Text = "По возрастанию",
                Location = new Point(controlX, y),
                Checked = true
            };
            rbDescending = new RadioButton
            {
                Text = "По убыванию",
                Location = new Point(controlX + 120, y)
            };
            Controls.Add(rbAscending);
            Controls.Add(rbDescending);
            y += padding;

            // Кнопка поиска
            btnSearch = new Button
            {
                Text = "Найти",
                Location = new Point(controlX, y),
                Width = 100
            };
            btnSearch.Click += BtnSearch_Click;

            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(controlX + 110, y),
                Width = 100,
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnSearch);
            Controls.Add(btnClose);
            y += padding;

            // Таблица результатов
            dgvResults = new DataGridView
            {
                Location = new Point(labelX, y),
                Size = new Size(750, 250),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgvResults);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    switch (_entityType)
                    {
                        case "Сотрудник":
                            SearchSortEmployees(context);
                            break;
                        case "Статус_дня":
                            SearchSortStatuses(context);
                            break;
                        case "Табель":
                            SearchSortTimesheets(context);
                            break;
                        case "Отклонение":
                            SearchSortDeviations(context);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchSortEmployees(AppDbContext context)
        {
            var query = context.Сотрудники.AsQueryable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                switch (cmbSearchField.SelectedItem.ToString())
                {
                    case "Фамилия":
                        query = query.Where(s => s.Фамилия.ToLower().Contains(searchText));
                        break;
                    case "Имя":
                        query = query.Where(s => s.Имя.ToLower().Contains(searchText));
                        break;
                    case "Отчество":
                        query = query.Where(s => s.Отчество.ToLower().Contains(searchText));
                        break;
                    case "Табельный номер":
                        if (int.TryParse(searchText, out int tabNum))
                        {
                            query = query.Where(s => s.Табельный_номер == tabNum);
                        }
                        break;
                }
            }

            // Сортировка
            bool isAscending = rbAscending.Checked;
            switch (cmbSortField.SelectedItem.ToString())
            {
                case "ID":
                    query = isAscending ? query.OrderBy(s => s.Сотрудник_id) : query.OrderByDescending(s => s.Сотрудник_id);
                    break;
                case "Фамилия":
                    query = isAscending ? query.OrderBy(s => s.Фамилия) : query.OrderByDescending(s => s.Фамилия);
                    break;
                case "Имя":
                    query = isAscending ? query.OrderBy(s => s.Имя) : query.OrderByDescending(s => s.Имя);
                    break;
                case "Табельный номер":
                    query = isAscending ? query.OrderBy(s => s.Табельный_номер) : query.OrderByDescending(s => s.Табельный_номер);
                    break;
                case "Дата приема":
                    query = isAscending ? query.OrderBy(s => s.Дата_приема) : query.OrderByDescending(s => s.Дата_приема);
                    break;
            }

            dgvResults.DataSource = query.ToList();
        }

        private void SearchSortStatuses(AppDbContext context)
        {
            var query = context.Статусы_дней.AsQueryable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                query = query.Where(s => s.Название_Рабочий_Отпуск_Больни.ToLower().Contains(searchText));
            }

            // Сортировка
            bool isAscending = rbAscending.Checked;
            switch (cmbSortField.SelectedItem.ToString())
            {
                case "ID":
                    query = isAscending ? query.OrderBy(s => s.Статус_id) : query.OrderByDescending(s => s.Статус_id);
                    break;
                case "Название":
                    query = isAscending ? query.OrderBy(s => s.Название_Рабочий_Отпуск_Больни) : query.OrderByDescending(s => s.Название_Рабочий_Отпуск_Больни);
                    break;
            }

            dgvResults.DataSource = query.ToList();
        }

        private void SearchSortTimesheets(AppDbContext context)
        {
            var query = context.Табели
                .Include(t => t.Сотрудник)
                .Include(t => t.Статус)
                .AsQueryable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                switch (cmbSearchField.SelectedItem.ToString())
                {
                    case "Дата":
                        if (DateTime.TryParse(searchText, out DateTime date))
                        {
                            query = query.Where(t => t.Дата.HasValue && t.Дата.Value.Date == date.Date);
                        }
                        break;
                    case "Часы работы":
                        if (int.TryParse(searchText, out int hours))
                        {
                            query = query.Where(t => t.Часы_работы == hours);
                        }
                        break;
                    case "Сотрудник":
                        query = query.Where(t => t.Сотрудник.Фамилия.ToLower().Contains(searchText) ||
                                               t.Сотрудник.Имя.ToLower().Contains(searchText));
                        break;
                    case "Статус":
                        query = query.Where(t => t.Статус.Название_Рабочий_Отпуск_Больни.ToLower().Contains(searchText));
                        break;
                }
            }

            // Сортировка
            bool isAscending = rbAscending.Checked;
            switch (cmbSortField.SelectedItem.ToString())
            {
                case "ID":
                    query = isAscending ? query.OrderBy(t => t.Табель_id) : query.OrderByDescending(t => t.Табель_id);
                    break;
                case "Дата":
                    query = isAscending ? query.OrderBy(t => t.Дата) : query.OrderByDescending(t => t.Дата);
                    break;
                case "Часы работы":
                    query = isAscending ? query.OrderBy(t => t.Часы_работы) : query.OrderByDescending(t => t.Часы_работы);
                    break;
                case "Сотрудник":
                    query = isAscending ? query.OrderBy(t => t.Сотрудник.Фамилия) : query.OrderByDescending(t => t.Сотрудник.Фамилия);
                    break;
                case "Статус":
                    query = isAscending ? query.OrderBy(t => t.Статус.Название_Рабочий_Отпуск_Больни) : query.OrderByDescending(t => t.Статус.Название_Рабочий_Отпуск_Больни);
                    break;
            }

            var result = query.Select(t => new
            {
                t.Табель_id,
                t.Дата,
                t.Часы_работы,
                t.Сотрудник_id,
                ФИО_сотрудника = t.Сотрудник.Фамилия + " " + t.Сотрудник.Имя,
                t.Статус_id,
                Статус = t.Статус.Название_Рабочий_Отпуск_Больни
            }).ToList();

            dgvResults.DataSource = result;
        }

        private void SearchSortDeviations(AppDbContext context)
        {
            var query = context.Отклонения
                .Include(o => o.Табель)
                .ThenInclude(t => t.Сотрудник)
                .Include(o => o.Табель)
                .ThenInclude(t => t.Статус)
                .AsQueryable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                switch (cmbSearchField.SelectedItem.ToString())
                {
                    case "Тип":
                        query = query.Where(o => o.Тип_Переработка_Недоработка.ToLower().Contains(searchText));
                        break;
                    case "Часы":
                        if (int.TryParse(searchText, out int hours))
                        {
                            query = query.Where(o => o.Часы == hours);
                        }
                        break;
                    case "Сотрудник":
                        query = query.Where(o => o.Табель.Сотрудник.Фамилия.ToLower().Contains(searchText) ||
                                              o.Табель.Сотрудник.Имя.ToLower().Contains(searchText));
                        break;
                    case "Статус":
                        query = query.Where(o => o.Табель.Статус.Название_Рабочий_Отпуск_Больни.ToLower().Contains(searchText));
                        break;
                }
            }

            // Сортировка
            bool isAscending = rbAscending.Checked;
            switch (cmbSortField.SelectedItem.ToString())
            {
                case "ID":
                    query = isAscending ? query.OrderBy(o => o.Отклонение_id) : query.OrderByDescending(o => o.Отклонение_id);
                    break;
                case "Тип":
                    query = isAscending ? query.OrderBy(o => o.Тип_Переработка_Недоработка) : query.OrderByDescending(o => o.Тип_Переработка_Недоработка);
                    break;
                case "Часы":
                    query = isAscending ? query.OrderBy(o => o.Часы) : query.OrderByDescending(o => o.Часы);
                    break;
                case "Табель":
                    query = isAscending ? query.OrderBy(o => o.Табель_id) : query.OrderByDescending(o => o.Табель_id);
                    break;
                case "Сотрудник":
                    query = isAscending ? query.OrderBy(o => o.Табель.Сотрудник.Фамилия) : query.OrderByDescending(o => o.Табель.Сотрудник.Фамилия);
                    break;
                case "Статус":
                    query = isAscending ? query.OrderBy(o => o.Табель.Статус.Название_Рабочий_Отпуск_Больни) : query.OrderByDescending(o => o.Табель.Статус.Название_Рабочий_Отпуск_Больни);
                    break;
            }

            var result = query.Select(o => new
            {
                o.Отклонение_id,
                o.Тип_Переработка_Недоработка,
                o.Часы,
                o.Табель_id,
                o.Сотрудник_id,
                ФИО_сотрудника = o.Табель.Сотрудник.Фамилия + " " + o.Табель.Сотрудник.Имя,
                o.Статус_id,
                Статус = o.Табель.Статус.Название_Рабочий_Отпуск_Больни
            }).ToList();

            dgvResults.DataSource = result;
        }
    }
}
