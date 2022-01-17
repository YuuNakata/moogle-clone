using MoogleEngine;

public class FileContent
{
    private float init_score=0f;
    public FileContent(string file_name , string content , float ini_score = 0.0f)
    {
        this.FileName=file_name;
        this.Content=content;
        this.Initial_Score=ini_score;
        
    }
    public string FileName{get ; private set;}
    public string Content{get ; private set;}
    public float Initial_Score{get => init_score ; set{init_score=value;} }
}