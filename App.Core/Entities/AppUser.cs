using Microsoft.AspNetCore.Identity;
using App.Core.Entities.Base;

namespace App.Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string PhoneOTP { get; set; }
        public string EmailOTP { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
