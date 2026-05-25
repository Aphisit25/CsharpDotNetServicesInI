//dotnet add package DotNetEnv
// ✅ 2. ตัวอย่างไฟล์ .env

// DB_HOST=localhost
// DB_PORT=1433
// USE_SSL=true

using System;

public class EnvService
{
    public EnvService(string fileName = ".env")
    {
        // โหลดไฟล์ .env
        DotNetEnv.Env.Load(fileName);

        // ✅ หรือ load path เอง (ชัวร์สุด)
        // DotNetEnv.Env.Load(Path.Combine(
        //     AppDomain.CurrentDomain.BaseDirectory, ".env"));

    }

    // ✅ string
    public string Get(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    // ✅ generic (int, bool, etc.)
    public T Get<T>(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (value == null)
            return default;

        return (T)Convert.ChangeType(value, typeof(T));
    }

  // ✅ ✅ เวอร์ชันปลอดภัย (กัน error)
    public T GetSafe<T>(string key, T defaultValue)
    {
        try
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(val))
                return defaultValue;
    
            return (T)Convert.ChangeType(val, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}

// วิธีเรียกใช้งาน
// var env = new EnvService();

// string host = env.Get("DB_HOST");
// int port = env.Get<int>("DB_PORT");
// bool ssl = env.Get<bool>("USE_SSL");

