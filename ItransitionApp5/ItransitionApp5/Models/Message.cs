using System.ComponentModel.DataAnnotations;

namespace ItransitionApp5.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        public string SenderId { get; set; }

        public string ReceiverId { get; set; }

        public string MessageText { get; set; }

        public User Sender { get; set; }

        public User Receiver { get; set; }
    }
}
