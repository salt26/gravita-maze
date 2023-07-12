using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using System;
using Newtonsoft.Json.Linq;

public class MetaUtil
{
    public static DirectoryInfo CreateDirectory(string path)
    {
        foreach (char c in Path.GetInvalidPathChars())
        {
            if (path.Contains(c))
            {
                Debug.LogError("File invalid: invalid directory name");
                return null;
            }
        }
        path = path.Replace("\\", "/");
        while (path.Contains("//"))
        {
            path = path.Replace("//", "/");
        }
        string[] dirs = path.Split('/');
        List<string> validDirs = new List<string>();
        foreach (string dir in dirs)
        {
            Debug.Log(dir);
            if (dir.Length == 0 || dir.Replace(".", "").Length != 0)
            {
                validDirs.Add(dir);
            }
            else
            {
                Debug.LogWarning("File warning: invalid directory name");
            }
        }
        path = string.Join("/", validDirs);
        Debug.Log(path);
        return Directory.CreateDirectory(path);
    }

    public static void CreateNewMetaFile(string metaPath, string mapHash, FileMode fileMode)
    {
        FileStream fs = null;
        StringWriter sw = null;
        try
        {
            fs = new FileStream(metaPath, fileMode, FileAccess.Write, FileShare.None);
            StringBuilder sb = new StringBuilder();
            sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();

                writer.WritePropertyName("mapHash");        // Hash code
                writer.WriteValue(mapHash);

                writer.WritePropertyName("timeLimitMode");
                writer.WriteStartObject();
                writer.WritePropertyName("tryCount");       // Try count for time limit mode
                writer.WriteValue(0);
                writer.WritePropertyName("hasClearedOnce"); // Has cleared in time limit mode?
                writer.WriteValue(false);
                writer.WriteEnd();

                writer.WritePropertyName("moveLimitMode");
                writer.WriteStartObject();
                writer.WritePropertyName("minMoveCount");   // Minimum known move count for move limit mode
                writer.WriteValue(int.MaxValue);
                writer.WritePropertyName("hasClearedOnce"); // Has cleared in move limit mode?
                writer.WriteValue(false);
                writer.WriteEnd();

                writer.WriteEndObject();

                using (StreamWriter streamWriter = new(fs, Encoding.UTF8))
                {
                    streamWriter.WriteLine(sb.ToString());
                }

                /* Output meta JSON example (actually not indented):
                 * {
                 *   "mapHash": "0d5b6b62ed79432c196462b962e02a3dc4de16496fbec63573fb0f010bef72c5",
                 *   "timeLimitMode": {
                 *     "tryCount": 0,
                 *     "hasClearedOnce": False,
                 *   },
                 *   "moveLimitMode": {
                 *     "minMoveCount": 2147483647,
                 *     "hasClearedOnce": False,
                 *   }
                 * }
                 */
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            try
            {
                GameManager.mm.hasClearedOnceInTime = false;
                GameManager.mm.tryCount = 0;
                GameManager.mm.hasClearedOnceInMove = false;
                GameManager.mm.MoveLimit = int.MaxValue;
                sw?.Close();
                fs?.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public static void ReinitializeMetaFile(string metaPath, string mapHash, MapManager.LimitModeEnum limitMode)
    {
        JObject json = ReadMetaFile(metaPath);
        if (json == null)
        {
            CreateNewMetaFile(metaPath, mapHash, FileMode.Create);
            return;
        }

        FileStream fs = null;
        StringBuilder sb = null;
        StringWriter sw = null;

        if (limitMode == MapManager.LimitModeEnum.Time) {
            JToken minMoveCount = GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "minMoveCount");
            JToken hasClearedOnceInMove = GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Move, "hasClearedOnce");
            if (minMoveCount == null || hasClearedOnceInMove == null)
            {
                CreateNewMetaFile(metaPath, mapHash, FileMode.Create);
            }
            else
            {
                try
                {
                    fs = new FileStream(metaPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    sb = new StringBuilder();
                    sw = new StringWriter(sb);

                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.None;
                        writer.WriteStartObject();

                        writer.WritePropertyName("mapHash");        // Hash code
                        writer.WriteValue(mapHash);

                        writer.WritePropertyName("timeLimitMode");
                        writer.WriteStartObject();
                        writer.WritePropertyName("tryCount");       // Try count for time limit mode
                        writer.WriteValue(0);
                        writer.WritePropertyName("hasClearedOnce"); // Has cleared in time limit mode?
                        writer.WriteValue(false);
                        writer.WriteEnd();

                        writer.WritePropertyName("moveLimitMode");
                        writer.WriteStartObject();
                        writer.WritePropertyName("minMoveCount");   // Minimum known move count for move limit mode
                        writer.WriteValue(minMoveCount);
                        writer.WritePropertyName("hasClearedOnce"); // Has cleared in move limit mode?
                        writer.WriteValue(hasClearedOnceInMove);
                        writer.WriteEnd();

                        writer.WriteEndObject();

                        using (StreamWriter streamWriter = new(fs, Encoding.UTF8))
                        {
                            streamWriter.WriteLine(sb.ToString());
                        }

                        /* Output meta JSON example (actually not indented):
                         * {
                         *   "mapHash": "0d5b6b62ed79432c196462b962e02a3dc4de16496fbec63573fb0f010bef72c5",
                         *   "timeLimitMode": {
                         *     "tryCount": 0,
                         *     "hasClearedOnce": False,
                         *   },
                         *   "moveLimitMode": {
                         *     "minMoveCount": 17,
                         *     "hasClearedOnce": True,
                         *   }
                         * }
                         */
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    try
                    {
                        GameManager.mm.hasClearedOnceInTime = false;
                        GameManager.mm.tryCount = 0;
                        sw?.Close();
                        fs?.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
        else // limitMode == MapManager.LimitModeEnum.Move
        {
            JToken tryCount = GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "tryCount");
            JToken hasClearedOnceInTime = GetValueFromMetaObject(json, mapHash, MapManager.LimitModeEnum.Time, "hasClearedOnce");
            if (tryCount == null || hasClearedOnceInTime == null)
            {
                CreateNewMetaFile(metaPath, mapHash, FileMode.Create);
            }
            else
            {
                try
                {
                    fs = new FileStream(metaPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    sb = new StringBuilder();
                    sw = new StringWriter(sb);

                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.None;
                        writer.WriteStartObject();

                        writer.WritePropertyName("mapHash");        // Hash code
                        writer.WriteValue(mapHash);

                        writer.WritePropertyName("timeLimitMode");
                        writer.WriteStartObject();
                        writer.WritePropertyName("tryCount");       // Try count for time limit mode
                        writer.WriteValue(tryCount);
                        writer.WritePropertyName("hasClearedOnce"); // Has cleared in time limit mode?
                        writer.WriteValue(hasClearedOnceInTime);
                        writer.WriteEnd();

                        writer.WritePropertyName("moveLimitMode");
                        writer.WriteStartObject();
                        writer.WritePropertyName("minMoveCount");   // Minimum known move count for move limit mode
                        writer.WriteValue(int.MaxValue);
                        writer.WritePropertyName("hasClearedOnce"); // Has cleared in move limit mode?
                        writer.WriteValue(false);
                        writer.WriteEnd();

                        writer.WriteEndObject();

                        using (StreamWriter streamWriter = new(fs, Encoding.UTF8))
                        {
                            streamWriter.WriteLine(sb.ToString());
                        }

                        /* Output meta JSON example (actually not indented):
                         * {
                         *   "mapHash": "0d5b6b62ed79432c196462b962e02a3dc4de16496fbec63573fb0f010bef72c5",
                         *   "timeLimitMode": {
                         *     "tryCount": 6,
                         *     "hasClearedOnce": True,
                         *   },
                         *   "moveLimitMode": {
                         *     "minMoveCount": 2147483647,
                         *     "hasClearedOnce": False,
                         *   }
                         * }
                         */
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    try
                    {
                        GameManager.mm.hasClearedOnceInMove = false;
                        GameManager.mm.MoveLimit = int.MaxValue;
                        sw?.Close();
                        fs?.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
        
        
    }

    public static JObject ReadMetaFile(string metaPath)
    {
        string json = File.ReadAllText(metaPath);
        UnityEngine.Debug.Log(metaPath);
        UnityEngine.Debug.Log(json);
        JObject jsonObj = JObject.Parse(json);
        if (jsonObj == null)
        {
            Debug.LogWarning("File warning: meta file cannot be parsed into json");
        }
        return jsonObj;
    }

    public static void ModifyMetaFile(string metaPath, string mapHash, MapManager.LimitModeEnum limitMode, Dictionary<string, object> keyValuePairs)
    {
        string json = File.ReadAllText(metaPath);
        UnityEngine.Debug.Log(metaPath);
        UnityEngine.Debug.Log(json);
        JObject jsonObj = JObject.Parse(json);
        if (jsonObj == null)
        {
            Debug.LogWarning("File warning: meta file cannot be parsed into json");
            return;
        }
        else if (jsonObj.GetValue("mapHash") == null || !((string)jsonObj.GetValue("mapHash")).Equals(mapHash))
        {
            Debug.LogWarning("File warning: map hash of the meta file is invalid");
            return;
        }

        string modeKey;
        if (limitMode == MapManager.LimitModeEnum.Time) modeKey = "timeLimitMode";
        else modeKey = "moveLimitMode";

        if (jsonObj.GetValue(modeKey) == null || jsonObj.GetValue(modeKey) is not JObject)
        {
            Debug.LogWarning("File warning: cannot find mode key from the meta file");
            return;
        }

        foreach (KeyValuePair<string, object> pair in keyValuePairs)
        {
            if (((JObject)jsonObj[modeKey]).GetValue(pair.Key) != null)
            {
                jsonObj[modeKey][pair.Key] = JToken.FromObject(pair.Value);
            }
            else
            {
                Debug.LogWarning("File warning: meta file doesn't have one of those keys");
            }
        }
        string output = JsonConvert.SerializeObject(jsonObj, Formatting.None);

        try
        {
            FileStream fs = new FileStream(metaPath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None);
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine(output);
            }
            fs?.Close();
            fs = null;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static JToken GetValueFromMetaObject(JObject metaObject, string mapHash, MapManager.LimitModeEnum limitMode, string key)
    {
        if (metaObject == null)
        {
            Debug.LogWarning("File warning: meta file cannot be parsed into json");
            return null;
        }
        else if (metaObject.GetValue("mapHash") == null || !((string)metaObject.GetValue("mapHash")).Equals(mapHash))
        {
            Debug.LogWarning("File warning: map hash of the meta file is invalid");
            return null;
        }

        string modeKey;
        if (limitMode == MapManager.LimitModeEnum.Time) modeKey = "timeLimitMode";
        else modeKey = "moveLimitMode";

        if (metaObject.GetValue(modeKey) == null || !(metaObject.GetValue(modeKey) is JObject))
        {
            Debug.LogWarning("File warning: cannot find mode key from the meta file");
            return null;
        }

        return ((JObject)metaObject[modeKey]).GetValue(key);
    }

    public static string GetMapHashFromMetaObject(JObject metaObject)
    {
        if (metaObject == null)
        {
            Debug.LogWarning("File warning: meta file cannot be parsed into json");
            return null;
        }
        if (metaObject.GetValue("mapHash") == null)
        {
            return null;
        }
        return (string)metaObject.GetValue("mapHash");
    }

    public static string ExtentionTxtToJson(string txtPath)
    {
        string newPath = txtPath.TrimStart('/');
        int i = newPath.LastIndexOf('.');
        if (i == -1)
        {
            Debug.LogError("File error: " + txtPath + " is not a file");
            return txtPath;
        }
        newPath = newPath.Substring(0, i) + ".json";
        Debug.Log(newPath);
        return newPath;
    }
}
