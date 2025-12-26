//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models;


//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.IO
//{
//    /// <summary>
//    /// .staging 直下に .tmp で追記し、Complete() で .tsv に確定。
//    /// </summary>
//    public sealed class TsvStagingWriter : IDisposable
//    {
//        private readonly string _tmpPath;
//        private readonly string _finalPath;
//        private StreamWriter? _sw;

//        public TsvStagingWriter(string stagingDir, string finalDir, string fileBaseNameNoExt)
//        {
//            Directory.CreateDirectory(stagingDir);
//            Directory.CreateDirectory(finalDir);
//            _tmpPath = Path.Combine(stagingDir, $"{fileBaseNameNoExt}.tsv.tmp");
//            _finalPath = Path.Combine(finalDir, $"{fileBaseNameNoExt}.tsv");

//            // ヘッダは新規作成時のみ
//            var exists = File.Exists(_tmpPath);
//            _sw = new StreamWriter(_tmpPath, append: true, new UTF8Encoding(false));
//            if (!exists)
//                _sw.WriteLine(string.Join('\t', RecordLineV1.Header));
//        }

//        public void Append(RecordLineV1 rec)
//        {
//            if (_sw is null) throw new ObjectDisposedException(nameof(TsvStagingWriter));
//            _sw.WriteLine(string.Join('\t', rec.ToTsvRow()));
//        }

//        public void Flush() => _sw?.Flush();

//        /// <summary>
//        /// .tmp → .tsv に移動（上書きしない）。戻り値: 完了した最終パス
//        /// </summary>
//        public string Complete()
//        {
//            Dispose();
//            if (!File.Exists(_tmpPath)) return _finalPath;

//            if (File.Exists(_finalPath))
//                throw new IOException($"Final TSV already exists: {_finalPath}");

//            File.Move(_tmpPath, _finalPath);
//            return _finalPath;
//        }

//        public void Dispose()
//        {
//            _sw?.Flush();
//            _sw?.Dispose();
//            _sw = null;
//        }
//    }
//}

