namespace MoogleEngine;

public class DocumentScore
{
    public DocumentScore(string filename , float score)
    {
        this.FileName=filename;
        this.Score=score;
    }
    public string FileName{get;private set;}
    public float Score{get;private set;}
}