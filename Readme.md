# EasyInjector

* 極簡實作 沒有任何其他套件依賴
* 仿照 .net core dependency injection 的使用方法
* 簡單的注入依賴，適用於舊專案.NET4 Framework
* 目標也只是讓一些很古老的專案在不升級的原則下能導入注入依賴
* 套件下載位置 [NUGET](https://www.nuget.org/packages/EasyInjector/)



## Singleton 範例
``` C#

 injector.AddSingleton<IWebApi>(sp => new ExampleWebApi());
 using(var injector = new EasyInjector()) {
	//取得服務
	var srv = injector.GetSerice<IWebApi>();
	srv.GetSomething();
 }

```

## Scope 範例
``` C#
 var injector = new EasyInjector();
 injector.AddScope<IWebApi>(sp => new ExampleWebApi());

 using(injector) {

	//取得服務
	using(var scope = injector.CreateScope()) {
		var srv = scope.ServiceProvider.GetSerice<IWebApi>();
		srv.GetSomething();
	}
 }
```

## 額外提供 INamed 作為同類服務有多個實例
``` C#
using (var injector = new EasyInjector())
{
	// 註冊服務 定義名稱如何取得實例
    injector.AddScoped<INamed<IFtp>>(sp =>
        new NamedService<IFtp>(name =>
            new FtpService(name)));
    using (var scope = injector.CreateScope())
    {
        var srvf = scope.ServiceProvider.GetRequiredService<INamed<IFtp>>();
		// 依名稱取的不同實例
        var srv1 = srvf.GetByName("Spring");
        var srv2 = srvf.GetByName("Summer");
		// 名稱相同會取到同一個
		var srv3 = srvf.GetByName("Spring");
    }
}
```

## 額外提供 IOptional 選擇性服務
``` C#

	// 某服務
	public class SomeService {

		IOptional<IFtp> _ftpf;
		IServiceScopeFactory _scopef;

		// 建構式有清楚的描述本服務 有依賴 IFtp服務的關係
		public SomeService(
			IOptional<IFtp> ftpf,
			IServiceScopeFactory scopef)
		{
			_ftpf = ftpf;
			_scopef = scopef;
		}

		// 某些動作才須要產生服務IFtp服務 呼叫完成就釋放
		public void DoSomething() {
			using(var scope = _scopef.CreateScope()) {
				// 這時候才真的產生這個服務
				var ftp = _ftpf.Get(scope.ServiceProvider);
				ftp.DoSomething();
			}
		}
	}

```