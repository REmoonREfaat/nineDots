﻿using App.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Entity;

namespace App.Core.Identity
{
    public class ApplicationUserManager : UserManager<AppUser>
    {
        public ApplicationUserManager(
            IUserStore<AppUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AppUser> passwordHasher,
            IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<AppUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<AppUser> FindByIdAsync(string userId)
        {
            var user = await Users.AsQueryable().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                Logger.LogWarning($"User with ID {userId} was not found.");
            }
            return user;
        }

        public override async Task<AppUser> FindByEmailAsync(string email)
        {
            var user = await Users.AsQueryable().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Logger.LogWarning($"User with email {email} was not found.");
            }
            return user;
        }

        public override async Task<AppUser> FindByNameAsync(string icNumber)
        {
            var user = await Users.AsQueryable().FirstOrDefaultAsync(u => u.UserName == icNumber);
            if (user == null)
            {
                Logger.LogWarning($"User with IC Number {icNumber} was not found.");
            }
            return user;
        }
    }
}
