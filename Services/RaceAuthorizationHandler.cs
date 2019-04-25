using Microsoft.AspNetCore.Authorization;
using Podwoozka.Entities;
using System.Threading.Tasks;

namespace Podwoozka.Services
{
    public class RaceAuthorizationHandler : AuthorizationHandler<IsOwnerRequirement, RaceItem>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsOwnerRequirement requirement,
            RaceItem item)
        {
            if (context.User.Identity?.Name == item.Owner)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
public class IsOwnerRequirement : IAuthorizationRequirement { }
