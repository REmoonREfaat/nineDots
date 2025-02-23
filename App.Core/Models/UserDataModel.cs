using System.ComponentModel.DataAnnotations;

namespace App.Core.Models
{
    public class UserDataModel : BaseModel
    {
        [Required]
        [RegularExpression(@"^\d{2}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])\d{6}$", ErrorMessage = "IC Number not in format")]
        public required string IcNumber { get; set; }

        [Required]
        [RegularExpression(@"([^0-9]*)$", ErrorMessage = "Full name not in format")]
        public required string FullName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public required string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Roles { get; set; }
    }
}