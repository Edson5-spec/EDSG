using System;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;

namespace EDSGHelper {
    public class LocalizationHelper {
        private readonly IStringLocalizer<LocalizationHelper> _localizer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Construtor para injeção de dependência
        public LocalizationHelper(IStringLocalizer<LocalizationHelper> localizer, IHttpContextAccessor httpContextAccessor) {
            _localizer = localizer;
            _httpContextAccessor = httpContextAccessor;
        }

        // Método de instância para obter strings localizadas
        public string Get(string key) {
            return _localizer[key];
        }

        // Exemplo de método que usa HttpContext
        public string GetCurrentCulture() {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}