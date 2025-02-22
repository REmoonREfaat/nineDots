using App.Core.Entities;
using App.Core.Entities.Base;
using App.Core.Identity;
using App.Core.Interfaces.Services;
using App.Core.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace App.Core.Services
{
    public class AppUserManagerService : IAppUserManagerService
    {
        #region Constructor
        private readonly ApplicationUserManager _userManager;
        private readonly IMapper _mapper;
        public AppUserManagerService(
            ApplicationUserManager userManager,
            IMapper mapper)

        {
            _mapper = mapper;
            _userManager = userManager;
        }
        #endregion

        public async Task<AppUserModel> GetUserById(string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);
            var appUserModel = _mapper.Map<AppUserModel>(appUser);
            appUserModel.Roles = _userManager.GetRolesAsync(appUser).Result.FirstOrDefault();
            return appUserModel;
        }

        public async Task<AppUserModel> GetUserByEmail(string email)
        {
            AppUser appUser = await _userManager.FindByEmailAsync(email);
            return _mapper.Map<AppUserModel>(appUser);

        }
        public async Task<AppUserModel> FindbyIC_NumberAsync(string IC_Number)
        {
            AppUser appUser = await _userManager.FindByNameAsync(IC_Number);
            return _mapper.Map<AppUserModel>(appUser);

        }

        public async Task<AppUserModel> GetUserByPhoneNumber(string phoneNumber)
        {
            var appUser = _userManager.Users.Where(x => x.PhoneNumber == phoneNumber).FirstOrDefault();
            return _mapper.Map<AppUserModel>(appUser);
        }

        public async Task<ResponseModel<BooleanResultDTO>> AddUser(AppUserModel model)
        {
            try
            {
                ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>()
                {
                    IsError = false,
                    Result = new BooleanResultDTO() { Success = true },
                    Description = "Registeration success"
                };

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    responseModel.Result.Success = false;
                    responseModel.IsError = true;
                    responseModel.Description = "Email exists before";
                    return responseModel;
                }

                var findByPhone = GetUserByPhoneNumber(model.PhoneNumber);
                if (findByPhone != null)
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
                        return responseModel;

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
                appUser.UserName = model.IC_Number;
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

        public ResponseModel<BooleanResultDTO> IsExistUser(AppUserModel model)
        {

            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>()
            {
                IsError = false,
                Result = new BooleanResultDTO() { Success = true },
                Description = "Updated Successfully"
            };

            var users = _userManager.Users.Where(x => x.Id != model.Id
            && (x.Email == model.Email || x.PhoneNumber == model.PhoneNumber));
            foreach (var cst in users)
            {
                if (cst.Email == model.Email)
                {
                    responseModel.IsError = true;
                    responseModel.Description = "Email exists before";
                    return responseModel;
                }
                if (cst.PhoneNumber == model.PhoneNumber)
                {
                    responseModel.IsError = true;
                    responseModel.Description = "PhoneNumber exists before";
                    return responseModel;
                }
            }
            return responseModel;
        }

        public async Task<ResponseModel<BooleanResultDTO>> ActivatePhoneNumberOTP(string IC_Number, string code)
        {
            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>() { Result = new BooleanResultDTO() };

            var user = await _userManager.FindByNameAsync(IC_Number);
            if (user == null)
            {
                responseModel.IsError = true;
                responseModel.Result.Success = false;
                responseModel.Description = "This IC Number is not exist";
                return responseModel;
            }
            else
            {
                if (user.EmailOTP != code)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = "Invalid Phone Number OTP";
                    return responseModel;
                }

                else
                {
                    user.PhoneNumberConfirmed = true;
                    var res = await _userManager.UpdateAsync(user);
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
        public async Task<ResponseModel<BooleanResultDTO>> ActivateEmailOTP(string IC_Number, string code)
        {
            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>() { Result = new BooleanResultDTO() };

            var user = await _userManager.FindByNameAsync(IC_Number);
            if (user == null)
            {
                responseModel.IsError = true;
                responseModel.Result.Success = false;
                responseModel.Description = "This IC Number is not exist";
                return responseModel;
            }
            else
            {
                if (user.EmailOTP != code)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = "Invalid Email OTP";
                    return responseModel;
                }

                else
                {
                    user.EmailConfirmed = true;
                    var res = await _userManager.UpdateAsync(user);
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
        public async Task<ResponseModel<BooleanResultDTO>> ActivateUser(string IC_Number, string code)
        {
            ResponseModel<BooleanResultDTO> responseModel = new ResponseModel<BooleanResultDTO>() { Result = new BooleanResultDTO() };

            var user = await _userManager.FindByNameAsync(IC_Number);
            if (user == null)
            {
                responseModel.IsError = true;
                responseModel.Result.Success = false;
                responseModel.Description = "This IC Number is not exist";
                return responseModel;
            }
            else
            {
                user.RecordStatus = RecordStatus.Enabled;
                user.PasswordHash = code;
                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                {
                    responseModel.IsError = true;
                    responseModel.Result.Success = false;
                    responseModel.Description = "Error in activate account try again";
                    return responseModel;
                }
                else
                {
                    responseModel.IsError = false;
                    responseModel.Result.Success = true;
                    responseModel.Description = "Activation Success";
                    return responseModel;
                }
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
                UserName = model.IC_Number,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = model.EmailConfirmed,
                CreationDate = model.CreationDate,
            };
            return newuser;
        }


    }
}
