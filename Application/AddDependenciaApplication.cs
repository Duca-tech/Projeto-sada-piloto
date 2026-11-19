using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class AddDependenciaApplication
    {

        public static void AddIndependencieApplication(this IServiceCollection services)
        {
            services.AddScoped<Tarefa.TarefaCasoUso>();
            services.AddScoped<Tarefa.TarefaService>();
        }
    }
}
