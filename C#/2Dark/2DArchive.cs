using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace _2Dark
{
    public class _2DArchive
    {
        public List<Tuple<string, int, int>> Entries = new List<Tuple<string, int, int>>();
        private string entryname;
        private int sign, numentries, entryoffset, entrysize, entrynamelength, pos, dataoffset;
        private bool result;
        private FileStream fs;
        private FileStream fs2;
        private BinaryReader br;
        private BinaryWriter bw;

        private bool ReadHeader(string bigfile)
        {
            result = false;
            fs = new FileStream(bigfile, FileMode.Open);
            br = new BinaryReader(fs);
            sign = br.ReadInt32();
            if (sign == 16)
            {
                numentries = br.ReadInt32();
                dataoffset = br.ReadInt32();
                br.ReadInt32();
                result = true;
            }
            else
            {
                br.Close();
                fs.Close();
                result = false;
            }
            return result;
        }

        public bool ReadEntry(string bigfile)
        {
            result = false;
            Entries.Clear();
            if (ReadHeader(bigfile))
            {
                int indexoffset = (int)fs.Position + numentries * 8;
                for (int i = 0; i < numentries; i++)
                {
                    entrynamelength = br.ReadInt32();
                    entrysize = br.ReadInt32();
                    pos = (int)fs.Position;
                    fs.Seek(indexoffset, SeekOrigin.Begin);
                    entryname = new string(br.ReadChars(entrynamelength));
                    fs.Seek(dataoffset, SeekOrigin.Begin);
                    entryoffset = (int)fs.Position;
                    Entries.Add(new Tuple<string, int, int>(entryname, entryoffset, entrysize));
                    indexoffset += entrynamelength;
                    dataoffset += entrysize;
                    fs.Seek(pos, SeekOrigin.Begin);
                }
                br.Close();
                fs.Close();
                result = true;
            }
            else
            {
                br.Close();
                fs.Close();
                result = false;
            }
            return result;
        }

        public bool WriteBIGFILE(string selectedfolder)
        {
            result = false;
            fs = new FileStream(selectedfolder + "_NEW.bigfile", FileMode.Create);
            bw = new BinaryWriter(fs);
            bw.Write(Convert.ToUInt32(16));
            string list = selectedfolder + "_list.txt";
            if (File.Exists(list))
            {
                string[] texts = File.ReadAllLines(list);
                bw.Write(Convert.ToUInt32(texts.Count()));
                pos = (int)fs.Position;
                fs.Seek(16L, SeekOrigin.Begin);
                for (int i = 0; i < texts.Count(); i++)
                {
                    bw.Write(Convert.ToUInt32(texts[i].Length));
                    FileInfo fi = new FileInfo(selectedfolder + "\\" + texts[i]);
                    bw.Write(Convert.ToUInt32(fi.Length));
                }
                for (int i = 0; i < texts.Count(); i++)
                {
                    char[] cs = texts[i].ToArray();
                    foreach (char c in cs)
                        bw.Write(c);
                }
                dataoffset = (int)fs.Position;
                fs.Seek(pos, SeekOrigin.Begin);
                bw.Write(Convert.ToUInt32(dataoffset));
                bw.Write(Convert.ToUInt32(549519595));
                fs.Seek(dataoffset, SeekOrigin.Begin);
                for (int i = 0; i < texts.Count(); i++)
                {
                    fs2 = new FileStream(selectedfolder + "\\" + texts[i], FileMode.Open);
                    byte[] buffer = new byte[(int)fs2.Length];
                    fs2.Read(buffer, 0, (int)fs2.Length);
                    fs.Write(buffer, 0, (int)fs2.Length);
                    fs2.Close();
                }
                bw.Close();
                fs.Close();
                result = true;
            }
            return result;
        }
    }
}
