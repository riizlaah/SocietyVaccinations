using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SocietyVaccinations;
using SocietyVaccinations.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<TokenBlacklister>();
builder.Services.AddDbContext<SVContext>();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            ctx.HandleResponse();
            var res = ApiResponse<object>.Error("Authorization token missing or invalid", 401);
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = "application/json";
            ctx.Response.WriteAsJsonAsync(res);
        },
        OnForbidden = async ctx =>
        {
            var res = ApiResponse<object>.Error("Access denied, require higher role", 403);
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "application/json";
            ctx.Response.WriteAsJsonAsync(res);
        },
        OnTokenValidated = async ctx =>
        {
            var blacklister = ctx.HttpContext.RequestServices.GetRequiredService<TokenBlacklister>();
            var tokId = ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? "";

            if (blacklister.IsBanned(tokId))
            {
                ctx.Fail("Token not valid");
            }
        }

    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ResponseWrapper>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    });
    opts.OperationFilter<SecurityFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errs = ctx.ModelState
        .Where(e => e.Value.Errors.Count > 0)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
        var res = ApiResponse<object>.Error("Validation failed", 422, errs);
        return new ObjectResult(res)
        {
            StatusCode = 422
        };
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
