using System.ComponentModel.DataAnnotations.Schema;

namespace EKF_AI.DataBase.Models
{
    public class Image
    {
        public long ChatId { get; set; }
        public bool HasSend { get; set; }

        public string Id { get; set; }
        public string Path { get; set; }
        public string? Name { get; set; }
        public int? Precision { get; set; }
        public bool HasProcessed { get; set; }

        public string? Result { get; set; }

        [NotMapped]
        public string? Base64 { get; set; }
    }
}
