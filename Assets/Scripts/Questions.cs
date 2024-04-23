using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine.Networking;

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

    
    //private string connectionString = "URI=file:" + Application.dataPath + "/Plugins/flags2.db";
    
    private string connectionString = "URI=file:" + Application.persistentDataPath + "/flags2.db";


    private string correctAnswer;

    void Start()
    {
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

            // Choose a random question
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM flags ORDER BY RANDOM() LIMIT 1";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string imageUrl = "https://flagcdn.com/h240/" + reader.GetString(1) + ".png"; // Get the image URL
                        correctAnswer = reader.GetString(0); // Select the correct answer from the database according to the country code
                        Debug.Log(reader.GetString(1));
                        Debug.Log(correctAnswer);

                        // Randomly select the correct answer and other options
                        int randomsayi = Random.Range(0, 4);

                        switch (randomsayi)
                        {
                            case 0:
                                chooseAText.text = reader.GetString(0);
                                chooseBText.text = GetRandomCountryName();
                                chooseCText.text = GetRandomCountryName();
                                chooseDText.text = GetRandomCountryName();
                                break;
                            case 1:
                                chooseAText.text = GetRandomCountryName();
                                chooseBText.text = reader.GetString(0);
                                chooseCText.text = GetRandomCountryName();
                                chooseDText.text = GetRandomCountryName();
                                break;
                            case 2:
                                chooseAText.text = GetRandomCountryName();
                                chooseBText.text = GetRandomCountryName();
                                chooseCText.text = reader.GetString(0);
                                chooseDText.text = GetRandomCountryName();
                                break;
                            case 3:
                                chooseAText.text = GetRandomCountryName();
                                chooseBText.text = GetRandomCountryName();
                                chooseCText.text = GetRandomCountryName();
                                chooseDText.text = reader.GetString(0);
                                break;
                        }

                        // Load the image from the URL
                        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
                        {
                            yield return webRequest.SendWebRequest();

                            if (webRequest.result == UnityWebRequest.Result.Success)
                            {
                                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                                questionImage.sprite = sprite;

                            }
                            else
                            {
                                Debug.LogError("Error downloading image: " + webRequest.error);
                            }
                        }
                    }
                }
            }
            dbConnection.Close();
        }
    }

    // Check the answer
    public void CheckAnswer()
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
