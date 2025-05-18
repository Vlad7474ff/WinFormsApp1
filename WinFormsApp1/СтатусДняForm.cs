using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp1
{
    public partial class СтатусДняForm : Form
    {
        private Статус_дня _статус;
        private bool _isEdit;

        private NumericUpDown nudStatusId;
        private TextBox txtStatusName;

        public СтатусДняForm(int? id = null)
        {
            InitializeComponent();

            _статус = new Статус_дня();

            if (id.HasValue)
            {
                _isEdit = true;
                using (var context = new AppDbContext())
                {
                    var loadedEntity = context.Статусы_дней.Find(id.Value);
                    if (loadedEntity != null)
                    {
                        _статус.Статус_id = loadedEntity.Статус_id;
                        _статус.Название_Рабочий_Отпуск_Больни = loadedEntity.Название_Рабочий_Отпуск_Больни;
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
            this.Text = _isEdit ? "Редактирование статуса дня" : "Добавление статуса дня";
            this.Size = new Size(450, 200);
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

            // ID статуса
            Controls.Add(new Label { Text = "ID статуса:", Location = new Point(labelX, y), AutoSize = true });
            nudStatusId = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Minimum = 0,
                Maximum = 9999
            };

            if (_статус.Статус_id > 0)
            {
                nudStatusId.Value = _статус.Статус_id;
            }

            nudStatusId.Enabled = !_isEdit;
            Controls.Add(nudStatusId);
            y += padding;

            // Название статуса
            Controls.Add(new Label { Text = "Название статуса:", Location = new Point(labelX, y), AutoSize = true });
            txtStatusName = new TextBox
            {
                Location = new Point(controlX, y),
                Width = controlWidth,
                Text = _статус.Название_Рабочий_Отпуск_Больни
            };
            Controls.Add(txtStatusName);
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
                if (string.IsNullOrWhiteSpace(txtStatusName.Text))
                {
                    MessageBox.Show("Поле 'Название статуса' должно быть заполнено", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Сохранение данных
                _статус.Статус_id = (int)nudStatusId.Value;
                _статус.Название_Рабочий_Отпуск_Больни = txtStatusName.Text;

                using (var context = new AppDbContext())
                {
                    if (_isEdit)
                    {
                        context.Entry(_статус).State = EntityState.Modified;
                    }
                    else
                    {
                        context.Статусы_дней.Add(_статус);
                    }

                    context.SaveChanges();

                    MessageBox.Show(_isEdit ? "Статус дня успешно обновлен" : "Статус дня успешно добавлен",
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
