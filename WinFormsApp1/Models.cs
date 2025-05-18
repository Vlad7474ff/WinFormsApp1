using System;
using System.Collections.Generic;

namespace WinFormsApp1
{
    public class Сотрудник
    {
        public int Сотрудник_id { get; set; }
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public int? Табельный_номер { get; set; }
        public DateTime? Дата_приема { get; set; }

        // Навигационное свойство
        public virtual ICollection<Табель> Табели { get; set; }
    }

    public class Статус_дня
    {
        public int Статус_id { get; set; }
        public string Название_Рабочий_Отпуск_Больни { get; set; }

        // Навигационное свойство
        public virtual ICollection<Табель> Табели { get; set; }
    }

    public class Табель
    {
        public int Табель_id { get; set; }
        public DateTime? Дата { get; set; }
        public int? Часы_работы { get; set; }
        public int Сотрудник_id { get; set; }
        public int Статус_id { get; set; }

        // Навигационные свойства
        public virtual Сотрудник Сотрудник { get; set; }
        public virtual Статус_дня Статус { get; set; }
        public virtual ICollection<Отклонение> Отклонения { get; set; }
    }

    public class Отклонение
    {
        public int Отклонение_id { get; set; }
        public string Тип_Переработка_Недоработка { get; set; }
        public int? Часы { get; set; }
        public int Табель_id { get; set; }
        public int Сотрудник_id { get; set; }
        public int Статус_id { get; set; }

        // Навигационное свойство
        public virtual Табель Табель { get; set; }
    }
}
