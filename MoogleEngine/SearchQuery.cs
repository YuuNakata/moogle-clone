namespace MoogleEngine;

public class SearchQuery
{
    public SearchQuery(string[] files , string query)
    {
        this.FilesRaw=files;
        this.Query=query;

        this.Files= new string[files.Length];
        
        for (int i = 0; i < files.Length; i++)
        {
            this.Files[i]=Path.GetFileName(files[i]);
        }

    }
    public string Query{get ;private set;}
    public string[] FilesRaw{get ;private set;}
    public string[] Files{get ;private set;}
  }

