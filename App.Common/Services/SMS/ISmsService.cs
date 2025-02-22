namespace App.Common.Services.SMS
{
    public interface ISmsService
    {
        Task SendOtpAsync(string phoneNumber, string otp);
    }
}
