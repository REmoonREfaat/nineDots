using System.ComponentModel.DataAnnotations;

namespace App.API.DTOs
{
    public class ChangePinDTO
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN must contain only numbers")]
        public string PIN { get; set; }
    }
}
