using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter.FluentBuilderItems
{
    // todo メンバの入れ子問題
    // todo メンバのインタフェースだったら問題 -> 初期化処理を登録できるようにするのでそこでやってもらう
    // todo メンバの配列（可変長）問題
    // todo Getオンリーのプロパティ（固定値）
    // todo メンバに所属すらしない情報（ヘッダーの識別値とか）上と一緒か？
    public interface IBuildItem<T>
    {
        int Size { get; }
        void Write(T model, BinaryWriter w);
        object Read(T model, BinaryReader r, IDictionary<string, object> _context);
        // todo そもそも_contextがダサいけどTag作るのとReadの戻りをobjectにするのとどっちがマシか
        string Tag { get; set; }
    }
}
