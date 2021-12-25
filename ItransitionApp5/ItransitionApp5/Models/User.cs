using Microsoft.AspNetCore.Identity;

namespace ItransitionApp5.Models
{
    public class User : IdentityUser
    {

        public string Name { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LoginDate { get; set; }

        public Status Status { get; set; }

        public UserRole UserRole { get; set; }

        public List<Message> SentMessages { get; set; }

        public List<Message> ReceivedMessages { get; set; }
    }

    public enum Status
    {
        Block,
        Active
    }

    public enum UserRole
	{
        User,
        Admin
	}
}
