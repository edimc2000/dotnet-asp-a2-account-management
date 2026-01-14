using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Net.Mime;
using System.Security.Principal;
using System.Text.Json;
using static AccountManagement.Helper;


namespace AccountManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // swagger 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            // swagger page - configuration 
            builder.Services.AddSwaggerGen(doc =>
            {
                doc.SwaggerDoc("v1",
                    new OpenApiInfo()
                    {
                        Title = "Customer Account Management API",
                        Description =
                            "API for managing customer accounts. Allows customers to register new accounts, retrieve account data by ID or email, and update existing account information.",
                        Version = "1.0"
                    }


                );

                doc.UseAllOfForInheritance();
                doc.SelectDiscriminatorNameUsing(type => type.Name);
                doc.SelectDiscriminatorValueUsing(type => type.Name);

            });


            //******** these are optional environment variables and secrets which might not be
            // required 
            builder.Configuration.Sources.Clear();
            builder.Configuration
                .AddJsonFile("sharedSettings.json", true);
            builder.Configuration.AddJsonFile("appsettings.json", true, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddJsonFile("secrets2.json", true, true);

            builder.Services.Configure<AppDisplaySettings>(
                builder.Configuration.GetSection("AppDisplaySettings"));
            //******** these are optional environment variables and secrets which might not be
            // required 

            //if (builder.Environment.IsDevelopment())
            //{
            //    builder.Configuration.AddUserSecrets<Program>();
            //}

            WebApplication app = builder.Build();

            // swagger exposition 
            app.UseSwagger();
            app.UseSwaggerUI();


            // Define endpoints WITH NAMES for link references


            app.MapGet("/account/{id}", (string id) =>
                {
                    return Results.Ok(new { message = $"Account {id}" });
                })
                .WithName("GetAccountById") 
                .WithSummary("Search for account")
                .WithTags("Search for an account using an Id number");

            //improvement needed  this looks ok 

            app.MapPost("/account/register",
                async (HttpContext context) =>
                {
                    // Model binding handles validation automatically
                    // But you can add additional validation



                    (InputDataConverter? dataConverter, IResult? error) =
                        await TryReadJsonBodyAsync<InputDataConverter>(context.Request);
                    if (error != null)
                    {
                        return error;
                    }



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
                    var accountId = Guid.NewGuid().ToString();

                    //Create account object(add to database, etc.)
                    var newAccount = new
                    {
                        Id = accountId,
                        FirstName = dataConverter.FirstName,
                        LastName = dataConverter.LastName,
                        EmailAddress = dataConverter.EmailAddress
                    };

                    // TODO: Save to database

                    // Return success with created account
                    return Results.CreatedAtRoute( "GetAccountById", new { id = accountId },
                        // this is the correct one 

                        new 
                        {
                            success = true,  // Fixed syntax: use = not :
                            message = "Account created successfully",
                            data = newAccount  // Fixed syntax: removed semicolon
                        }
                    );
        
                })
                //.AddEndpointFilter<ValidateJsonFilter>()
                .WithSummary("Register a new account")
                .Accepts<Account>("application/json")
                .WithTags("Register new account")
                .WithName("RegisterAccount")
                
                .Produces<ApiResponseMalformed>(400) // not possible 
                .Produces<ApiResponseNull>(422) 
                .Produces<ApiResponse<AccountResponse>>(201) 
                

                //.Produces(201)


                ;




            //app.MapGet("/",
            //    () =>
            //    {
            //        string? zoomLevel = builder.Configuration["MapSettings:DefaultZoomLevel"];

            //        string? lat = builder.Configuration
            //            .GetSection("MapSettings")["DefaultLocation:Latitude"];

            //        string? pass = builder.Configuration
            //            .GetSection("account")["password"];

            //        WriteLine($"zoom level from app settings json file - {zoomLevel}");
            //        WriteLine($"lat from app settings json file - {lat}");
            //        WriteLine($"pass from json env- {pass}");


            //        return app.Configuration.AsEnumerable();
            //    });


            //app.MapGet("/display-settings",
            //    (IOptionsSnapshot<AppDisplaySettings> options) =>
            //    {
            //        AppDisplaySettings settings = options.Value;
            //        return new
            //        {
            //            title = settings.Title,
            //            showCopyright = settings.ShowCopyright
            //        };
            //    })
            //    .WithTags("Display app settings ")  // Add tag for grouping in Swagger/OpenAPI
            //    .Produces<object>(StatusCodes.Status200OK)  // Document the successful response
            //    .ProducesProblem(StatusCodes.Status500InternalServerError)
            //    .ProducesProblem(401); 
            //// Document possible errors


            //// requirement from A2 (1) - data ID, firstname, lastname and email address < -- use
            //// json 
           
            //// requirement from A2 (2) - retrieve  data using either the id or the email address
            //app.MapGet("/account/search/id/{id}",
            //    (int id) => "search using id number"); // validate int 
            //app.MapGet("/account/search/email/{email}",
            //    (string email) =>
            //    {
            //        WriteLine($"search using email address {email}");
            //        WriteLine($"is email valid? {InputValidation.IsValidEmail(email)}");
            //    }); // validate string name   // validate if the email format is correct and if the
            //// record exists - later on  

            //// requirement from A2 (3), update the data either by id or name 
            //app.MapPut("/account/update/", () => "Updating using ID");


            //app.MapGet("/register/{username}", RegisterUser);
            //app.MapGet("/products/)", () => "products").WithName("products2");
            ////app.MapRazorPages(); // remove later as it might not be needed

            //app.MapGet("/links",
            //    (LinkGenerator? links) =>
            //    {
            //        string link = links.GetPathByName("products2");
            //        WriteLine(link);
            //        return $"View the product at {link}";
            //    });


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

//public class EmailSender
//{
//    private readonly NetworkClient _client;
//    private readonly MessageFactory _factory;
//    public EmailSender(MessageFactory factory, NetworkClient client)
//    {
//        _factory = factory;
//        _client = client;
//    }
//    public void SendEmail(string username)
//    {
//        var email = _factory.Create(username);
//        _client.SendEmail(email);
//        Console.WriteLine($"Email sent to {username}!");
//    }
//}

//public class NetworkClient
//{
//    private readonly EmailServerSettings _settings;
//    public NetworkClient(EmailServerSettings settings)
//    {
//        _settings = settings;
//    }
//}


public class AppDisplaySettings
{
    public string Title { get; set; }
    public bool ShowCopyright { get; set; }
}



