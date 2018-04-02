using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Company.DataAccess.Core;
using Company.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Company.DataAccess
{
    public class UserRepository : BaseRepositoryCosmos<User>, IUserRepository
    {
        public UserRepository(IOptions<CosmosConfiguration> options) : base(options)
        {

        }

        public bool CreateUser(User user)
        {
            var result = AsyncHelpers.RunSync<IdentityResult>(() => this.CreateAsync(user, CancellationToken.None));
            return result == IdentityResult.Success;
        }

        public bool DeleteUser(string id)
        {
            var user = new User()
            {
                Id = id
            };

            var result = AsyncHelpers.RunSync<IdentityResult>(() => this.DeleteAsync(user, CancellationToken.None));
            return result == IdentityResult.Success;
        }

        public User GetUserByName(string username)
        {
            var user = AsyncHelpers.RunSync<User>(() => this.FindByNameAsync(username, CancellationToken.None));

            return user;
        }

        public User GetUserById(string id)
        {
            var user = AsyncHelpers.RunSync<User>(() => this.FindByIdAsync(id, CancellationToken.None));

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
            //throw new NotImplementedException();
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            cancellationToken.ThrowIfCancellationRequested();

            var user = this.GetFirstOrDefault(u => u.Id == userId);

            return await Task.FromResult(user);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            cancellationToken.ThrowIfCancellationRequested();

            var user = this.GetFirstOrDefault(u => string.Compare(u.UserName, normalizedUserName, true) == 0);

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

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            var has = !string.IsNullOrEmpty(user.PasswordHash);

            return Task.FromResult(has);
        }

        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            user.Roles = user.Roles ?? new List<string>();
            user.Roles.Add(roleName);

            await this.Upsert(user);
        }

        public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var roles = await this.GetRolesAsync(user, cancellationToken);

            if (roles.Any(r => string.Compare(r, roleName, true) == 0))
            {
                roles.Remove(roleName);

                user.Roles = roles.ToList();

                await this.Upsert(user);
            }
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.Id))
                throw new ArgumentNullException(nameof(user.Id));

            cancellationToken.ThrowIfCancellationRequested();

            user = this.GetFirstOrDefault(u => u.Id == user.Id);

            var roles = user.Roles ?? new List<string>();

            return Task.FromResult(roles as IList<string>);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var inRole = user.Roles?.Any(r => string.Compare(r, roleName, true) == 0);

            return Task.FromResult(inRole.GetValueOrDefault());
        }

        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            var users = this.GetAll(u => (u.Roles?.Any(r => string.Compare(r, roleName, true) == 0)).GetValueOrDefault());

            return Task.FromResult(users as IList<User>);
        }

        #endregion
    }
}
