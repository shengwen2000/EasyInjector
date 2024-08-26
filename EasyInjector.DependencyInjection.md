# EasyInjector.DependencyInjection

* EasyInjector 於 .net core 上 提供一些專用的服務
  - EasyInjector 沒有 .net core版本 而是直接改用內建的 Dependency Injection


## 額外提供 INamed 作為同類服務有多個實例
``` C#
  var services = new ServiceCollection();
  services.AddEasyInjector();
  // 註冊服務 定義名稱如何取得實例
  services.AddNamedScoped<IFtpAdminService>((sp, name) => new FtpAdminService(name));

  var provider = services.BuildServiceProvider(true);
  using var scope = provider.CreateScope();
  // 取得本服務 再透過他取得其他實例 by Name
  var srv = scope.ServiceProvider
  	.GetRequiredService<INamed<IFtpAdminService>>()
	.GetByName("Default");

```

## 額外提供 IProvider 特定服務的提供者 常用於Scope場合。
``` C#

	// 某服務的應用情境展示
	public class SomeService : ISomeService {

		readonly IProvider<IFtp> _ftpf;
		readonly IServiceScopeFactory _scopef;

		// 建構式有清楚的描述本服務 有依賴IFtp的關係
		public SomeService(
			IProvider<IFtp> ftpf,
			IServiceScopeFactory scopef)
		{
			_ftpf = ftpf;
			_scopef = scopef;
		}

		// 某些動作才須要實際取得IFtp服務 呼叫完成就釋放
		public void DoSomething() {
			using(var scope = _scopef.CreateScope()) {
				// 這時候才真的產生這個服務
				var ftp = _ftpf.Get(scope);

				ftp.DoSomething();
			}
		}
	}

```
