// Файл: Models/GoogleSheetsSettings.cs (или в другом подходящем месте)
namespace Argus_WPF.Models // Убедитесь, что пространство имен соответствует вашему проекту
{
    public class GoogleSheetsSettings
    {
        public string ApplicationName { get; set; }
        public string CredentialsPath { get; set; }
        public string SpreadsheetId { get; set; }
        public string SheetName { get; set; }
    }
}