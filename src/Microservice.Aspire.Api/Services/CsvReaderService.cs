namespace Microservice.Aspire.Api.Services;

using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class CsvReaderService
{
    public IEnumerable<T> Read<T>(byte[] data)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var memory = new MemoryStream(data);
        using var reader = new StreamReader(memory);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }
}
