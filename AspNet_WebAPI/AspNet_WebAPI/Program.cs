using AspNet_WebAPI.Swagger;
using AspNet_WebAPI.BusinessLayer.Implementation;
using AspNet_WebAPI.BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Security.Claims;
using AspNet_WebAPI.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSingleton<IAuthorizationHandler, WritePolicyHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ReaderPolicyHandler>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:SecretKey"])),
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"]
    };
});

//Policy-based Authorization
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Read", config =>
    {
        config.RequireAuthenticatedUser();
        config.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        config.Requirements.Add(new ReaderRequirement()); //Call Read Policy based clase.
    });
    opt.AddPolicy("Write", config =>
    {
        config.RequireAuthenticatedUser();
        config.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        config.Requirements.Add(new WriterRequirement()); //Call Write Policy based clase
    });
    opt.AddPolicy("All", config =>
    {
        config.RequireAuthenticatedUser();
        config.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        config.Requirements.Add(new WriterRequirement()); //Call All Policy based clase
    });
    opt.AddPolicy("ContainRole", config =>
    {
        config.RequireClaim(ClaimTypes.Role);
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    //Handle versions on Swagger UI
    options.OperationFilter<SwaggerDefaultValues>();
    //options.SwaggerDoc("v1", new OpenApiInfo // After enabling versioning with swagger no need to this version will bind dynamically
    //{
    //    Title = "JWT Authentication",
    //    Version = "v1"
    //});

    //To enable Authentication on swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer jhfdkj.jkdsakjdsa.jkdsajk\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/heath");

app.Run();
