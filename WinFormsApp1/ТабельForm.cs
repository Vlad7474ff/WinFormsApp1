using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class ТабельForm : Form
    {
        private Табель _табель;
        private bool _isEdit;

        private NumericUpDown nudTimesheetId;
        private DateTimePicker dtpDate;
        private NumericUpDown nudHours;
        private ComboBox cmbEmployee;
        private ComboBox cmbStatus;

        public ТабельForm(int? timesheetId = null, int? employeeId = null, int? statusId = null)
        {
            InitializeComponent();

            _табель = new Табель();

            if (timesheetId.HasValue && employeeId.HasValue && statusId.HasValue)
            {
                _isEdit = true;
                using (var context = new AppDbContext())
                {
                    var loadedEntity = context.Табели.Find(timesheetId.Value, employeeId.Value, statusId.Value);
                    if (loadedEntity != null)
                    {
                        _табель.Табель_id = loadedEntity.Табель_id;
                        _табель.Дата = loadedEntity.Дата;
                        _табель.Часы_работы = loadedEntity.Часы_работы;
                        _табель.Сотрудник_id = loadedEntity.Сотрудник_id;
                        _табель.Статус_id = loadedEntity.Статус_id;
                    }
                    else
                    {
                        _isEdit = false;
                    }
                }
            }
            else
            {
                _isEdit = false;
            }

            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = _isEdit ? "Редактирование табеля" : "Добавление табеля";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Элементы управления
            int labelX = 20;
            int controlX = 180;
            int y = 20;
            int padding = 40;
            int controlWidth = 220;

            // ID табеля
            Controls.Add(new Label { Text = "ID табеля:", Location = new Point(labelX, y), AutoSize = true });
            nudTimesheetId = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 9999
            };

            if (_табель.Табель_id > 0)
            {
                nudTimesheetId.Value = _табель.Табель_id;
            }

            nudTimesheetId.Enabled = !_isEdit;
            Controls.Add(nudTimesheetId);
            y += padding;

            // Дата
            Controls.Add(new Label { Text = "Дата:", Location = new Point(labelX, y), AutoSize = true });
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short
            };

            if (_табель.Дата.HasValue)
            {
                dtpDate.Value = _табель.Дата.Value;
            }
            else
            {
                dtpDate.Value = DateTime.Now;
            }

            Controls.Add(dtpDate);
            y += padding;

            // Часы работы
            Controls.Add(new Label { Text = "Часы работы:", Location = new Point(labelX, y), AutoSize = true });
            nudHours = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 24
            };

            if (_табель.Часы_работы.HasValue)
            {
                nudHours.Value = _табель.Часы_работы.Value;
            }

            Controls.Add(nudHours);
            y += padding;

            // Сотрудник
            Controls.Add(new Label { Text = "Сотрудник:", Location = new Point(labelX, y), AutoSize = true });
            cmbEmployee = new ComboBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Загружаем список сотрудников
            using (var context = new AppDbContext())
            {
                var employees = context.Сотрудники
                    .Select(e => new
                    {
                        e.Сотрудник_id,
                        ФИО = $"{e.Фамилия} {e.Имя} {e.Отчество}"
                    })
                    .ToList();

                cmbEmployee.DataSource = employees;
                cmbEmployee.DisplayMember = "ФИО";
                cmbEmployee.ValueMember = "Сотрудник_id";

                if (_табель.Сотрудник_id > 0)
                {
                    cmbEmployee.SelectedValue = _табель.Сотрудник_id;
                }
            }

            Controls.Add(cmbEmployee);
            y += padding;

            // Статус
            Controls.Add(new Label { Text = "Статус:", Location = new Point(labelX, y), AutoSize = true });
            cmbStatus = new ComboBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Загружаем список статусов
            using (var context = new AppDbContext())
            {
                var statuses = context.Статусы_дней
                    .Select(s => new
                    {
                        s.Статус_id,
                        s.Название_Рабочий_Отпуск_Больни
                    })
                    .ToList();

                cmbStatus.DataSource = statuses;
                cmbStatus.DisplayMember = "Название_Рабочий_Отпуск_Больни";
                cmbStatus.ValueMember = "Статус_id";

                if (_табель.Статус_id > 0)
                {
                    cmbStatus.SelectedValue = _табель.Статус_id;
                }
            }

            Controls.Add(cmbStatus);
            y += padding + 10;

            // Кнопки
            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(controlX - 50, y),
                Width = 100,
                DialogResult = DialogResult.None
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(controlX + 60, y),
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка данных
                if (cmbEmployee.SelectedValue == null || cmbStatus.SelectedValue == null)
                {
                    MessageBox.Show("Выберите сотрудника и статус", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Сохранение данных
                _табель.Табель_id = (int)nudTimesheetId.Value;
                _табель.Дата = dtpDate.Value;
                _табель.Часы_работы = (int)nudHours.Value;
                _табель.Сотрудник_id = (int)cmbEmployee.SelectedValue;
                _табель.Статус_id = (int)cmbStatus.SelectedValue;

                using (var context = new AppDbContext())
                {
                    if (_isEdit)
                    {
                        context.Entry(_табель).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Табели.Add(_табель);
                    }

                    context.SaveChanges();

                    MessageBox.Show(_isEdit ? "Табель успешно обновлен" : "Табель успешно добавлен",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
