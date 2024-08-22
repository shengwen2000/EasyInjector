using EasyApiProxys;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    // 使用 DefaultApi 協助 json 選項
    .AddDefaultApiJsonOptions();

var app = builder.Build();
// Microsoft.Extensions.Options.IConfigureOptions`1[Microsoft.AspNetCore.Mvc.JsonOptions
// var opt1 = app.Services.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// 啟用驗證機制
app.UseAuthentication();
// 啟用授權機制 (必需在使用路由之後)
app.UseAuthorization();

// 啟用 API Controllers
app.MapControllers();

app.Run();

