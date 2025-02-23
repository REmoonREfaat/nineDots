using App.Common.Services.Email;
using App.Common.Services.SMS;
using App.Core.Entities;
using App.Core.Identity;
using App.Core.Interfaces.Services;
using App.Core.Models;
using AutoMapper;
using System.Security.Claims;

namespace App.Core.Services
{
    public class AppUserManagerService : IAppUserManagerService
    {
        #region Constructor
        private readonly ApplicationUserManager _userManager;
        private readonly IMapper _mapper;
        ISmsService _smsService;
        IEmailService _emailService;

        public AppUserManagerService(
            ApplicationUserManager userManager,
            IMapper mapper,
            ISmsService smsService,
            IEmailService emailService)

        {
            _mapper = mapper;
            _userManager = userManager;
            _smsService = smsService;
            _emailService = emailService;
        }
        #endregion

        public async Task<AppUserModel> FindByIcNumberAsync(string icNumber)
        {
            AppUser appUser = await _userManager.FindByNameAsync(icNumber);
            if (appUser != null)
            {
                var otp = GenerateOTP();
                appUser.PhoneOTP = otp;
                SendPhoneNumberOtp(appUser.PhoneNumber, otp);
                await _userManager.UpdateAsync(appUser);
            }
            return _mapper.Map<AppUserModel>(appUser);
        }

        public async Task<ResponseModel<BooleanResultDTO>> AddUserAsync(AppUserModel model)
        {
            try
            {
                ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>()
                {
                    IsError = false,
                    Result = new BooleanResultDTO() { Success = true },
                    Description = "Registeration success"
                };

                var appUser = await _userManager.FindByNameAsync(model.IcNumber);
                if (appUser != null)
                {
                    responseModel.Result.Success = false;
                    responseModel.IsError = true;
                    responseModel.Description = "IC Number exists before";
                    return responseModel;
                }

                appUser = await _userManager.FindByEmailAsync(model.Email);
                if (appUser != null)
                {
                    responseModel.Result.Success = false;
                    responseModel.IsError = true;
                    responseModel.Description = "Email exists before";
                    return responseModel;
                }

                appUser = GetUserByPhoneNumber(model.PhoneNumber).Result;
                if (appUser != null)
                {
                    responseModel.Result.Success = false;
                    responseModel.IsError = true;
                    responseModel.Description = "Phone Number exists ";
                    return responseModel;
                }

                AppUser newuser = GetAppUser(model);
                newuser.CreationDate = DateTime.Now;
                Guid guid = new Guid();
                var saveuser = await _userManager.CreateAsync(newuser, guid.ToString());
                if (!saveuser.Succeeded)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = saveuser.Errors.Select(x => x.Description).ElementAt(0).ToString();
                    return responseModel;
                }
                else
                {
                    var roles = new List<string> { model.Roles };
                    var saveroles = await _userManager.AddToRolesAsync(newuser, roles);
                    var claims = new[]
                    {new Claim("UserFullName", newuser.FullName )};
                    var saveClaims = await _userManager.AddClaimsAsync(newuser, claims);
                    if (saveroles.Succeeded && saveClaims.Succeeded)
                    {
                        appUser.EmailOTP = GenerateOTP();
                        SendEmailOtp(appUser.PhoneNumber, appUser.EmailOTP);
                        var res = await _userManager.UpdateAsync(appUser);
                        if (!res.Succeeded)
                        {
                            responseModel.IsError = true;
                            responseModel.Result.Success = false;
                            responseModel.Description = "Error in updating user try again";
                            return responseModel;
                        }
                        return responseModel;
                    }

                    await _userManager.DeleteAsync(newuser);
                    responseModel.IsError = true;
                    responseModel.Description = saveroles.Errors.Select(x => x.Description).ElementAt(0).ToString();
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BooleanResultDTO>()
                {
                    IsError = true,
                    Result = new BooleanResultDTO() { Success = false },
                    Description = ex.Message
                };
            }
        }

        public async Task<ResponseModel<BooleanResultDTO>> ActivatePhoneNumberOTPAsync(string icNumber, string code)
        {
            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>() { Result = new BooleanResultDTO() };

            var appUser = await _userManager.FindByNameAsync(icNumber);
            if (appUser == null)
            {
                responseModel.IsError = true;
                responseModel.Result.Success = false;
                responseModel.Description = "This IC Number is not exist";
                return responseModel;
            }
            else
            {
                if (appUser.PhoneOTP != code)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = "Invalid Phone Number OTP";
                    return responseModel;
                }

                else
                {
                    appUser.PhoneNumberConfirmed = true;
                    appUser.PhoneOTP = GenerateOTP();
                    SendPhoneNumberOtp(appUser.PhoneNumber, appUser.PhoneOTP);
                    var res = await _userManager.UpdateAsync(appUser);
                    if (!res.Succeeded)
                    {
                        responseModel.IsError = true;
                        responseModel.Result.Success = false;
                        responseModel.Description = "Error in activate Phone Number OTP try again";
                        return responseModel;
                    }
                    else
                    {
                        responseModel.IsError = false;
                        responseModel.Result.Success = true;
                        responseModel.Description = "Phone Number OTP Activated Success";
                        return responseModel;
                    }
                }
            }
        }
        public async Task<ResponseModel<BooleanResultDTO>> ActivateEmailOTPAsync(string icNumber, string code)
        {
            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>() { Result = new BooleanResultDTO() };

            var appUser = await _userManager.FindByNameAsync(icNumber);
            if (appUser == null)
            {
                responseModel.IsError = true;
                responseModel.Result.Success = false;
                responseModel.Description = "This IC Number is not exist";
                return responseModel;
            }
            else
            {
                if (appUser.EmailOTP != code)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = "Invalid Email OTP";
                    return responseModel;
                }

                else
                {
                    appUser.EmailConfirmed = true;
                    var res = await _userManager.UpdateAsync(appUser);
                    if (!res.Succeeded)
                    {
                        responseModel.IsError = true;
                        responseModel.Result.Success = false;
                        responseModel.Description = "Error in activate Email OTP try again";
                        return responseModel;
                    }
                    else
                    {
                        responseModel.IsError = false;
                        responseModel.Result.Success = true;
                        responseModel.Description = "Email OTP Activated Success";
                        return responseModel;
                    }
                }
            }
        }

        public async Task<AppUserModel> GetUserById(string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);
            var appUserModel = _mapper.Map<AppUserModel>(appUser);
            appUserModel.Roles = _userManager.GetRolesAsync(appUser).Result.FirstOrDefault();
            return appUserModel;
        }


        public async Task<ResponseModel<BooleanResultDTO>> UpdateUser(AppUserModel model)
        {
            try
            {
                ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>()
                {
                    IsError = false,
                    Result = new BooleanResultDTO() { Success = true },
                    Description = "User modified !"
                };

                AppUser appUser = await _userManager.FindByIdAsync(model.Id);
                appUser.FullName = model.FullName;
                appUser.EmailConfirmed = model.EmailConfirmed;
                appUser.PhoneNumber = model.PhoneNumber;
                appUser.Email = model.Email;
                appUser.UserName = model.IcNumber;
                var res = await _userManager.UpdateAsync(appUser);
                if (res.Succeeded)
                {
                    List<string> roles = _userManager.GetRolesAsync(appUser).Result.ToList();
                    if (roles.Contains(model.Roles)) // no change in result
                        return responseModel;
                    else
                    {
                        for (int i = 0; i < roles.Count; i++) // remove last role
                        {
                            await _userManager.RemoveFromRoleAsync(appUser, roles[i]);
                        }

                        var AddRole = await _userManager.AddToRoleAsync(appUser, model.Roles);
                        if (AddRole.Succeeded)
                            return responseModel;

                        else
                        {
                            responseModel.IsError = true;
                            responseModel.Result.Success = false;
                            responseModel.Description = AddRole.Errors.ElementAt(0).Description;
                            return responseModel;
                        }
                    }

                }
                else
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = res.Errors.ElementAt(0).Description;
                    return responseModel;
                }


            }
            catch (Exception ex)
            {
                return new ResponseModel<BooleanResultDTO>()
                {
                    IsError = true,
                    Result = new BooleanResultDTO() { Success = false },
                    Description = ex.Message
                };
            }
        }

        public async Task<ResponseModel<BooleanResultDTO>> DeleteUser(string id)
        {

            try
            {
                ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>()
                {
                    IsError = false,
                    Result = new BooleanResultDTO() { Success = true },
                    Description = "User Deleted !"
                };

                AppUser updateUser = await _userManager.FindByIdAsync(id);
                updateUser.RecordStatus = Entities.Base.RecordStatus.Deleted;

                var result = await _userManager.UpdateAsync(updateUser);

                if (!result.Succeeded)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = result.Errors.Select(x => x.Description).ElementAt(0).ToString();
                    return responseModel;
                }
                return responseModel;
            }
            catch (Exception ex)
            {
                return new ResponseModel<BooleanResultDTO>()
                {
                    IsError = true,
                    Result = new BooleanResultDTO() { Success = false },
                    Description = ex.Message
                };
            }

        }

        private AppUser GetAppUser(AppUserModel model)
        {
            string ActivationCode = DateTime.Now.Millisecond.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Hour.ToString();
            ActivationCode = ActivationCode.Length > 6 ? ActivationCode.Substring(0, 6) : ActivationCode;
            AppUser newuser = new AppUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.IcNumber,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = model.EmailConfirmed,
                CreationDate = model.CreationDate,
            };
            return newuser;
        }


        static string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(0000, 9999).ToString();
        }
        void SendPhoneNumberOtp(string phoneNumber, string otp)
        {
            _smsService.SendOtpAsync(phoneNumber, otp);
        }
        void SendEmailOtp(string email, string otp)
        {
            _emailService.SendOtpAsync(email, otp);
        }

    }
}
