using System.ComponentModel.DataAnnotations;

namespace App.API.DTOs
{
    public class ValidateOtpDTO
    {
        [Required]
        public required string Otp { get; set; }
    }
}
