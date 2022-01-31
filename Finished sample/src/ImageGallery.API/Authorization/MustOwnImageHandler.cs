using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.API.Authorization
{

    // sle note: This handler authorises access to this controller's  " public IActionResult GetImage(Guid id)... " action of this Client service.
    //    To make it work an attribute preceeds the method, which in this case is '  [Authorize("MustOwnImage")]'
    //    "MustOwnImage" is a policy name. It is setup up in 'ConfigureServices' in the Startup.cs file, and bound to this authorisation handler
    //    It is the middleware that makes it all happen.
    //
    // Example gleaned from web calling an APi with a bearer token: -
    // 
    // GET /StrikeIron/emv6Hygiene/EMV6Hygiene/VerifyEmail? VerifyEmail.Email=user @domain.com&VerifyEmail.Timeout= 5
    // HTTP/1.1
    // Host: ws.strikeiron.com
    // Authorization: Bearer e66ce8de3d87454eb236211b4005d570
    //
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // sle note: dependency injection, therefore must be set up in the startup scoped section.
        public MustOwnImageHandler(IGalleryRepository galleryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _galleryRepository = galleryRepository ??
                throw new ArgumentNullException(nameof(galleryRepository));
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        // sle note: This will be called by the middleware to check the image is owned by the user.
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustOwnImageRequirement requirement)
        {
            var imageId = _httpContextAccessor.HttpContext.GetRouteValue("id").ToString();
            if (!Guid.TryParse(imageId, out Guid imageIdAsGuid))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // sle note: The Bearer token is passed in the http context; the owner id is passed as a query string parameter.
            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            if (!_galleryRepository.IsImageOwner(imageIdAsGuid, ownerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // all checks out
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
