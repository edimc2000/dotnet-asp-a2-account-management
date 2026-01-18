using AccountManagement.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Reflection;
using AccountManagement.EntityModels;
using static AccountManagement.AccountEndpoints;
using static AccountManagement.Support.ApiDocsResponseFormat;


namespace AccountManagement;

/// <summary>>Main program class for the Account Management API.</summary>
/// <para>Author: Eddie C.</para>
/// <para>Version: 1.0</para>
/// <para>Date: Jan. 18, 2026</para>
public class Program
{
 
    /// <summary>Main entry point for the application.</summary>
    public static void Main(string[] args)
    {
        // Enhanced logging using SeriLog 
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Endpoint} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();


        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // SeriLog 
        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddDbContext<AccountDb>(options =>
                options.UseSqlite("Data Source=./account.db"),
            ServiceLifetime.Scoped); // Each request gets its own instance

        // Swagger 
        builder.Services.AddEndpointsApiExplorer();

        // Swagger page - configuration 
        builder.Services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Customer Account Management API",
                    Description =
                        "API for managing customer accounts. Allows customers to register new accounts, retrieve account data by ID or email, and update existing account information.",
                    Version = "1.1"
                }
            );

            string file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, file));

            opt.UseAllOfForInheritance();
            opt.SelectDiscriminatorNameUsing(type => type.Name);
            opt.SelectDiscriminatorValueUsing(type => type.Name);
        });

        WebApplication app = builder.Build();

        // SeriLog
        app.Use(async (context, next) =>
        {
            Endpoint? endpoint = context.GetEndpoint();

            // Push endpoint name to log context
            LogContext.PushProperty("EndpointName",
                endpoint?.DisplayName ?? context.Request.Path);

            // Log request start
            Log.Information("********** Request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            Stopwatch sw = Stopwatch.StartNew();
            await next();
            sw.Stop();

            // Log request completion
            Log.Information(
                "********** Request completed in {ElapsedMilliseconds}ms with {StatusCode}",
                sw.ElapsedMilliseconds,
                context.Response.StatusCode);
        });


        // Swagger  
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Account API V1");

            // custom CSS to hide schemas and other page elements 
            options.InjectStylesheet("/swagger-ui/custom.css");
        });

        app.UseSwagger();

        // Enabled static files to serve css to reformat Swagger UI
        app.UseStaticFiles();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            AccountDb db = scope.ServiceProvider.GetRequiredService<AccountDb>();
            db.Database.Migrate(); // Apply any pending migrations
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;"); // Enable WAL
        }


        // Search all endpoint - this is not required but good to have 
        app.MapGet("/account/search/all", SearchAll)
            .WithName("Search")
            .WithSummary("Retrieve all accounts")
            .WithTags("2 - Read (Search)")
            .Produces<
                ApiSearchResponseFormat<Account[]>>(
                200);

        // Search by id endpoint
        app.MapGet("/account/search/id/{id}",
                async (string id, AccountDb db) =>
                {
                    (IResult result, int counter) result = await SearchById(id, db);
                    return result.result;
                })
            .WithName("GetAccountById")
            .WithSummary("Search for account using an account id")
            .WithTags("2 - Read (Search)")
            .Produces<ApiResponseFail>(400)
            .Produces<
                ApiSearchResponseFormat<Account[]>>(200);

        // Search by email endpoint
        app.MapGet("/account/search/email/{email}",
                async (string email, AccountDb db) =>
                {
                    (IResult result, int counter) result = await SearchByEmail(email, db);
                    return result.result;
                })
            .WithName("GetAccountByEmail")
            .WithSummary("Search for account using an email address")
            .WithTags("2 - Read (Search)")
            .Produces<ApiSearchResponseFormat<Account[]>>(200);


        // Register endpoint
        app.MapPost("/account/register",
                async (HttpContext context, AccountDb db) => await AddAccount(context, db))
            .WithSummary("Register a new account")
            .WithName("RegisterAccount")
            .WithTags("1 - Create (Register)")
            .Accepts<IJsonAccountInput>("application/json")
            .Produces<ApiResponseSuccess<AccountData>>(201)
            .Produces<ApiResponseNull>(422)
            .Produces<ApiResponseMalformed>(400)
            .Produces<ApiResponseDuplicate>(409);

        // Update using account Id endpoint
        app.MapPatch("/account/update/id/{id}",
                async (HttpContext context, AccountDb db, string id) =>
                    await UpdateAccount(context, db, id))
            .WithName("UpdateAccountById")
            .WithSummary("Update account using an account id")
            .WithTags("3 - Update")
            .Accepts<IJsonAccountInput>("application/json")
            .Produces<ApiResponseSuccess<AccountData>>(200)
            .Produces<ApiResponseNull>(422)
            .Produces<ApiResponseMalformed>(400)
            .Produces<ApiResponseDuplicate>(409);
        
        // Delete using account Id endpoint
        app.MapDelete("/account/delete/id/{id}",
                async (string id, AccountDb db) =>
                {
                    IResult result = await DeleteById(id, db);
                    return result;
                })
            .WithName("DeleteAccountById")
            .WithSummary("Delete account using an account id")
            .WithTags("4 - Delete")
            .Produces<ApiResponseDeleteFailed>(400)
            .Produces<ApiResponseDeleteSuccess>(200);
        
        app.Run();
    }
}