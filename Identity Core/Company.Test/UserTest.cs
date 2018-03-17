using Company.DataAccess;
using Company.DataAccess.Core;
using Company.Model;
using Company.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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

            serviceCollection.AddSingleton<IUserRepository, UserRepository>();

            

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

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
                    Id = 1,
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
        public void CreateRoleTest()
        {
        }
    }
}
