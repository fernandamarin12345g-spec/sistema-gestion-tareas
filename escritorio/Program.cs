namespace Escritorio;

internal static class Program
{
    /// <summary>
    /// Punto de entrada. Muestra el inicio de sesión y, si sale bien, la ventana principal.
    /// Al cerrar sesión (o si la sesión expira) vuelve al inicio de sesión.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var api = new ApiCliente();
        string? mensaje = null;

        while (true)
        {
            using (var login = new FormLogin(api, mensaje))
            {
                if (login.ShowDialog() != DialogResult.OK)
                {
                    return; // El usuario cerró la ventana de inicio de sesión
                }
            }

            using (var principal = new FormPrincipal(api))
            {
                DialogResult resultado = principal.ShowDialog();

                if (resultado == DialogResult.Retry)
                {
                    mensaje = null; // Cerró sesión: vuelve al login
                }
                else if (resultado == DialogResult.Abort)
                {
                    mensaje = "Tu sesión expiró. Vuelve a iniciar sesión.";
                }
                else
                {
                    return; // Cerró la ventana principal: termina la aplicación
                }
            }
        }
    }
}
