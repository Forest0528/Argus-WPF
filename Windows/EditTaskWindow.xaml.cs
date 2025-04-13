using System;
using System.Windows;
using System.Windows.Controls;
using Argus_WPF; // Где TaskItem

namespace Argus_WPF.Windows
{
    public partial class EditTaskWindow : Window
    {
        private TaskItem _taskItem;

        public EditTaskWindow(TaskItem task)
        {
            InitializeComponent();
            _taskItem = task;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Заполняем поля из _taskItem
            txtExecutor.Text = _taskItem.Executor;
            txtDescription.Text = _taskItem.Description;

            // Синхронизируем ComboBox со статусом
            cmbStatus.SelectedItem = null;
            foreach (var item in cmbStatus.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content.ToString() == _taskItem.Status)
                {
                    cmbStatus.SelectedItem = comboItem;
                    break;
                }
            }

            // Дата создания (через DatePicker)
            dateCreated.SelectedDate = _taskItem.CreatedAt.Date;

            // Последнее обновление (read-only)
            txtLastUpdated.Text = _taskItem.LastUpdated.ToString("dd.MM.yyyy HH:mm");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Простейшая проверка: Исполнитель не пустой
            if (string.IsNullOrWhiteSpace(txtExecutor.Text))
            {
                MessageBox.Show("Поле 'Исполнитель' не может быть пустым.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                txtExecutor.Focus();
                return;
            }
            // Ещё можно проверять Описание, если нужно

            // Сохраняем изменения в _taskItem
            _taskItem.Executor = txtExecutor.Text.Trim();
            _taskItem.Description = txtDescription.Text.Trim();

            // Выбор статуса из ComboBox
            if (cmbStatus.SelectedItem is ComboBoxItem selected)
                _taskItem.Status = selected.Content.ToString();

            // Дату создания тоже можем перезаписать, если нужно
            if (dateCreated.SelectedDate.HasValue)
            {
                // Сохраним время (часы/минуты) прежние, а дату меняем
                DateTime oldDateTime = _taskItem.CreatedAt;
                DateTime newDate = dateCreated.SelectedDate.Value;
                _taskItem.CreatedAt = new DateTime(newDate.Year, newDate.Month, newDate.Day,
                                                   oldDateTime.Hour, oldDateTime.Minute, oldDateTime.Second);
            }

            // Обновляем время последнего изменения
            _taskItem.LastUpdated = DateTime.Now;

            // Закрываем окно с успехом
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно без сохранения
            this.DialogResult = false;
        }
    }
}
