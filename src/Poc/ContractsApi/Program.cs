using Contracts.Data.Configurations;
using Contracts.Api.Configurations;
using System.Text.Json.Serialization;
using Contracts.Service.Interfaces;
using Contracts.Service;
using SqlComplexOperations.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.ConfigureSqlComplexOperations();
builder.Services.AddEntityFrameworkConfiguration(builder.Configuration);

builder.Services.AddScoped<IClubeService, ClubeService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IContratoService, ContratoService>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerSetup();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
