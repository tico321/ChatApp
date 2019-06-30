using ApplicationCore.Users.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationCore.Users.Selectors
{
    public static class UserSelectors
    {
        public static Task<ApplicationUser> FromClaims(
            this DbSet<ApplicationUser> users,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var email = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            return users
                .AsNoTracking()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
