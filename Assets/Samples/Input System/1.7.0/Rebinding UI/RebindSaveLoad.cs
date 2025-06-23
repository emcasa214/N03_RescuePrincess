using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;

    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "rebinds.json");
    }

    private void OnEnable()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            actions.LoadBindingOverridesFromJson(json);
        }
    }

    private void OnDisable()
    {
        string json = actions.SaveBindingOverridesAsJson();
        File.WriteAllText(savePath, json);
    }
}
