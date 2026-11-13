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

        // Si ocurre un error inesperado, se guarda en errores.log (junto al .exe) y se muestra un mensaje
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => RegistroErrores.Mostrar(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => RegistroErrores.Guardar(e.ExceptionObject as Exception);

        var api = new ApiCliente();
        string? mensaje = null;

        try
        {
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
        catch (Exception ex)
        {
            RegistroErrores.Mostrar(ex);
        }
    }
}

/// <summary>Guarda los errores inesperados en un archivo para poder revisarlos después.</summary>
internal static class RegistroErrores
{
    private static readonly string Ruta = Path.Combine(AppContext.BaseDirectory, "errores.log");

    public static void Guardar(Exception? error)
    {
        try
        {
            File.AppendAllText(Ruta, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {error}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Si ni siquiera se puede escribir el archivo, no hay nada más que hacer
        }
    }

    public static void Mostrar(Exception error)
    {
        Guardar(error);
        MessageBox.Show($"Ocurrió un error inesperado:\n\n{error.Message}\n\nEl detalle quedó en:\n{Ruta}",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
