using ASTA.SharedModels;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ASTA.GameApi;

public sealed class SwaggerExamples : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Player))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(0),
                ["userName"] = new OpenApiString("Stanley"),
                ["level"] = new OpenApiInteger(7)
            };
        }
        else if (context.Type == typeof(Dungeon))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(0),
                ["name"] = new OpenApiString("Forteresse EFREI"),
                ["rooms"] = new OpenApiArray
                {
                    new OpenApiObject { ["id"] = new OpenApiInteger(0), ["name"] = new OpenApiString("Hall"), ["dungeonId"] = new OpenApiInteger(0) }
                }
            };
        }
        else if (context.Type == typeof(Room))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(0),
                ["name"] = new OpenApiString("Salle du Boss"),
                ["dungeonId"] = new OpenApiInteger(1)
            };
        }
        else if (context.Type == typeof(Admin))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(0),
                ["login"] = new OpenApiString("root")
            };
        }
    }
}
