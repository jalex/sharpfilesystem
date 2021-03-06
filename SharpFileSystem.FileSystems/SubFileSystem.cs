﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpFileSystem.Collections;

namespace SharpFileSystem.FileSystems {

    public class SubFileSystem : IFileSystem {

        #region properties

        public IFileSystem FileSystem { get; }
        public FileSystemPath Root { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SubFileSystem(IFileSystem fileSystem, FileSystemPath root) {
            this.FileSystem = fileSystem;
            this.Root = root;
        }

        protected FileSystemPath AppendRoot(FileSystemPath path) {
            return Root.AppendPath(path);
        }

        protected FileSystemPath RemoveRoot(FileSystemPath path) {
            return path.RemoveParent(Root);
        }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path) {
            var paths = FileSystem.GetEntities(AppendRoot(path));
            return new EnumerableCollection<FileSystemPath>(paths.Select(p => RemoveRoot(p)), paths.Count);
        }

        public bool Exists(FileSystemPath path) {
            return FileSystem.Exists(AppendRoot(path));
        }

        public Stream CreateFile(FileSystemPath path) {
            return FileSystem.CreateFile(AppendRoot(path));
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access) {
            return FileSystem.OpenFile(AppendRoot(path), access);
        }

        public void CreateDirectory(FileSystemPath path) {
            FileSystem.CreateDirectory(AppendRoot(path));
        }

        public void Delete(FileSystemPath path) {
            FileSystem.Delete(AppendRoot(path));
        }

        public void Dispose() {
            FileSystem.Dispose();
        }
    }
}
