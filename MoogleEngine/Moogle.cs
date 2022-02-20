using System;
using System.Text;
namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        SearchQuery search_query = new SearchQuery(GetFiles(),query);
        TestEmpty(search_query);
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
        string[] files=Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Content",".",SearchOption.AllDirectories);
        return files; 
    }
    public static SearchResult SetItems(string[] compared , FileContent[] file_content,string query)
    {
        System.Console.WriteLine(TestSuggestion(query,file_content));
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
                string snippet = S_Reader(Directory.GetCurrentDirectory()+@"/Content/"+compared[i]);
                items[i] = new SearchItem(compared[i],snippet.Substring(0,(int)((float)snippet.Length*0.3f)),rand.NextSingle());
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
        //int c_count=0;

        //Leemos los ficheros y los guardamos en un array de FileContent
        FileContent[] file_content = new FileContent[files.Length+1];

        for (int i = 0; i < file_content.Length-1; i++)
        {
            file_content[i] = new FileContent(files[i],S_Reader(files_raw[i]));
        }
        file_content[file_content.Length-1]= new FileContent("Query",query);
        file_content = S_Operator(file_content , search_query.Query);
        file_content = Clean_File(file_content);
        DocumentScore[] ds = Vectorizar(file_content);
        //Guardamos el score de cada coincidencia
        // float[] f_count = new float[file_content.Length];
        // for (int i = 0; i < f_count.Length; i++)
        // {
        //     f_count[i]=S_Bunch(file_content,file_content[i],query)+file_content[i].Initial_Score;
        // }
        // //Lo ordenamos
        // Array.Sort(f_count);
        // //Ahora lo hacemos coincidir con su respectivo archivo
        string[] compared = new string[ds.Length];
        // for (int i = 0; i < compared.Length; i++)
        // {
        //     for (int j = 0; j < compared.Length; j++)
        //     {
        //         float score_base = f_count[i]+file_content[i].Initial_Score;
        //         float score_match = S_Bunch(file_content,file_content[j],query)+file_content[j].Initial_Score;
        //         //System.Console.WriteLine("//"+score_base+"//"+score_match);
        //         if((score_match==score_base) && (!S_In(compared,file_content[j].FileName)) && score_match !=0)
        //         {
        //             compared[c_count]=file_content[j].FileName;
        //         }
        //     }
        //     c_count++;
        // }
        // //Lo invertimos debido a que se ordeno de menor a mayor
        // Array.Reverse(compared);

        float[] f_count = new float[ds.Length];
        for (int i = 0; i < ds.Length; i++)
        {
            if(ds[i]?.Score !=null){  
                f_count[i] = ds[i].Score;
                System.Console.WriteLine($"{ds[i].Score}-{ds[i].FileName}");
            }
        }

        Array.Sort(f_count);
        Array.Reverse(f_count);
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
        return (compared,file_content.Where((source, index) =>index != file_content.Length-1).ToArray());

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
        return file_content ;

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

    //Temporal Algorithim
//     private static float S_Distance(string[] s1, string s2)
//     {
//     float score = 0.0f;
//     for (int i = 0; i < s1.Length; i++)
//     {
//         if(s1[i]==s2)
//         {
//             score += 0.1f;
//         }
//     }

//     return score;
// } 

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
        return (float)Math.Log(doc.Length/1+is_in_doc);
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
    // public static float S_Contain(string[] s1 , string s2)
    // {
    //     float lost = 0.0f;
    //     string[] s2_a = s2.Split(" ");

    //     for (int i = 0; i < s1.Length; i++)
    //     {
    //         for (int j = 0; j < s2_a.Length; j++)
    //         {
    //             if(s1[i]==s2_a[j])
    //             {
    //                 lost += 0.05f;
    //             }
    //         }
    //     }
    //     return lost;
    // }
    public static DocumentScore[] Vectorizar(FileContent[] docs )
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
        DocumentScore[] ds = Scorize(vectores,docs);
        return ds;
    }
    public static DocumentScore[] Scorize(Vector[] vectores,FileContent[] files)
    {
        DocumentScore[] ds = new DocumentScore[vectores.Length-1];
        int count=-1;
        float num=0;
        float temp_vdoc=0;
        float temp_vquery=0;
        for (int i = 0; i < vectores[0].Values.Length-1; i++)
        {
            //Recorremos cada vector para compar su tf-idf con el ultimo de ellos que es nuestro query
            for (int j = 0; j < vectores.Length-1; j++)
            {
                float v_doc = vectores[j].Values[i];
                float v_query = vectores[j].Values[vectores[j].Values.Length-1];
                num+=v_doc*v_query;
                
            }
            for (int j = 0; j < vectores.Length-1; j++)
            {
                float v_doc = vectores[j].Values[i];
                float v_query = vectores[j].Values[vectores[j].Values.Length-1];
                temp_vdoc+=(float)Math.Sqrt(Math.Pow((double)v_doc,2));
                temp_vquery+=(float)Math.Sqrt(Math.Pow((double)v_query,2));

            }
            float result=num/temp_vdoc*temp_vquery;
            if(!Single.IsNaN(result) && result!=0f)
            {
                System.Console.WriteLine(result);
                ds[++count]=new DocumentScore(files[i].FileName.ToString(),result);
            }
            
        }
            

        return ds;
    }
    public static float S_Bunch(FileContent[] all_doc , FileContent doc, string query)
    {
        float result = 0f;
        string[] temp_words = doc.Content.Split(" ");
        string[] word_s = query.Split(" ");
        for (int i = 0; i < word_s.Length ; i++)
        {
            result+=TF(doc,word_s[i])*IDF(all_doc,word_s[i]);
        }
        return result;

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
    public static bool TestEmpty(SearchQuery search_query)
    {
        string [] files = search_query.Files;
        if(files[0] == null || files[0] == "")
        {
            throw new Exception("El array de archivos esta vacio");
        }
        return true;
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
                if(!b_query[i])
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
                                System.Console.WriteLine(query);
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
