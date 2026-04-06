using LightningArc.Primitives.ValueObjects;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

#if NET9_0
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif

namespace LightningArc.OpenAPI.AspNetCore.Filters
{
    /// <summary>
    /// An OpenAPI schema transformer that ensures the <see cref="Email"/> type
    /// is represented as a simple string in the OpenAPI document.
    /// </summary>
    public class EmailSchemaTransformer : IOpenApiSchemaTransformer
    {

        /// <summary>
        /// Transforms the OpenAPI schema for the <see cref="Email"/> type,
        /// ensuring it is represented as a string with an "email" format.
        /// </summary>
        /// <param name="schema">The current OpenAPI schema to be transformed.</param>
        /// <param name="context">The schema transformer context, containing information about the schema being processed.</param>
        /// <param name="cancellationToken">A token to observe for operation cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// If the <paramref name="context"/> indicates that the schema belongs to the <see cref="Email"/> type,
        /// its properties will be removed, its type will be set to "string",
        /// its format to "email", and an example will be added for better documentation.
        /// </remarks>
        public Task TransformAsync(
            OpenApiSchema schema,
            OpenApiSchemaTransformerContext context,
            CancellationToken cancellationToken
        )
        {
            if (context.JsonTypeInfo?.Type == typeof(Email))
            {
#if NET9_0
                schema.Properties.Clear();
                schema.Type = "string";
                schema.Example = new OpenApiString("usuario@exemplo.com");
#endif

#if NET10_0
                schema.Properties?.Clear();
                schema.Type = JsonSchemaType.String;
                schema.Example = "usuario@exemplo.com";
#endif

                schema.Format = "email";
            }

            return Task.CompletedTask;
        }
    }
}




