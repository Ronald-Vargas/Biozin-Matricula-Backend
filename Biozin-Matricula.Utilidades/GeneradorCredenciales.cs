using System.Text;

namespace Biozin_Matricula.Utilidades
{
    public static class GeneradorCredenciales
    {
        private const string Dominio = "@biozin.edu.cr";

        private static readonly char[] Minusculas = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] Mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Digitos = "0123456789".ToCharArray();
        private static readonly char[] Especiales = "!@#$%&*".ToCharArray();

        /// <summary>
        /// Genera la parte local del email a partir del nombre y apellido paterno.
        /// Ej: "Juan Carlos", "Pérez" → "juan.perez"
        /// </summary>
        public static string GenerarBaseEmail(string nombre, string apellidoPaterno)
        {
            var primerNombre = nombre.Trim().Split(' ')[0];
            return $"{Normalizar(primerNombre)}.{Normalizar(apellidoPaterno)}";
        }

        /// <summary>
        /// Construye el email completo con el dominio institucional.
        /// Si sufijo > 0, lo agrega al final: "juan.perez2@biozin.edu.cr"
        /// </summary>
        public static string ConstruirEmail(string baseEmail, int sufijo = 0)
        {
            var local = sufijo > 0 ? $"{baseEmail}{sufijo}" : baseEmail;
            return $"{local}{Dominio}";
        }

        /// <summary>
        /// Genera una contraseña aleatoria de 10 caracteres con mayúscula,
        /// minúscula, dígito y carácter especial garantizados.
        /// </summary>
        public static string GenerarContrasena()
        {
            var rng = new Random();
            var chars = new char[10];

            // Garantizar al menos uno de cada tipo
            chars[0] = Mayusculas[rng.Next(Mayusculas.Length)];
            chars[1] = Minusculas[rng.Next(Minusculas.Length)];
            chars[2] = Digitos[rng.Next(Digitos.Length)];
            chars[3] = Especiales[rng.Next(Especiales.Length)];

            // Rellenar el resto con caracteres mixtos
            var todos = new char[Minusculas.Length + Mayusculas.Length + Digitos.Length + Especiales.Length];
            Minusculas.CopyTo(todos, 0);
            Mayusculas.CopyTo(todos, Minusculas.Length);
            Digitos.CopyTo(todos, Minusculas.Length + Mayusculas.Length);
            Especiales.CopyTo(todos, Minusculas.Length + Mayusculas.Length + Digitos.Length);

            for (int i = 4; i < 10; i++)
                chars[i] = todos[rng.Next(todos.Length)];

            // Mezclar para que los tipos no queden en orden fijo
            return new string(chars.OrderBy(_ => rng.Next()).ToArray());
        }

        private static string Normalizar(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                var categoria = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .ToLower()
                     .Replace('ñ', 'n');
        }
    }
}
