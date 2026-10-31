using PlantGuard_proyecto.Views;

namespace PlantGuard_proyecto;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("registrar-planta", typeof(RegistrarPlantaPage));
        Routing.RegisterRoute("detalle-planta", typeof(DetallePlantaPage));
        Routing.RegisterRoute("resultado-identificacion", typeof(ResultadoIdentificacionPage));
        Routing.RegisterRoute("analisis-salud", typeof(AnalisisSaludPage));
        Routing.RegisterRoute("recordatorios", typeof(RecordatoriosPage));
        Routing.RegisterRoute("cuidados", typeof(CuidadosPage));
    }
}
