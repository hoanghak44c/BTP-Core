using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using QLBH.Core.Exceptions;

namespace QLBH.Core.Data
{
    public class GtidCryptionEx
    {
        private const string SPECIALIZE_SECURITY_KEY = "usleX2obrX0J10kAIztRbBaWXh2kGrKjxOx8";

        public static string EncryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Encrypt(toEncrypt, useHashing);
        }
        public static string DecryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Decrypt(toEncrypt, useHashing);
        }
    }

    public class GtidCryptionSemiPubEx
    {
        private const string SPECIALIZE_SECURITY_KEY = "nsKujD8AOMUHm2NUR1JhCk51E7vjCEg16pzBhfPd0xKMNsbLTNTeoCKoeeYBiK4Ds7D0MIX1o4ax5M7fSMrMe3FR6eaq5eL20Ioz7u";

        public static string EncryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Encrypt(toEncrypt, useHashing);
        }
        public static string DecryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Decrypt(toEncrypt, useHashing);
        }
    }

    internal class GtidCryptionReg
    {
        private const string SPECIALIZE_SECURITY_KEY = "d0xKMNsbLTs7DUR1JhCk51E7vjCEg16p0MInsKujD8AOMUHm2NzBhCKoeueYBiK4fPX1o4ax5M7fSMrMe3FR6eaqNTeoD5eL20Ioz7";

        internal static string EncryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Encrypt(toEncrypt, useHashing);
        }
        internal static string DecryptEx(string toEncrypt, bool useHashing)
        {
            return GtidCryption.InstanceOf(SPECIALIZE_SECURITY_KEY).Decrypt(toEncrypt, useHashing);
        }
    }

    public class GtidCryption
    {
        private const string SECURITY_KEY = "1IlhGK2usrX0A0JkrWBXRbx8eX2Oobztakjx";

        private const CipherMode CIPHER_MODE = CipherMode.ECB;

        private const PaddingMode PADDING_MODE = PaddingMode.PKCS7;

        internal readonly string KeyEx = String.Empty;

        internal GtidCryption(string keyEx)
        {
            KeyEx = keyEx;
        }

        private static List<GtidCryption> instance;

        private static bool bA;

        public static GtidCryption Me
        {
            get
            {
                bA = true;

                return InstanceOf(String.Empty);
            }
        }

        internal static GtidCryption InstanceOf(string keyEx)
        {
            if(!bA && String.IsNullOrEmpty(keyEx))

                throw new ManagedException("Invalid operation");

            if(instance == null) instance = new List<GtidCryption>();

            if (!instance.Exists(delegate(GtidCryption match)
                                 {
                                     return match.KeyEx == keyEx;
                                 }) != null)

                instance.Add(new GtidCryption(keyEx));

            var result = instance.Find(delegate(GtidCryption match)
                                     {
                                         return match.KeyEx == keyEx;
                                     });

            if (bA && String.IsNullOrEmpty(keyEx)) bA = false;

            return result;
        }

        private string GetKey()
        {
            return SECURITY_KEY + KeyEx;
        }

        public string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            //System.Configuration.AppSettingsReader settingsReader =
            //                                    new AppSettingsReader();
            // Get the key from config file
            //
            //string key = (string)settingsReader.GetValue("SecurityKey",
            //                                                 typeof(String));

            string key = GetKey();

            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            //tdes.Mode = CipherMode.ECB;
            tdes.Mode = CIPHER_MODE;

            //padding mode(if any extra byte added)

            //tdes.Padding = PaddingMode.PKCS7;
            tdes.Padding = PADDING_MODE;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string Decrypt(string cipherString, bool useHashing)
        {
            string result = String.Empty;
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            //System.Configuration.AppSettingsReader settingsReader =
            //                                    new AppSettingsReader();
            ////Get your key from config file to open the lock!
            //string key = (string)settingsReader.GetValue("SecurityKey",
            //                                             typeof(String));

            string key = GetKey();

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            //tdes.Mode = CipherMode.ECB;
            tdes.Mode = CIPHER_MODE;

            //padding mode(if any extra byte added)
            //tdes.Padding = PaddingMode.PKCS7;
            tdes.Padding = PADDING_MODE;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            result = UTF8Encoding.UTF8.GetString(resultArray);
            //Debug.Print(result);
            return result;
        }
    }
}