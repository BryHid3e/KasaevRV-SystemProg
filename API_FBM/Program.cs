using API_FBM.Models;
using API_FBM.Services;
using API_FBM.Filters;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddScoped<UserService>();

// Настройка сериализации JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Игнорирование циклических ссылок
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API для управления пользователями",
        Version = "v1",
        Description = "API для выполнения операций с пользователями в базе данных Supabase",
        Contact = new OpenApiContact
        {
            Name = "Администратор API",
            Email = "admin@example.com"
        }
    });
    
    // Включаем XML-комментарии для Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // Создаем файл XML-документации, если он не существует
    if (!File.Exists(xmlPath))
    {
        // Указываем Swagger использовать комментарии без XML-файла
        c.IncludeXmlComments(() => new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<doc></doc>")));
    }
    else
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Настройка схемы отображения свойств модели
    c.UseInlineDefinitionsForEnums();
    c.UseAllOfForInheritance();
    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});

// Добавление CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API пользователей v1");
        c.DefaultModelsExpandDepth(1); // Глубина развёртывания моделей по умолчанию
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // Все операции свёрнуты изначально
        c.DisplayRequestDuration(); // Показываем длительность запроса
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Логирование успешного запуска
Console.WriteLine("Приложение запущено. Проверьте соединение с базой данных через /api/Users/test-connection");

app.Run();
