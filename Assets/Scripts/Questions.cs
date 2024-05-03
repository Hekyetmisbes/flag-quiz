using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

public class Questions : MonoBehaviour
{
    [SerializeField]
    Image questionImage;
    [SerializeField]
    Image gameOverImage;

    [SerializeField]
    TextMeshProUGUI chooseAText;
    [SerializeField]
    TextMeshProUGUI chooseBText;
    [SerializeField]
    TextMeshProUGUI chooseCText;
    [SerializeField]
    TextMeshProUGUI chooseDText;
    [SerializeField]
    TextMeshProUGUI gameOverText;
    [SerializeField]
    TextMeshProUGUI scoreText;

    [SerializeField]
    Button chooseAButton;
    [SerializeField]
    Button chooseBButton;
    [SerializeField]
    Button chooseCButton;
    [SerializeField]
    Button chooseDButton;

    [SerializeField]
    AudioSource CorrectSound;

    [SerializeField]
    AudioSource WrongSound;

    int score = 0;

    //private string connectionString = "URI=file:" + Application.dataPath + "/StreamingAssets/" + "flags2.db";

    //private string connectionString = "URI=file:" + Application.streamingAssetsPath + "/" + "flags2.db";

    private string connectionString = "URI=file:" + "jar:file://" + Application.dataPath + "!/StreamingAssets/" + "flags2.db";

    /*private string databaseName = "flags2.db";
    private string connectionString;*/


    private string correctAnswer;

    void Start()
    {
        /*
        // Veritabanı dosyasını PersistentDataPath'e kopyala
        string filePath = Path.Combine(Application.persistentDataPath, databaseName);
        if (!File.Exists(filePath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, databaseName);
            File.Copy(sourcePath, filePath, true);
        }

        connectionString = "URI=file:" + filePath;*/
        StartCoroutine(LoadQuestion());
    }

    public IEnumerator LoadQuestion()
    {
        chooseAButton.interactable = true;
        chooseBButton.interactable = true;
        chooseCButton.interactable = true;
        chooseDButton.interactable = true;

        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                // Choose a random question
                string sqlQuery = "SELECT * FROM flags ORDER BY RANDOM() LIMIT 1";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string countryCode = reader.GetString(1);
                        correctAnswer = reader.GetString(0);

                        // Choose a random answer for the correct answer
                        int randomIndex = Random.Range(0, 4);
                        SetRandomAnswers(randomIndex, correctAnswer);

                        // Load the question image
                        string imageUrl = "https://flagcdn.com/h240/" + countryCode + ".png";
                        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl);
                        yield return webRequest.SendWebRequest();

                        // If the image is downloaded successfully, set the image to the questionImage.
                        if (webRequest.result == UnityWebRequest.Result.Success)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            questionImage.sprite = sprite;
                        }
                        else
                        {
                            Debug.LogError("Resim indirme hatası: " + webRequest.error);
                        }
                    }
                }
            }
            dbConnection.Close();
        }
    }

    // Set the random answers
    private void SetRandomAnswers(int randomIndex, string correctAnswer)
    {
        switch (randomIndex)
        {
            case 0:
                chooseAText.text = correctAnswer;
                chooseBText.text = GetRandomCountryName();
                chooseCText.text = GetRandomCountryName();
                chooseDText.text = GetRandomCountryName();
                break;
            case 1:
                chooseAText.text = GetRandomCountryName();
                chooseBText.text = correctAnswer;
                chooseCText.text = GetRandomCountryName();
                chooseDText.text = GetRandomCountryName();
                break;
            case 2:
                chooseAText.text = GetRandomCountryName();
                chooseBText.text = GetRandomCountryName();
                chooseCText.text = correctAnswer;
                chooseDText.text = GetRandomCountryName();
                break;
            case 3:
                chooseAText.text = GetRandomCountryName();
                chooseBText.text = GetRandomCountryName();
                chooseCText.text = GetRandomCountryName();
                chooseDText.text = correctAnswer;
                break;
        }
    }

    // Check the answer
    protected void CheckAnswer()
    {
        // If the selected answer is correct
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text == correctAnswer)
        {
            PlayCorrectSound();
            score++;
            scoreText.text = "Score: " + score;
            StartCoroutine(LoadQuestion());
        }
        // If the selected answer is wrong
        else
        {
            chooseAButton.interactable = false;
            chooseBButton.interactable = false;
            chooseCButton.interactable = false;
            chooseDButton.interactable = false;
            PlayWrongSound();
            if (score > PlayerPrefs.GetInt("HighScore"))
            {
                PlayerPrefs.SetInt("HighScore", score);
            }
            gameOverText.text = "Game Over\r\n Answer: " + correctAnswer + "\r\nScore : " + score + "\r\nHigh Score: " + PlayerPrefs.GetInt("HighScore");
            gameOverImage.gameObject.SetActive(true);
        }
    }

    // Get a random country name from the database for the wrong answers
    string GetRandomCountryName()
    {
        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT countryname FROM flags ORDER BY RANDOM() LIMIT 1";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetString(0) != correctAnswer)
                        {
                            return reader.GetString(0);
                        }
                        else
                        {
                            GetRandomCountryName();
                        }
                    }
                }
            }
            dbConnection.Close();
        }
        return null;
    }

    // Play the correct sound
    void PlayCorrectSound()
    {
        CorrectSound.Play();
    }

    // Play the wrong sound
    void PlayWrongSound()
    {
        WrongSound.Play();
    }
}
