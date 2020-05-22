using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PdfView.LIC
{
     class LicenseData
    {
        public DateTime InitDate { get;  set; }


        public DateTime LastRun { get;  set; }

        public DateTime EndDate { get;  set; }
    }

    class clsLicense
    {
        private static string _sLicPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\pdfstamper\";
        private static string _sLicFile = _sLicPath + @"\lfile.sys";

        public static bool IsLicenseFileExist()
        {
            return System.IO.File.Exists(_sLicFile);

        }

        public static int LicenseStatus()
        {
            int sLic = -1;
            try
            {
                if(IsLicenseFileExist())
                {
                    var licdata = GetLicenseData();
                    if(licdata.LastRun > System.DateTime.Today)
                    {
                        sLic = -3;
                    }
                    else if(licdata.EndDate>= System.DateTime.Today)
                    {
                        sLic = 1;
                    }
                    else if(System.DateTime.Today>licdata.EndDate)
                    {
                        sLic = 0;
                    }
                }
                else
                {
                    sLic = -2;
                }
            }
            catch
            { sLic = -2; }
            return sLic;

        }

        public static string GetUID()
        {
            string sRetval = "";
            try
            {

                var baseboardSearcher = new System.Management.ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
                var motherboardSearcher = new System.Management.ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");

                var mbs = new System.Management.ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                System.Management.ManagementObjectCollection mbsList = mbs.Get();
                string pid = "";
                foreach (System.Management.ManagementObject mo in mbsList)
                {
                    pid = mo["ProcessorId"].ToString();
                    break;
                }

                foreach (System.Management.ManagementObject queryObj in baseboardSearcher.Get())
                {
                    pid+= queryObj["SerialNumber"].ToString();
                    break;
                }

                byte[] bsPID = Encoding.UTF8.GetBytes(pid);
                using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] bsUID = sha256Hash.ComputeHash(bsPID);
                    int[] psUID = new int[] { 31, 3, 15, 18, 2, 25  };

                    foreach(var b in psUID)
                    {

                        int s =  (int)bsUID[b] % 26;
                        s += 65;
                        if (s == 73 || s == 79) s += 1;
                        sRetval += Convert.ToChar(s).ToString();
                    }
                }
            }
            catch (Exception ex)
            { }
            return sRetval;

        }

        public static void UpdateLastRun()
        {

            try
            {
                if(IsLicenseFileExist())
                {
                   string sData= System.IO.File.ReadAllText(_sLicFile);
                    var libdata = GetLicenseData();
                    libdata.LastRun = DateTime.Today;
                    UpdateLicenseFile(libdata);
                }
            }
            catch
            { }
        }

        private static void UpdateLicenseFile(LicenseData licData)
        {
            try
            {
                if(licData!=null)
                {
                    if (!System.IO.Directory.Exists(_sLicPath)) System.IO.Directory.CreateDirectory(_sLicPath);

                   

                    var licDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(licData);

                    string uid = GetUID();
                    byte[] key_uid, vi_uid;
                    GenerateAESKeyVI(uid, out key_uid, out vi_uid);

                    string encryptedData = Crytographic.EncryptAesManaged(licDataJson, key_uid, vi_uid);

                    System.IO.File.WriteAllText(_sLicFile, encryptedData);
                }
            }
            catch
            { throw; }
        }
        private static LicenseData GetLicenseData()
        {
            LicenseData licData = null;
            try
            {
                if (IsLicenseFileExist())
                {

                    string sData = System.IO.File.ReadAllText(_sLicFile);
                    string uid = GetUID();
                    byte[] key_uid, vi_uid;
                    GenerateAESKeyVI(uid, out key_uid, out vi_uid);
                    string decryptedData = Crytographic.DecryptAesManaged(sData, key_uid, vi_uid);

                    licData = Newtonsoft.Json.JsonConvert.DeserializeObject<LicenseData>(decryptedData, new Newtonsoft.Json.JsonSerializerSettings() { DateParseHandling = Newtonsoft.Json.DateParseHandling.None }) ;
                    
                }

            }
            catch
            { throw; }

            return licData;
        }
        public static bool Generate_NewLicenseFile()
        {
            bool bretval = false;
            try
            {
               

                

                var sLicStructure = new LicenseData();
                sLicStructure.InitDate = System.DateTime.Today;
                sLicStructure.LastRun = System.DateTime.Today;
                sLicStructure.EndDate = System.DateTime.Today.AddMonths(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                UpdateLicenseFile(sLicStructure);

                /*
                byte[] key_uid, vi_uid;
                GenerateAESKeyVI(uid, out key_uid, out vi_uid);

                var licDataJson= Newtonsoft.Json.JsonConvert.SerializeObject(sLicStructure);
                string encryptedData = Crytographic.EncryptAesManaged(licDataJson, key_uid, vi_uid);

                System.IO.File.WriteAllText(_sLicFile, encryptedData);
                string decryptedData = Crytographic.DecryptAesManaged(encryptedData, key_uid, vi_uid);
                var libdata = Newtonsoft.Json.JsonConvert.DeserializeObject(decryptedData);*/
            }
            catch (Exception ex)
            {

            }
 
            return bretval;
        }

        static void GenerateAESKeyVI(string source, out byte[] key, out byte[] iv)
        {
            key = new byte[32]; iv = new byte[16];
            try
            {
                if(!string.IsNullOrEmpty(source))
                {
                    using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
                    {
                        key = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                        iv = new byte[16];
                        Buffer.BlockCopy(key, 8, iv, 0, 8);
                        Buffer.BlockCopy(key, 2, iv, 8, 8);

                    }
                }
            }
            catch
            { throw; }

        }
    }


    class Crytographic
    {
        internal static string EncryptAesManaged(string raw, byte[] Key, byte[] IV)
        {
            string sRetval = raw;
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                using (System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged())
                {
                    // Encrypt string    
                    byte[] encrypted = Encrypt(raw, Key, IV);
                    sRetval = System.Convert.ToBase64String(encrypted);
                 
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return sRetval;
        }


        internal static string DecryptAesManaged(string cipherText, byte[] Key, byte[] IV)
        {
            string sRetval = "";
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                using (System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged())
                {
                    // Encrypt string    

                    byte[] encrypted = System.Convert.FromBase64String(cipherText);
                    sRetval = Decrypt(encrypted, Key, IV);
                    

                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            return sRetval;
        }

        static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged())
            {
                // Create encryptor    
                System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return encrypted;
        }


        static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            // Create AesManaged    
            using (System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged())
            {
                // Create a decryptor    
                System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
    }
}
