namespace Argus_WPF.Models
{
    public class AdminEmployeeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public bool IsRoleEditable { get; set; }

        public List<string> AvailableRoles { get; } = new()
        {
            "Руководитель", "Директор", "Тимлид", "Разработчик",
            "Веб-разработчик", "Дизайнер", "SMM", "QA | Backend"
        };
    }
}
