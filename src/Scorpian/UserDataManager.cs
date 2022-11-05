using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using static Scorpian.SDL.SDL;

namespace Scorpian;

public class UserDataManager
{
    private Dictionary<string, string> _data;
    private readonly string _appdata;

    private const string Filename = "user.properties";

    public UserDataManager(EngineSettings settings)
    {
        _data = new Dictionary<string, string>();
        _appdata = SDL_GetPrefPath("jorros", settings.Name);
    }

    internal void Load()
    {
        var propFile = Path.Combine(_appdata, Filename);

        if (!File.Exists(propFile))
        {
            return;
        }

        var content = File.ReadAllText(propFile);
        _data = JsonSerializer.Deserialize<Dictionary<string, string>>(content, new JsonSerializerOptions());
    }

    private void Save()
    {
        var propFile = Path.Combine(_appdata, Filename);
        var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions());

        File.WriteAllText(propFile, json);
    }

    public void Set<T>(string key, T value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value.ToString();
        }
        else
        {
            _data.Add(key, value.ToString());
        }

        Save();
    }

    public T Get<T>(string key, T @default = default)
    {
        if (!_data.ContainsKey(key))
        {
            Set(key, @default);
        }

        return (T) Convert.ChangeType(_data[key], typeof(T));
    }
}