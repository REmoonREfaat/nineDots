namespace App.Common.Services.Email
{
    public interface IEmailService
    {
        Task SendOtpAsync(string phoneNumber, string otp);
    }
}
