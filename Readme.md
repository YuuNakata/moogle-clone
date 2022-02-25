## Moogle!

![](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

>Raydel E. Reuco García C113

En este proyecto he implementado algunos de los objetivos propuestos,agregando nuevas clases y elementos para poder llegar al resultado esperado. 

# Funcionalidades implementadas en el proyecto:

-Agregada una clase para guardar y ordenar la query junto con la dirección de los documentos sobre los cuales se realizará la busqueda(SearchQuery).
-Mediante otra clase FileContent se guarda el nombre del archivo con su contenido ya formateado junto a los operadores(!,^,~,*) y ejecutar sus propósitos sobre la clase de archivos antes mencionada,afectando si el documento tiene o no que estar , tanco como su score a tener en cuenta a la hora de ordenar.
-Con la implementacion de el algoritmo TF(Text Frecuency)-IDF(Inverse Document Frecuency) y una clase Vector se crea un array de esta clase que incluye una palabra con todos sus valores TF*IDF de cada documento,teniendo en cuenta con esto la implementación abstracta de una matriz tomando todas las palabras como las filas y todos los documentos como las columnas , luego se realiza la misma operacion para el query y se crean vectores de esta misma forma para cada una de sus palabras.
-Seguido de esto pasa a otro metodo el cual se encarga de tomando los vectores antes formados calcular la similitud entre ellos y el query mediante el cáñculo del coseno entre cada unos de esos vectores y el véctor query, para luego asignarle ese valor de similitud a cada uno de sus documentos y devolverlos.
-Despúes se guardan todos estos valores en un array , se ordena y luego se iguala con cada uno de los valores para devolverlos en otro array ya ordenados.
-También esta la sugerencia la cual si hay una palabra de query que no esta en el documento se usa el algoritmo de Levenshtein Distance para calcular las palabras mas próximas en los documentos y devolvérsela al usuario.
-Por último se analiza en los documentos devueltos la ubicacion de las palabras del query y se añaden al snippet dando una breve informacion del contenido de los mismos.
