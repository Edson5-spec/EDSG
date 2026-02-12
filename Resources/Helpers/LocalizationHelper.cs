using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace EDSG.Helpers {
    public static class LocalizationHelper {
        public static string GetLocalizedString(string key) {
            var httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext == null)
                return key;

            var culture = httpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture
                          ?? CultureInfo.CurrentCulture;

            var resourceManager = new System.Resources.ResourceManager("EDSG.Resources.Shared.Resources",
                typeof(Program).Assembly);

            return resourceManager.GetString(key, culture) ?? key;
        }
    }
}