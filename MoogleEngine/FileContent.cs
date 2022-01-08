using MoogleEngine;

public class FileContent
{
    public FileContent(string file_name , string content)
    {
        this.FileName=file_name;
        this.Content=content;
    }
    public string FileName{get ; private set;}
    public string Content{get ; private set;}
}