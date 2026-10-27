using System.Security.Cryptography;

namespace Backend.Seguridad
{
    // Las contraseñas NUNCA se guardan en texto plano.
    // Se guarda un "hash" PBKDF2-SHA256 con una sal aleatoria distinta para cada contraseña.
    // Formato guardado en Usuarios.contrasena_hash:  pbkdf2-sha256$iteraciones$salBase64$hashBase64
    public static class ServicioContrasenas
    {
        private const string Algoritmo = "pbkdf2-sha256";
        private const int Iteraciones = 100_000;   // Hace lento probar contraseñas por fuerza bruta
        private const int TamanoSal = 16;          // bytes
        private const int TamanoHash = 32;         // bytes

        public static string GenerarHash(string contrasena)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
            return string.Join('$', Algoritmo, Iteraciones.ToString(), Convert.ToBase64String(sal), Convert.ToBase64String(hash));
        }

        // Calcula el hash de la contraseña escrita con la MISMA sal e iteraciones guardadas,
        // y lo compara con el hash guardado.
        public static bool Verificar(string contrasena, string? hashGuardado)
        {
            if (string.IsNullOrEmpty(hashGuardado)) return false;

            string[] partes = hashGuardado.Split('$');
            if (partes.Length != 4 || partes[0] != Algoritmo) return false;
            if (!int.TryParse(partes[1], out int iteraciones)) return false;

            byte[] sal;
            byte[] esperado;
            try
            {
                sal = Convert.FromBase64String(partes[2]);
                esperado = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            // Comparación en tiempo constante: no revela cuántos bytes coinciden
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
