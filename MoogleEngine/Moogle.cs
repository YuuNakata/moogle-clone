namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        SearchQuery search_query = new SearchQuery(GetFiles(),query);
        TestEmpty(search_query);
        if(query != "" && query != null && query != " " )
            return new SearchResult(SetItems(SortItems(search_query)),Split_Query(query).ToLower());
        return new SearchResult(new SearchItem[1]{new SearchItem("Por favor escriba algo","...",0.0f)},"");    
    }
    private static string[] GetFiles()
    {
        //Propiedad para obtener el nombre de los archivos
        string[] files=Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Content",".",SearchOption.AllDirectories);
        return files; 
    }
    public static SearchItem[] SetItems(string[] compared)
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
                items[i] = new SearchItem(compared[i],S_Reader(Directory.GetCurrentDirectory()+@"/Content/"+compared[i]).Substring(0,200),rand.NextSingle());
                empty=false;
            }
            
        }
        //Si no es vacio se regresan los items obtenidos
        if(!empty)
            return items;
        empty=true;       
        //De lo contrario se informa
        return new SearchItem[1]{new SearchItem("No se encontro ninguna coincidencia" , "" ,rand.NextSingle())};    
    }

    private static string[] SortItems(SearchQuery search_query)
    {
        //Variables necesarias
        string[] files_raw=search_query.FilesRaw;
        string[] files = search_query.Files;
        //Formateamos la query de operadores y signos de puntuacion
        string query = Split_Query(search_query.Query);
        System.Console.WriteLine(query);
        int c_count=0;

        //Leemos los ficheros y los guardamos en un array de FileContent
        FileContent[] file_content = new FileContent[files.Length];

        for (int i = 0; i < file_content.Length; i++)
        {
            file_content[i] = new FileContent(files[i],S_Reader(files_raw[i]));
        }
        file_content = S_Operator(file_content , search_query.Query);
        file_content = Clean_File(file_content);
        //Guardamos el score de cada coincidencia
        float[] f_count = new float[file_content.Length];
        for (int i = 0; i < f_count.Length; i++)
        {
            f_count[i]=S_Bunch(file_content[i].Content.Split(' '),query)+file_content[i].Initial_Score;
        }
        //Lo ordenamos
        Array.Sort(f_count);
        //Ahora lo hacemos coincidir con su respectivo archivo
        string[] compared = new string[file_content.Length];
        for (int i = 0; i < compared.Length; i++)
        {
            for (int j = 0; j < compared.Length; j++)
            {
                float score_base = f_count[i]+file_content[i].Initial_Score;
                float score_match = S_Bunch(file_content[j].Content.Split(" "),query)+file_content[j].Initial_Score;
                //System.Console.WriteLine("//"+score_base+"//"+score_match);
                if((score_match==score_base) && (!S_In(compared,file_content[j].FileName)) && score_match !=0)
                {
                    compared[c_count]=file_content[j].FileName;
                }
            }
            c_count++;
        }
        //Lo invertimos debido a que se ordeno de menor a mayor
        Array.Reverse(compared);

        //Regresamos el array ya ordenado con las coincidencias
        return compared;

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
                                if(temp_score <= 0f)
                                    file_content[i] = new FileContent("","");
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
                                        System.Console.WriteLine(temp_query[j].Split('*')[1]);
                                    }

                                }

                        }
                        System.Console.WriteLine(file_content[i].Initial_Score);
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
    private static float S_Distance(string[] s1, string s2)
    {
    float score = 0.0f;
    for (int i = 0; i < s1.Length; i++)
    {
        if(s1[i]==s2)
        {
            score += 0.1f;
        }
    }

    return score;
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
    public static float S_Contain(string[] s1 , string s2)
    {
        float lost = 0.0f;
        string[] s2_a = s2.Split(" ");

        for (int i = 0; i < s1.Length; i++)
        {
            for (int j = 0; j < s2_a.Length; j++)
            {
                if(s1[i]==s2_a[j])
                {
                    lost += 0.05f;
                }
            }
        }
        return lost;
    }
    public static float S_Bunch(string[] s1 , string s2)
    {
        return S_Distance(s1,s2) + S_Contain(s1,s2);
    }

    #endregion

    #region Tests
    public static bool TestEmpty(SearchQuery search_query)
    {
        string [] files = search_query.Files;
        if(files[0] == null || files[0] == "")
        {
            throw new Exception("El array de archivos esta vacio");
        }
        return true;
    }
    #endregion
}
