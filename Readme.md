# EasyInjector

* 極簡實作 沒有任何其他套件依賴
* 仿照 .net core dependency injection 的使用方法
* 簡單的注入依賴，適用於舊專案.NET4 Framework
* 目標也只是讓一些很古老的專案在無法升級的請況下，有限度的導入注入依賴。
* [Nuget Package Install](https://www.nuget.org/packages/EasyInjector/)


## Singleton 範例
``` C#
 using(var injector = new EasyInjector()) {
	//註冊服務
	injector.AddSingleton<IWebApi>(sp => new MockWebApi());

	//取得服務
	var srv = injector.GetService<IWebApi>();
	srv.GetSomething();
 }

```

## Scope 範例
``` C#

 using(var injector = new EasyInjector()) {
	//註冊服務
 	injector.AddScope<IWebApi>(sp => new MockWebApi());

	// 取得服務
	using(var scope = injector.CreateScope()) {
		var srv = scope.ServiceProvider.GetService<IWebApi>();
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
            new MockFtp(name)));

    using (var scope = injector.CreateScope())
    {
		// 取得本服務 再透過他取得其他實例 by Name
        var srvf = scope.ServiceProvider.GetRequiredService<INamed<IFtp>>();

		// 依名稱取的不同實例
        var srv1 = srvf.GetByName("Spring");
        var srv2 = srvf.GetByName("Summer");

		// 名稱相同會取到同一個 於相同Scope中
		var srv3 = srvf.GetByName("Spring");
    }
}
```

## 額外提供 IOptional 選擇性服務
``` C#

	// 某服務的應用情境展示
	public class SomeService {

		readonly IOptional<IFtp> _ftpf;
		readonly IServiceScopeFactory _scopef;

		// 建構式有清楚的描述本服務 有依賴IFtp的關係
		public SomeService(
			IOptional<IFtp> ftpf,
			IServiceScopeFactory scopef)
		{
			_ftpf = ftpf;
			_scopef = scopef;
		}

		// 某些動作才須要實際取得IFtp服務 呼叫完成就釋放
		public void DoSomething() {
			using(var scope = _scopef.CreateScope()) {
				// 這時候才真的產生這個服務
				var ftp = _ftpf.Get(scope.ServiceProvider);

				ftp.DoSomething();
			}
		}
	}

```

## 匯入其他的注入器的服務
> 這個想法希望是可以快速方便使用，不用重複註冊工作，只是不能算是很嚴謹就是。
``` C#
	// 權限專案的注入器
	class SecurityInjector : EasyInjector {
		SecurityInjector() {
			//註冊註冊
			AddSingleton<ISecurityBusiness>(sp => ...);
		}
	}

	// 本專案的注入器
	class AppInjector : EasyInjector {

		AppInjector() {

			// 匯入權限的服務的服務註冊表
			ImportServices(new SecurityInjector());

			// 註冊本專案的服務
			AddSingleton<IMockBusindess>(sp => ...);
		}
	}

	var injetor = new AppInjector();

	// 取得 其他模組的服務
	var srv1 = injector.Get<ISecurityBusiness>();

```