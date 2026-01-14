using Microsoft.OpenApi;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using static AccountManagement.Helper;
using static AccountManagement.ApiXmlDoc;
using static AccountManagement.DbOperation;

namespace AccountManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {

            using AccountDb db = new();
            DbSet<Account> accounts = db.Accounts;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // swagger 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // swagger page - configuration 
            builder.Services.AddSwaggerGen(doc =>
            {
                doc.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Customer Account Management API",
                        Description =
                            "API for managing customer accounts. Allows customers to register new accounts, retrieve account data by ID or email, and update existing account information.",
                        Version = "1.1"
                    }
                );

                string file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                doc.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, file));

                doc.UseAllOfForInheritance();
                doc.SelectDiscriminatorNameUsing(type => type.Name);
                doc.SelectDiscriminatorValueUsing(type => type.Name);
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

            // swagger exposition 
            app.UseSwagger();
            app.UseSwaggerUI();



            app.MapGet("/account/search/all", 
                    ()=> 
                    SearchAll (accounts)
                    
                    )
                .WithName("GetAllAccounts")
                .WithTags("Show all accounts")
                .Produces<ApiResponseSuccess<AccountData>>(200); // got to match the info since this is not automated;

            app.MapGet("/account/search/{id}",
                    (string id) => { return Results.Ok(new { message = $"Account {id}" }); })
                .WithName("GetAccountById")
                .WithSummary("Search for account")
                .WithTags("Search for an account using an Id number")
                .Produces<ApiResponseSuccess<AccountData>>(200); // got to match the info since this is not automated


            //improvement needed  this looks ok 


            app.MapPost("/account/register",
                    async (HttpContext context) =>
                    {
                        // Model binding handles validation automatically
                        // But you can add additional validation


                        (InputDataConverter? dataConverter, IResult? error) =
                            await TryReadJsonBodyAsync<InputDataConverter>(context.Request);
                        if (error != null) return error;


                        // Log the received data
                        WriteLine($"-- data received ---\n --- with valid json format ---\n" +
                                  $"FirstName: {dataConverter.FirstName}");
                        WriteLine($"LastName: {dataConverter.LastName}");
                        WriteLine($"EmailAddress: {dataConverter.EmailAddress}");

                        // Validate input (using your InputValidation class)
                        //if (!InputValidation.IsString(request.FirstName))
                        //    return Results.BadRequest("Invalid first name format");

                        //if (!InputValidation.IsString(request.LastName))
                        //    return Results.BadRequest("Invalid last name format");

                        // Check if email already exists (pseudo-code)
                        // bool emailExists = await CheckEmailExists(request.EmailAddress);
                        // if (emailExists) return Results.Conflict("Email already registered");

                        // Generate ID (you mentioned accounts need an ID)
                        // query the database and generate using the last number +1
                        string accountId = Guid.NewGuid().ToString();

                        //Create account object(add to database, etc.)
                        var newAccount = new
                        {
                            Id = accountId,
                            FirstName = dataConverter.FirstName,
                            LastName = dataConverter.LastName,
                            EmailAddress = dataConverter.EmailAddress
                        };

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
                            new { id = accountId },
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
                .WithTags("Register new account")
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