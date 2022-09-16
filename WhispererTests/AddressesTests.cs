using System.Diagnostics;
using System.Text.Json;
using Whisperer;

namespace WhispererTests;

public class AddressesTests : IClassFixture<IndexFixture>
{
    private IndexFixture _indexFixture;
    public AddressesTests(IndexFixture indexFixture)
    {
        _indexFixture = indexFixture;
    }
    
    [Theory]
    [InlineData("p")]
    [InlineData("běl par")]
    [InlineData("i p")]
    [InlineData("dlo PRA")]
    [InlineData("jar jar")]
    [InlineData("jar! jar")]
    [InlineData("jar. jar")]
    [InlineData("jar,jar")]
    [InlineData("jar;jar")]
    [InlineData("aš")]
    [InlineData("kar var")]
    public void Test_all_queries_find_results(string query)
    {
        var addresses = _indexFixture.Index.Search(query).ToArray();
        
        Assert.Equal(10, addresses.Length);
        
        Assert.All(addresses, item =>
        {
            var querySplit = query.Split(" .,!;".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            foreach (var subQuery in querySplit)
            {
                Assert.Contains(subQuery.RemoveAccents(), item.Adresa.RemoveAccents(), StringComparison.InvariantCultureIgnoreCase);
            }
        });
    }
    
    [Theory]
    [InlineData("p")]
    [InlineData("běl par")]
    [InlineData("i p")]
    [InlineData("dlo PRA")]
    [InlineData("jar jar")]
    [InlineData("jar! jar")]
    [InlineData("jar. jar")]
    [InlineData("jar,jar")]
    [InlineData("jar;jar")]
    [InlineData("aš")]
    [InlineData("kar var")]
    [InlineData("pra vi chor")]
    public void Test_Speed(string query)
    {
        _ = _indexFixture.Index.Search("a");  // warm up
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var addresses = _indexFixture.Index.Search(query).ToArray();
        sw.Stop();
        
        Assert.True(sw.ElapsedMilliseconds < 100, "Search took longer than 100 ms.");

    }
    
    [Theory]
    [InlineData("p")]
    [InlineData("běl par")]
    [InlineData("i p")]
    [InlineData("dlo PRA")]
    [InlineData("jar jar")]
    [InlineData("jar! jar")]
    [InlineData("jar. jar")]
    [InlineData("jar,jar")]
    [InlineData("jar;jar")]
    [InlineData("aš")]
    [InlineData("kar var")]
    [InlineData("pra vi chor")]
    public void Test_SpeedWithFilter_0(string query)
    {
        _ = _indexFixture.Index.Search("a");  // warm up
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var addresses = _indexFixture.Index.Search(query, filter: _indexFixture.FilterVals[0]).ToArray();
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 100, "Search took longer than 100 ms.");

    }
    
    [Theory]
    [InlineData("p")]
    [InlineData("běl par")]
    [InlineData("i p")]
    [InlineData("dlo PRA")]
    [InlineData("jar jar")]
    [InlineData("jar! jar")]
    [InlineData("jar. jar")]
    [InlineData("jar,jar")]
    [InlineData("jar;jar")]
    [InlineData("aš")]
    [InlineData("kar var")]
    [InlineData("pra vi chor")]
    public void Test_SpeedWithFilter_1(string query)
    {
        _ = _indexFixture.Index.Search("a");  // warm up
        Stopwatch sw = new Stopwatch();
        sw.Start();
        var addresses = _indexFixture.Index.Search(query, filter: _indexFixture.FilterVals[1]).ToArray();
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 100, "Search took longer than 100 ms.");
    }

    [Fact]
    public void Test_multi_thread_search()
    {
        string[] queries = new[]
        {
            "pardu", "prah", "hradec", "brno", "jar jar", "ostr", "kar var"
        };
        
        var loops = Enumerable.Range(1,100);

        Parallel.ForEach(loops, new ParallelOptions(){MaxDegreeOfParallelism = 10}, i =>
        {
            foreach (var query in queries)
            {
                var querySplit = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
                var addresses = _indexFixture.Index.Search(query).ToArray();
                Assert.NotEmpty(addresses);
                Assert.All(addresses, item =>
                {
                    foreach (var subQuery in querySplit)
                    {
                        Assert.Contains(subQuery.RemoveAccents(), item.Adresa.RemoveAccents(), StringComparison.InvariantCultureIgnoreCase);
                
                    }
                });
            }

        });
    }
    
    [Fact]
    public void Test_returns_only_one_address()
    {
        string query = "577 pol par";
        
        var result = _indexFixture.Index.Search(query);
        
        Assert.Single(result);
        
    }
}

public class IndexFixture : IDisposable
{
    public Index<Addr> Index { get; private set; }
    public string[] FilterVals { get; } = new[] { "sude", "liche" };

    public IndexFixture()
    {
        Index = new Whisperer.Index<Addr>(@"./addrTest/temp");
        
        var file = File.ReadAllBytes(@"./adresy.brotli");
        var decompressed = BrotliCompression.Decompress(file);
        var deserialized = JsonSerializer.Deserialize<IEnumerable<Addr>>(decompressed);
        Index.AddDocuments(deserialized, 
            addr => addr.Adresa,
            addr => addr.Adresa.Contains("praha", StringComparison.InvariantCultureIgnoreCase) ? 2f : 1f, //boost prahy
            addr => FilterVals[addr.Id % 2]);
    }

    public void Dispose()
    {
        Index.DeleteDocuments();
        Index.Dispose();
    }

    
}