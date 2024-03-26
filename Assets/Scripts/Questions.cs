using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine.Networking;
using Unity.VisualScripting;
using System.Net;

public class Questions : MonoBehaviour
{
    [SerializeField]
    Image soruResmi;

    [SerializeField]
    TextMeshProUGUI secenekAText;
    [SerializeField]
    TextMeshProUGUI secenekBText;
    [SerializeField]
    TextMeshProUGUI secenekCText;
    [SerializeField]
    TextMeshProUGUI secenekDText;

    [SerializeField]
    Button secenekAButton;
    [SerializeField]
    Button secenekBButton;
    [SerializeField]
    Button secenekCButton;
    [SerializeField]
    Button secenekDButton;

    private string connectionString = "URI=file:" + Application.dataPath + "/Plugins/flags2.db";

    private string dogruCevap;

    void Start()
    {
        StartCoroutine(LoadQuestion());
    }

    public IEnumerator LoadQuestion()
    {
        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Rastgele bir soru seçme
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM flags ORDER BY RANDOM() LIMIT 1"; // Rastgele bir soru seçme
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("https://flagcdn.com/h240/bi.png"))
                        {
                            // Web isteği yap
                            yield return webRequest.SendWebRequest();

                            // Hata kontrolü
                            if (webRequest.result == UnityWebRequest.Result.Success)
                            {
                                // Texture'ı al ve RawImage'a at
                                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                                // RawImage'a sprite'ı at
                                soruResmi.sprite = sprite;

                            }
                            else
                            {
                                Debug.LogError("Error downloading image: " + webRequest.error);
                            }
                        }

                        Debug.Log("Soru Url: " + reader.GetString(3));
                        dogruCevap = reader.GetString(0);
                        secenekAText.text = "Amerika";
                        secenekBText.text = "Almanya";
                        secenekCText.text = "Fransa";
                        secenekDText.text = "İngiltere";


                    }
                }
            }

            dbConnection.Close();
        }
    }

    public void CheckAnswer(string secilenCevap)
    {
        if (secilenCevap == dogruCevap)
        {
            Debug.Log("Doğru cevap!");
        }
        else
        {
            Debug.Log("Yanlış cevap! Doğru cevap: " + dogruCevap);
        }
    }
}
