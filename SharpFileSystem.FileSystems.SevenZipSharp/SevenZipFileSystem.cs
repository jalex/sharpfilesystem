﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SevenZip;
using SharpFileSystem.IO;

namespace SharpFileSystem.FileSystems.SevenZip {

    public class SevenZipFileSystem : IFileSystem {
        readonly SevenZipExtractor _extractor;
        readonly ICollection<FileSystemPath> _entities = new List<FileSystemPath>();

        /// <summary>
        /// Constructor
        /// </summary>
        SevenZipFileSystem(SevenZipExtractor extractor) {
            _extractor = extractor;
            foreach(var file in _extractor.ArchiveFileData) {
                AddEntity(GetVirtualFilePath(file));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SevenZipFileSystem(Stream stream) : this(new SevenZipExtractor(stream)) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SevenZipFileSystem(string physicalPath) : this(new SevenZipExtractor(physicalPath)) {
        }

        public void AddEntity(FileSystemPath path) {
            if(!_entities.Contains(path)) _entities.Add(path);
            if(!path.IsRoot) AddEntity(path.ParentPath);
        }

        public string GetSevenZipPath(FileSystemPath path) {
            return path.ToString().Remove(0, 1);
        }

        public FileSystemPath GetVirtualFilePath(ArchiveFileInfo archiveFile) {
            string path = FileSystemPath.DirectorySeparatorChar + archiveFile.FileName.Replace(Path.DirectorySeparatorChar, FileSystemPath.DirectorySeparatorChar);
            if(archiveFile.IsDirectory && path[path.Length - 1] != FileSystemPath.DirectorySeparatorChar) path += FileSystemPath.DirectorySeparatorChar;
            return FileSystemPath.Parse(path);
        }

        #region IFileSystem members 

        /// <summary>
        /// Gets a collection of entities in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            if(!path.IsDirectory) throw new ArgumentException("The specified path is not a directory.", nameof(path));
            return _entities.Where(p => !p.IsRoot && p.ParentPath.Equals(path)).ToArray();
        }

        /// <summary>
        /// Returns true if the specified <see cref="FileSystemPath"/> exists; otherwise, false.
        /// </summary>
        public bool Exists(FileSystemPath path) {
            return _entities.Contains(path);
        }

        /// <summary>
        /// Creates or overwrites a file in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        public Stream CreateFile(FileSystemPath path) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Opens a file on the specified <see cref="FileSystemPath"/>.
        /// </summary>
        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            if(access == FileAccess.Write) throw new NotSupportedException();

            Stream s = new ProducerConsumerStream();
            ThreadPool.QueueUserWorkItem(delegate {
                _extractor.ExtractFile(GetSevenZipPath(path), s);
                s.Close();
            });
            return s;
        }

        /// <summary>
        /// Creates or overwrites a directory in the specified <see cref="FileSystemPath"/>.
        /// </summary>
        public void CreateDirectory(FileSystemPath path) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Deletes an entity on the specified <see cref="FileSystemPath"/>.
        /// </summary>
        public void Delete(FileSystemPath path) {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable members

        /// <summary>
        /// Dispose this <see cref="SevenZipFileSystem"/>.
        /// </summary>
        public void Dispose() {
            _extractor.Dispose();
        }

        #endregion
    }
}
