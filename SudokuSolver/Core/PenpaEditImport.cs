// import code ported from https://github.com/swaroopg92/penpa-edit/blob/master/docs/js/general.js
// which was forked from https://github.com/rjrudman/penpa-edit/blob/master/docs/js/general.js#L474
// and in turn was forked from https://github.com/opt-pan/penpa-edit/blob/master/docs/js/general.js#L621 (original creator)
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SudokuSolver.Core
{
    public class PenpaEditImport
    {
        public static object Import(string urlstring)
        {
            var urlParts = urlstring.Split('?');
            var queryParts = urlParts[1].Split('&');
            var encodedPuzzle = queryParts.First(x => x.StartsWith("p=")).Substring(2);
            var compressedPuzzle = Convert.FromBase64String(encodedPuzzle);

            // TODO: this doesn't work
            // the p parameter, when base64 decoded, doesn't start with zlib compression method (should be 0x78, but we get 0xD5)
            using (var stream = new MemoryStream(compressedPuzzle, 2, compressedPuzzle.Length - 2))
            using (var inflater = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress))
            using (var streamReader = new StreamReader(inflater))
            {
                var output = streamReader.ReadToEnd();
                var json = JsonConvert.DeserializeObject(output);
                return json;
            }
            /*
            using (var inStream = new MemoryStream(compressedPuzzle))
            using (var gzStream = new ZlibStream(inStream, Ionic.Zlib.CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                gzStream.CopyTo(outStream);
                var output = Encoding.UTF8.GetString(outStream.ToArray());
                var json = JsonConvert.DeserializeObject(output);
                return json;
            }
            */
        }
    }
}
