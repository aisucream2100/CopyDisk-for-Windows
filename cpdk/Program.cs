using System;
using System.IO;
using System.Security.Principal;

namespace cpdk
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- タイトル表示 ---
            Console.WriteLine("ディスクのコピーと管理ツール");
            Console.WriteLine("Version: 11.1(v1.1)");
            Console.WriteLine("Windows x64_Administrator");
            Console.WriteLine(); // 改行を入れて見やすくします

            // 1. 管理者権限のチェック
            if (!IsAdministrator())
            {
                Console.WriteLine("[エラー] 管理者権限がありません。");
                return;
            }

            // 2. 引数のチェック (ここが原因で終了していた可能性があります)
            if (args.Length < 2)
            {
                Console.WriteLine("使用方法: cpdk \"コピー元\" \"コピー先\"");
                Console.WriteLine("例: cpdk \"E:\\\" \"F:\\backup.img\"");
                return;
            }

            string sourceDriveInput = args[0];
            string destinationFilePath = args[1];

            // 3. パス整形 (末尾の \ を消して \\.\E: の形にする)
            string driveLetter = sourceDriveInput.TrimEnd('\\');
            if (!driveLetter.Contains(":"))
            {
                Console.WriteLine($"エラー: ドライブ名が正しくありません ({sourceDriveInput})");
                return;
            }
            string devicePath = $@"\\.\{driveLetter}";

            try
            {
                // コピー処理の開始
                using (FileStream sourceStream = new FileStream(devicePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream destStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Console.WriteLine($"コピー元: {devicePath}");
                    Console.WriteLine($"コピー先: {destinationFilePath}");
                    Console.WriteLine("--------------------------------------");

                    int bufferSize = 4 * 1024 * 1024; // 4MB
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        // 100MBごとに進捗を表示
                        if (totalBytesRead % (100 * 1024 * 1024) < bufferSize)
                        {
                            Console.Write($"\rコピー済み: {totalBytesRead / (1024 * 1024):N0} MB");
                        }
                    }
                    Console.WriteLine($"\n完了！ 合計: {totalBytesRead / (1024 * 1024):N0} MB");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[エラー発生] {ex.Message}");
            }
        }

        private static bool IsAdministrator()
        {
            if (!OperatingSystem.IsWindows()) return false;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}