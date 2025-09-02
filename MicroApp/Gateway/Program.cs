var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

const string CorsPolicy = "AllowFlutterDev";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            // For dev you can allow all origins. If you use cookies, don't use AllowAnyOrigin.
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()  // includes Authorization, Content-Type
            .AllowAnyMethod(); // includes OPTIONS preflight, POST, etc.
        // If you use cookies with SameSite=None:
        // .AllowCredentials();
        // and then DO NOT use AllowAnyOrigin — specify exact origins instead.
    });
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Gateway is up"));
app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy); 

app.Run();

