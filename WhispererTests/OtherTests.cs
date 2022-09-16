using Whisperer;

namespace WhispererTests;

public class OtherTests : IDisposable
{
    private const string DirPath = @"./OtherTests/temp";
    private Index<Artificial> _index = new Whisperer.Index<Artificial>(DirPath); 

    [Fact]
    public void Test_if_document_is_correctly_indexed()
    {
        var document = new Artificial()
        {
            Id = 1,
            Text = "prvni kousek"
        };

        _index.AddDocuments(new []{document}, d => d.Text);

        var res = _index.Search("pr");
        Assert.Collection(res, item => Assert.Equal(1, item.Id));
        
        res = _index.Search("ko");
        Assert.Collection(res, item => Assert.Equal(1, item.Id));
    }
    
    
    [Fact]
    public void Test_if_reused_index_contains_original_values()
    {
        var document = new Artificial()
        {
            Id = 1,
            Text = "prvni kousek"
        };

        _index.AddDocuments(new []{document}, d => d.Text);
        _index.Dispose();

        // try to read old index, add to id and search
        _index = new Whisperer.Index<Artificial>(DirPath);
        var document2 = new Artificial()
        {
            Id = 2,
            Text = "prvni druhy"
        };
        _index.AddDocuments(new []{document2}, d => d.Text);
        
        // check if original document is still there toget
        var res = _index.Search("ko");
        Assert.Collection(res, item => Assert.Equal(1, item.Id));
    }
    
    [Fact]
    public void Test_if_reused_index_contains_new_values()
    {
        var document = new Artificial()
        {
            Id = 1,
            Text = "prvni kousek"
        };

        _index.AddDocuments(new []{document}, d => d.Text);
        _index.Dispose();

        // try to read old index, add to id and search
        _index = new Whisperer.Index<Artificial>(DirPath);
        var document2 = new Artificial()
        {
            Id = 2,
            Text = "prvni druhy"
        };
        _index.AddDocuments(new []{document2}, d => d.Text);
        
        // check if original document is still there toget
        var res = _index.Search("dru");
        Assert.Collection(res, item => Assert.Equal(2, item.Id));
    }
    
    [Fact]
    public void Test_if_reused_index_contains_all_values()
    {
        var document = new Artificial()
        {
            Id = 1,
            Text = "prvni kousek"
        };

        _index.AddDocuments(new []{document}, d => d.Text);
        _index.Dispose();

        // try to read old index, add to id and search
        _index = new Whisperer.Index<Artificial>(DirPath);
        var document2 = new Artificial()
        {
            Id = 2,
            Text = "prvni druhy"
        };
        _index.AddDocuments(new []{document2}, d => d.Text);
        
        var res = _index.Search("pr"); 
        Assert.Collection(res, 
            item => Assert.Equal(1, item.Id),
            item => Assert.Equal(2, item.Id)
        );
        
    }
    
    [Fact]
    public void Test_if_synonyms_return_only_one_document()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Text = "slovo"
            },
            new()
            {
                Id = 1,
                Text = "synonymum"
            }
            
        };

        _index.AddDocuments(documents, d => d.Text);
        
        var res = _index.Search("s").ToArray();

        Assert.Single(res);
        Assert.Equal(1, res.First().Id);

    }

    [Fact]
    public void Test_sorting_by_text_length()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Text = "slovo nejspíš na třetím místě"
            },
            new()
            {
                Id = 2,
                Text = "slovo na druhém místě"
            },
            new()
            {
                Id = 3,
                Text = "slovo první"
            }
            
        };

        _index.AddDocuments(documents, d => d.Text);
        
        var res = _index.Search("s").ToArray();
        
        Assert.Collection(res, 
            item => Assert.Equal(3, item.Id),
            item => Assert.Equal(2, item.Id),
            item => Assert.Equal(1, item.Id)
        );

    }
    
    [Fact]
    public void Test_boost()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Text = "slovo nejspíš na třetím místě"
            },
            new()
            {
                Id = 2,
                Text = "slovo na druhém místě"
            },
            new()
            {
                Id = 3,
                Text = "slovo první"
            }
            
        };

        _index.AddDocuments(documents, d => d.Text, b => 4 - b.Id);
        
        var res = _index.Search("s").ToArray();
        
        Assert.Collection(res, 
            item => Assert.Equal(1, item.Id),
            item => Assert.Equal(2, item.Id),
            item => Assert.Equal(3, item.Id)
        );

    }
    
    [Theory]
    [InlineData("cty")]
    [InlineData("jed cty")]
    [InlineData("dva jed")]
    public void Test_search_for_nonexisting_text(string query)
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Text = "slovo jedna tri"
            },
            new()
            {
                Id = 2,
                Text = "slovo dva tri"
            },
            new()
            {
                Id = 3,
                Text = "slovo tri tri"
            }
            
        };
        _index.AddDocuments(documents, d => d.Text);
        
        var res = _index.Search(query).ToArray();
        
        Assert.Empty(res);
    }
    
    [Fact]
    public void Test_filter_no_filter_in_search()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Filter = "krabice",
                Text = "slovo nejspíš na třetím místě"
            },
            new()
            {
                Id = 2,
                Filter = "kruh",
                Text = "slovo na druhém místě"
            },
            new()
            {
                Id = 3,
                Filter = "trojuhelník",
                Text = "slovo první"
            }
        };

        _index.AddDocuments(documents, d => d.Text, filterSelector: artificial => artificial.Filter);
        
        var res = _index.Search("s").ToArray();
        
        Assert.Equal(3, res.Length);
    }
    
    [Fact]
    public void Test_filter_one_filter_in_search()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Filter = "krabice",
                Text = "slovo nejspíš na třetím místě"
            },
            new()
            {
                Id = 2,
                Filter = "kruh",
                Text = "slovo na druhém místě"
            },
            new()
            {
                Id = 3,
                Filter = "trojuhelník",
                Text = "slovo první"
            }
        };

        _index.AddDocuments(documents, d => d.Text, filterSelector: artificial => artificial.Filter);
        
        var res = _index.Search("s", filter: "kruh").ToArray();
        Assert.Single(res);
        Assert.Equal(2, res.First().Id);
    }
    
    [Fact]
    public void Test_filter_two_filters_in_search()
    {
        var documents = new List<Artificial>()
        {
            new()
            {
                Id = 1,
                Filter = "krabice",
                Text = "slovo nejspíš na třetím místě"
            },
            new()
            {
                Id = 2,
                Filter = "kruh",
                Text = "slovo na druhém místě"
            },
            new()
            {
                Id = 3,
                Filter = "Trojuhelnik",
                Text = "slovo první"
            }
        };

        _index.AddDocuments(documents, d => d.Text, filterSelector: artificial => artificial.Filter);
        
        var res = _index.Search("s", filter: "kruh trojuhelník").ToArray();
        Assert.Equal(2, res.Length);
        Assert.Contains(res, artificial => artificial.Id == 2);
        Assert.Contains(res, artificial => artificial.Id == 3);
    }




    public void Dispose()
    {
        _index.DeleteDocuments();
        _index.Dispose();
    }
}