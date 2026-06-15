using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NKSilk.Web.Areas.Admin.Controllers;

/// <summary>Base for all admin controllers: scopes them to the Admin area and the Admin role.</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : Controller
{
}
