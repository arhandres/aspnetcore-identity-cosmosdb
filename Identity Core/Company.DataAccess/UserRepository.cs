using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Company.DataAccess.Core;
using Company.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Company.DataAccess
{
    public class UserRepository : BaseRepositoryCosmos<User>,  IUserRepository
    {
        public UserRepository(IOptions<CosmosConfiguration> options) :base(options)
        {

        }

        public bool CreateUser(User user)
        {
            var result = AsyncHelpers.RunSync<IdentityResult>(() => this.CreateAsync(user, CancellationToken.None));
            return result == IdentityResult.Success;
        }

        public bool DeleteUser(int id)
        {
            var user = new User()
            {
                Id = id
            };

            var result = AsyncHelpers.RunSync<IdentityResult>(() => this.DeleteAsync(user, CancellationToken.None));
            return result == IdentityResult.Success;
        }

        public User GetUserById(int id)
        {
            var user = AsyncHelpers.RunSync<User>(() => this.FindByIdAsync(id.ToString(), CancellationToken.None));

            return user;
        }

        #region Async Methods

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var result = await this.Upsert(user);

            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var result = await this.Delete(user);

            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            cancellationToken.ThrowIfCancellationRequested();

            var id = int.Parse(userId);

            var user = this.GetFirstOrDefault(u => u.Id == id);

            return await Task.FromResult(user);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            cancellationToken.ThrowIfCancellationRequested();

            var user = this.GetFirstOrDefault(u => u.NormalizedUserName == normalizedUserName);

            return await Task.FromResult(user);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedUserName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            cancellationToken.ThrowIfCancellationRequested();

            user.NormalizedUserName = normalizedUserName;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            user.UserName = userName;

            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var result = await this.Upsert(user);

            return result ? IdentityResult.Success : IdentityResult.Failed();
        }

        #endregion
    }
}
