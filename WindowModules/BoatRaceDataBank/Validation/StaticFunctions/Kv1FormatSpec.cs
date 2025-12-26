using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File: Validation/StaticFunctions/Kv1FormatSpec.cs
// 目的：v1 の “行数の正解” を 1 箇所に集約（定数だけ）
// 方針：シンプル最小。ロジックなし。副作用なし。

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions
{
    public static class Kv1FormatSpec
    {
        /// <summary>KBGN 行を含めた開催ヘッダの行数（固定）</summary>
        public const int HeaderLinesFixed = 27;

        /// <summary>rR 行を含めた各レースブロックの行数（固定）</summary>
        public const int RaceLinesFixed = 21;

        /// <summary>1開催あたりのレース数（固定）</summary>
        public const int RacesPerVenue = 12;
    }
}

