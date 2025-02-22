using App.Core.Models;

namespace App.Core.Interfaces.Services
{
    public interface IAppUserManagerService
    {
        Task<AppUserModel> GetUserById(string id);
        Task<AppUserModel> GetUserByEmail(string email);
        Task<AppUserModel> GetUserByPhoneNumber(string mobile);
        Task<ResponseModel<BooleanResultDTO>> AddUser(AppUserModel model);
        Task<ResponseModel<BooleanResultDTO>> UpdateUser(AppUserModel model);
        Task<ResponseModel<BooleanResultDTO>> DeleteUser(string id);
        ResponseModel<BooleanResultDTO> IsExistUser(AppUserModel model);
        Task<ResponseModel<BooleanResultDTO>> ActivatePhoneNumberOTP(string email, string code);
        Task<ResponseModel<BooleanResultDTO>> ActivateEmailOTP(string email, string code);
        Task<ResponseModel<BooleanResultDTO>> ActivateUser(string email, string code);
    }
}
