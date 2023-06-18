using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json; 
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Runtime.InteropServices;

public class PlayerMovement: MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    [Header("Controllers")]
    public JSONParserV2 jsonParser;
    public ASController asController;
    public TMP_Text text, textImage;

    [Header("Camera Parameters")]
    public float playerHeight;
    public Transform orientation;
    public GameObject cameraObject;

    [Header("Reference Images")]
    public Image image;
    public Image image2;   
    
    [Header("UI Buttons")]
    public Button closeButton;
    public Button maximizeButton;
    public Button closeMaximizedButton;
    public Button minimizeButton;
    public TMP_Text tmp2;
    public TMP_Text tmp;

    [Header("UI GameObjects")]
    public GameObject ui_canvas;
    public GameObject imageHolder;
    public GameObject maximizedImageHolder;
    public GameObject buttonHolder;
    public GameObject buttonHolder2;
    public GameObject buttonHolder3;
    public GameObject buttonHolder4;
    public GameObject reloadHolder;
    public GameObject sliderHolder;
    public GameObject sliderHolder2;
    public GameObject tmpHolder;
    public GameObject positionIndicator;
    public GameObject viewPort;
    public GameObject scrollBar;
    public GameObject sphereParent;
    public HorizontalLayoutGroup parentObject;
    public GameObject infoHolder;
    public GameObject infoTitle;
    public Slider slider;
    public GameObject contentHolder;
    
    GraphicRaycaster ui_raycaster;
    PointerEventData click_data;
    List<RaycastResult> click_results;

    private bool maximized = false;    
    private bool inViewPort = false;
    private bool clickedWhileInViewPort = false;
    private bool firstTime = true;
    private bool isZeroToOne = false;
    private bool imageCreated = false;

    private float zoomRatio = 1.0f;
    private float inicialWidth = 959.0f;
    private float inicialHeight = 542.0f;

    private Vector3 initialPosition;
    private Vector3 initialScale;    
    private Vector3 targetPosition;
    private Vector3 layoutScale;

    private string actualSpace = "";

    private PlayerCamera playerCamera;

    private string s = File.ReadAllText("Assets/Resources/test.txt");

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private const int SWP_SHOWWINDOW = 0x0040;
    private const int HWND_TOP = 0;
    private const int HWND_TOPMOST = -1;

    private void Start()
    {

        getComponents();
        setupClickEnvironment();
        addButtonListeners();
        initialImageValues();

        //rb.freezeRotation = true;
        jsonParser.deleteFolders();
        jsonParser.readJSON();
        Cursor.visible=true;
        CreateImage();
        foreach (Transform child in sphereParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        tmpHolder.SetActive(false);
        

        tmp2.text="";
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
        SetWindowPos(GetActiveWindow(), HWND_TOP, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_SHOWWINDOW);
    
    }

 

    private void CreateImage() 
    {
        string[] directories = new string[] {"nave","cApse","nApse","sApse"};
        //abrir path en Resources/Nave2
        foreach(string directory in directories) {
            string path = "Assets/Resources/" + directory + "/";
            //para cada path de imagen dentro de la carpeta path
            foreach (string file in Directory.GetFiles(path))
            {
                //3 ultimos caracteres del path
                if (file.Substring(file.Length - 3) == "jpg")
                {
                    // Crear el objeto
                    GameObject newObject = new GameObject("NuevaImagen"+file);
                    //poner tag al objeto
                    newObject.tag = "NuevaImagen_"+directory;
                    // Agregar el componente Image al objeto
                    Image imageComponent = newObject.AddComponent<Image>();

                    // Establecer el padre del objeto recién creado
                    newObject.transform.SetParent(parentObject.transform, false);

                    // Asignar el sprite de la imagen al componente Image
                    newObject.GetComponent<Image>().sprite = CreateSprite(file);            

                    // Cambiar escala de la imagen
                    newObject.GetComponent<RectTransform>().localScale = layoutScale;

                    // Obtener el RectTransform del objeto hijo
                    RectTransform rectTransform = newObject.GetComponent<RectTransform>();

                    // Ajustar el tamaño de la imagen usando las dimensiones preferidas del RectTransform
                    float preferredWidth = rectTransform.rect.width;
                    float preferredHeight = rectTransform.rect.height;

                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

                
                }

            }
        }
        

    }

    private void CreateImageC(string location) 
    {
        //abrir path en Resources/Nave2
        string path = "Assets/Resources/" + location + "/";

        //para cada path de imagen dentro de la carpeta path
        foreach (string file in Directory.GetFiles(path))
        {
            //3 ultimos caracteres del path
            if (file.Substring(file.Length - 3) == "jpg")
            {
                // Crear el objeto
                GameObject newObject = new GameObject("NuevaImagen"+file);
                //poner tag al objeto
                newObject.tag = "NuevaImagen";
                // Agregar el componente Image al objeto
                Image imageComponent = newObject.AddComponent<Image>();

                // Establecer el padre del objeto recién creado
                newObject.transform.SetParent(parentObject.transform, false);

                // Asignar el sprite de la imagen al componente Image
                newObject.GetComponent<Image>().sprite = CreateSprite(file);            

                // Cambiar escala de la imagen
                newObject.GetComponent<RectTransform>().localScale = layoutScale;

                // Obtener el RectTransform del objeto hijo
                RectTransform rectTransform = newObject.GetComponent<RectTransform>();

                // Ajustar el tamaño de la imagen usando las dimensiones preferidas del RectTransform
                float preferredWidth = rectTransform.rect.width;
                float preferredHeight = rectTransform.rect.height;

                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

          
            }

        }
        imageCreated = true;

    }

    private void reloadJSON() {
        foreach (Transform child in parentObject.transform)
        {
            Destroy(child.gameObject);
        }
        //delete all object under sphereParent
        foreach (Transform child in sphereParent.transform)
        {
            Destroy(child.gameObject);
        }
        jsonParser.deleteFolders();
        jsonParser.readJSON();
        //delete all images under Content
        isZeroToOne = true;
        OnSliderValueChanged(0f);
        CreateImage();
        updateMenu(text.text);
        closeImage();
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

    //decodificar imagen en base64 y guardarlo en un nuevo sprite
    private Sprite decodeImage(string base64) 
    {
        byte[] imageBytes = Convert.FromBase64String(base64);

        // Crear una textura a partir de los bytes de la imagen
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageBytes);

        // Crear un sprite a partir de la textura
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        return sprite;
    }

    //por cada imagen en la carpeta Resources/Images, crear un objeto hijo de parentObject y asignarle el sprite de la imagen
    private void CreateImages() 
    {
        // Obtener todas las imágenes en la carpeta Resources/Nave
        Sprite[] sprites = Resources.LoadAll<Sprite>("Nave");

        // Crear un objeto hijo de parentObject por cada imagen en la carpeta
        foreach (Sprite sprite in sprites)
        {
            // Crear el objeto
            GameObject newObject = new GameObject("Image");

            // Agregar el componente Image al objeto
            Image imageComponent = newObject.AddComponent<Image>();

            // Establecer el padre del objeto recién creado
            newObject.transform.SetParent(parentObject.transform, false);

            // Asignar el sprite de la imagen al componente Image
            newObject.GetComponent<Image>().sprite = sprite;

            // Cambiar escala de la imagen
            newObject.GetComponent<RectTransform>().localScale = layoutScale;

            // Obtener el RectTransform del objeto hijo
            RectTransform rectTransform = newObject.GetComponent<RectTransform>();

            // Ajustar el tamaño de la imagen usando las dimensiones preferidas del RectTransform
            float preferredWidth = rectTransform.rect.width;
            float preferredHeight = rectTransform.rect.height;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        }

        GameObject newObject2 = new GameObject("Image");

        // Agregar el componente Image al objeto
        Image imageComponent2 = newObject2.AddComponent<Image>();

        // Establecer el padre del objeto recién creado
        newObject2.transform.SetParent(parentObject.transform, false);

        // Asignar el sprite de la imagen al componente Image
        newObject2.GetComponent<Image>().sprite = decodeImage(s);

        // Cambiar escala de la imagen
        newObject2.GetComponent<RectTransform>().localScale = layoutScale;

        // Obtener el RectTransform del objeto hijo
        RectTransform rectTransform2 = newObject2.GetComponent<RectTransform>();

        // Ajustar el tamaño de la imagen usando las dimensiones preferidas del RectTransform
        float preferredWidth2 = rectTransform2.rect.width;
        float preferredHeight2 = rectTransform2.rect.height;

        rectTransform2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth2);
        rectTransform2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight2);
        
    }


    private void getComponents() 
    {

        //rb = GetComponent<Rigidbody>();
        ui_raycaster = ui_canvas.GetComponent<GraphicRaycaster>();
        playerCamera = cameraObject.GetComponent<PlayerCamera>();

    }

    private void addButtonListeners() 
    {

        closeButton.onClick.AddListener(closeImage); 
        maximizeButton.onClick.AddListener(maximizeImage);
        closeMaximizedButton.onClick.AddListener(closeMaximizedImage); 
        minimizeButton.onClick.AddListener(minimizeImage);
        slider.onValueChanged.AddListener(OnSliderValueChanged);

    }

    private void initialImageValues() 
    {        

        initialPosition = maximizedImageHolder.transform.position;
        initialScale = maximizedImageHolder.transform.localScale;
        layoutScale = new Vector3(2.0f, 1.129882f, 1.129882f);

    }

    private void setupClickEnvironment() 
    {        

        click_data = new PointerEventData(EventSystem.current);
        click_results = new List<RaycastResult>();

    }

    private void OnSliderValueChanged(float value)
    {
        if (!isZeroToOne && value >= 1.0f)
        {
            // El slider ha pasado de 0 a 1
            isZeroToOne = true;
            // Activar objetos bajo SphereParent
            foreach (Transform child in sphereParent.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else if (isZeroToOne && value < 1.0f)
        {
            // El slider ha vuelto a ser menor que 1
            isZeroToOne = false;
            // Desactivar objetos bajo SphereParent
            foreach (Transform child in sphereParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {  

        drawPosition(); 

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {

            GetUiElementsClicked();

        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {

            if(!moveToImage()) moveToClick();

        } 

        if (maximized) 
        {

            maximizedImageNavigation();

        } 

        inViewPort = isInViewPort();
        checkIfclickedWhileInViewPort();
        isMouseOverSlider();
        //moveToClickInstant();
    
    }
    
    private void maximizedImageNavigation() 
    {
        //zoom in/out maximizedImageHolder when mouse wheel hitted in function of the mouse wheel direction, avoiding being less than the original size
        float zoomVal = Mouse.current.scroll.y.ReadValue();

        if (zoomVal != 0) 
        {    

            zoomImage(zoomVal);     

        }
        //move maximizedImageHolder when drag-and-drop with left click, whole scren is covered by the image not allowing to show whats behind it.
        if (Mouse.current.leftButton.isPressed) 
        {        

            moveImage();     
                    
        }

    }

    private void zoomImage(float zoomVal) 
    {
        if(zoomVal > 0) 
        {     

            maximizedImageHolder.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);                    
            zoomRatio += 0.1f;

        } 
        else if (zoomRatio >= 1.1f) 
        {

            maximizedImageHolder.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
            zoomRatio -= 0.1f;

        }

    }

    private void moveImage() 
    {
        float moveX = Mouse.current.delta.x.ReadValue();
        float moveY = Mouse.current.delta.y.ReadValue();

        float wDiff = (inicialWidth * (maximizedImageHolder.transform.localScale.x/2.07f)) - inicialWidth;
        float hDiff = (inicialHeight * (maximizedImageHolder.transform.localScale.y/2.07f)) - inicialHeight; 

        if (moveX != 0 || moveY != 0) 
        {

            Vector3 newPos = new Vector3(maximizedImageHolder.transform.position.x + moveX, maximizedImageHolder.transform.position.y + moveY, maximizedImageHolder.transform.position.z);

            if (newPos.x > (inicialWidth - wDiff) && newPos.x < (inicialWidth + wDiff)) 
            {

                maximizedImageHolder.transform.position = new Vector3(maximizedImageHolder.transform.position.x + moveX, maximizedImageHolder.transform.position.y, maximizedImageHolder.transform.position.z);

            }

            if (newPos.y > (inicialHeight - hDiff) && newPos.y < (inicialHeight + hDiff)) 
            {

                maximizedImageHolder.transform.position = new Vector3(maximizedImageHolder.transform.position.x, maximizedImageHolder.transform.position.y + moveY, maximizedImageHolder.transform.position.z);

            }

        }

    }
    //ver si el mouse esta sobre el slider
    private void isMouseOverSlider() 
    {

        if (EventSystem.current.IsPointerOverGameObject())
        {

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.name == "Slider")
                {

                    

                }

            }

        }

    }

    //lanza un rayo desde la camara hasta el punto donde se ha hecho click y si ha chocado con algun objeto de nombre "nave" se dirige a su posición XZ, sin variar la y, que la adaptara a la altura del terreno
    private void moveToClickInstant() 
    {
        //lanzar rayo al hacer click derecho
        click_data.position = Input.mousePosition;
        EventSystem.current.RaycastAll(click_data, click_results);

        foreach (RaycastResult result in click_results)
        {
            if (result.gameObject.name == "nave")
            {

                Vector3 newPos = new Vector3(result.gameObject.transform.position.x, transform.position.y, result.gameObject.transform.position.z);
                transform.position = newPos;

            }

        }

    }


    //check if mouse is over an object in canvas named viewPort or Scrollbar
    private bool isInViewPort()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.name == "Viewport" || result.gameObject.name == "Scrollbar" || result.gameObject.name == "Background" || result.gameObject.name == "SliderText" || result.gameObject.name == "Fill")
                {

                    return true;

                }

            }

        }
        return false;

    }

    private void checkIfclickedWhileInViewPort()
    {

        if(inViewPort) 
        {
            //if right mouse button is clicked
            if (Mouse.current.leftButton.wasPressedThisFrame) 
            {

                playerCamera.updateGirCam(false);
            
            }
        }
            //if right mouse button is released
        if (Mouse.current.leftButton.wasReleasedThisFrame && !maximized) 
        {

            playerCamera.updateGirCam(true);
        
        }


    }
    
    void GetUiElementsClicked()
    {
        /** Get all the UI elements clicked, using the current mouse position and raycasting. **/

        click_data.position = Mouse.current.position.ReadValue();
        click_results.Clear();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ui_raycaster.Raycast(click_data, click_results);

        foreach (RaycastResult result in click_results)
        {

            GameObject ui_element = result.gameObject;

            if (ui_element.name.Contains("Image") && !ui_element.name.Contains("resized") && !maximized)
            {

                string[] splitted = ui_element.name.Split('/');
                string name = (splitted[splitted.Length - 1]).Substring(0, (splitted[splitted.Length - 1]).Length - 4);
                image.sprite = jsonParser.decodeImageHighRes(name);
                image2.sprite = jsonParser.decodeImageHighRes(name);
                tmp.text = name;
                string info = jsonParser.getText(name);
                if(info.Length > 0) 
                {
                    infoTitle.SetActive(true);
                    infoHolder.SetActive(true);                    
                    tmp2.text = info;
                }
                else 
                {
                    infoTitle.SetActive(false);
                    infoHolder.SetActive(false);
                    tmp2.text = "";
                }
                openImage();

            }
            else if(ui_element.name.Contains("Reload")) {
                reloadJSON();
            }



        }
    }


    private void OnTriggerEnter(Collider collider)
    {

        asController.updateAS(collider.name);
        text.text = asController.getActualSpace();
        if(text.text != actualSpace) 
        {
            
            updateMenu(text.text);       

        }        
        actualSpace = text.text;
        firstTime = false;

    }

    private void updateMenu(string space) {
        foreach (Transform child in parentObject.transform) {
            if (!child.name.Contains(space)) { child.gameObject.SetActive(false); }
            else { child.gameObject.SetActive(true); }
        }
    }

    private void closeImage() 
    {

        imageHolder.SetActive(false);
        buttonHolder.SetActive(false);
        buttonHolder2.SetActive(false);        
        reloadHolder.SetActive(true);
        sliderHolder.SetActive(true);
        sliderHolder2.SetActive(true);
        tmpHolder.SetActive(false);
        infoTitle.SetActive(false);
        infoHolder.SetActive(false);
        maximized = false;

    }

    private void maximizeImage() 
    {

        closeImage();
        zoomRatio = 1.0f;
        maximizedImageHolder.transform.position = initialPosition;
        maximizedImageHolder.transform.localScale = initialScale;
        maximizedImageHolder.SetActive(true);        
        buttonHolder3.SetActive(true);
        buttonHolder4.SetActive(true);
        reloadHolder.SetActive(false);
        sliderHolder.SetActive(false);
        sliderHolder2.SetActive(false);
        tmpHolder.SetActive(false);
        playerCamera.updateGirCam(false);
        infoTitle.SetActive(false);
        infoHolder.SetActive(false);
        maximized = true;

    }

    private void minimizeImage() 
    {

        closeMaximizedImage();
        playerCamera.updateGirCam(true);
        reloadHolder.SetActive(true);
        sliderHolder.SetActive(true);
        sliderHolder2.SetActive(true);
        tmpHolder.SetActive(true);
        if(tmp2.text.Length > 0) 
        {
            infoTitle.SetActive(true);
            infoHolder.SetActive(true);
        }
        else 
        {
            infoTitle.SetActive(false);
            infoHolder.SetActive(false);
        }
        openImage(); 
        maximized = false;   

    }

    private void closeMaximizedImage() 
    {

        maximizedImageHolder.SetActive(false);
        playerCamera.updateGirCam(true);
        buttonHolder.SetActive(false);
        buttonHolder2.SetActive(false);
        buttonHolder3.SetActive(false);
        buttonHolder4.SetActive(false);
        tmpHolder.SetActive(false);
        maximized = false;

    }

    private void openImage() 
    {

        imageHolder.SetActive(true);
        buttonHolder.SetActive(true);
        buttonHolder2.SetActive(true);
        tmpHolder.SetActive(true);

    }

    private void drawPosition() 
    {

        if(!inViewPort) 
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            int a = 0;
            bool hitted = false;
            foreach (RaycastHit hit in hits) 
            {
                if (hit.collider.gameObject.name == "limiter") {
                    positionIndicator.SetActive(false);
                    break;
                }
                //print name of hit object

                if (!hitted && hit.normal.y > 0.90f && hit.point.y >= -1 && hit.point.y <= (3-1.6f)) 
                {
                    hitted = true;
                    targetPosition = hit.point;        
                    positionIndicator.transform.position = targetPosition;
                    targetPosition.y = 1.6f + hit.point.y;
                    positionIndicator.SetActive(true);
                    //break;

                }

                //positionIndicator.SetActive(false);

                //if(a > 2 || (hit.normal.y < 0.1)) break;

                ++a;
            }
        }
        else
        {

            positionIndicator.SetActive(false);

        }

    }

    private bool moveToImage() 
    {

        if(!inViewPort) 
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            int a = 0;
            foreach (RaycastHit hit in hits) 
            {
                //si he hiteado un objeto que se llama "nave"
                if (hit.collider.name.Contains("imagePosition_"))
                {
                    
                    Vector3 hitPos = hit.point;
                    hitPos.y  = hit.collider.gameObject.transform.position.y;
                    //lazar rayo desde el hit point hacia abajo
                    Ray ray2 = new Ray(hitPos, Vector3.down);
                    RaycastHit hit2;
                    //ver la distancia hasta el suelo desde el nuevo rayo
                    Physics.Raycast(ray2, out hit2);
                    //imprimir la distancia
                    float distancia = hit2.distance;

                    //diferencia respecto de 1.6f
                    distancia = distancia - 1.6f;


                    //ponerme en la ubicacion de la nave, pero que mi altura respecto del suelo en ese punto sea de 1.6f
                    Vector3 newPos = hitPos;
                    //sumar diferencia a la y
                    newPos.y = newPos.y - distancia;
                    transform.position = newPos;

                    foreach (Transform child in sphereParent.transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                    slider.value = 0.0f;
                    
                    GameObject[] imagePositions;

                    if(hit.collider.name.Contains("cApse")) {
                        updateMenu("cApse");
                        imagePositions = GameObject.FindGameObjectsWithTag("NuevaImagen_cApse");
                    } else if(hit.collider.name.Contains("nave")) {
                        updateMenu("nave");
                        imagePositions = GameObject.FindGameObjectsWithTag("NuevaImagen_nave");
                    } else {
                        updateMenu("sApse");
                        imagePositions = GameObject.FindGameObjectsWithTag("NuevaImagen_sApse");
                    }
                    // Obtener todos los GameObject que empiecen por "imagePosition_" en el nombre, no en el tag
                     
                
                    // ver el nombre real del objeto hiteado, quitandole el "imagePosition_"
                    string name = hit.collider.name.Replace("imagePosition_", "");
                    // ver si algun elemento en imagePositions contiene el nombre del objeto hiteado "name"
                    foreach (GameObject imagePosition in imagePositions)
                    {
                        if (imagePosition.name.Contains(name))
                        {                       
                            string[] splitted = name.Split('/');
                            string name2 = (splitted[1]);
                            image.sprite = jsonParser.decodeImageHighRes(name2);
                            image2.sprite = jsonParser.decodeImageHighRes(name2);
                            tmp.text = name2;
                            openImage();
                            break;
                        }
                    }    
                    return true;
                }
            }
        }
        else
        {

            positionIndicator.SetActive(false);

        }
        return false;

    }

    private void moveToClick() 
    {

        if(!seeName(targetPosition).Contains("imagePosition") && positionIndicator.activeSelf && !maximized) StartCoroutine(MoveToTarget(targetPosition));
        

                
    }

    //see name of an object given a vec3 position
    private string seeName(Vector3 position) 
    {

        Ray ray = new Ray(position, Vector3.down);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        return hit.collider.name;

    }


    private System.Collections.IEnumerator MoveToTarget(Vector3 targetPosition)
    {

        Vector3 startPosition = transform.position;

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < journeyLength / moveSpeed)
        {

            elapsedTime = Time.time - startTime;
            float fractionOfJourney = elapsedTime / (journeyLength / moveSpeed);

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            RaycastHit hit;

            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit)) 
            {

                if(hit.point.y >= -1)
                {
                    
                    currentPosition.y = 1.6f + hit.point.y;

                }
                else 
                {
                    
                    currentPosition.y = 1.6f;

                }

            }  
            
            transform.position = currentPosition;
            yield return null;

        }

        transform.position = targetPosition;

    }    
   
}