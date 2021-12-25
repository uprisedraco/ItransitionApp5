namespace ItransitionApp5.Models
{
    public class IndexViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }
        
        public DateTime LastLoginDate { get; set; }

        public UserRole Role { get; set; }

        public Status Status { get; set; }
    }
}
