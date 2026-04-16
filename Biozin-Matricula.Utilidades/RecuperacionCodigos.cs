using System.Collections.Concurrent;

namespace Biozin_Matricula.Utilidades
{
    /// <summary>
    /// Almacén en memoria de códigos de recuperación de contraseña.
    /// Cada entrada expira a los 15 minutos.
    /// </summary>
    public static class RecuperacionCodigos
    {
        private static readonly ConcurrentDictionary<string, (string Codigo, DateTime Expira)> _store = new();

        public static string Generar(string email)
        {
            var codigo = new Random().Next(100000, 999999).ToString();
            _store[email.ToLower()] = (codigo, DateTime.UtcNow.AddMinutes(15));
            return codigo;
        }

        public static bool Validar(string email, string codigo)
        {
            var key = email.ToLower();
            if (!_store.TryGetValue(key, out var entry)) return false;
            if (DateTime.UtcNow > entry.Expira) { _store.TryRemove(key, out _); return false; }
            if (entry.Codigo != codigo) return false;
            _store.TryRemove(key, out _);
            return true;
        }
    }
}
