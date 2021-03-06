﻿namespace edatat
{
    using System;

    internal abstract class HashGenerator
    {
        protected HashGenerator()
        {
        }

        public bool compareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (value1[i + offset1] != value2[i + offset2])
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool doFinal(byte[] generateHash)
        {
            return false;
        }

        public virtual void doInit(byte[] key)
        {
        }

        public virtual void doUpdate(byte[] i, int inOffset, int len)
        {
        }

        public virtual void setHashLen(int len)
        {
        }
    }
}

