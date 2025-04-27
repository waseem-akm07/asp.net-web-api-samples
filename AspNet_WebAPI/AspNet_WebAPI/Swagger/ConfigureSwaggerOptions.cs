using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNet_WebAPI.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        public static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription versionDescription)
        {
            var info = new OpenApiInfo()
            {
                Title = "Versioning Web API",
                Version = versionDescription.ApiVersion.ToString(),
                Description = "Description for the Versioning Web Api"
            };

            if (versionDescription.IsDeprecated)
            {
                info.Description += "This API version has been deprecated";
            }

            return info;
        }
    }
}
