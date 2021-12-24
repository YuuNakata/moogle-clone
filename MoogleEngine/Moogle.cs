namespace MoogleEngine;

public static class Moogle
{
    //Campos usados
    public static string[] files = GetFiles(true);
    public static string[] files_raw = GetFiles();
    private static string g_query = "";
    public static SearchResult Query(string query)
    {
        g_query = query;
        TestEmpty();
        return new SearchResult(SetItems(SortItems()),query);
    }
    private static string[] GetFiles(bool name_only = false)
    {
        //Propiedad para obtener el nombre de los archivos
        string[] files=Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Files",".",SearchOption.AllDirectories);
        if(name_only)
        {
            for (int i = 0; i < files.Length; i++)
            {
                files[i]=Path.GetFileName(files[i]);
            }
            return files;
        }
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

    private static string[] SortItems()
    {
        string[] compared = new string[files.Length];
        float[] f_count = new float[files.Length];
        int c_count=0;
        //Guardamos el score de cada coincidencia
        for (int i = 0; i < files.Length; i++)
        {
            f_count[i]=S_Bunch(S_Reader(files_raw[i]).Split(" "),g_query);
        }
        //Lo ordenamos
        Array.Sort(f_count);
        //Ahora lo hacemos coincidir con su respectivo archivo
        for (int i = 0; i < compared.Length; i++)
        {
            for (int j = 0; j < compared.Length; j++)
            {
                if((S_Bunch(S_Reader(files_raw[j]).Split(" "),g_query)==f_count[i] && !S_In(compared,files[j])) && S_Bunch(S_Reader(files_raw[j]).Split(" "),g_query) !=0)
                {
                    compared[c_count]=files[j];
                }
            }
            c_count++;
        }
        //Lo invertimos debido a que se ordeno de menor a mayor
        Array.Reverse(compared);

        //Regresamos el array ya ordenado con las coincidencias
        return compared;

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
    public static bool TestEmpty()
    {
        if(files[0] == null || files[0] == "")
        {
            throw new Exception("El array de archivos esta vacio");
        }
        return true;
    }
    #endregion
}
