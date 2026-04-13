using Biozin_Matricula.Dominio.InterfacesLN;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Ahora => DateTime.UtcNow;
    }
}
