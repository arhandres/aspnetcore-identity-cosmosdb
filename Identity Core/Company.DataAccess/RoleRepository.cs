using Company.DataAccess.Core;
using Company.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Company.DataAccess
{
    public class RoleRepository : BaseRepositoryCosmos<Role>, IRoleRepository
    {
        public RoleRepository(IOptions<CosmosConfiguration> options) : base(options)
        {

        }

        #region Async Methods

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            var success = await this.Upsert(role);

            return success ? IdentityResult.Success : IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var success = await this.Delete(role);

            return success ? IdentityResult.Success : IdentityResult.Failed();
        }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (roleId == null)
                throw new ArgumentNullException(nameof(roleId));

            cancellationToken.ThrowIfCancellationRequested();

            var role = this.GetFirstOrDefault(r => string.Compare(r.Id, roleId, true) == 0);

            return Task.FromResult(role);
        }

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (normalizedRoleName == null)
                throw new ArgumentNullException(nameof(normalizedRoleName));

            cancellationToken.ThrowIfCancellationRequested();

            var role = this.GetFirstOrDefault(r => r.NormalizedName == normalizedRoleName);

            return Task.FromResult(role);
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrEmpty(normalizedName))
                throw new ArgumentNullException(nameof(normalizedName));

            cancellationToken.ThrowIfCancellationRequested();

            role.NormalizedName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException(nameof(roleName));

            cancellationToken.ThrowIfCancellationRequested();

            role.Name = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            var success = await this.Upsert(role);

            return success ? IdentityResult.Success : IdentityResult.Failed();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RoleRepository() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
