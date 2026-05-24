using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BootTracker.Models;

namespace BootTracker.Services
{
    public class DatabaseService : IDisposable
    {
        private IntPtr _db;
        private static DatabaseService _instance;
        private static readonly object _lock = new object();

        private static readonly string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BootTracker", "boot_records.db");

        private const string SqliteDll = "sqlite3";

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_open(byte[] filename, out IntPtr db);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_close(IntPtr db);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_errmsg(IntPtr db);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_prepare_v2(IntPtr db, byte[] sql, int nByte, out IntPtr stmt, out IntPtr pzTail);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_step(IntPtr stmt);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_column_bytes(IntPtr stmt, int iCol);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_column_text(IntPtr stmt, int iCol);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern long sqlite3_column_int64(IntPtr stmt, int iCol);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] val, int n, IntPtr free);

        [DllImport(SqliteDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);

        private const int SQLITE_OK = 0;
        private const int SQLITE_ROW = 100;
        private const int SQLITE_DONE = 101;

        public static DatabaseService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new DatabaseService();
                    return _instance;
                }
            }
        }

        private DatabaseService()
        {
            var dir = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (sqlite3_open(ToUtf8(DbPath), out _db) != SQLITE_OK)
                throw new Exception("Cannot open database: " + GetError());

            Exec("CREATE TABLE IF NOT EXISTS records (" +
                 "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                 "user_name TEXT NOT NULL," +
                 "reason TEXT NOT NULL," +
                 "approver TEXT NOT NULL," +
                 "boot_time TEXT NOT NULL," +
                 "shutdown_time TEXT DEFAULT ''," +
                 "created_at TEXT DEFAULT (datetime('now','localtime')));");
            Exec("CREATE INDEX IF NOT EXISTS idx_boot_time ON records(boot_time);");
            Exec("CREATE INDEX IF NOT EXISTS idx_user ON records(user_name);");

            // Migrate existing databases that lack shutdown_time column
            try { Exec("ALTER TABLE records ADD COLUMN shutdown_time TEXT DEFAULT '';"); }
            catch { }
        }

        public void AddRecord(string userName, string reason, string approver, string bootTime = null, string shutdownTime = null)
        {
            var bt = string.IsNullOrEmpty(bootTime)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : bootTime;
            var sql = "INSERT INTO records (user_name, reason, approver, boot_time, shutdown_time) VALUES (@u, @r, @a, @t, @s)";
            using (var stmt = Prepare(sql))
            {
                BindText(stmt, 1, userName);
                BindText(stmt, 2, reason);
                BindText(stmt, 3, approver);
                BindText(stmt, 4, bt);
                BindText(stmt, 5, shutdownTime ?? "");
                Step(stmt);
            }
        }

        public void DeleteRecord(long id)
        {
            using (var stmt = Prepare("DELETE FROM records WHERE id = @id"))
            {
                sqlite3_bind_int64(stmt.Handle, 1, id);
                Step(stmt);
            }
        }

        public void UpdateRecord(long id, string userName, string reason, string approver, string shutdownTime = null)
        {
            var sql = "UPDATE records SET user_name=@u, reason=@r, approver=@a, shutdown_time=@s WHERE id=@id";
            using (var stmt = Prepare(sql))
            {
                BindText(stmt, 1, userName);
                BindText(stmt, 2, reason);
                BindText(stmt, 3, approver);
                BindText(stmt, 4, shutdownTime ?? "");
                sqlite3_bind_int64(stmt.Handle, 5, id);
                Step(stmt);
            }
        }

        public void UpdateLatestShutdownTime(string shutdownTime)
        {
            var sql = "UPDATE records SET shutdown_time = @t WHERE id = " +
                      "(SELECT id FROM records WHERE shutdown_time IS NULL OR shutdown_time = '' " +
                      "ORDER BY boot_time DESC LIMIT 1)";
            using (var stmt = Prepare(sql))
            {
                BindText(stmt, 1, shutdownTime);
                Step(stmt);
            }
        }

        public void UpdateShutdownTime(long id, string shutdownTime)
        {
            using (var stmt = Prepare("UPDATE records SET shutdown_time = @t WHERE id = @id"))
            {
                BindText(stmt, 1, shutdownTime);
                sqlite3_bind_int64(stmt.Handle, 2, id);
                Step(stmt);
            }
        }

        public List<BootRecord> GetRecords(string dateFrom, string dateTo, string userName)
        {
            var sql = "SELECT id, user_name, reason, approver, boot_time, shutdown_time, created_at FROM records WHERE 1=1";
            var paramIdx = 0;
            if (!string.IsNullOrEmpty(dateFrom)) { sql += " AND date(boot_time) >= @p" + (++paramIdx); }
            if (!string.IsNullOrEmpty(dateTo)) { sql += " AND date(boot_time) <= @p" + (++paramIdx); }
            if (!string.IsNullOrEmpty(userName)) { sql += " AND user_name LIKE @p" + (++paramIdx); }
            sql += " ORDER BY boot_time ASC";

            using (var stmt = Prepare(sql))
            {
                paramIdx = 0;
                if (!string.IsNullOrEmpty(dateFrom)) BindText(stmt, ++paramIdx, dateFrom);
                if (!string.IsNullOrEmpty(dateTo)) BindText(stmt, ++paramIdx, dateTo);
                if (!string.IsNullOrEmpty(userName)) BindText(stmt, ++paramIdx, "%" + userName + "%");

                var result = new List<BootRecord>();
                while (Step(stmt))
                {
                    result.Add(new BootRecord
                    {
                        Id = ColumnInt64(stmt, 0),
                        UserName = ColumnText(stmt, 1),
                        Reason = ColumnText(stmt, 2),
                        Approver = ColumnText(stmt, 3),
                        BootTime = ColumnText(stmt, 4),
                        ShutdownTime = ColumnText(stmt, 5),
                        CreatedAt = ColumnText(stmt, 6),
                    });
                }
                return result;
            }
        }

        public List<DayStats> GetStatsByDay(string dateFrom, string dateTo)
        {
            var result = new List<DayStats>();
            var sql = "SELECT date(boot_time) as day, COUNT(*) as cnt FROM records " +
                      "WHERE date(boot_time) >= @p1 AND date(boot_time) <= @p2 " +
                      "GROUP BY day ORDER BY day";
            using (var stmt = Prepare(sql))
            {
                BindText(stmt, 1, dateFrom);
                BindText(stmt, 2, dateTo);
                while (Step(stmt))
                    result.Add(new DayStats { Day = ColumnText(stmt, 0), Count = (int)ColumnInt64(stmt, 1) });
            }
            return result;
        }

        public List<UserStats> GetStatsByUser(string dateFrom, string dateTo)
        {
            var result = new List<UserStats>();
            var sql = "SELECT user_name, COUNT(*) as cnt FROM records " +
                      "WHERE date(boot_time) >= @p1 AND date(boot_time) <= @p2 " +
                      "GROUP BY user_name ORDER BY cnt DESC";
            using (var stmt = Prepare(sql))
            {
                BindText(stmt, 1, dateFrom);
                BindText(stmt, 2, dateTo);
                while (Step(stmt))
                    result.Add(new UserStats { UserName = ColumnText(stmt, 0), Count = (int)ColumnInt64(stmt, 1) });
            }
            return result;
        }

        public List<string> GetAllUsers()
        {
            var result = new List<string>();
            using (var stmt = Prepare("SELECT DISTINCT user_name FROM records ORDER BY user_name"))
                while (Step(stmt)) result.Add(ColumnText(stmt, 0));
            return result;
        }

        // ── Low-level helpers ─────────────────────────────

        private void Exec(string sql)
        {
            using (var stmt = Prepare(sql)) { Step(stmt); }
        }

        private class StmtHandle : IDisposable
        {
            public IntPtr Handle;
            public void Dispose()
            {
                if (Handle != IntPtr.Zero) { sqlite3_finalize(Handle); Handle = IntPtr.Zero; }
            }
        }

        private StmtHandle Prepare(string sql)
        {
            IntPtr stmt;
            IntPtr tail;
            if (sqlite3_prepare_v2(_db, ToUtf8(sql), -1, out stmt, out tail) != SQLITE_OK)
                throw new Exception("Prepare failed: " + GetError());
            return new StmtHandle { Handle = stmt };
        }

        private static void BindText(StmtHandle stmt, int index, string value)
        {
            sqlite3_bind_text(stmt.Handle, index, ToUtf8(value), -1, IntPtr.Zero);
        }

        private static bool Step(StmtHandle stmt)
        {
            int rc = sqlite3_step(stmt.Handle);
            if (rc == SQLITE_ROW) return true;
            if (rc == SQLITE_DONE) return false;
            throw new Exception("Step error");
        }

        private static string ColumnText(StmtHandle stmt, int col)
        {
            IntPtr ptr = sqlite3_column_text(stmt.Handle, col);
            if (ptr == IntPtr.Zero) return "";
            int len = sqlite3_column_bytes(stmt.Handle, col);
            byte[] bytes = new byte[len];
            Marshal.Copy(ptr, bytes, 0, len);
            return Encoding.UTF8.GetString(bytes);
        }

        private static long ColumnInt64(StmtHandle stmt, int col)
        {
            return sqlite3_column_int64(stmt.Handle, col);
        }

        private string GetError()
        {
            IntPtr ptr = sqlite3_errmsg(_db);
            if (ptr == IntPtr.Zero) return "Unknown error";
            return Marshal.PtrToStringAnsi(ptr) ?? "Unknown error";
        }

        private static byte[] ToUtf8(string s)
        {
            return Encoding.UTF8.GetBytes((s ?? "") + "\0");
        }

        public void Dispose()
        {
            if (_db != IntPtr.Zero) { sqlite3_close(_db); _db = IntPtr.Zero; }
        }
    }
}
