﻿using System;
using System.IO;

namespace SharpFileSystem.IO {

    public class ReadOnlyStream : Stream {
        readonly Stream _stream;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReadOnlyStream(Stream stream) {
            _stream = stream;
        }

        #region Stream members

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override void Flush() {
        }

        public override long Length => _stream.Length;

        public override long Position {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override void Close() {
            _stream.Close();
        }

        #endregion
    }
}
