using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace System.Application.Data
{
    public static class IdentityTableNames
    {
        const string Users = "Users";
        const string Roles = "Roles";
        const string RoleClaims = "RoleClaims";
        const string UserClaims = "UserClaims";
        const string UserLogins = "UserLogins";
        const string UserRoles = "UserRoles";
        const string UserTokens = "UserTokens";

        public static void ReNameAspNetIdentity<TUser, TRole, TKey>(this ModelBuilder builder,
            string? tablePrefix = null)
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
        {
            var hasTablePrefix = !string.IsNullOrEmpty(tablePrefix);
            string GetString(string str) => hasTablePrefix ? tablePrefix + str : str;
            builder.Entity<TUser>().ToTable(GetString(Users));
            builder.Entity<TRole>().ToTable(GetString(Roles));
            builder.Entity<IdentityRoleClaim<TKey>>().ToTable(GetString(RoleClaims));
            builder.Entity<IdentityUserClaim<TKey>>().ToTable(GetString(UserClaims));
            builder.Entity<IdentityUserLogin<TKey>>().ToTable(GetString(UserLogins));
            builder.Entity<IdentityUserRole<TKey>>().ToTable(GetString(UserRoles));
            builder.Entity<IdentityUserToken<TKey>>().ToTable(GetString(UserTokens));
        }
    }
}