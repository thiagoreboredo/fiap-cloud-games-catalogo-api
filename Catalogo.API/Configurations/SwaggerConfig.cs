namespace FIAP_Cloud_Games.Configurations
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "FIAP Cloud Games Swagger",
                    Version = "v1",
                    Description = "FIAP Cloud Games é uma POC para uma plataforma de jogos da FIAP",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "FIAP Cloud Games",
                        Email = "FIAPCloudGames@FIAPCloudGames.com"
                    }
                });
            });
        }
    }
}
