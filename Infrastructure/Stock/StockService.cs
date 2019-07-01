using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Bot.Domain.Stock;
using ApplicationCore.Interfaces;
using CsvHelper;

namespace Infrastructure.Stock
{
    public class StockService : IStockService
    {
        public static readonly string ClientName = nameof(StockService);
        private readonly IHttpClientFactory factory;
        private readonly StockConfiguration config;

        public StockService(IHttpClientFactory factory, StockConfiguration config)
        {
            this.factory = factory;
            this.config = config;
        }

        public async Task<CompanyStock> GetStock(string company, CancellationToken cancellationToken)
        {
            var client = this.factory.CreateClient(ClientName);
            var uri = $"{this.config.BaseUrl}/?s={company}&f=sd2t2ohlcv&h&e=csv";
            using(var result = await client.GetAsync(uri))
            {
                var csvStream = await result.Content.ReadAsStreamAsync();
                var reader = new StreamReader(csvStream);
                var csvReader = new CsvReader(reader);
                var records = csvReader.GetRecords<StockRecord>();
                var record = records.FirstOrDefault();
                return record == null ?
                    null :
                    new CompanyStock
                    {
                        Symbol = record.Symbol,
                        Date = DateTime.ParseExact(
                            $"{record.Date} {record.Time}",
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture),
                        Open = record.Open,
                        High = record.High,
                        Low = record.Low,
                        Close = record.Close,
                        Volume = record.Volume
                    };
            }
        }
    }

    class StockRecord
    {
        public string Symbol { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    public class StockConfiguration
    {
        public string BaseUrl { get; set; }
    }
}