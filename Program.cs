using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Options;
using static AccountManagement.Helper;


namespace AccountManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            //builder.Services.AddRazorPages();  // remove later as it might not be needed
            //builder.Services.AddRazorPages(options=> options.Conventions.A);  // remove later as
            // it might not be needed

            // swagger 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            


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


            app.MapGet("/",
                () =>
                {
                    string? zoomLevel = builder.Configuration["MapSettings:DefaultZoomLevel"];

                    string? lat = builder.Configuration
                        .GetSection("MapSettings")["DefaultLocation:Latitude"];

                    string? pass = builder.Configuration
                        .GetSection("account")["password"];

                    WriteLine($"zoom level from app settings json file - {zoomLevel}");
                    WriteLine($"lat from app settings json file - {lat}");
                    WriteLine($"pass from json env- {pass}");


                    return app.Configuration.AsEnumerable();
                });


            app.MapGet("/display-settings",
                (IOptionsSnapshot<AppDisplaySettings> options) =>
                {
                    AppDisplaySettings settings = options.Value;
                    return new
                    {
                        title = settings.Title,
                        showCopyright = settings.ShowCopyright
                    };
                });


            // requirement from A2 (1) - data ID, firstname, lastname and email address < -- use
            // json 
            app.MapPost("/account/register",
                async (HttpContext context) =>
                {
                    // Read the JSON body
                    using StreamReader reader = new(context.Request.Body);
                    string json = await reader.ReadToEndAsync();

                    // Parse the JSON
                    JsonDocument? jsonObject = JsonSerializer.Deserialize<JsonDocument>(json);

                    // Log all keys
                    foreach (JsonProperty property in jsonObject.RootElement.EnumerateObject())
                        WriteLine(
                            $"{property.Name} :  {property.Value}  -- string input validation " +
                            $"{InputValidation.IsString(property.Value.ToString())}"); // conversion
                    // to string
                    // when the
                    // data is
                    // sent will
                    // be the key
                    // 

                    // Or log specific keys if you know the structure
                    JsonElement root = jsonObject.RootElement;
                    if (root.TryGetProperty("firstName", out JsonElement firstName))
                        WriteLine($"firstName: {firstName}");

                    return Results.Ok();
                }); // validate data   id - int, string for the rest and email address should be the
            // right format 

            // requirement from A2 (2) - retrieve  data using either the id or the email address
            app.MapGet("/account/search/id/{id}",
                (int id) => "search using id number"); // validate int 
            app.MapGet("/account/search/email/{email}",
                (string email) =>
                {
                    WriteLine($"search using email address {email}");
                    WriteLine($"is email valid? {InputValidation.IsValidEmail(email)}");
                }); // validate string name   // validate if the email format is correct and if the
            // record exists - later on  

            // requirement from A2 (3), update the data either by id or name 
            app.MapPut("/account/update/", () => "Updating using ID");


            app.MapGet("/register/{username}", RegisterUser);
            app.MapGet("/products/)", () => "products").WithName("products2");
            //app.MapRazorPages(); // remove later as it might not be needed

            app.MapGet("/links",
                (LinkGenerator? links) =>
                {
                    string link = links.GetPathByName("products2");
                    WriteLine(link);
                    return $"View the product at {link}";
                });


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