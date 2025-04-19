using Couchbase.Extensions.DependencyInjection;
using MyCryptoAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<CouchbaseSettings>(builder.Configuration.GetSection("Couchbase"));

builder.Services.AddCouchbase(opt =>
{
    var config = builder.Configuration.GetSection("Couchbase").Get<CouchbaseSettings>();
    if (config is null)
    {
        throw new InvalidOperationException("Couchbase settings are not configured properly.");
    }

    opt.ConnectionString = config.ConnectionString;
    opt.UserName = config.Username;
    opt.Password = config.Password;
});

builder.Services.AddCouchbaseBucket<INamedBucketProvider>("bucketname"); // Profesor duhet te keni te instaluar Couchbase si aplikacion dhe tek appsettings.json 
                                                                         // te ndryshoni te dhenat e databazes dhe tek couchbaseservice.cs dhe couchbasesettings.cs 
                                                                         // te vendosni emrin e bucketit qe krijoni ne couchbase ne menyre qe databaza dhe backendi 
                                                                         // te funksjonojne sic duhet
builder.Services.AddSingleton<UserService>();
var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapControllers(); 

app.Lifetime.ApplicationStopped.Register(() =>
{
    app.Services.GetRequiredService<ICouchbaseLifetimeService>().Close();
});

app.Run();
