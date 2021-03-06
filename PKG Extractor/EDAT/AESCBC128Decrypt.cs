﻿namespace edatat
{
    using System;
    using System.Security.Cryptography;

    internal class AESCBC128Decrypt : Decryptor
    {
        private RijndaelManaged c;
        private ICryptoTransform ct;

        public override void doInit(byte[] key, byte[] iv)
        {
            try
            {
                this.c = new RijndaelManaged();
                this.c.Padding = PaddingMode.None;
                this.c.Mode = CipherMode.CBC;
                this.c.Key = key;
                this.c.IV = iv;
                this.ct = this.c.CreateDecryptor();
            }
            catch (Exception)
            {
            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            try
            {
                this.ct.TransformBlock(i, inOffset, len, o, outOffset);
            }
            catch (Exception)
            {
            }
        }
    }
}

