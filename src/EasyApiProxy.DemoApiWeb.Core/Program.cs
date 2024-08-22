using EasyApiProxys;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 授權服務註冊
builder.Services.AddAuthentication()
    // 註冊Hawk驗證
    .AddHawkAuthorize(opt =>
    {
        // 設定證書
        opt.Credentials.Add(new HawkNet.AspNetCore.HawkCredential
        {
            Id = "123",
            Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn",
            Algorithm = "sha256",
            User = "Admin",
            Roles= ["Admins"]
        });
    });

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

// 使用路由
app.UseRouting();

// 啟用路由後 底下這種的就可以執行
//app.MapGet("/hello/{name:alpha}", (string name) => $"Hello {name}!");

// 啟用驗證機制
app.UseAuthentication();
// 啟用授權機制 (必需在使用路由之後)
app.UseAuthorization();

// 啟用 API Controllers
app.MapControllers();

app.Run();

