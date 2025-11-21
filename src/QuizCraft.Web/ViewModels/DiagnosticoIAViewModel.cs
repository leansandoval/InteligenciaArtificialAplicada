namespace QuizCraft.Web.ViewModels;

public class DiagnosticoIAViewModel
{
    public DateTime FechaVerificacion { get; set; }
    public bool ConfiguracionValida { get; set; }
    public bool ApiKeyConfigurada { get; set; }
    public string? ApiKeyParcial { get; set; }
    public string? ModeloConfigurudo { get; set; }
    public bool ConexionExitosa { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? MensajeError { get; set; }
    public int TokensUsados { get; set; }
}
