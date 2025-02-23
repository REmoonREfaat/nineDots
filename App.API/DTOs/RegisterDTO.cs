using System.ComponentModel.DataAnnotations;

namespace App.API.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [RegularExpression(@"^\d{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])\d{6}$", ErrorMessage = "IC Number not in format")]
        public required string IcNumber { get; set; }

        [Required]
        [RegularExpression(@"([^0-9]*)$", ErrorMessage = "Full name should be letters only")]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?:\+60[- ]?)?(1[0-9]{1}-?\d{7,8})$", ErrorMessage = "Invalid Malaysian Phone Number")]
        public required string PhoneNumber { get; set; }

    }
}
