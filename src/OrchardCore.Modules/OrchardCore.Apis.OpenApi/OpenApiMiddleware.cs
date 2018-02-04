using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Apis.OpenApi
{
    public class OpenApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly OpenApiSettings _settings;
        private readonly ShellSettings _shellSettings;

        public OpenApiMiddleware(
            RequestDelegate next,
            OpenApiSettings settings,
            ShellSettings shellSettings)
        {
            _next = next;
            _settings = settings;
            _shellSettings = shellSettings;
        }

        public Task Invoke(HttpContext context)
        {
            if (!IsOpenApiRequest(context))
            {
                return _next(context);
            }
            else
            {
                return ExecuteAsync(context);
            }
        }

        private bool IsOpenApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                && String.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase);
        }

        private Task ExecuteAsync(HttpContext context)
        {
            var descriptionProvider = context
                .RequestServices
                .GetService<IApiDescriptionGroupCollectionProvider>();


            var document = new OpenApiDocument();

            document.Info = new OpenApiInfo
            {
                Title = _shellSettings.Name
            };

            document.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = context.Request.Path }
            };

            document.Paths = new OpenApiPaths
            {
            };

            foreach (var group in descriptionProvider.ApiDescriptionGroups.Items)
            {
                foreach (var description in group.Items)
                {
                    document.Paths.Add(
                        description.RelativePath,
                        new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [(OperationType)Enum.Parse(typeof(OperationType), description.HttpMethod)] = new OpenApiOperation {
                                    
                                } 
                            }
                        }
                    );
                }
            }
            
            return Task.FromResult(document);
        }
    }
}