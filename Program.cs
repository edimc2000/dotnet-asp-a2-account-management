using AccountManagement.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Reflection;
using static AccountManagement.AccountEndpoints;
using static AccountManagement.Support.ApiDocsResponseFormat;


namespace AccountManagement;

public class Program
{
    public static void Main(string[] args)
    {
        // enhanced logging seri logging 
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Endpoint} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();


        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // seri log 
        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddDbContext<AccountDb>(options =>
                // the connection string should be a secret / configuration variable 
                options.UseSqlite("Data Source=./account.db"),
            ServiceLifetime.Scoped); // Each request gets its own instance

        // swagger 
        builder.Services.AddEndpointsApiExplorer();

        // swagger page - configuration 
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

        ////******** these are optional environment variables and secrets which might not be
        //// required 
        //builder.Configuration.Sources.Clear();
        //builder.Configuration
        //    .AddJsonFile("sharedSettings.json", true);
        //builder.Configuration.AddJsonFile("appsettings.json", true, true);
        //builder.Configuration.AddEnvironmentVariables();
        //builder.Configuration.AddJsonFile("secrets2.json", true, true);

        //builder.Services.Configure<AppDisplaySettings>(
        //    builder.Configuration.GetSection("AppDisplaySettings"));
        ////******** these are optional environment variables and secrets which might not be
        //// required 

        ////if (builder.Environment.IsDevelopment())
        ////{
        ////    builder.Configuration.AddUserSecrets<Program>();
        ////}


        WebApplication app = builder.Build();

        //seri logging 
        app.Use(async (context, next) =>
        {
            Endpoint? endpoint = context.GetEndpoint();

            // Push endpoint name to log context
            LogContext.PushProperty("EndpointName",
                endpoint?.DisplayName ?? context.Request.Path);

            // Log request start
            Log.Information("\n-----\nRequest started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            Stopwatch sw = Stopwatch.StartNew();
            await next();
            sw.Stop();

            // Log request completion
            Log.Information("Request completed in {ElapsedMilliseconds}ms with {StatusCode}",
                sw.ElapsedMilliseconds,
                context.Response.StatusCode);
        });


        // swagger  
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Account API V1");

            // custom CSS to hide schemas and other page elements 
            options.InjectStylesheet("/swagger-ui/custom.css");
        });

        app.UseSwagger();

        // enabled static files to serve css to reformat swagger UI
        app.UseStaticFiles();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            AccountDb db = scope.ServiceProvider.GetRequiredService<AccountDb>();
            db.Database.Migrate(); // Apply any pending migrations
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;"); // Enable WAL
        }



        // search all - this is not required but good to have 
        app.MapGet("/account/search/all", SearchAll)
            .WithName("Search")
            .WithTags("Search")
            .Produces<
                ApiSearchResponseFormat<Account[]>>(
                200); // got to match the info since this is not automated;

        // search by id
        app.MapGet("/account/search/id/{id}",
                async (string id, AccountDb db) =>
                {
                    (IResult result, int counter) result = await SearchById(id, db);
                    return result.result;
                }
            )
            .WithName("GetAccountById")
            .WithSummary("Search for account using an account id")
            .WithTags("Search")
            .Produces<ApiResponseFail>(400)
            .Produces<
                ApiSearchResponseFormat<Account[]>>(
                200); // got to match the info since this is not automated

        // search by email 
        app.MapGet("/account/search/email/{email}",
                async
                    (string email, AccountDb db) =>
                {
                    (IResult result, int counter) result = await SearchByEmail(email, db);
                    return result.result;
                })
            .WithName("GetAccountByEmail")
            .WithSummary("Search for account using an email address")
            .WithTags("Search")
            .Produces<ApiSearchResponseFormat<Account[]>>(200);


        // Register endpoint
        app.MapPost("/account/register",
                async (HttpContext context, AccountDb db) => await AddAccount(context, db))
            .WithSummary("Register a new account")
            .Accepts<IJsonAccountInput>("application/json")
            .WithTags("Register")
            .WithName("RegisterAccount")
            .Produces<ApiResponseSuccess<AccountData>>(201)
            .Produces<ApiResponseNull>(422)
            .Produces<ApiResponseMalformed>(400)
            .Produces<ApiResponseDuplicate>(409);

        // Update endpoint
        app.MapPatch("/account/update/id/{id}",
                async
                    (HttpContext context, AccountDb db, string id) =>
                    await UpdateAccount(context, db, id))
            .WithName("UpdateAccountById")
            .WithSummary("Update account using an account id")
            .WithTags("Update")
            .Accepts<IJsonAccountInput>("application/json")
            .Produces<ApiResponseSuccess<AccountData>>(200)
            .Produces<ApiResponseNull>(422)
            .Produces<ApiResponseMalformed>(400)
            .Produces<ApiResponseDuplicate>(409);



        app.Run();
    }
}