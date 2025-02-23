using App.Core.Models;

namespace App.Core.Interfaces.Services
{
    public interface IAppUserManagerService
    {
        Task<AppUserModel> FindByIcNumberAsync(string icNumber);
        Task<ResponseModel<BooleanResultDTO>> AddUserAsync(AppUserModel model);
        Task<ResponseModel<BooleanResultDTO>> ActivatePhoneNumberOTPAsync(string icNumber, string code);
        Task<ResponseModel<BooleanResultDTO>> ActivateEmailOTPAsync(string icNumber, string code);
        Task<ResponseModel<BooleanResultDTO>> UpdateUserAsync(AppUserModel model);
        Task<ResponseModel<BooleanResultDTO>> DeleteUserAsync(string id);
    }
}
