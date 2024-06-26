using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Infrastructure;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples;

public class WholeSiteSpider(
    IOptions<SpiderOptions> options,
    DependenceServices services,
    ILogger<Spider> logger)
    : Spider(options, services, logger)
{
    public static async Task RunAsync()
    {
        var builder = Builder.CreateDefaultBuilder<WholeSiteSpider>(options =>
        {
            options.Depth = 1000;
        });
        builder.UseSerilog();
        await builder.Build().RunAsync();
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken)
    {
        AddDataFlow<MyDataParser>();
        AddDataFlow<ConsoleStorage>(); // 控制台打印采集结果
        await AddRequestsAsync("http://www.cnblogs.com/"); // 设置起始链接
    }

    protected override SpiderId GenerateSpiderId()
    {
        return new(ObjectId.CreateId().ToString(), "博客园全站采集");
    }

    class MyDataParser : DataParser
    {
        public override Task InitializeAsync()
        {
            AddRequiredValidator("cnblogs\\.com");
            AddFollowRequestQuerier(Selectors.XPath("."));
            return Task.CompletedTask;
        }

        protected override Task ParseAsync(DataFlowContext context)
        {
            context.AddData("URL", context.Request.RequestUri);
            context.AddData("Title", context.Selectable.XPath(".//title")?.Value);
            return Task.CompletedTask;
        }
    }
}
