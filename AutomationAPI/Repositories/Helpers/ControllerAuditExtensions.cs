using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AutomationAPI.Repositories.Helpers
{
    // Small shared helpers so every controller that writes an audit log entry doesn't
    // need to re-derive "who is calling" from the JWT claims itself - several controllers
    // already had their own private GetCurrentUserId() doing the UserId half of this
    // (e.g. EnvironmentController, NotificationController); this adds the matching
    // ActorUserName half without touching any of those existing, already-working methods.
    public static class ControllerAuditExtensions
    {
        public static int? GetAuditUserId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var userId) && userId > 0 ? userId : null;
        }

        public static string GetAuditUserName(this ControllerBase controller)
        {
            return controller.User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        }
    }
}
