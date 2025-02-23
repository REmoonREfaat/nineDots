using System.ComponentModel.DataAnnotations;

namespace App.Core.Models
{
    public class AppUserModel : UserDataModel
    {

        public string PhoneOTP { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public string EmailOTP { get; set; }
        public bool EmailConfirmed { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN must contain only numbers")]
        public string PIN { get; set; }
        public bool BiometricEnabled { get; set; }
    }
}