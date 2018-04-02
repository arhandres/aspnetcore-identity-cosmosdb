using Company.DataAccess;
using Company.DataAccess.Core;
using Company.Model;
using Company.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Company.Test
{
    [TestClass]
    public class UserTest
    {
        protected ServiceProvider _serviceProvider = null;

        [TestInitialize]
        public void Initialize()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var cosmosConfiguration = new CosmosConfiguration();
            configuration.GetSection("CosmosConfiguration").Bind(cosmosConfiguration);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.Configure<CosmosConfiguration>(c => 
            {
                configuration.GetSection("CosmosConfiguration").Bind(c);
            });

            //serviceCollection.AddSingleton<IUserRepository, UserRepository>();
            //serviceCollection.AddSingleton<IRoleRepository, RoleRepository>();

            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IUserStore<User>, UserRepository>();

            serviceCollection.AddScoped<IRoleRepository, RoleRepository>();
            serviceCollection.AddScoped<IRoleStore<Role>, RoleRepository>();

            serviceCollection.AddScoped<IPasswordHasher<User>, ApplicationPasswordHasher>();
            serviceCollection.AddScoped<UserManager<User>, ApplicationUserManager>();
            serviceCollection.AddScoped<RoleManager<Role>, ApplicationRoleManager>();
            serviceCollection.AddScoped<SignInManager<User>, ApplicationSignInManager>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        //[TestMethod]
        //public async Task AddToRoleAsync()
        //{
        //    var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        //    var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();

        //    var user = userRepository.GetUserByName("arhandres@hotmail.com");

        //    var result = await userManager.AddToRoleAsync(user, "Admin");
        //}

        [TestMethod]
        public void CreateUserTest()
        {
            try
            {

                var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
                
                var userName = "arhandres@hotmail.com";
                var password = "Password11;";

                var user = new User()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = userName,
                    PasswordHash = ApplicationPasswordHasher.CreateHashPassword(password)
                };

                var success = userRepository.CreateUser(user);

                Assert.AreEqual(true, success);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [TestMethod]
        public async Task CreateRoleTest()
        {
            var roleRepository = _serviceProvider.GetRequiredService<IRoleRepository>();

            var result = await roleRepository.CreateAsync(new Role()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Admin",
                NormalizedName = "Admin"
            }, CancellationToken.None);
        }

        
    }
}
