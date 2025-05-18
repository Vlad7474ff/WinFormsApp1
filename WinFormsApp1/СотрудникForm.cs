using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class СотрудникForm : Form
    {
        private Сотрудник _сотрудник;
        private bool _isEdit;

        private NumericUpDown nudEmployeeId;
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private NumericUpDown nudTabNumber;
        private DateTimePicker dtpHireDate;

        public СотрудникForm(int? id = null)
        {
            InitializeComponent();

            _сотрудник = new Сотрудник();

            if (id.HasValue)
            {
                _isEdit = true;
                using (var context = new AppDbContext())
                {
                    var loadedEntity = context.Сотрудники.Find(id.Value);
                    if (loadedEntity != null)
                    {
                        _сотрудник.Сотрудник_id = loadedEntity.Сотрудник_id;
                        _сотрудник.Фамилия = loadedEntity.Фамилия;
                        _сотрудник.Имя = loadedEntity.Имя;
                        _сотрудник.Отчество = loadedEntity.Отчество;
                        _сотрудник.Табельный_номер = loadedEntity.Табельный_номер;
                        _сотрудник.Дата_приема = loadedEntity.Дата_приема;
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
            this.Text = _isEdit ? "Редактирование сотрудника" : "Добавление сотрудника";
            this.Size = new Size(450, 350);
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

            // ID сотрудника
            Controls.Add(new Label { Text = "ID сотрудника:", Location = new Point(labelX, y), AutoSize = true });
            nudEmployeeId = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 9999
            };

            if (_сотрудник.Сотрудник_id > 0)
            {
                nudEmployeeId.Value = _сотрудник.Сотрудник_id;
            }

            nudEmployeeId.Enabled = !_isEdit;
            Controls.Add(nudEmployeeId);
            y += padding;

            // Фамилия
            Controls.Add(new Label { Text = "Фамилия:", Location = new Point(labelX, y), AutoSize = true });
            txtLastName = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Text = _сотрудник.Фамилия
            };
            Controls.Add(txtLastName);
            y += padding;

            // Имя
            Controls.Add(new Label { Text = "Имя:", Location = new Point(labelX, y), AutoSize = true });
            txtFirstName = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Text = _сотрудник.Имя
            };
            Controls.Add(txtFirstName);
            y += padding;

            // Отчество
            Controls.Add(new Label { Text = "Отчество:", Location = new Point(labelX, y), AutoSize = true });
            txtMiddleName = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Text = _сотрудник.Отчество
            };
            Controls.Add(txtMiddleName);
            y += padding;

            // Табельный номер
            Controls.Add(new Label { Text = "Табельный номер:", Location = new Point(labelX, y), AutoSize = true });
            nudTabNumber = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 999999
            };

            if (_сотрудник.Табельный_номер.HasValue)
            {
                nudTabNumber.Value = _сотрудник.Табельный_номер.Value;
            }

            Controls.Add(nudTabNumber);
            y += padding;

            // Дата приема
            Controls.Add(new Label { Text = "Дата приема:", Location = new Point(labelX, y), AutoSize = true });
            dtpHireDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Format = DateTimePickerFormat.Short
            };

            if (_сотрудник.Дата_приема.HasValue)
            {
                dtpHireDate.Value = _сотрудник.Дата_приема.Value;
            }
            else
            {
                dtpHireDate.Value = DateTime.Now;
            }

            Controls.Add(dtpHireDate);
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
                if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Поля 'Фамилия' и 'Имя' должны быть заполнены", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Сохранение данных
                _сотрудник.Сотрудник_id = (int)nudEmployeeId.Value;
                _сотрудник.Фамилия = txtLastName.Text;
                _сотрудник.Имя = txtFirstName.Text;
                _сотрудник.Отчество = txtMiddleName.Text;
                _сотрудник.Табельный_номер = (int)nudTabNumber.Value;
                _сотрудник.Дата_приема = dtpHireDate.Value;

                using (var context = new AppDbContext())
                {
                    if (_isEdit)
                    {
                        context.Entry(_сотрудник).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Сотрудники.Add(_сотрудник);
                    }

                    context.SaveChanges();

                    MessageBox.Show(_isEdit ? "Сотрудник успешно обновлен" : "Сотрудник успешно добавлен",
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
