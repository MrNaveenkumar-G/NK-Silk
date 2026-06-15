using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NKSilk.Web.Areas.Vendor.Controllers;

/// <summary>Base for all vendor controllers: scopes them to the Vendor area and Vendor role.</summary>
[Area("Vendor")]
[Authorize(Roles = "Vendor")]
public abstract class VendorBaseController : Controller
{
}
