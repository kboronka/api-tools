/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HttpPack.Server
{
	public class HttpCachedFile
	{
		public DateTime LastModified { get; protected set; }
		public string ContentType { get; private set; }
		public string ETag { get; private set; }
		public byte[] Data { get; private set; }

		public bool ParsingRequired { get; protected set; }
		
		protected bool embedded;
		protected string path;
		
		private FileSystemWatcher watcher;
		
		#region constructors
		
		protected HttpCachedFile(string path, byte[] data)
		{
			this.path = path;
			this.embedded = true;

            string extension = Path.GetExtension(path).ToLower();
			this.ContentType = HttpMimeTypes.GetMimeType(extension);
			this.Data = data;
			this.ETag = GetETag(this.Data);
			this.ParsingRequired = false;
			
			if (this.ContentType.Contains("text") || this.ContentType.Contains("xml"))
			{
				string text = Encoding.ASCII.GetString(this.Data);
				MatchCollection matches = Regex.Matches(text, HttpContent.INCLUDE_RENDER_SYNTAX);
				if (matches.Count > 0) this.ParsingRequired = true;
				
				// include linked externals
				matches = Regex.Matches(text, HttpContent.CONTENT_RENDER_SYNTAX);
				if (matches.Count > 0) this.ParsingRequired = true;
			}
		}
		
		public HttpCachedFile(string path) : this(path, File.ReadAllBytes(path))
		{
			this.LastModified = File.GetLastWriteTimeUtc(path);
			
			watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(path);
			watcher.Filter = Path.GetFileName(path);
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Deleted += new FileSystemEventHandler(OnDelete);
			watcher.Renamed += new RenamedEventHandler(OnRenamed);
			watcher.EnableRaisingEvents = true;
		}
		
		#endregion

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			this.Data = ReadAllBytes(path);
			this.ETag = GetETag(this.Data);
			this.LastModified = File.GetLastWriteTimeUtc(path);
		}
		
		private void OnDelete(object sender, FileSystemEventArgs e)
		{
			
		}

		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			
		}
		
		private static string GetETag(byte[] data)
		{
			var hash = new MD5CryptoServiceProvider().ComputeHash(data);
			var hex = "";
			
			foreach (var b in hash)
			{
				hex += b.ToString("X2");
			}
			
			return @""" + hex + @""";
		}

        private static byte[] ReadAllBytes(string path)
        {
            byte[] buffer;

            using (var fs = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }

            return buffer;
        }

        private static FileStream WaitForFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            // check if file is locked by another application
            for (int attempts = 0; attempts < 10; attempts++)
            {
                try
                {
                    var fs = new FileStream(path, mode, access, share);

                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);

                    return fs;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}