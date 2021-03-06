﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OksidiCom.AspNetCore.UserService.Db;
using Microsoft.EntityFrameworkCore.Storage;
using SampleApi.Models;
using OksidiCom.AspNetCore.UserService.Models;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Models;
using OpenIddict.Core;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;

namespace SampleApi.Db
{
    internal class InitDbDev : IInitDb
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UserServiceContext userServiceContext;
        private readonly BusinessDbContext businessDbContext;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> applicationManager;

        public InitDbDev(UserManager<ApplicationUser> userManager, UserServiceContext userServiceContext,
            BusinessDbContext businessDbContext, OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            this.userManager = userManager;
            this.userServiceContext = userServiceContext;
            this.businessDbContext = businessDbContext;
            this.applicationManager = applicationManager;
        }

        private async Task CreateDatabaseAsync()
        {
            var userServiceDbCreator = userServiceContext.GetService<IRelationalDatabaseCreator>();
            await userServiceDbCreator.DeleteAsync();
            await userServiceDbCreator.CreateAsync();
            await userServiceDbCreator.CreateTablesAsync();

            var businessServiceDbCreator = businessDbContext.GetService<IRelationalDatabaseCreator>();
            await businessServiceDbCreator.CreateTablesAsync();
        }

        private async Task<List<ApplicationUser>> CreateUsers()
        {
            var newUsers = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "test@example.com",
                    Email = "test@example.com",
                },
                new ApplicationUser()
                {
                    UserName = "test2@example.com",
                    Email = "test2@example.com"
                }
            };
            await userManager.CreateAsync(newUsers[0], "!Test1");
            await userManager.CreateAsync(newUsers[1], "!Test1");
            //await userServiceContext.Users.AddRangeAsync(newUsers);
            return newUsers;
        }

        private async Task CreateOpenIdClients()
        {
            await applicationManager.CreateAsync(new OpenIddictApplication()
            {
                ClientId = "example-client",
                DisplayName = "Example Client",
                RedirectUri = "http://localhost:5002/o2c.html",
                LogoutRedirectUri = "http://localhost:5002/o2c-logout.html",
                Type = OpenIddictConstants.ClientTypes.Public
            }, new CancellationToken());
        }

        private async Task PopulateAsync()
        {
            var newUsers = await CreateUsers();

            await CreateOpenIdClients();

            var businessThings = new List<BusinessThing>()
            {
                new BusinessThing()
                {
                    Name = "Acme Corp",
                    OwnerUserId = newUsers[0].Id
                },
                new BusinessThing()
                {
                    Name = "Corporation Inc",
                    OwnerUserId = newUsers[1].Id
                }
            };
            await businessDbContext.BusinessThings.AddRangeAsync(businessThings);
        }

        public async Task InitAsync()
        {
            await CreateDatabaseAsync();
            await PopulateAsync();
        }
    }
}
