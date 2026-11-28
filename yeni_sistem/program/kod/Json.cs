// YENİ SİSTEM · küçük JSON okuyucu (PowerShell 5.1 / .NET 4.8'de hazır okuyucu yok; ConvertFrom-Json binlerce dosyada çok yavaş).
// Sonuç: Dictionary<string, object>, List<object>, string, double, bool ya da null. C# 5.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace YeniSistem
{
    public static class Json
    {
        public static object Oku(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            int i = 0;
            try { Bosluk(s, ref i); object v = Deger(s, ref i); return v; } catch { return null; }
        }

        static void Bosluk(string s, ref int i) { while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n' || s[i] == '\uFEFF')) i++; }

        static object Deger(string s, ref int i)
        {
            Bosluk(s, ref i);
            if (i >= s.Length) throw new FormatException();
            char c = s[i];
            if (c == '{') return Nesne(s, ref i);
            if (c == '[') return Dizi(s, ref i);
            if (c == '"') return Metin(s, ref i);
            if (c == 't' && string.CompareOrdinal(s, i, "true", 0, 4) == 0) { i += 4; return true; }
            if (c == 'f' && string.CompareOrdinal(s, i, "false", 0, 5) == 0) { i += 5; return false; }
            if (c == 'n' && string.CompareOrdinal(s, i, "null", 0, 4) == 0) { i += 4; return null; }
            int b = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            return double.Parse(s.Substring(b, i - b), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        static Dictionary<string, object> Nesne(string s, ref int i)
        {
            Dictionary<string, object> d = new Dictionary<string, object>(); i++;
            Bosluk(s, ref i);
            if (s[i] == '}') { i++; return d; }
            while (true)
            {
                Bosluk(s, ref i); string k = Metin(s, ref i); Bosluk(s, ref i);
                if (s[i] != ':') throw new FormatException(); i++;
                d[k] = Deger(s, ref i); Bosluk(s, ref i);
                if (s[i] == ',') { i++; continue; }
                if (s[i] == '}') { i++; return d; }
                throw new FormatException();
            }
        }

        static List<object> Dizi(string s, ref int i)
        {
            List<object> l = new List<object>(); i++;
            Bosluk(s, ref i);
            if (s[i] == ']') { i++; return l; }
            while (true)
            {
                l.Add(Deger(s, ref i)); Bosluk(s, ref i);
                if (s[i] == ',') { i++; continue; }
                if (s[i] == ']') { i++; return l; }
                throw new FormatException();
            }
        }

        static string Metin(string s, ref int i)
        {
            if (s[i] != '"') throw new FormatException(); i++;
            StringBuilder sb = new StringBuilder();
            while (s[i] != '"')
            {
                char c = s[i++];
                if (c != '\\') { sb.Append(c); continue; }
                char e = s[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber)); i += 4; break;
                    default: sb.Append(e); break;
                }
            }
            i++;
            return sb.ToString();
        }

        // yardımcılar: güvenli erişim
        public static object Al(object o, string k) { Dictionary<string, object> d = o as Dictionary<string, object>; object v; return d != null && d.TryGetValue(k, out v) ? v : null; }
        public static List<object> Liste(object o, string k) { return Al(o, k) as List<object> ?? new List<object>(); }
        public static string Yazi(object o, string k) { object v = Al(o, k); if (v == null) return ""; if (v is double) return ((double)v).ToString("0.##########", CultureInfo.InvariantCulture); return v.ToString(); }
        public static double Sayi(object o, string k) { object v = Al(o, k); return v is double ? (double)v : double.NaN; }
    }
}
