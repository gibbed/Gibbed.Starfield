/* Copyright (c) 2023 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Starfield.FileFormats.ComponentDatabase
{
    internal class StringTable : IDisposable
    {
        private MemoryStream _Stream;
        private bool _IsDisposed;

        public StringTable(byte[] bytes)
        {
            this._Stream = new(bytes, false);
        }

        public string Get(int offset)
        {
            if (offset < 0 || offset >= this._Stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            this._Stream.Position = offset;
            return this._Stream.ReadStringZ(Encoding.ASCII);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._IsDisposed == false)
            {
                if (disposing == true)
                {
                    this._Stream.Dispose();
                    this._Stream = null;
                }

                this._IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
