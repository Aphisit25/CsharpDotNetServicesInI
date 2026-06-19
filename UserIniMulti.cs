
// วิธีเรียกใช้งาน
// private UserInI FtpIni;
// public frmFTP()
// {
//     InitializeComponent();
//     cboEndCoding.SelectedIndex = 0;

//     // โหลดค่า FTP จากไฟล์ INI
//     FtpIni = new UserInI("ftp_conf.ini");
//     LoadFtpConfig();
// }
// private void LoadFtpConfig()
// {
//     cboServer.Text = FtpIni.Get("FTPConfig", "Host");
//     txtLib.Text = FtpIni.Get("FTPConfig", "Lib");
//     txtUserName.Text = FtpIni.Get("FTPConfig", "Username");
// }



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartCSVExport.services
{
    public class UserInI
    {
        #region Win32 API
        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSectionNames", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSectionNames(
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSection", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(
            string lpAppName,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);
        #endregion

        #region Properties
        /// <summary>
        /// ชื่อไฟล์ INI ที่กำลังใช้งาน
        /// </summary>
        public string IniFileName { get; private set; }

        /// <summary>
        /// Path เต็มของไฟล์ INI
        /// </summary>
        public string IniFilePath { get; private set; }

        /// <summary>
        /// ค่าเริ่มต้นที่ใช้เมื่อไม่พบ Key
        /// </summary>
        public string DefaultValue { get; set; } = "";

        /// <summary>
        /// ขนาด Buffer สำหรับอ่านค่า (ค่าเริ่มต้น 32768)
        /// </summary>
        public int BufferSize { get; set; } = 32768;
        #endregion

        #region Constructors
        /// <summary>
        /// สร้าง Instance ใหม่ด้วยชื่อไฟล์ INI ที่กำหนด
        /// </summary>
        /// <param name="fileName">ชื่อไฟล์ INI (เช่น "config.ini")</param>
        public UserInI(string fileName = "config.ini")
        {
            Initialize(fileName);
        }

        /// <summary>
        /// สร้าง Instance ใหม่ด้วย path เต็มของไฟล์ INI
        /// </summary>
        /// <param name="filePath">Path เต็มของไฟล์ INI</param>
        /// <param name="isFullPath">กำหนดว่าเป็น Path เต็มหรือไม่</param>
        public UserInI(string filePath, bool isFullPath)
        {
            if (isFullPath)
            {
                IniFilePath = filePath;
                IniFileName = Path.GetFileName(filePath);
            }
            else
            {
                Initialize(filePath);
            }
        }

        private void Initialize(string fileName)
        {
            IniFileName = fileName;
            IniFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// อ่านค่า String จาก INI
        /// </summary>
        public string Get(string section, string key, string defaultValue = null)
        {
            try
            {
                if (!File.Exists(IniFilePath))
                {
                    return defaultValue ?? DefaultValue;
                }

                string defaultVal = defaultValue ?? DefaultValue;
                StringBuilder builder = new StringBuilder(BufferSize);

                int result = GetPrivateProfileString(section, key, defaultVal, builder, BufferSize, IniFilePath);

                if (result > 0)
                {
                    return builder.ToString();
                }

                return defaultVal;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading INI file: {IniFileName}\n{ex.Message}");
            }
        }

        /// <summary>
        /// เขียนค่า String ลงใน INI
        /// </summary>
        public bool Write(string section, string key, string value)
        {
            try
            {
                EnsureDirectoryExists();
                return WritePrivateProfileString(section, key, value, IniFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing to INI file: {IniFileName}\n{ex.Message}");
            }
        }

        /// <summary>
        /// อ่านค่าเป็น Integer
        /// </summary>
        public int GetInt(string section, string key, int defaultValue = 0)
        {
            string value = Get(section, key, defaultValue.ToString());
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// เขียนค่าเป็น Integer
        /// </summary>
        public bool WriteInt(string section, string key, int value)
        {
            return Write(section, key, value.ToString());
        }

        /// <summary>
        /// อ่านค่าเป็น Boolean
        /// </summary>
        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            string value = Get(section, key, defaultValue.ToString());
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            // รองรับค่า Y/N, Yes/No, True/False, 1/0
            string lowerValue = value.ToLower();
            if (lowerValue == "y" || lowerValue == "yes" || lowerValue == "true" || lowerValue == "1")
                return true;
            if (lowerValue == "n" || lowerValue == "no" || lowerValue == "false" || lowerValue == "0")
                return false;

            return defaultValue;
        }

        /// <summary>
        /// เขียนค่าเป็น Boolean
        /// </summary>
        public bool WriteBool(string section, string key, bool value)
        {
            return Write(section, key, value.ToString());
        }

        /// <summary>
        /// อ่านค่าเป็น DateTime
        /// </summary>
        public DateTime GetDateTime(string section, string key, DateTime defaultValue = default)
        {
            string value = Get(section, key);
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// อ่านค่าเป็น List ของ String
        /// </summary>
        public List<string> GetList(string section, string key, char separator = ',')
        {
            string value = Get(section, key);
            if (string.IsNullOrEmpty(value))
            {
                return new List<string>();
            }
            return value.Split(separator).Select(s => s.Trim()).ToList();
        }

        /// <summary>
        /// อ่านทุก Key ใน Section
        /// </summary>
        public Dictionary<string, string> GetSection(string section)
        {
            try
            {
                if (!File.Exists(IniFilePath))
                {
                    return new Dictionary<string, string>();
                }

                StringBuilder builder = new StringBuilder(BufferSize);
                int result = GetPrivateProfileSection(section, builder, BufferSize, IniFilePath);

                if (result == 0)
                {
                    return new Dictionary<string, string>();
                }

                Dictionary<string, string> dict = new Dictionary<string, string>();
                string[] entries = builder.ToString().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string entry in entries)
                {
                    int equalPos = entry.IndexOf('=');
                    if (equalPos > 0)
                    {
                        string key = entry.Substring(0, equalPos);
                        string value = entry.Substring(equalPos + 1);
                        dict[key] = value;
                    }
                }

                return dict;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading section from INI file: {IniFileName}\n{ex.Message}");
            }
        }

        /// <summary>
        /// อ่านทุก Section ในไฟล์ INI
        /// </summary>
        public List<string> GetSections()
        {
            try
            {
                if (!File.Exists(IniFilePath))
                {
                    return new List<string>();
                }

                StringBuilder builder = new StringBuilder(BufferSize);
                int result = GetPrivateProfileSectionNames(builder, BufferSize, IniFilePath);

                if (result == 0)
                {
                    return new List<string>();
                }

                return builder.ToString()
                    .Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading sections from INI file: {IniFileName}\n{ex.Message}");
            }
        }

        /// <summary>
        /// ลบ Key ที่ระบุ
        /// </summary>
        public bool DeleteKey(string section, string key)
        {
            return Write(section, key, null);
        }

        /// <summary>
        /// ลบ Section ที่ระบุ
        /// </summary>
        public bool DeleteSection(string section)
        {
            return Write(section, null, null);
        }

        /// <summary>
        /// ตรวจสอบว่า Key มีอยู่หรือไม่
        /// </summary>
        public bool KeyExists(string section, string key)
        {
            string value = Get(section, key, "___KEY_NOT_FOUND___");
            return value != "___KEY_NOT_FOUND___";
        }

        /// <summary>
        /// ตรวจสอบว่า Section มีอยู่หรือไม่
        /// </summary>
        public bool SectionExists(string section)
        {
            List<string> sections = GetSections();
            return sections.Contains(section);
        }

        /// <summary>
        /// คัดลอก INI ไปยังไฟล์ใหม่
        /// </summary>
        public bool CopyTo(string newFilePath, bool overwrite = false)
        {
            try
            {
                if (!File.Exists(IniFilePath))
                {
                    return false;
                }

                if (File.Exists(newFilePath) && !overwrite)
                {
                    return false;
                }

                EnsureDirectoryExists(Path.GetDirectoryName(newFilePath));
                File.Copy(IniFilePath, newFilePath, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// สร้าง INI ใหม่จาก Dictionary
        /// </summary>
        public bool CreateFromDictionary(Dictionary<string, Dictionary<string, string>> data)
        {
            try
            {
                EnsureDirectoryExists();

                foreach (var section in data)
                {
                    foreach (var key in section.Value)
                    {
                        WritePrivateProfileString(section.Key, key.Key, key.Value, IniFilePath);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// สร้าง Backup ของไฟล์ INI
        /// </summary>
        public bool Backup(string backupFolder = null)
        {
            try
            {
                if (!File.Exists(IniFilePath))
                {
                    return false;
                }

                string folder = backupFolder ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string backupFileName = $"{Path.GetFileNameWithoutExtension(IniFileName)}_{DateTime.Now:yyyyMMdd_HHmmss}.ini";
                string backupPath = Path.Combine(folder, backupFileName);

                File.Copy(IniFilePath, backupPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// โหลด INI จาก Resource
        /// </summary>
        public bool LoadFromResource(string resourceContent)
        {
            try
            {
                EnsureDirectoryExists();
                File.WriteAllText(IniFilePath, resourceContent);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Private Methods
        private void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(IniFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void EnsureDirectoryExists(string directory)
        {
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// สร้าง Instance ใหม่สำหรับไฟล์ INI ที่ระบุ
        /// </summary>
        public static UserInI Create(string fileName)
        {
            return new UserInI(fileName);
        }

        /// <summary>
        /// สร้าง Instance ใหม่สำหรับไฟล์ INI ที่ระบุ (Path เต็ม)
        /// </summary>
        public static UserInI CreateFromPath(string filePath)
        {
            return new UserInI(filePath, true);
        }

        /// <summary>
        /// อ่านไฟล์ INI โดยไม่ต้องสร้าง Instance (Quick Access)
        /// </summary>
        public static string QuickRead(string fileName, string section, string key, string defaultValue = "")
        {
            var ini = new UserInI(fileName);
            return ini.Get(section, key, defaultValue);
        }

        /// <summary>
        /// เขียนไฟล์ INI โดยไม่ต้องสร้าง Instance (Quick Access)
        /// </summary>
        public static bool QuickWrite(string fileName, string section, string key, string value)
        {
            var ini = new UserInI(fileName);
            return ini.Write(section, key, value);
        }
        #endregion
    }
}
