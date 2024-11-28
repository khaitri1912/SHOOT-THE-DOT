
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Database;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private Image _soundImage;

    [SerializeField]
    private Sprite _activeSoundSprite, _inactiveSoundSprite;

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;


    // Start is called before the first frame update
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;

        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND) : 1) == 1;
        Debug.Log(sound);
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;

        AudioManager.Instance.AddButtonSound();
    }

    public void ClickPlay()
    {
        SceneManager.LoadScene(Constants.DATA.GAMEPLAY_SCENE);
    }

    public void ClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


    public void ToggleSound()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND) : 1) == 1;
        sound = !sound;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;
        PlayerPrefs.SetInt(Constants.DATA.SETTINGS_SOUND, sound ? 1 : 0);
        AudioManager.Instance.ToggleSound();

    }

    public void SignOutUser()
    {
        auth.SignOut();
        Debug.Log("Đăng xuất thành công.");
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
