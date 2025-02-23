using App.API.DTOs;
using App.API.Helper;
using App.Common.Services.SMS;
using App.Core.Entities;
using App.Core.Identity;
using App.Core.Interfaces.Services;
using App.Core.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace App.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        protected readonly IAppUserManagerService _appUserManagerService;
        public AuthController(IMapper mapper,
             ApplicationUserManager userManager,
             SignInManager<AppUser> signInManager,
            IAppUserManagerService appUserManagerService,
            ISmsService smsService,
             IConfiguration config)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _appUserManagerService = appUserManagerService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<AppUserDTO>> CheckIfIcNumberExistsAsync(string icNumber)
        {
            try
            {
                var res = await _appUserManagerService.FindByIcNumberAsync(icNumber);
                if (res == null)
                    return HelperClass<AppUserDTO>.CreateResponseModel(null, true, "IC Number not exist");

                var appUserDTO = _mapper.Map<AppUserDTO>(res);
                return HelperClass<AppUserDTO>.CreateResponseModel(appUserDTO, false, null);

            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\CheckEmailExist" + email + " with EX: " + ex.ToString());
                return HelperClass<AppUserDTO>.CreateResponseModel(null, true, ex.Message);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<BooleanResultDTO>> RegisterAsync([FromBody] RegisterDTO registerModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                      string.Join(",", ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage)));

                var newuser = new AppUserModel
                {
                    FullName = registerModel.FullName,
                    IcNumber = registerModel.IcNumber,
                    Email = registerModel.Email,
                    PhoneNumber = registerModel.PhoneNumber,
                    Roles = "Citizen",

                };
                return await _appUserManagerService.AddUserAsync(newuser);

            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\Registeration" + " with EX: " + ex.ToString());
                return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                       ex.Message);
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<BooleanResultDTO>> CheckPhoneNumberOtpAsync([FromBody] ValidateOtpDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                      string.Join(",", ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage)));


                string id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, "User not exist");


                return await _appUserManagerService.ActivatePhoneNumberOTPAsync(user.UserName, model.Otp);

            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\ChangePassword" + " with EX: " + ex.ToString());
                return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, ex.Message);
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<BooleanResultDTO>> CheckEmailOtpAsync([FromBody] ValidateOtpDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                      string.Join(",", ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage)));


                string id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, "User not exist");

                return await _appUserManagerService.ActivateEmailOTPAsync(user.UserName, model.Otp);

            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\ChangePassword" + " with EX: " + ex.ToString());
                return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, ex.Message);
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<BooleanResultDTO>> ChangePinAsync([FromBody] ChangePinDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                      string.Join(",", ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage)));

                string id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, "User not exist");

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.PIN);
                if (!result.Succeeded)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, result.Errors.ElementAt(0).Description);

                return HelperClass<BooleanResultDTO>.CreateResponseModel(new BooleanResultDTO()
                {
                    Success = true,
                }, false, "Password Changed Successfully");
            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\ChangePassword" + " with EX: " + ex.ToString());
                return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("api/[controller]/[action]")]
        public async Task<ResponseModel<BooleanResultDTO>> EnableBiometric()
        {
            try
            {
                if (!ModelState.IsValid)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true,
                     string.Join(",", ModelState.Values
                     .SelectMany(v => v.Errors)
                     .Select(e => e.ErrorMessage)));

                string id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, "Invalid User");

                var appUserModel = _mapper.Map<AppUserModel>(user);
                appUserModel.BiometricEnabled = true;
                var result = await _appUserManagerService.UpdateUserAsync(appUserModel);

                if (result.IsError)
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, result.Description);

                else
                    return HelperClass<BooleanResultDTO>.CreateResponseModel(new BooleanResultDTO() { Success = true },
                        false, "Account Biometric Enabled Successfully");
            }
            catch (Exception ex)
            {
                //_logger.Error("Error occured AccountController\\EditProfile" + " with EX: " + ex.ToString());
                return HelperClass<BooleanResultDTO>.CreateResponseModel(null, true, ex.Message);
            }
        }

    }
}
