using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ApplicationSuite.AppShared.Commands
{
    /// <summary>
    /// ===============================================================
    /// RelayCommand / RelayCommand<T>
    /// ---------------------------------------------------------------
    /// - WPF における ICommand の汎用実装。
    /// - ボタンやメニュー等から呼ばれる「コマンド処理」を簡単に書ける。
    /// - ViewModel 層で使うことを前提としたクラス。
    /// 
    /// 【ポイント】
    /// 1) コードビハインドに書かずに、ViewModel に処理を集約できる。
    /// 2) Action(処理内容) と Func(実行可否) を渡すだけでコマンド完成。
    /// 3) 非ジェネリック版は object? を受け取る。
    /// 4) ジェネリック版 RelayCommand<T> は「型付き」で安全に受け取れる。
    /// ===============================================================
    /// </summary>
    public class RelayCommand : ICommand
    {
        // ==== フィールド ====
        private readonly Action<object?> _execute;            // 実行内容（必須）
        private readonly Func<object?, bool>? _canExecute;    // 実行可否（任意）

        // ==== コンストラクタ ====
        /// <summary>
        /// RelayCommand の生成。
        /// </summary>
        /// <param name="execute">実際に実行する処理（必須）。</param>
        /// <param name="canExecute">実行可能かどうか判定する処理（省略可）。</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ==== ICommand 実装 ====
        /// <summary>
        /// コマンドが実行可能かどうかを返す。
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// コマンドを実行する。
        /// </summary>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// 実行可否の変化を通知するイベント。
        /// CommandManager.RequerySuggested に委譲するのが定番。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// ===============================================================
    /// RelayCommand<T>
    /// ---------------------------------------------------------------
    /// - 型付きの ICommand 汎用実装。
    /// - 例えば RelayCommand<string> とすると、parameter が string 型で受け取れる。
    /// 
    /// 【メリット】
    /// - キャスト不要。バインドパラメータがそのまま T 型で渡る。
    /// - 実行可否 (CanExecute) も T 型で書けるので安全。
    /// ===============================================================
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        // ==== フィールド ====
        private readonly Action<T?> _execute;           // 実行内容（必須）
        private readonly Func<T?, bool>? _canExecute;   // 実行可否（任意）

        // ==== コンストラクタ ====
        /// <summary>
        /// RelayCommand<T> の生成。
        /// </summary>
        /// <param name="execute">実際に実行する処理（必須）。</param>
        /// <param name="canExecute">実行可能かどうか判定する処理（省略可）。</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ==== ICommand 実装 ====
        public bool CanExecute(object? parameter)
        {
            // parameter が T 型にキャスト可能か試す
            if (parameter == null && default(T) != null)
            {
                // 例えば T が値型(intなど)で null が来た場合は実行不可
                return false;
            }
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            T? value = default;
            if (parameter != null)
            {
                value = (T)parameter;
            }
            _execute(value);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
