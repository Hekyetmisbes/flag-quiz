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

    private IEnumerator LoadQuestion()
    {
        using (var dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Rastgele bir soru seçme
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM flags ORDER BY RANDOM() LIMIT 1"; // Rastgele bir soru se�me
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(reader.GetString(3)))
                        {
                            yield return www.SendWebRequest();

                            if (www.result == UnityWebRequest.Result.Success)
                            {
                                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                                soruResmi.sprite = sprite;

                                Debug.Log("Soru yüklendi!");
                            }
                            else
                            {
                                Debug.Log("Resim yükleme hatası: " + www.error);
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
