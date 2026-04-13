using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.Extensions.Configuration;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class DateTimeProviderConfigurable : IDateTimeProvider
    {
        private readonly IConfiguration _config;

        public DateTimeProviderConfigurable(IConfiguration config)
        {
            _config = config;
        }

        public DateTime Ahora
        {
            get
            {
                var fechaSimulada = _config["DevTools:FechaSimulada"];
                if (!string.IsNullOrEmpty(fechaSimulada) && DateTime.TryParse(fechaSimulada, out var fecha))
                    return fecha.ToUniversalTime();

                return DateTime.UtcNow;
            }
        }
    }
}
