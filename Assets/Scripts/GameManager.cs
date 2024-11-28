using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private TMP_Text _scoreText, _endScoreText, _bestScoreText;

    private int score;

    [SerializeField]
    private Animator _scoreAnimator;

    [SerializeField]
    private AnimationClip _scoreClip;

    [SerializeField]
    private Obstacle _targetPrefab;

    [SerializeField]
    private float _maxSpawnOffset;

    [SerializeField]
    private Vector3 _startTargetPos;

    [SerializeField]
    private GameObject _endPanel;

    [SerializeField]
    private Image _soundImage;

    [SerializeField]
    private Sprite _activeSoundSprite, _inactiveSoundSprite;

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        auth = FirebaseAuth.DefaultInstance;
        currentUser = auth.CurrentUser;


        _endPanel.SetActive(false);
        if(AudioManager.Instance !=  null)
        {
            AudioManager.Instance.AddButtonSound();
        }
        //AudioManager.Instance.AddButtonSound();
        score = 0;
        _scoreText.text = score.ToString();
        _scoreAnimator.Play(_scoreClip.name, -1, 0f);
        SpawnObstacle();
    }

    public void SaveScoreToFirebase(int newScore)
    {
        if (currentUser == null)
        {
            Debug.LogError("Không có người chơi đăng nhập.");
            return;
        }

        string userId = currentUser.UserId;

        // Lấy điểm số hiện tại từ Firebase
        dbReference.Child("highscores").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int currentHighScore = 0;

                if (snapshot.Exists)
                {
                    currentHighScore = int.Parse(snapshot.Child("score").Value.ToString());
                }

                // Cập nhật điểm số nếu cao hơn
                if (newScore > currentHighScore)
                {
                    UpdateHighScore(userId, newScore);
                }
            }
        });
    }

    // Hàm để cập nhật điểm số cao nhất cho người chơi
    private void UpdateHighScore(string userId, int newHighScore)
    {
        dbReference.Child("highscores").Child(userId).Child("score").SetValueAsync(newHighScore).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Đã cập nhật điểm cao nhất: " + newHighScore);
            }
            else
            {
                Debug.LogError("Lỗi khi cập nhật điểm cao nhất: " + task.Exception);
            }
        });
    }

    private void SpawnObstacle()
    {
        Obstacle temp = Instantiate(_targetPrefab);
        Vector3 tempPos = _startTargetPos;
        _startTargetPos.x = Random.Range(-_maxSpawnOffset, _maxSpawnOffset);
        temp.MoveToPos(tempPos);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.DATA.MAIN_MENU_SCENE);
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene(Constants.DATA.GAMEPLAY_SCENE);
    }

    public void ToggleSound()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND)
          : 1) == 1;
        sound = !sound;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;
        PlayerPrefs.SetInt(Constants.DATA.SETTINGS_SOUND, sound ? 1 : 0);
        AudioManager.Instance.ToggleSound();
    }

    public void UpdateScore()
    {
        score++;
        _scoreText.text = score.ToString();
        _scoreAnimator.Play(_scoreClip.name, -1, 0f);
        SpawnObstacle();
    }

    public void EndGame()
    {
        _scoreText.gameObject.SetActive(false);
        _endPanel.SetActive(true);
        _endScoreText.text = score.ToString();

        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND)
         : 1) == 1;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;

        int highScore = PlayerPrefs.HasKey(Constants.DATA.HIGH_SCORE) ? PlayerPrefs.GetInt(Constants.DATA.HIGH_SCORE) : 0;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(Constants.DATA.HIGH_SCORE, highScore);
            _bestScoreText.text = "NEW BEST";
        }
        else
        {
            _bestScoreText.text = "BEST " + highScore.ToString();
        }

        // Lưu điểm số lên Firebase
        SaveScoreToFirebase(score);
    }
}
