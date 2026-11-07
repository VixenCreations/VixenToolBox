using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static VixenTools.MMD.Editor.VixenMMDText;

namespace VixenTools.MMD.Editor
{
    internal sealed class VmdMotion
    {
        public string Path;
        public string Model;
        public int Tracks;
        public readonly Dictionary<string, float> Peaks = new Dictionary<string, float>();
        public string Error;
        public string FileName => System.IO.Path.GetFileName(Path);
    }

    internal static class VixenMMDVmd
    {
        private static Encoding _sjis;
        private static bool _sjisTried;

        private static Encoding ShiftJis
        {
            get
            {
                if (_sjisTried) return _sjis;
                _sjisTried = true;
                try { _sjis = Encoding.GetEncoding(932); }
                catch (Exception) { _sjis = null; }
                return _sjis;
            }
        }

        internal static VmdMotion Read(string path)
        {
            var motion = new VmdMotion { Path = path };
            byte[] b;
            try
            {
                b = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                motion.Error = L("Could not read the file.") + " " + e.Message;
                return motion;
            }
            var enc = ShiftJis;
            if (enc == null)
            {
                motion.Error = L("This Unity editor cannot read Japanese (Shift-JIS) text, so the morph names cannot be read.");
                return motion;
            }
            string head = Encoding.ASCII.GetString(b, 0, Math.Min(30, b.Length));
            int off, modelLen;
            if (head.StartsWith("Vocaloid Motion Data 0002")) { off = 30; modelLen = 20; }
            else if (head.StartsWith("Vocaloid Motion Data file")) { off = 30; modelLen = 10; }
            else
            {
                motion.Error = L("This is not a VMD motion file.");
                return motion;
            }
            try
            {
                motion.Model = Name(b, off, modelLen, enc);
                off += modelLen;
                uint bones = BitConverter.ToUInt32(b, off);
                off += 4;
                long skip = (long)bones * 111;
                if (off + skip + 4 > b.Length) return motion;
                off += (int)skip;
                uint count = BitConverter.ToUInt32(b, off);
                off += 4;
                for (uint i = 0; i < count && off + 23 <= b.Length; i++)
                {
                    string name = Name(b, off, 15, enc);
                    float weight = BitConverter.ToSingle(b, off + 19);
                    off += 23;
                    motion.Tracks++;
                    if (!motion.Peaks.TryGetValue(name, out float peak) || weight > peak) motion.Peaks[name] = weight;
                }
            }
            catch (Exception e)
            {
                motion.Error = L("The file is damaged.") + " " + e.Message;
            }
            return motion;
        }

        private static string Name(byte[] b, int off, int len, Encoding enc)
        {
            int end = off;
            while (end < off + len && end < b.Length && b[end] != 0) end++;
            return enc.GetString(b, off, end - off);
        }
    }
}
