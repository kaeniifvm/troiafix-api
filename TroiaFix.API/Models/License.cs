using System;
using System.ComponentModel.DataAnnotations;

namespace TroiaFix.API.Models
{
    public class License
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string KeyHash { get; set; }

        [MaxLength(128)]
        public string? Hwid { get; set; }

        [MaxLength(256)]
        public string? KeyOriginal { get; set; }

        public DateTime? DataAtivacao { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataExpiracao { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pendente";

        [MaxLength(500)]
        public string? Observacoes { get; set; }
    }
}