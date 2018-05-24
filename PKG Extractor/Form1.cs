using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Media;
using PeXploit;
using edatat;
using System.Security.Cryptography;

namespace PKG_Extractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string[] id;
        List<String> pkglist = new List<string>(); //pkg list
        List<String> pkgID = new List<string>();//to get each package file name
        private byte[] AesKey = new byte[0x10];
        private byte[] PKGFileKey = new byte[0x10];
        private byte[] PS3AesKey = new byte[] { 0x2e, 0x7b, 0x71, 0xd7, 0xc9, 0xc9, 0xa1, 0x4e, 0xa3, 0x22, 0x1f, 0x18, 0x88, 40, 0xb8, 0xf8 };
        private byte[] PSPAesKey = new byte[] { 7, 0xf2, 0xc6, 130, 0x90, 0xb5, 13, 0x2c, 0x33, 0x81, 0x8d, 0x70, 0x9b, 0x60, 230, 0x2b };

        private byte[] PS4AesKey = new byte[] { };//currently we do not have any keys
        private uint uiEncryptedFileStartOffset;

        public void log(string log)
        {
            listBox1.Items.Add(log + "\r\n");
            Application.DoEvents();

        }

        static string CMD(string com,string arguments )
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Application.StartupPath;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = com;
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();
            System.Threading.Thread.Sleep(5000);
            return string.Empty;
        }

        #region Files Needed For PKG Decrypt
        private string DecryptPKGFile(string PKGFileName, FileInfo ini)
        {
            try
            {
                int moltiplicator = 65536;
                byte[] EncryptedData = new byte[AesKey.Length * moltiplicator];
                byte[] DecryptedData = new byte[AesKey.Length * moltiplicator];

                byte[] PKGXorKey = new byte[AesKey.Length];
                byte[] EncryptedFileStartOffset = new byte[4];
                byte[] EncryptedFileLenght = new byte[8];

                Stream PKGReadStream = new FileStream(PKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader brPKG = new BinaryReader(PKGReadStream);

                PKGReadStream.Seek(0x00, SeekOrigin.Begin);
                byte[] pkgMagic = brPKG.ReadBytes(4);
                if (pkgMagic[0x00] != 0x7F || pkgMagic[0x01] != 0x50 || pkgMagic[0x02] != 0x4B || pkgMagic[0x03] != 0x47)
                {
                    //secondary check for ps4
                    if (pkgMagic[0x00] != 0x7F || pkgMagic[0x01] != 0x43 || pkgMagic[0x02] != 0x4E || pkgMagic[0x03] != 0x54)
                    {
                        //this is either a ps3 or psp or psvita pkg
                        MessageBox.Show("ERROR: Selected file isn't a Pkg file.");
                        SystemSounds.Beep.Play();
                        return string.Empty;
                    }
                }

                //Finalized byte
                PKGReadStream.Seek(0x04, SeekOrigin.Begin);
                byte pkgFinalized = brPKG.ReadByte();

                if (pkgFinalized != 0x80)
                {

                    File.Copy(ini.FullName, Application.StartupPath + "\\" + ini.Name, true);
                    string arguments = ini.Name;
                    string com = Application.StartupPath + "@\\pkg.exe ";
                    CMD(com, arguments);


                    if (File.Exists("PARAM.SFO") == true)
                    {
                        string dir = Application.StartupPath + @"\Extracted";
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        PARAM_SFO parrameters = new PARAM_SFO("PARAM.SFO");
                        string pkgid = parrameters.TitleID;
                        pkgID.Add(pkgid);
                        if (!Directory.Exists(dir + "\\Extracted\\" + pkgid))
                        {
                            string pkgdir = dir + "\\" + pkgid;
                            Directory.CreateDirectory(pkgdir);
                            if (Directory.Exists("USRDIR") == true)
                                Directory.Move("USRDIR", pkgdir + "\\USRDIR");
                            if (Directory.Exists("TROPDIR") == true)
                                Directory.Move("TROPDIR", pkgdir + "\\TROPDIR");
                            if (File.Exists("PARAM.SFO") == true)
                                File.Move("PARAM.SFO", pkgdir + "\\PARAM.SFO");
                            if (File.Exists("ICON0.PNG") == true)
                                File.Move("ICON0.PNG", pkgdir + "\\ICON0.PNG");
                            if (File.Exists("PIC1.PNG") == true)
                                File.Move("PIC1.PNG", pkgdir + "\\PIC1.PNG");
                            if (File.Exists("PS3LOGO.DAT") == true)
                                if (File.Exists("PARAM.HIP") == true)
                                    File.Move("PARAM.HIP", pkgdir + "\\PARAM.HIP");
                            if (File.Exists("PS3LOGO.DAT") == true)
                                File.Move("PS3LOGO.DAT", pkgdir + "\\PS3LOGO.DAT");
                            if (File.Exists("out.txt"))
                            {
                                File.Copy("out.txt", pkgdir + "[" + pkgid + "]extractedlist.txt");
                                File.Delete("out.txt");
                            }
                        }

                        else
                        {
                            if (Directory.Exists("USRDIR") == true)
                                Directory.Delete("USRDIR",true);
                            if (Directory.Exists("TROPDIR") == true)
                                Directory.Delete("TROPDIR", true);
                            if (File.Exists("PARAM.SFO") == true)
                                File.Delete("PARAM.SFO");
                            if (File.Exists("ICON0.PNG") == true)
                                File.Delete("ICON0.PNG");
                            if (File.Exists("PIC1.PNG") == true)
                                File.Delete("PIC1.PNG");
                            if (File.Exists("PS3LOGO.DAT") == true)
                                if (File.Exists("PARAM.HIP") == true)
                                    File.Delete("PARAM.HIP");
                            if (File.Exists("PS3LOGO.DAT") == true)
                                File.Delete("PS3LOGO.DAT");
                        }
                    }
                    SystemSounds.Beep.Play();
                    return string.Empty;
                }

                //PKG Type PSP/PS3
                PKGReadStream.Seek(0x07, SeekOrigin.Begin);
                byte pkgType = brPKG.ReadByte();

                switch (pkgType)
                {
                    case 0x01:
                        //PS3
                        AesKey = PS3AesKey;
                        break;

                    case 0x02:
                        //PSP
                        AesKey = PSPAesKey;
                        break;

                    default:
                        MessageBox.Show( "ERROR: Selected pkg isn't Valid.");
                        SystemSounds.Beep.Play();
                        return string.Empty;
                }

                //0x24 Store the start Address of the encrypted file to decrypt
                PKGReadStream.Seek(0x24, SeekOrigin.Begin);
                EncryptedFileStartOffset = brPKG.ReadBytes((int)EncryptedFileStartOffset.Length);
                Array.Reverse(EncryptedFileStartOffset);
                uiEncryptedFileStartOffset = BitConverter.ToUInt32(EncryptedFileStartOffset, 0);

                //0x1C Store the length of the whole pkg file

                //0x2C Store the length of the encrypted file to decrypt
                PKGReadStream.Seek(0x28, SeekOrigin.Begin);
                EncryptedFileLenght = brPKG.ReadBytes((int)EncryptedFileLenght.Length);
                Array.Reverse(EncryptedFileLenght);
                uint uiEncryptedFileLenght = BitConverter.ToUInt32(EncryptedFileLenght, 0);

                //0x70 Store the PKG file Key.
                PKGReadStream.Seek(0x70, SeekOrigin.Begin);
                PKGFileKey = brPKG.ReadBytes(16);
                byte[] incPKGFileKey = new byte[16];
                Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length);

                //the "file" key at 0x70 have to be encrypted with a "global AES key" to generate the "xor" key
                //PSP uses CipherMode.ECB, PaddingMode.None that doesn't need IV
                PKGXorKey = AESEngine.Encrypt(PKGFileKey, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);

                // Pieces calculation
                double division = (double)uiEncryptedFileLenght / (double)AesKey.Length;
                UInt64 pieces = (UInt64)Math.Floor(division);
                UInt64 mod = (UInt64)uiEncryptedFileLenght / (UInt64)AesKey.Length;
                if (mod > 0)
                    pieces += 1;

                if (File.Exists(PKGFileName + ".Dec"))
                {
                    File.Delete(PKGFileName + ".Dec");
                }

                //Write File
                FileStream DecryptedFileWriteStream = new FileStream(PKGFileName + ".Dec", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                BinaryWriter bwDecryptedFile = new BinaryWriter(DecryptedFileWriteStream);

                //Put the read pointer on the encrypted starting point.
                PKGReadStream.Seek((int)uiEncryptedFileStartOffset, SeekOrigin.Begin);

                // Pieces calculation
                double filedivision = (double)uiEncryptedFileLenght / (double)(AesKey.Length * moltiplicator);
                UInt64 filepieces = (UInt64)Math.Floor(filedivision);
                UInt64 filemod = (UInt64)uiEncryptedFileLenght % (UInt64)(AesKey.Length * moltiplicator);
                if (filemod > 0)
                    filepieces += 1;

                Application.DoEvents();

                for (UInt64 i = 0; i < filepieces; i++)
                {
                    //If we have a mod and this is the last piece then...
                    if ((filemod > 0) && (i == (filepieces - 1)))
                    {
                        EncryptedData = new byte[filemod];
                        DecryptedData = new byte[filemod];
                    }

                    //Read 16 bytes of Encrypted data
                    EncryptedData = brPKG.ReadBytes(EncryptedData.Length);

                    //In order to retrieve a fast AES Encryption we pre-Increment the array
                    byte[] PKGFileKeyConsec = new byte[EncryptedData.Length];
                    byte[] PKGXorKeyConsec = new byte[EncryptedData.Length];

                    for (int pos = 0; pos < EncryptedData.Length; pos += AesKey.Length)
                    {
                        Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length);

                        IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
                    }

                    //the incremented "file" key have to be encrypted with a "global AES key" to generate the "xor" key
                    //PSP uses CipherMode.ECB, PaddingMode.None that doesn't need IV
                    PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);

                    //XOR Decrypt and save every 16 bytes of data:
                    DecryptedData = XOREngine.XOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);

                    Application.DoEvents();

                    bwDecryptedFile.Write(DecryptedData);
                }
                Application.DoEvents();

                DecryptedFileWriteStream.Close();
                bwDecryptedFile.Close();

                return PKGFileName + ".Dec";
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: An error occured during the decrypting process ");
                SystemSounds.Beep.Play();
                return string.Empty;
            }
        }

        private Boolean IncrementArray(ref byte[] sourceArray, int position)
        {
            if (sourceArray[position] == 0xFF)
            {
                if (position != 0)
                {
                    if (IncrementArray(ref sourceArray, position - 1))
                    {
                        sourceArray[position] = 0x00;
                        return true;
                    }
                    else return false; //Maximum reached yet
                }
                else return false; //Maximum reached yet
            }
            else
            {
                sourceArray[position] += 0x01;
                return true;
            }
        }

        public static string HexStringToAscii(string HexString, bool cleanEndOfString)
        {
            try
            {
                string StrValue = "";
                // While there's still something to convert in the hex string
                while (HexString.Length > 0)
                {
                    // Use ToChar() to convert each ASCII value (two hex digits) to the actual character
                    StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexString.Substring(0, 2), 16)).ToString();

                    // Remove from the hex object the converted value
                    HexString = HexString.Substring(2, HexString.Length - 2);
                }
                // Clean String
                if (cleanEndOfString)
                    StrValue = StrValue.Replace("\0", "");

                return StrValue;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string ByteArrayToAscii(byte[] ByteArray, int startPos, int length, bool cleanEndOfString)
        {
            byte[] byteArrayPhrase = new byte[length];
            Array.Copy(ByteArray, startPos, byteArrayPhrase, 0, byteArrayPhrase.Length);
            string hexPhrase = ByteArrayToHexString(byteArrayPhrase);
            return HexStringToAscii(hexPhrase, true);
        }

        public static string ByteArrayToHexString(byte[] ByteArray)
        {
            string HexString = "";
            for (int i = 0; i < ByteArray.Length; ++i)
                HexString += ByteArray[i].ToString("X2"); // +" ";
            return HexString;
        }

        private byte[] DecryptData(int dataSize, long dataRelativeOffset, long pkgEncryptedFileStartOffset, byte[] AesKey, Stream encrPKGReadStream, BinaryReader brEncrPKG)
        {
            int size = dataSize % 16;
            if (size > 0)
                size = ((dataSize / 16) + 1) * 16;
            else
                size = dataSize;

            byte[] EncryptedData = new byte[size];
            byte[] DecryptedData = new byte[size];
            byte[] PKGFileKeyConsec = new byte[size];
            byte[] PKGXorKeyConsec = new byte[size];
            byte[] incPKGFileKey = new byte[PKGFileKey.Length];
            Array.Copy(PKGFileKey, incPKGFileKey, PKGFileKey.Length);

            encrPKGReadStream.Seek(dataRelativeOffset + pkgEncryptedFileStartOffset, SeekOrigin.Begin);
            EncryptedData = brEncrPKG.ReadBytes(size);

            for (int pos = 0; pos < dataRelativeOffset; pos += 16)
            {
                IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
            }

            for (int pos = 0; pos < size; pos += 16)
            {
                Array.Copy(incPKGFileKey, 0, PKGFileKeyConsec, pos, PKGFileKey.Length);

                IncrementArray(ref incPKGFileKey, PKGFileKey.Length - 1);
            }

            //the incremented "file" key have to be encrypted with a "global AES key" to generate the "xor" key
            //PSP uses CipherMode.ECB, PaddingMode.None that doesn't need IV
            PKGXorKeyConsec = AESEngine.Encrypt(PKGFileKeyConsec, AesKey, AesKey, CipherMode.ECB, PaddingMode.None);

            //XOR Decrypt and save every 16 bytes of data:
            DecryptedData = XOREngine.XOR(EncryptedData, 0, PKGXorKeyConsec.Length, PKGXorKeyConsec);

            return DecryptedData;
        }

        private Boolean ExtractFiles(string decryptedPKGFileName, string encryptedPKGFileName, FileInfo line)
        {
            try
            {
                int twentyMb = 1024 * 1024 * 20;
                UInt32 ExtractedFileOffset = 0;
                UInt32 ExtractedFileSize = 0;
                UInt32 OffsetShift = 0;
                long positionIdx = 0;

                String WorkDir = "";
                id = line.Name.Trim(new[] { '[', ']' }).Split('-');
                pkgID.Add(id[0].Replace("]", ""));
                WorkDir = Application.StartupPath + @"\Extracted\" + id[0].Replace("]", "");
                if (Directory.Exists(WorkDir))
                {
                    Directory.Delete(WorkDir, true);
                    System.Threading.Thread.Sleep(100);

                    Directory.CreateDirectory(WorkDir);
                    System.Threading.Thread.Sleep(100);
                }

                byte[] FileTable = new byte[320000];
                byte[] dumpFile;
                byte[] sdkVer = new byte[8];
                byte[] firstFileOffset = new byte[4];
                byte[] firstNameOffset = new byte[4];
                byte[] fileNr = new byte[4];
                byte[] isDir = new byte[4];
                byte[] Offset = new byte[4];
                byte[] Size = new byte[4];
                byte[] NameOffset = new byte[4];
                byte[] NameSize = new byte[4];
                byte[] Name = new byte[32];
                byte[] bootMagic = new byte[8];
                byte contentType = 0;
                byte fileType = 0;
                bool isFile = false;

                Stream decrPKGReadStream = new FileStream(decryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader brDecrPKG = new BinaryReader(decrPKGReadStream);

                Stream encrPKGReadStream = new FileStream(encryptedPKGFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader brEncrPKG = new BinaryReader(encrPKGReadStream);

                //Read the file Table
                decrPKGReadStream.Seek((long)0, SeekOrigin.Begin);
                FileTable = brDecrPKG.ReadBytes(FileTable.Length);

                positionIdx = 0;

                OffsetShift = 0;   //Shift Relative to os.raw

                Array.Copy(FileTable, 0, firstNameOffset, 0, firstNameOffset.Length);
                Array.Reverse(firstNameOffset);
                uint uifirstNameOffset = BitConverter.ToUInt32(firstNameOffset, 0);

                uint uiFileNr = uifirstNameOffset / 32;

                Array.Copy(FileTable, 12, firstFileOffset, 0, firstFileOffset.Length);
                Array.Reverse(firstFileOffset);
                uint uifirstFileOffset = BitConverter.ToUInt32(firstFileOffset, 0);

                //Read the file Table
                decrPKGReadStream.Seek((long)0, SeekOrigin.Begin);
                FileTable = brDecrPKG.ReadBytes((int)uifirstFileOffset);

                //If number of files is negative then something is wrong...
                if ((int)uiFileNr < 0)
                {
                   MessageBox.Show( "ERROR: An error occured during the files extraction process cause of a decryption error");
                    SystemSounds.Beep.Play();
                    return false;
                }
                Application.DoEvents();

                //Table:
                //0-3         4-7         8-11        12-15       16-19       20-23       24-27       28-31
                //|name loc | |name size| |   NULL  | |file loc | |  NULL   | |file size| |cont type| |   NULL  |

                for (int ii = 0; ii < (int)uiFileNr; ii++)
                {
                    Array.Copy(FileTable, positionIdx + 12, Offset, 0, Offset.Length);
                    Array.Reverse(Offset);
                    ExtractedFileOffset = BitConverter.ToUInt32(Offset, 0) + OffsetShift;

                    Array.Copy(FileTable, positionIdx + 20, Size, 0, Size.Length);
                    Array.Reverse(Size);
                    ExtractedFileSize = BitConverter.ToUInt32(Size, 0);

                    Array.Copy(FileTable, positionIdx, NameOffset, 0, NameOffset.Length);
                    Array.Reverse(NameOffset);
                    uint ExtractedFileNameOffset = BitConverter.ToUInt32(NameOffset, 0);

                    Array.Copy(FileTable, positionIdx + 4, NameSize, 0, NameSize.Length);
                    Array.Reverse(NameSize);
                    uint ExtractedFileNameSize = BitConverter.ToUInt32(NameSize, 0);

                    contentType = FileTable[positionIdx + 24];
                    fileType = FileTable[positionIdx + 27];

                    Name = new byte[ExtractedFileNameSize];
                    Array.Copy(FileTable, (int)ExtractedFileNameOffset, Name, 0, ExtractedFileNameSize);
                    string ExtractedFileName = ByteArrayToAscii(Name, 0, Name.Length, true);

                    //Write Directory
                    if (!Directory.Exists(WorkDir))
                    {
                        Directory.CreateDirectory(WorkDir);
                        System.Threading.Thread.Sleep(100);
                    }

                    FileStream ExtractedFileWriteStream = null;

                    //File / Directory
                    if ((fileType == 0x04) && (ExtractedFileSize == 0x00))
                        isFile = false;
                    else
                        isFile = true;

                    //contentType == 0x90 = PSP file/dir
                    if (contentType == 0x90)
                    {
                        string FileDir = WorkDir + "\\" + ExtractedFileName;
                        FileDir = FileDir.Replace("/", "\\");
                        DirectoryInfo FileDirectory = Directory.GetParent(FileDir);

                        if (!Directory.Exists(FileDirectory.ToString()))
                        {
                            Directory.CreateDirectory(FileDirectory.ToString());
                        }
                        ExtractedFileWriteStream = new FileStream(FileDir, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                    }
                    else
                    {
                        //contentType == (0x80 || 0x00) = PS3 file/dir
                        //fileType == 0x01 = NPDRM File
                        //fileType == 0x03 = Raw File
                        //fileType == 0x04 = Directory

                        //Decrypt PS3 Filename
                        byte[] DecryptedData = DecryptData((int)ExtractedFileNameSize, (long)ExtractedFileNameOffset, (long)uiEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG);
                        Array.Copy(DecryptedData, 0, Name, 0, ExtractedFileNameSize);
                        ExtractedFileName = ByteArrayToAscii(Name, 0, Name.Length, true);

                        if (!isFile)
                        {
                            //Directory
                            try
                            {
                                if (!Directory.Exists(ExtractedFileName))
                                    Directory.CreateDirectory(WorkDir + "\\" + ExtractedFileName);
                            }
                            catch (Exception ex)
                            {
                                //This should not happen xD
                                ExtractedFileName = ii.ToString() + ".raw";
                                if (!Directory.Exists(ExtractedFileName))
                                    Directory.CreateDirectory(WorkDir + "\\" + ExtractedFileName);
                            }
                        }
                        else
                        {
                            //File
                            try
                            {
                                ExtractedFileWriteStream = new FileStream(WorkDir + "\\" + ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                            }
                            catch (Exception ex)
                            {
                                //This should not happen xD
                                ExtractedFileName = ii.ToString() + ".raw";
                                ExtractedFileWriteStream = new FileStream(WorkDir + "\\" + ExtractedFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                            }
                        }
                    }

                    if (contentType == 0x90 && isFile)
                    {
                        //Read/Write File
                        BinaryWriter ExtractedFile = new BinaryWriter(ExtractedFileWriteStream);
                        decrPKGReadStream.Seek((long)ExtractedFileOffset, SeekOrigin.Begin);

                        // Pieces calculation
                        double division = (double)ExtractedFileSize / (double)twentyMb;
                        UInt64 pieces = (UInt64)Math.Floor(division);
                        UInt64 mod = (UInt64)ExtractedFileSize % (UInt64)twentyMb;
                        if (mod > 0)
                            pieces += 1;

                        dumpFile = new byte[twentyMb];
                        for (UInt64 i = 0; i < pieces; i++)
                        {
                            //If we have a mod and this is the last piece then...
                            if ((mod > 0) && (i == (pieces - 1)))
                                dumpFile = new byte[mod];

                            //Fill buffer
                            brDecrPKG.Read(dumpFile, 0, dumpFile.Length);

                            ExtractedFile.Write(dumpFile);

                            Application.DoEvents();
                        }

                        ExtractedFileWriteStream.Close();
                        ExtractedFile.Close();
                    }

                    if (contentType != 0x90 && isFile)
                    {
                        //Read/Write File
                        BinaryWriter ExtractedFile = new BinaryWriter(ExtractedFileWriteStream);
                        decrPKGReadStream.Seek((long)ExtractedFileOffset, SeekOrigin.Begin);

                        // Pieces calculation
                        double division = (double)ExtractedFileSize / (double)twentyMb;
                        UInt64 pieces = (UInt64)Math.Floor(division);
                        UInt64 mod = (UInt64)ExtractedFileSize % (UInt64)twentyMb;
                        if (mod > 0)
                            pieces += 1;

                        dumpFile = new byte[twentyMb];
                        long elapsed = 0;
                        for (UInt64 i = 0; i < pieces; i++)
                        {
                            //If we have a mod and this is the last piece then...
                            if ((mod > 0) && (i == (pieces - 1)))
                                dumpFile = new byte[mod];

                            //Fill buffer
                            byte[] DecryptedData = DecryptData(dumpFile.Length, (long)ExtractedFileOffset + elapsed, (long)uiEncryptedFileStartOffset, PS3AesKey, encrPKGReadStream, brEncrPKG);
                            elapsed += dumpFile.Length;

                            //To avoid decryption pad we use dumpFile.Length that's the actual decrypted file size!
                            ExtractedFile.Write(DecryptedData, 0, dumpFile.Length);

                            Application.DoEvents();
                        }

                        ExtractedFileWriteStream.Close();
                        ExtractedFile.Close();
                    }

                    positionIdx = positionIdx + 32;

                    Application.DoEvents();
                }

                Application.DoEvents();

                //Close File
                encrPKGReadStream.Close();
                brEncrPKG.Close();

                decrPKGReadStream.Close();
                brDecrPKG.Close();

                //Delete decrypted file
                if (File.Exists(decryptedPKGFileName))
                {
                    File.Delete(decryptedPKGFileName);
                }

                //richTextBox1.Invoke(new Action(() => richTextBox1.Text +="\nSUCCESS: Pkg extracted and decrypted successfully."));
                SystemSounds.Beep.Play();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show( "ERROR: An error occured during the files extraction process ");
                SystemSounds.Beep.Play();
                return false;
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
              OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Select PKG File";
                theDialog.Filter = "PKG Files|*.pkg";
                theDialog.InitialDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPKG.Text = theDialog.FileName.ToString();
                    pkglist.Add(new DirectoryInfo(txtPKG.Text).FullName);//add the pkg to a list so we can do batch install still working out the kinks
                    listBox1.Items.Add(new DirectoryInfo(txtPKG.Text).Name);
                }
                if(richTextBox1.Text.Contains("All .pkg(s) decrypted"))
                {
                    richTextBox1.Text = "===Welcome TO PKG Extractor ===";
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           DialogResult result= MessageBox.Show("Are You Sure You Want To Delete This PKG From The List", "?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                foreach (string item in pkglist)
                {
                    if (item.Contains(listBox1.SelectedItem.ToString()))
                    {
                        pkglist.Remove(item);
                        listBox1.Items.Remove(listBox1.SelectedItem);
                        return;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            btnRUN.Text = "Decrypting";
            btnRUN.Enabled = false;
            progressBar1.Visible = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {  
            try
            {
                foreach (string line in pkglist)
                {
                    FileInfo ini = new FileInfo(line);
                    richTextBox1.Invoke(new Action(() => richTextBox1.Text += "\nDecrypting " + ini.Name ));
                    string decryptedPKGFileName = this.DecryptPKGFile(line, ini);
                    if ((decryptedPKGFileName != null) && (decryptedPKGFileName != string.Empty))
                    {
                        this.ExtractFiles(decryptedPKGFileName, line, ini);
                    }
                }
            }

            catch
            {
                MessageBox.Show("An Error Accoured ");
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            listBox1.Items.Clear();
            pkgID.Clear();
            pkglist.Clear();
            richTextBox1.Clear();
            richTextBox1.Invoke(new Action(() => richTextBox1.Text += "===Welcome TO PKG Extractor ===\nAll .pkg(s) decrypted"));
            IEnumerable<string> enumerable;
            string path = Application.StartupPath;
            try
            {
                enumerable = Directory.EnumerateFiles(path, "*.DEC", SearchOption.AllDirectories);
                foreach (string str4 in enumerable)
                {
                    File.Delete(str4);
                }
            }
            catch
            {
            }
            try
            {
                enumerable = Directory.EnumerateFiles(path, "*.pkg", SearchOption.AllDirectories);
                foreach (string str4 in enumerable)
                {
                    File.Delete(str4);
                }
            }
            catch
            {
            }
            if (File.Exists("out.txt"))
                File.Delete("out.txt");
            btnRUN.Text = "Un Package";
            btnRUN.Enabled = true;
            if (Directory.Exists(Application.StartupPath + @"\\Extracted"))
                System.Diagnostics.Process.Start(Application.StartupPath + @"\\Extracted");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (Directory.Exists(Application.StartupPath + @"\\Extracted"))
                System.Diagnostics.Process.Start(Application.StartupPath + @"\\Extracted");
            else
                System.Diagnostics.Process.Start(Application.StartupPath);
        }
    }
}
