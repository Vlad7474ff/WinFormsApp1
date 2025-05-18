using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class ОтклонениеForm : Form
    {
        private Отклонение _отклонение;
        private bool _isEdit;

        private NumericUpDown nudDeviationId;
        private TextBox txtType;
        private NumericUpDown nudHours;
        private ComboBox cmbTimesheet;

        public ОтклонениеForm(int? deviationId = null, int? timesheetId = null, int? employeeId = null, int? statusId = null)
        {
            InitializeComponent();

            _отклонение = new Отклонение();

            if (deviationId.HasValue && timesheetId.HasValue && employeeId.HasValue && statusId.HasValue)
            {
                _isEdit = true;
                using (var context = new AppDbContext())
                {
                    var loadedEntity = context.Отклонения.Find(deviationId.Value, timesheetId.Value, employeeId.Value, statusId.Value);
                    if (loadedEntity != null)
                    {
                        _отклонение.Отклонение_id = loadedEntity.Отклонение_id;
                        _отклонение.Тип_Переработка_Недоработка = loadedEntity.Тип_Переработка_Недоработка;
                        _отклонение.Часы = loadedEntity.Часы;
                        _отклонение.Табель_id = loadedEntity.Табель_id;
                        _отклонение.Сотрудник_id = loadedEntity.Сотрудник_id;
                        _отклонение.Статус_id = loadedEntity.Статус_id;
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
            this.Text = _isEdit ? "Редактирование отклонения" : "Добавление отклонения";
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

            // ID отклонения
            Controls.Add(new Label { Text = "ID отклонения:", Location = new Point(labelX, y), AutoSize = true });
            nudDeviationId = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 9999
            };

            if (_отклонение.Отклонение_id > 0)
            {
                nudDeviationId.Value = _отклонение.Отклонение_id;
            }

            nudDeviationId.Enabled = !_isEdit;
            Controls.Add(nudDeviationId);
            y += padding;

            // Тип отклонения
            Controls.Add(new Label { Text = "Тип отклонения:", Location = new Point(labelX, y), AutoSize = true });
            txtType = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Text = _отклонение.Тип_Переработка_Недоработка
            };
            Controls.Add(txtType);
            y += padding;

            // Часы
            Controls.Add(new Label { Text = "Часы:", Location = new Point(labelX, y), AutoSize = true });
            nudHours = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = -24,
                Maximum = 24
            };

            if (_отклонение.Часы.HasValue)
            {
                nudHours.Value = _отклонение.Часы.Value;
            }

            Controls.Add(nudHours);
            y += padding;

            // Табель
            Controls.Add(new Label { Text = "Табель:", Location = new Point(labelX, y), AutoSize = true });
            cmbTimesheet = new ComboBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Загружаем список табелей
            using (var context = new AppDbContext())
            {
                var timesheets = context.Табели
                    .Include(t => t.Сотрудник)
                    .Include(t => t.Статус)
                    .Select(t => new
                    {
                        t.Табель_id,
                        t.Сотрудник_id,
                        t.Статус_id,
                        Описание = $"Табель #{t.Табель_id} - {t.Сотрудник.Фамилия} {t.Сотрудник.Имя} - {t.Дата:dd.MM.yyyy} - {t.Статус.Название_Рабочий_Отпуск_Больни}"
                    })
                    .ToList();

                cmbTimesheet.DataSource = timesheets;
                cmbTimesheet.DisplayMember = "Описание";
                cmbTimesheet.ValueMember = "Табель_id";

                // Если это редактирование, выбираем соответствующий табель
                if (_isEdit)
                {
                    foreach (var item in timesheets)
                    {
                        if (item.Табель_id == _отклонение.Табель_id &&
                            item.Сотрудник_id == _отклонение.Сотрудник_id &&
                            item.Статус_id == _отклонение.Статус_id)
                        {
                            cmbTimesheet.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            cmbTimesheet.SelectedIndexChanged += CmbTimesheet_SelectedIndexChanged;
            Controls.Add(cmbTimesheet);
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

        private void CmbTimesheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTimesheet.SelectedItem != null)
            {
                dynamic selectedItem = cmbTimesheet.SelectedItem;
                _отклонение.Табель_id = selectedItem.Табель_id;
                _отклонение.Сотрудник_id = selectedItem.Сотрудник_id;
                _отклонение.Статус_id = selectedItem.Статус_id;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка данных
                if (string.IsNullOrWhiteSpace(txtType.Text) || cmbTimesheet.SelectedItem == null)
                {
                    MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Сохранение данных
                _отклонение.Отклонение_id = (int)nudDeviationId.Value;
                _отклонение.Тип_Переработка_Недоработка = txtType.Text;
                _отклонение.Часы = (int)nudHours.Value;

                using (var context = new AppDbContext())
                {
                    if (_isEdit)
                    {
                        context.Entry(_отклонение).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Отклонения.Add(_отклонение);
                    }

                    context.SaveChanges();

                    MessageBox.Show(_isEdit ? "Отклонение успешно обновлено" : "Отклонение успешно добавлено",
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
