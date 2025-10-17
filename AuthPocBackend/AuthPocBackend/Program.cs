using AuthPocBackend.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

const string myAllowAnyOrigin = "_myAllowAnyOrigin";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowAnyOrigin, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var signingCert = builder.Configuration["JwtTokenOptions:SigningCertificate"];
    var cert = X509Certificate2.CreateFromPem(signingCert);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new X509SecurityKey(cert),
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = builder.Configuration["JwtTokenOptions:Audience"],
        ValidIssuer = builder.Configuration["JwtTokenOptions:Issuer"],
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsUser", policy => policy.RequireClaim("LE-User-Login").Build());
});

builder.Services.Configure<GithubOptions>(
    builder.Configuration.GetSection(GithubOptions.Github));
builder.Services.Configure<JwtTokenOptions>(
    builder.Configuration.GetSection(JwtTokenOptions.Jwt));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(myAllowAnyOrigin);
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();
