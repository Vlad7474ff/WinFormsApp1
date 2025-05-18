using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class RelatedTablesForm : Form
    {
        private string _entityType;
        private int _id1;
        private int? _id2;
        private int? _id3;

        private DataGridView dgvMain;
        private DataGridView dgvRelated;
        private Label lblMainInfo;
        private Label lblRelatedInfo;

        public RelatedTablesForm(string entityType, int id1, int? id2 = null, int? id3 = null)
        {
            _entityType = entityType;
            _id1 = id1;
            _id2 = id2;
            _id3 = id3;

            // Базовая настройка формы
            this.Text = $"Связанные записи для {_entityType}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            string title = _entityType switch
            {
                "Сотрудник" => $"Связанные записи для сотрудника #{_id1}",
                "Статус_дня" => $"Связанные записи для статуса #{_id1}",
                "Табель" => $"Связанные записи для табеля #{_id1}",
                "Отклонение" => $"Связанные записи для отклонения #{_id1}",
                _ => "Связанные записи"
            };

            this.Text = title;

            // Основная информация
            lblMainInfo = new Label
            {
                Text = $"Информация о {_entityType}:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };
            Controls.Add(lblMainInfo);

            dgvMain = new DataGridView
            {
                Location = new Point(20, 50),
                Size = new Size(850, 100),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgvMain);

            // Связанные записи
            lblRelatedInfo = new Label
            {
                Text = "Связанные записи:",
                Location = new Point(20, 170),
                AutoSize = true,
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };
            Controls.Add(lblRelatedInfo);

            dgvRelated = new DataGridView
            {
                Location = new Point(20, 200),
                Size = new Size(850, 300),
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgvRelated);

            // Кнопка закрытия
            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(750, 520),
                Width = 120,
                Height = 40,
                DialogResult = DialogResult.OK
            };
            Controls.Add(btnClose);
        }

        private void LoadData()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    switch (_entityType)
                    {
                        case "Сотрудник":
                            LoadEmployeeData(context);
                            break;
                        case "Статус_дня":
                            LoadStatusData(context);
                            break;
                        case "Табель":
                            LoadTimesheetData(context);
                            break;
                        case "Отклонение":
                            LoadDeviationData(context);
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

        private void LoadEmployeeData(AppDbContext context)
        {
            // Загружаем информацию о сотруднике
            var employee = context.Сотрудники
                .Where(e => e.Сотрудник_id == _id1)
                .Select(e => new
                {
                    e.Сотрудник_id,
                    e.Фамилия,
                    e.Имя,
                    e.Отчество,
                    e.Табельный_номер,
                    e.Дата_приема
                })
                .FirstOrDefault();

            if (employee != null)
            {
                dgvMain.DataSource = new[] { employee };
                lblMainInfo.Text = $"Информация о сотруднике: {employee.Фамилия} {employee.Имя} {employee.Отчество}";

                // Загружаем табели сотрудника
                var timesheets = context.Табели
                    .Where(t => t.Сотрудник_id == _id1)
                    .Include(t => t.Статус)
                    .Select(t => new
                    {
                        t.Табель_id,
                        t.Дата,
                        t.Часы_работы,
                        t.Статус_id,
                        Статус = t.Статус.Название_Рабочий_Отпуск_Больни
                    })
                    .ToList();

                dgvRelated.DataSource = timesheets;
                lblRelatedInfo.Text = $"Табели сотрудника ({timesheets.Count}):";
            }
        }

        private void LoadStatusData(AppDbContext context)
        {
            // Загружаем информацию о статусе
            var status = context.Статусы_дней
                .Where(s => s.Статус_id == _id1)
                .Select(s => new
                {
                    s.Статус_id,
                    s.Название_Рабочий_Отпуск_Больни
                })
                .FirstOrDefault();

            if (status != null)
            {
                dgvMain.DataSource = new[] { status };
                lblMainInfo.Text = $"Информация о статусе: {status.Название_Рабочий_Отпуск_Больни}";

                // Загружаем табели с этим статусом
                var timesheets = context.Табели
                    .Where(t => t.Статус_id == _id1)
                    .Include(t => t.Сотрудник)
                    .Select(t => new
                    {
                        t.Табель_id,
                        t.Дата,
                        t.Часы_работы,
                        t.Сотрудник_id,
                        Сотрудник = t.Сотрудник.Фамилия + " " + t.Сотрудник.Имя
                    })
                    .ToList();

                dgvRelated.DataSource = timesheets;
                lblRelatedInfo.Text = $"Табели с этим статусом ({timesheets.Count}):";
            }
        }

        private void LoadTimesheetData(AppDbContext context)
        {
            if (!_id2.HasValue || !_id3.HasValue) return;

            // Загружаем информацию о табеле
            var timesheet = context.Табели
                .Where(t => t.Табель_id == _id1 && t.Сотрудник_id == _id2.Value && t.Статус_id == _id3.Value)
                .Include(t => t.Сотрудник)
                .Include(t => t.Статус)
                .Select(t => new
                {
                    t.Табель_id,
                    t.Дата,
                    t.Часы_работы,
                    t.Сотрудник_id,
                    Сотрудник = t.Сотрудник.Фамилия + " " + t.Сотрудник.Имя,
                    t.Статус_id,
                    Статус = t.Статус.Название_Рабочий_Отпуск_Больни
                })
                .FirstOrDefault();

            if (timesheet != null)
            {
                dgvMain.DataSource = new[] { timesheet };
                lblMainInfo.Text = $"Информация о табеле #{timesheet.Табель_id} от {timesheet.Дата:dd.MM.yyyy}";

                // Загружаем отклонения для этого табеля
                var deviations = context.Отклонения
                    .Where(o => o.Табель_id == _id1 && o.Сотрудник_id == _id2.Value && o.Статус_id == _id3.Value)
                    .Select(o => new
                    {
                        o.Отклонение_id,
                        o.Тип_Переработка_Недоработка,
                        o.Часы
                    })
                    .ToList();

                dgvRelated.DataSource = deviations;
                lblRelatedInfo.Text = $"Отклонения по табелю ({deviations.Count}):";
            }
        }

        private void LoadDeviationData(AppDbContext context)
        {
            // Загружаем информацию об отклонении
            var deviation = context.Отклонения
                .Where(o => o.Отклонение_id == _id1)
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
                    Табель_Дата = o.Табель.Дата,
                    o.Сотрудник_id,
                    Сотрудник = o.Табель.Сотрудник.Фамилия + " " + o.Табель.Сотрудник.Имя,
                    o.Статус_id,
                    Статус = o.Табель.Статус.Название_Рабочий_Отпуск_Больни
                })
                .FirstOrDefault();

            if (deviation != null)
            {
                dgvMain.DataSource = new[] { deviation };
                lblMainInfo.Text = $"Информация об отклонении #{deviation.Отклонение_id}: {deviation.Тип_Переработка_Недоработка}";

                // Загружаем информацию о связанном табеле
                var timesheet = context.Табели
                    .Where(t => t.Табель_id == deviation.Табель_id &&
                               t.Сотрудник_id == deviation.Сотрудник_id &&
                               t.Статус_id == deviation.Статус_id)
                    .Include(t => t.Сотрудник)
                    .Include(t => t.Статус)
                    .Select(t => new
                    {
                        t.Табель_id,
                        t.Дата,
                        t.Часы_работы,
                        t.Сотрудник_id,
                        Сотрудник = t.Сотрудник.Фамилия + " " + t.Сотрудник.Имя,
                        t.Статус_id,
                        Статус = t.Статус.Название_Рабочий_Отпуск_Больни
                    })
                    .ToList();

                dgvRelated.DataSource = timesheet;
                lblRelatedInfo.Text = "Связанный табель:";
            }
        }
    }
}
