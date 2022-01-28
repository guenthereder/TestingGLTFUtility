using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Siccity.GLTFUtility;

public class ModelLoader : MonoBehaviour
{
    GameObject wrapper;
    string filePath;

    private void Start()
    {
        filePath = $"{Application.persistentDataPath}/Files/";
        wrapper = new GameObject
        {
            name = "Model"
        };
    }
    
    public void DownloadFile(string url)
    {
        string path = GetFilePath(url);
        if (File.Exists(path))
        {
            Debug.Log("Found file locally, loading...");
            LoadModel(path);
            return;
        }

        StartCoroutine(GetFileRequest(url, (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                // Log any errors that may happen
                Debug.Log($"{req.error} : {req.downloadHandler.text}");
            } else
            {
                // Save the model into a new wrapper
                LoadModel(path);
            }
        }));
    }

    string GetFilePath(string url)
    {
        string[] pieces = url.Split('/');
        string filename = pieces[pieces.Length - 1];

        return $"{filePath}{filename}";
    }

    public void LoadModel(string path)
    {
        ResetWrapper();

        AnimationClip[] animClips;
        GameObject result = Importer.LoadFromFile(path, new ImportSettings(), out animClips);
        Animation anim = result.AddComponent<Animation>();
        foreach (AnimationClip item in animClips)
        {
            item.legacy = true;
            item.wrapMode = WrapMode.Loop;
            anim.AddClip(item, item.name);
        }
        anim.Play(animClips[0].name);

        result.transform.SetParent(wrapper.transform);
    }

    IEnumerator GetFileRequest(string url, Action<UnityWebRequest> callback)
    {
        using(UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerFile(GetFilePath(url));
            yield return req.SendWebRequest();
            callback(req);
        }
    }

    void ResetWrapper()
    {
        if (wrapper != null)
        {
            foreach(Transform trans in wrapper.transform)
            {
                Destroy(trans.gameObject);
            }
        }
    }
}