namespace MoogleEngine;

public class Vector
{
    public Vector(float[] vector,string word)
    {
        this.Values=vector;
        this.Word=word;
    }
    public string Word{get;set;}
    public float[] Values{get;set;}
}