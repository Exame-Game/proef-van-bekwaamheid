using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveSystem
{
    private const string KEY = "your-32-char-key-here!!!!!!!!!!"; // Must be exactly 32 chars
    private const string IV  = "your-16-char-iv!";               // Must be exactly 16 chars

    /// <summary>Returns true if a save file with the given name exists.</summary>
    public static bool Exists(string fileName)
    {
        return File.Exists(GetPath(fileName));
    }

    /// <summary>Serialize and encrypt <paramref name="data"/> to a JSON file.</summary>
    public static void Save<T>(T data, string fileName)
    {
        try
        {
            string json      = JsonConvert.SerializeObject(data);
            string encrypted = Encrypt(json);
            string path      = GetPath(fileName);
            File.WriteAllText(path: path, contents: encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    /// <summary>Decrypt and deserialize a JSON file into <typeparamref name="T"/>.</summary>
    public static T Load<T>(string fileName) where T : new()
    {
        string path = GetPath(fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] No save file found at {path}. Returning default.");
            return new T();
        }

        try
        {
            string encrypted = File.ReadAllText(path);
            string json      = Decrypt(encrypted);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
            return new T();
        }
    }


    /// <summary>Deletes the save file with the given name.</summary>
    public static void Delete(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path)) 
            File.Delete(path);
    }

    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".sav");
    }

    private static string Encrypt(string plainText)
    {
        using Aes aes              = CreateAes();
        using ICryptoTransform enc = aes.CreateEncryptor();
        byte[] inputBytes          = Encoding.UTF8.GetBytes(plainText);
        byte[] outputBytes         = enc.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Convert.ToBase64String(outputBytes);
    }

    private static string Decrypt(string cipherText)
    {
        using Aes aes              = CreateAes();
        using ICryptoTransform dec = aes.CreateDecryptor();
        byte[] inputBytes          = Convert.FromBase64String(cipherText);
        byte[] outputBytes         = dec.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Encoding.UTF8.GetString(outputBytes);
    }

    private static Aes CreateAes()
    {
        Aes aes     = Aes.Create();
        aes.Key     = DeriveBytes(KEY, 32); // AES-256: 32 bytes
        aes.IV      = DeriveBytes(IV,  16); // AES block size: always 16 bytes
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] DeriveBytes(string input, int length)
    {
        using MD5 md5       = MD5.Create();
        byte[] hash         = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        byte[] result       = new byte[length];
        Array.Copy(hash, result, Math.Min(hash.Length, length));
        return result;
    }
}