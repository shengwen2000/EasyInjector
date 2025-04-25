using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

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
            Roles = ["Admins"]
        });

        // 也可以自行實作取得證書方法
        //opt.GetCredentialFunc = (id) => Task.FromResult(new HawkNet.AspNetCore.HawkCredential());
    })
    // 註冊 Basic 驗證
    .AddBasicAuthorize(opt =>
    {
        // 設定證書
        opt.Credentials.Add(new BasicAuth.AspNetCore.BasicCredential
        {
            Account = "admin",
            PassCode = "admin1234",
            User = "Admin",
            Roles = [ "Admins" ]
        });
    });

builder.Services.AddControllers()
    // 使用 KmuhomeApi 協助 json 選項
    .AddKmuhomeApiJsonOptions();

// requiredif 這種的
builder.Services.AddExpressiveAnnotations();

var app = builder.Build();

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

// 啟用憑證檢驗 確定憑證有效性
app.UseAuthentication();
// 啟用權限驗證機制 檢查你是否有權限存取方法
app.UseAuthorization();

// 啟用 API Controllers
app.MapControllers();

app.Run();

