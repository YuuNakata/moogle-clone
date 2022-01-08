namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query)
    {
        SearchQuery search_query = new SearchQuery(GetFiles(),query);
        TestEmpty(search_query);
        if(query != "" && query != null && query != " " )
            return new SearchResult(SetItems(SortItems(search_query)),query.ToLower());
        return new SearchResult(new SearchItem[1]{new SearchItem("Por favor escriba algo","...",0.0f)},"");    
    }
    private static string[] GetFiles()
    {
        //Propiedad para obtener el nombre de los archivos
        string[] files=Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Files",".",SearchOption.AllDirectories);
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
                items[i] = new SearchItem(compared[i],S_Reader(Directory.GetCurrentDirectory()+@"/Files/"+compared[i]).Substring(0,200),rand.NextSingle());
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
        string query = search_query.Query;
        int c_count=0;

        //Leemos los ficheros y los guardamos en un array de FileContent
        FileContent[] file_content = new FileContent[files.Length];

        for (int i = 0; i < file_content.Length; i++)
        {
            file_content[i] = new FileContent(files[i],S_Reader(files_raw[i]));
        }
        file_content = S_Operator(file_content , query);
        file_content = Clean_File(file_content);
        //Guardamos el score de cada coincidencia
        float[] f_count = new float[file_content.Length];
        for (int i = 0; i < f_count.Length; i++)
        {
            f_count[i]=S_Bunch(file_content[i].Content.Split(' '),query);
        }
        //Lo ordenamos
        Array.Sort(f_count);
        //Ahora lo hacemos coincidir con su respectivo archivo
        string[] compared = new string[file_content.Length];
        for (int i = 0; i < compared.Length; i++)
        {
            for (int j = 0; j < compared.Length; j++)
            {
                if((S_Bunch(file_content[j].Content.Split(" "),query)==f_count[i] && !S_In(compared,file_content[j].FileName)) && S_Bunch(file_content[j].Content.Split(" "),query) !=0)
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
