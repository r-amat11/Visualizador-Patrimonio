using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;

public class JSONParserV2 : MonoBehaviour
{
    public TextAsset textJSON;
    public GameObject sphereParent;
    public SphereRotation sr;
    private static int originalWidth = 1920;
    private static int  originalHeight = 1080;
    private static int  fixedWidth = 256;
    private static int  thumbnailWidth = fixedWidth;
    private static int  thumbnailHeight = (originalHeight * fixedWidth) / originalWidth;

    public class Data
    {
        public string nombre { get; set; }
        public string codedImage { get; set; }
        public string autor { get; set; }
        public string localizacion { get; set; }
        public Posicion posicion { get; set; }
        public string texto { get; set; }
    }

    public class Posicion
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Root
    {
        public List<Data> data { get; set; }
    }

    string destinationFilePath = "Assets/JSON_Data/data.json";

    // Start is called before the first frame update
    void Start()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("data");
        if (jsonAsset != null)
        {
            // Crear la ruta de acceso al archivo de destino
            
            //Crear carpeta JSON_Data
            string folderPath = "Assets/JSON_Data/";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log("Carpeta creada exitosamente.");
            }

            try
            {
                // Guardar el contenido del TextAsset en la carpeta de destino
                File.WriteAllText(destinationFilePath, jsonAsset.text);
                Debug.Log("Archivo JSON copiado exitosamente.");
            }
            catch (IOException e)
            {
                Debug.LogError("Error al copiar el archivo JSON: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo JSON desde la carpeta 'Resources'.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void readJSON()
    {
       
        //leer json de destinationFilePath
        string json = File.ReadAllText(destinationFilePath);
        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);
        List<Data> dataList = myDeserializedClass.data;
        foreach (Data data in dataList)
        {
            //Debug.Log(data.nombre);
            //Debug.Log(data.codedImage);

            //si no existe la carpeta, crearla
            CreateFolder(data.localizacion);
            //crear imagen en la carpeta decodificando la codedImage (base64 info)
            Sprite sprite = decodeImage(data.codedImage, data.localizacion);

            //crear imagen.jpg y añadirle el sprite
            string path = "Assets/Resources/" + data.localizacion + "/" + data.nombre + ".jpg";
            SaveSpriteAsJPG(path, sprite);  
            Debug.Log("Imagen " + data.nombre + "creada en: " + path);

            //si tiene el campo posicion, crear una esfera en esa posicion
            if (data.posicion != null)
            {
                SetSphereTexture(path, new Vector3((float)data.posicion.x, (float)data.posicion.y, (float)data.posicion.z), data.localizacion + "/" + data.nombre);
            }
            
        }

    }

    public void deleteFolders() {
        string[] vector = new string[] { "cApse", "nApse", "sApse", "nave"};
        foreach (string folder in vector)
        {
            string path = "Assets/Resources/" + folder;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Debug.Log("Carpeta " + folder + " eliminada.");
            }
        }


    }

   //crear esfera y meterle una imagen como textura, la funcion recibe la ubciacion (vec3) y la imagen como parametros
    public void SetSphereTexture(string texturePath, Vector3 spherePosition, string name)
    {
        // Crea una esfera
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Hace que la esfera sea hija de SHPHEREPARENT
        sphere.transform.parent = sphereParent.transform;

        sphere.transform.position = spherePosition;
        sphere.name = "imagePosition_" + name;

        //hacer que la escala sea 0.33
        sphere.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

        //Añadir el script sr a la esfera
        sphere.AddComponent<SphereRotation>();


        // Crea un nuevo material
        Material sphereMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Lit"));

        // Carga la imagen desde los archivos
        Texture2D texture = LoadTextureFromFile(texturePath);
        
        
        if (texture != null)
        {
            // Asigna la textura al material
            sphereMaterial.mainTexture = texture;
            Debug.Log("Textura cargada exitosamente.");
            //porque imprime esto y no me crea la esfera?

        }
        else
        {
            Debug.LogError("Error al cargar la textura.");
        }

        // Asigna el material a la esfera
        Renderer sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.sharedMaterial = sphereMaterial;
    }

    private Texture2D LoadTextureFromFile(string path)
    {
        // Carga la imagen desde la codedImage especificada
        Texture2D texture = new Texture2D(2, 2);
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        if (texture.LoadImage(fileData))
        {
            texture.Apply();
            return texture;
        }
        else
        {
            return null;
        }
    }

    public string getText(string name) {



        if (File.Exists(destinationFilePath))
        {
            Debug.Log("The file exists.");
        }
        else
        {
            Debug.Log("The file does not exist.");
        }
        
        string dataAsJson = File.ReadAllText(destinationFilePath);

        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(dataAsJson);
        List<Data> dataList = myDeserializedClass.data;
        //buscar en el json el base64 de la imagen
        string info = "";
        foreach (Data data in dataList)
        {
            if (data.nombre == name)
            {
                info = (data.texto != null) ? data.texto : "";
            }
        }
        return info;      
    }

    private void CreateFolder(string name)
    {
        string folderPath = "Assets/Resources/" + name;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Carpeta creada exitosamente.");
        }
        else
        {
            Debug.LogWarning("La carpeta ya existe.");
        }
    }

    private Sprite decodeImage(string base64, string where) 
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        // Crear una textura a partir de los bytes de la imagen
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageBytes);
        Texture2D thumbnail = ScaleTexture(texture, thumbnailWidth, thumbnailHeight);

        // Crear un sprite a partir de la textura
        Sprite sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), Vector2.one * 0.5f);

        return sprite;
    }

    //decode high resolution image given name, search base64 on json file
    public Sprite decodeImageHighRes(string name)
    { 

        if (File.Exists(destinationFilePath))
        {
            Debug.Log("The file exists.");
        }
        else
        {
            Debug.Log("The file does not exist.");
        }
        
        string dataAsJson = File.ReadAllText(destinationFilePath);

        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(dataAsJson);        
        List<Data> dataList = myDeserializedClass.data;
        //buscar en el json el base64 de la imagen
        string base64 = "";
        foreach (Data data in dataList)
        {
            if (data.nombre == name)
            {
                base64 = data.codedImage;
            }
        }

        byte[] imageBytes = Convert.FromBase64String(base64);
        print("LONGITUD= " + imageBytes.Length);
        return decodeImage2(base64, "decodeImageHighRes");
	}

    private Sprite decodeImage2(string base64, string where) 
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        // Crear una textura a partir de los bytes de la imagen
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageBytes);
        // Crear un sprite a partir de la textura
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        return sprite;
    }

    private void SaveSpriteAsJPG(string path, Sprite sprite)
    {
        // Crear una textura temporal a partir del sprite
        Texture2D texture = sprite.texture;
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        newTexture.SetPixels(texture.GetPixels());
        newTexture.Apply();        

        // Convertir la textura temporal a bytes
        byte[] bytes = newTexture.EncodeToJPG();

        // Guardar los bytes como archivo JPG
        File.WriteAllBytes(path, bytes);
    }

    private Texture2D ScaleTexture(Texture2D source, int width, int height)
    {
        Texture2D scaledTexture = new Texture2D(width, height);
        Color[] pixels = scaledTexture.GetPixels(0, 0, width, height);
        float incX = 1.0f / (float)width;
        float incY = 1.0f / (float)height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = x * incX;
                float v = y * incY;
                pixels[y * width + x] = source.GetPixelBilinear(u, v);
            }
        }

        scaledTexture.SetPixels(pixels);
        scaledTexture.Apply();

        return scaledTexture;
    }


    private Sprite CreateSprite(string imagePath)
    {
        // Cargar la imagen desde el directorio local
        byte[] imageData = System.IO.File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        // Crear un Sprite a partir de la textura
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
    
}
