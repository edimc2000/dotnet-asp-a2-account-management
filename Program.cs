using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Context;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using static AccountManagement.AccountEndpoints;
using static AccountManagement.ApiResponseFormat;
using static AccountManagement.DbOperation;
using static AccountManagement.Helper;

namespace AccountManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //using AccountDb db = new();
            //DbSet<Account> accounts = db.Accounts;

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
            //builder.Services.AddSwaggerGen();

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

            // enabled static files serving for swagger UI reformatting
            //app.UseStaticFiles();

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
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Account API V1");

                // custom CSS to hide schemas and other page elements 
                options.InjectStylesheet("/swagger-ui/custom.css");
            });

            app.UseStaticFiles(); // enabled static files for swagger UI reformatting
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
                    (string id, AccountDb db) =>
                    {
                        (IResult result, int counter) result = SearchById(id, db);
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
                    (string email, AccountDb db) =>
                    {
                        (IResult result, int counter) result = SearchByEmail(email, db);
                        return result.result;
                    })
                .WithName("GetAccountByEmail")
                .WithSummary("Search for account using an email address")
                .WithTags("Search")
                .Produces<ApiSearchResponseFormat<Account[]>>(200);



    

            app.MapPut("/account/update/id/{id}",
                    (string id) => { return Results.Ok(new { message = $"Account {id}" }); })
                .WithName("UpdateAccountById")
                .WithSummary("Update account using an account id")
                .WithTags("Update")
                .Produces<ApiResponseSuccess<AccountData>>(200);
            // got to match the info since this is not automated


            app.MapPut("/account/update/email/{email}",
                    (string id) => { return Results.Ok(new { message = $"Account {id}" }); })
                .WithName("UpdateAccountByEmail")
                .WithSummary("Update account using an email address")
                .WithTags("Update")
                .Produces<ApiResponseSuccess<AccountData>>(200);
            // got to match the info since this is not automated


            //improvement needed  this looks ok 


            app.MapPost("/account/register",
                    async (HttpContext context, AccountDb db) =>
                    {
           
                        (InputDataConverter? dataConverter, IResult? error) =
                            await TryReadJsonBodyAsync<InputDataConverter>(context.Request);
                        if (error != null) return error;


                        int newIdNumber = GetLastIdNumber(db) + 1;
                        string firstName = dataConverter.FirstName.ToString();
                        string lastName = dataConverter.LastName.ToString();
                        string emailAddress = dataConverter.EmailAddress.ToString();
                        
                        // Log the received data FILTER 1 
                        WriteLine($"-- data received (INPUT ---\n --- with valid json format ---\n" +
                                  $"   >>>> FirstName: {dataConverter.FirstName}");
                        WriteLine($"   >>>> LastName: {dataConverter.LastName}");
                        WriteLine($"   >>>> EmailAddress: {dataConverter.EmailAddress}");




                        //Create account object(add to database, etc.)
                        Account newAccount = new()
                        {
                            Id = newIdNumber,
                            FirstName = firstName,
                            LastName = lastName,
                            EmailAddress = emailAddress,
                            CreatedAt =  DateTime.UtcNow
         
                        };




                        var validationContext = new ValidationContext(newAccount);
                        var validationResults = new List<ValidationResult>();
                        bool isValid = Validator.TryValidateObject(newAccount, validationContext, validationResults, true);
                       
                        if (!isValid)
                        {
                            WriteLine($"----DATA ANNOTATION DEBUG DATA NOT VALID -"); 
                            var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                            return BadRequest($"Validation failed: {errors}");

                            //WriteLine($"Validation failed: {errors}");
                        }

                        // ensure that there are no existing records 

                        
                        int emailCount = SearchByEmail(emailAddress, db).counter;
                        WriteLine($"--- CHECKING FOR EXISTING RECORD - email record count {emailCount}---\n"); 

                        if (emailCount!=0)
                        {
                           
                            return ConflictResult($"The email address is either tied to an account or cannot be used for registration");

                        }





                        // TODO: Save to database

                        ////using AccountDb db = new();
                        ////DbSet<Account> accounts = db.Accounts;
                        //// Query 1: Get all accounts as IQueryable
                        //IQueryable<Account> allAccounts = db.Accounts;

                        // Execute the query and get results
                        //List<Account> accountList = allAccounts.ToList();

                        // Display all accounts
                        //WriteLine("All Accounts:");
                        //WriteLine("-------------");
                        //foreach (Account account in accountList)
                        //{
                        //    WriteLine($"ID: {account.Id}");
                        //    WriteLine($"Name: {account.FirstName} {account.LastName}");
                        //    WriteLine($"Email: {account.EmailAddress}");
                        //    WriteLine($"Created: {account.CreatedAt}");
                        //    WriteLine($"Updated: {account.UpdatedAt}");
                        //    WriteLine();
                        //}

                        // Return success with created account
                        return Results.CreatedAtRoute("GetAccountById",
                            new { id = newIdNumber },
                            // this is the correct one 
                            new
                            {
                                success = true, // Fixed syntax: use = not :
                                message = "Account created successfully",
                                data = newAccount // Fixed syntax: removed semicolon
                            }
                        );
                    })
                //.AddEndpointFilter<ValidateJsonFilter>()
                .WithSummary("Register a new account")
                .Accepts<IJsonAccountInput>("application/json")
                //.Accepts<AccountData>("a")
                .WithTags("Register")
                .WithName("RegisterAccount")
                .Produces<ApiResponseSuccess<AccountData>>(201)
                .Produces<ApiResponseNull>(422)
                .Produces<ApiResponseMalformed>(400)
                .Produces<ApiResponseDuplicate>(409)
                ;


            app.Run();
        }


        private static string RegisterUser(string username)
        {
            EmailSender email = new();
            email.SendEmail(username);

            return $"Email sent to {username}!";
        }
    }
}


public class EmailSender
{
    public void SendEmail(string username)
    {
        WriteLine(new string('-', 80) +
                  $"\nThis should be seen only at the back end >>>  Email sent to {username}!");
    }
}


public class AppDisplaySettings
{
    public string Title { get; set; }
    public bool ShowCopyright { get; set; }
}