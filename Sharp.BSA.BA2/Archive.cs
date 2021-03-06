﻿using ICSharpCode.SharpZipLib.Zip.Compression;
using lz4;
using SharpBSABA2.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpBSABA2
{
    public abstract class Archive
    {
        public bool RetrieveRealSize { get; set; }

        public string FullPath { get; private set; }
        public string FileName => Path.GetFileName(this.FullPath);

        public virtual int Chunks { get; set; }
        public virtual int FileCount { get; set; }
        public virtual bool HasNameTable { get; set; }
        public virtual string VersionString { get; set; } = "None";

        public virtual ArchiveTypes Type { get; protected set; }

        public Inflater Inflater { get; private set; } = new Inflater();
        public List<ArchiveEntry> Files { get; private set; } = new List<ArchiveEntry>();

        public BinaryReader BinaryReader { get; private set; }

        static Archive()
        {
            lz4.AnyCPU.loader.LZ4Loader.DisableVCRuntimeDetection = true;
        }

        public Archive(string filePath) : this(filePath, Encoding.UTF7) { }

        public Archive(string filePath, Encoding encoding)
        {
            this.FullPath = filePath;
            this.BinaryReader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read), encoding);

            this.Open(filePath);
        }

        public void Decompress(byte[] data, byte[] uncompressed)
        {
            this.Inflater.Reset();
            this.Inflater.SetInput(data);
            this.Inflater.Inflate(uncompressed);
        }

        public void DecompressLZ4(byte[] data, Stream stream)
        {
            using (var ms = new MemoryStream(data, false))
            using (var lz4Stream = LZ4Stream.CreateDecompressor(ms, LZ4StreamMode.Read))
            {
                lz4Stream.CopyTo(stream);
            }
        }

        public void Close()
        {
            this.BinaryReader?.Close();
        }

        public void Extract(string filePath, string destination, bool preserveFolder, bool overwrite = false)
        {
            this.FindFile(filePath).Extract(destination, preserveFolder, overwrite);
        }

        public ArchiveEntry FindFile(string fullpath)
        {
            return this.Files.Find(x => x.FullPath == fullpath);
        }

        public ArchiveEntry[] FindFiles(params string[] files)
        {
            return this.Files.FindAll(x => Array.IndexOf(files, x.FullPath) >= 0).ToArray();
        }

        protected abstract void Open(string filePath);
    }
}
