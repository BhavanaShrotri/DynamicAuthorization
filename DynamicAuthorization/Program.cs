using DynamicAuthorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var permissionsList = new PermissionsList();

builder.Services.AddSingleton(permissionsList);
builder.Services.AddSingleton<PermissionsDatabase>();
builder.Services.AddAuthentication("cookie").AddCookie("cookie");

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", (ClaimsPrincipal user) => user.Claims.Select(x => new { x.Type, x.Value }));

app.MapGet("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new Claim[] 
            {
                new Claim (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim (ClaimTypes.Country,"IN"),
            },
            "cookie"
        )),
    authenticationScheme: "cookie"
));


app.MapGet("/secret/one", ()=>"one").RequireAuthorization().WithTags("auth:secret/one", "auth:secret");
app.MapGet("/secret/one", ()=>"two").RequireAuthorization().WithTags("auth:secret/two", "auth:secret");
app.MapGet("/secret/one", ()=>"three").RequireAuthorization().WithTags("auth:secret/three", "auth:secret");

app.MapGet("/permissions", (PermissionsList p)=> p);

app.MapGet("/promote", (
    string permission,
    ClaimsPrincipal user,
    PermissionsDatabase database) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    database.AddPermissions(userId, permission);
});

var endpoints = app as IEndpointRouteBuilder;
var source = endpoints.DataSources.First();
foreach (var sourceEndpoint in source.Endpoints)
{
    var authTags = sourceEndpoint.Metadata.OfType<TagsAttribute>().SelectMany(x => x.Tags).Where(x => x.StartsWith("auth"));

    foreach (var authTag in authTags)
    {
        permissionsList.Add(authTag);
    }
}


app.Run();

public class PermissionsList : HashSet<string>
{

}