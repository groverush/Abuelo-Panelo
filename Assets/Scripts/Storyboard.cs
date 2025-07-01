using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroPresentation : MonoBehaviour
{
    public Sprite[] slides;              // Arreglo de imágenes de la presentación
    public Image displayImage;           // Objeto UI que mostrará las imágenes
    public string nextSceneName;         // Nombre de la siguiente escena
    private int currentSlide = 0;

    void Start()
    {
        if (slides.Length > 0 && displayImage != null)
        {
            displayImage.sprite = slides[0];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Click izquierdo
        {
            NextSlide();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // Tecla ESC para saltar
        {
            SkipPresentation();
        }
    }

    void NextSlide()
    {
        currentSlide++;
        if (currentSlide < slides.Length)
        {
            displayImage.sprite = slides[currentSlide];
        }
        else
        {
            SkipPresentation();
        }
    }

    void SkipPresentation()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}