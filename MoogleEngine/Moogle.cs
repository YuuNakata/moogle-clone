using System.Security.Cryptography;
using System;
using System.Text;
namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        SearchQuery search_query = new SearchQuery(GetFiles(),query);
        if(query != "" && query != null && query != " " )
        {
            (string[] compared , FileContent[] file_content) = SortItems(search_query);
            
            return SetItems(compared,file_content,query);

        }
        return new SearchResult(new SearchItem[1]{new SearchItem("Por favor escriba algo","...",0.0f)},"");    
    }
    private static string[] GetFiles()
    {
        //Propiedad para obtener el nombre de los archivos
        string[] files=Directory.GetFiles("../Content",".",SearchOption.AllDirectories);
        return files; 
    }
    public static SearchResult SetItems(string[] compared , FileContent[] file_content,string query)
    {

        //Provicional debido a que el score en el SearchItem no sirve de mucho :)
        Random rand = new Random();
        //Las variables uzadas
        bool empty=true;
        int items_size=0;
        //Revisamos que no exista ninguna coincidencia vacia
        for (int i = 0; i < compared.Length; i++)
        {
            if(compared[i] !="" && compared[i] !=null)
            {
                items_size++;
                empty=false;
            }

        }
        //Declaramos el string con el tamaño correspondiente sin elementos vacios
        SearchItem[] items = new SearchItem [items_size];
        //Le asignamos cada SearchItem con sus respectivos argumentos
        for (int i = 0; i < items.Length; i++)
        {
            if(compared[i] !="" && compared[i] !=null)
            {
                string snippet = S_Reader("../Content/"+compared[i]);
                string[] s_query = query.Split(" ");
                string final_snippet="";
                for (int j = 0; j < s_query.Length; j++)
                {
                    if(snippet.Contains(s_query[j]))
                    {
                        int start_index = snippet.IndexOf(s_query[j]);
                        start_index -= (int)0.01f*snippet.Length;
                        int final_index = snippet.LastIndexOf(s_query[j]);
                        final_index += (int)0.01f*snippet.Length;
                        final_index-=start_index;

                        if(start_index<0)
                            start_index=0;
                        if(final_index>snippet.Length)
                            final_index=snippet.Length;    
                        final_snippet+=$"{snippet.Substring(start_index,final_index)} ";
                    }
                }
                items[i] = new SearchItem(compared[i],final_snippet,rand.NextSingle());
                empty=false;
            }
            
        }
        //Si no es vacio se regresan los items obtenidos
        if(!empty)
            return new SearchResult(items,TestSuggestion(query,file_content));
        empty=true;       
        //De lo contrario se informa
        return new SearchResult(new SearchItem[1]{new SearchItem("No se encontro ninguna coincidencia" , "" ,rand.NextSingle())},TestSuggestion(query,file_content));    
    }

    private static (string[] compared,FileContent[] file_content) SortItems(SearchQuery search_query)
    {
        //Variables necesarias
        string[] files_raw=search_query.FilesRaw;
        string[] files = search_query.Files;
        //Formateamos la query de operadores y signos de puntuacion
        string query = Split_Query(search_query.Query);

        //Leemos los ficheros y los guardamos en un array de FileContent
        FileContent[] file_content = new FileContent[files.Length];

        for (int i = 0; i < file_content.Length; i++)
        {
            file_content[i] = new FileContent(files[i],S_Reader(files_raw[i]));
        }
        file_content = S_Operator(file_content , search_query.Query);
        file_content = Clean_File(file_content);
        DocumentScore[] ds = Vectorizar(file_content,query);
        //Guardamos el score de cada coincidencia

        //Ahora lo hacemos coincidir con su respectivo archivo
        string[] compared = new string[ds.Length];

        float[] f_count = new float[ds.Length];
        for (int i = 0; i < ds.Length; i++)
        {
            if(ds[i]?.Score !=null){  
                f_count[i] = ds[i].Score;
            }
        }

        Array.Sort(f_count);
        int count=0;
        for (int i = 0; i < ds.Length; i++)
        {
            for (int j = 0; j < ds.Length; j++)
            {
                if(f_count[i]==ds[j]?.Score)
                {
                    compared[count++]=ds[j].FileName;
                }
            }
        }


        //Regresamos el array ya ordenado con las coincidencias
        return (compared,file_content);

    }
    
    public static string Split_Query(string s_query)
    {
        string[] temp_query =s_query.Split(" ");
        string query="";
        for (int j = 0; j < temp_query.Length; j++)
        {
            bool space=false;
            if(temp_query[j] != "" && temp_query[j] != " ")
            {
                for (int k = 0; k < temp_query[j].Length; k++)
                {
    
                if(temp_query[j][k] == '!' || temp_query[j][k] == '^' || temp_query[j][k] == '~' || temp_query[j][k] == ',' || temp_query[j][k] == '.' || temp_query[j][k] == '?' || temp_query[j][k] == '*')
                {
                    continue;
                }
                else
                {
                    query+=temp_query[j][k];
                    space=true;
                }
                }
                
            }
            if(space)
            {
                query+=" ";
            }    
        }
        return query;
    }
    public static FileContent[] S_Operator(FileContent[] file_content , string query)
    {
        // Operadores:
        // - ! No debe estar la paralabra en ningun documento
        // - ^ Tiene que aparecer la palabra
        // - ~ Esas palabras que esten entre el operador mientras mas cerca aparezcan mas alto es el score
        // - * Le da mas importancia a la palabra y es acumulativo

        int lenght=file_content.Length;

        for (int i = 0; i < file_content.Length; i++)
        {
            string[] temp_words =file_content[i].Content.Split(" ");
            string[] temp_query =query.Split(" ");

            for (int j = 0; j < temp_query.Length; j++)
            {
                if(temp_query[j] != "" && temp_query[j] != " ")
                    if(temp_query[j][0] == '!')
                    {
                        for (int k = 0; k < temp_words.Length; k++)
                        {
                            if(temp_words[k]==temp_query[j].Split('!')[1])
                            {
                                file_content[i]=new FileContent("","");
                            }
                        }
                    }
                    
                    else if(temp_query[j][0] == '^')
                    {
                        bool exist=false;
                        for (int k = 0; k < temp_words.Length; k++)
                        {
                            if(temp_words[k]==temp_query[j].Split('^')[1])
                            {
                                exist=true;
                            }
                        }
                        if(!exist)
                        {
                        file_content[i]=new FileContent("","");    
                        }
                    }
                      
                    else if(temp_query[j][0] == '~')
                    {
                    bool one=false;
                    bool two=false;
                        for (int l = 0; l < temp_words.Length; l++)
                        {
                            if(temp_words[l]==temp_query[j-1])
                                one=true;
                            if(temp_words[l]==temp_query[j+1])
                                two=true;
                        }
                            if(one&&two){
                                float temp_score=0.0f;
                                int count=0;
                                for (int k = 0; k < temp_words.Length; k++)
                                {
                                    //formula dividir entre la cantidad de letras
                                    if(temp_words[k]==temp_query[j-1])
                                    {
                                        count++;
                                    }
                                    if(temp_words[k]==temp_query[j+1])
                                        break;
                                    
                                }
                                temp_score=((float)count/(float)temp_words.Length)*10f;
                                file_content[i].Initial_Score=temp_score;
                            }    
                    }
                    else if(temp_query[j][0] == '*')
                    {
                        for (int k = 0; k < temp_query[j].Length; k++)
                        {
                            if(temp_query[j][k]=='*')
                                for (int l = 0; l < temp_words.Length; l++)
                                {
                                    if(temp_query[j].Split('*')[1]==temp_words[l])
                                    {
                                        file_content[i].Initial_Score+=0.2f;
                                    }

                                }

                        }
                    }
        
            }   
            
        }
        return file_content;

    }
    public static FileContent[] Clean_File(FileContent[] file_content)
    {
        int count=0;
        for (int i = 0; i < file_content.Length; i++)
        {
            if(file_content[i].FileName!="" && file_content[i].FileName != null)
            {
                count++;
            }           
        }
        FileContent[] file_content_new= new FileContent[count];
        count=0;
        for (int i = 0; i < file_content.Length; i++)
        {
            if(file_content[i].FileName!="" && file_content[i].FileName != null)
            {
                file_content_new[count++]=file_content[i];
            }
        }
        return file_content_new;
    }
    public static DocumentScore[] Clean_File(DocumentScore[] file_content)
    {
        int count=0;
        for (int i = 0; i < file_content.Length; i++)
        {
            if(file_content[i].Score!=0f && file_content[i].FileName != "")
            {
                count++;
            }           
        }
        DocumentScore[] file_content_new= new DocumentScore[count];
        count=0;
        for (int i = 0; i < file_content.Length; i++)
        {
            if(file_content[i].FileName!="" && file_content[i].FileName != null)
            {
                file_content_new[count++]=file_content[i];
            }
        }
        return file_content_new;
    }
    #region S-string
    public static string S_Reader(string file)
    {
        string content="";
        StreamReader reader = new StreamReader(file);
        
        while(!reader.EndOfStream)
        {
            content += "\n" + reader.ReadLine(); 
        }
        reader.Close();
        
        return content;
    }

    public static float TF(FileContent doc , string word)
    {
        float result=0.0f;
        string[] doc_words = doc.Content.Split(" ");
        for (int i = 0; i < doc_words.Length ; i++)
        {
            if(word==doc_words[i])
                result+=1;
        }
        if(result>0)
            result=1+(float)Math.Log(result);
        return result;

    }
    public static float IDF(FileContent[] doc , string word)
    {
        int is_in_doc=0;
        for (int i = 0; i < doc.Length; i++)
        {
            string[] temp_content=doc[i].Content.Split(" ");
            for (int j = 0; j < temp_content.Length ; j++)
            {
                if(temp_content[j] ==word)
                    is_in_doc++;
            }
        }
        return (float)Math.Log10((float)doc.Length/(float)1+(float)is_in_doc);
    }

    public static bool S_In(string[] s1 , string s2)
    {
        for (int i = 0; i < s1.Length; i++)
        {
            if(s1[i]==s2)
                return true;
        }
        return false;
    }

    public static DocumentScore[] Vectorizar(FileContent[] docs ,string query)
    {
        int word_length=0;
        for (int i = 0; i < docs.Length; i++)
        {
            word_length+=docs[i].Content.Split(' ').Length;
        }
        //Guardar las palabras en un array
        string[] words = new string[word_length];
        int count=0;
        for (int i = 0; i < docs.Length; i++)
        {
            string[] temp_content = docs[i].Content.Split(" ");
            for (int j = 0; j < temp_content.Length; j++)
            {
                if(temp_content[j]!="la" || temp_content[j]!="de" || temp_content[j]!="del")
                    words[count++]=temp_content[j].ToLower();
            }
        }
        //Indexado los TF-IDF como vectores --(implementacion en cambios de mejora)
        Vector[] vectores = new Vector[word_length];
        for (int i = 0; i < word_length; i++)
        {
            float[] temp_vector = new float[docs.Length];
            for (int j = 0; j < docs.Length; j++)
            {
                temp_vector[j]=TF(docs[j],words[i])*IDF(docs,words[i]);
            }
            vectores[i]=new Vector(temp_vector,words[i]);
        }
        string[] s_query =  query.Split(" ");
        Vector[] v_query = new Vector[s_query.Length];
        for (int i = 0; i < s_query.Length; i++)
        {
            float[] v_float=new float[docs.Length];
            for (int j = 0; j < docs.Length; j++)
            {
                v_float[j]=TF(docs[j],s_query[i])*IDF(docs,s_query[i]);
            }
            v_query[i]=new Vector(v_float,s_query[i]);
        }
        DocumentScore[] ds = Scorize(vectores,docs,v_query);
        return ds;
    }
    public static DocumentScore[] Scorize(Vector[] vectores,FileContent[] files , Vector[] query)
    {
        DocumentScore[] ds = new DocumentScore[vectores.Length];
        int count=-1;
        for (int i = 0; i < vectores[1].Values.Length; i++)
        {
            //Recorremos cada vector para compar su tf-idf con el ultimo de ellos que es nuestro query
            float temp_vdoc=0;
            float num=0;
            float temp_vquery=0;
            for (int j = 0; j < vectores.Length; j++)
            {
                float v_doc = vectores[j].Values[i];
                for (int k = 0; k < query.Length; k++)
                {
                    num+=v_doc*query[k].Values[i];
                }
                
            }
            for (int j = 0; j < vectores.Length; j++)
            {
                float v_doc = vectores[j].Values[i];
                for (int k = 0; k < query.Length; k++)
                {
                temp_vquery+=(float)Math.Sqrt(Math.Pow((double)query[k].Values[i],2));   
                }
                temp_vdoc+=(float)Math.Sqrt(Math.Pow((double)v_doc,2));

            }
            float result=(float)(num)/(float)(temp_vdoc*temp_vquery);

            if(!Single.IsNaN(result))
            {
            ds[++count]=new DocumentScore(files[i].FileName.ToString(),result);
            }
            
        }
        for (int i = 0; i < query.Length; i++)
        {
            System.Console.Write(query[i].Word+"-");
            for (int j = 0; j < query[i].Values.Length; j++)
            {
                System.Console.Write($"{query[i].Values[j]} |");
            }
        }
            

        return ds;
    }
    
    #endregion

    #region Tests
    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        // Step 1
        if (n == 0)
        {
            return m;
        }
        if (m == 0)
        {
            return n;
        }
        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }
        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }
        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }
        public static string TestSuggestion(string query,FileContent[] file_content)
        {
            string[] s_query=query.Split(" ");
            bool[] b_query = new bool[s_query.Length];
            string result="";
            for (int i = 0; i < s_query.Length; i++)
            {
                for (int j = 0; j < file_content.Length; j++)
                {
                    if(file_content[j].Content.Contains(" "+s_query[i]+" "))
                        b_query[i]=true;
                }
            }
            int global_cost = int.MaxValue;
            for (int i = 0; i < b_query.Length; i++)
            {
                if(!b_query[i] && s_query[i].Length!=0)
                {
                    string last_word=s_query[i];
                    for (int j = 0; j < file_content.Length; j++)
                    {
                       string[] temp_content = file_content[j].Content.Split(" ");
                       for (int k = 0; k < temp_content.Length; k++)
                       {
                           int temp_cost=LevenshteinDistance(s_query[i],temp_content[k]);
                           if(temp_cost<global_cost && temp_cost<=3)
                           {
                                global_cost=temp_cost;
                                query=query.Replace(last_word,temp_content[k]);
                                last_word=temp_content[k];
                           }
                       }
                    }
                    result=query;


                }

            }
            return query;
        }
    #endregion

}
