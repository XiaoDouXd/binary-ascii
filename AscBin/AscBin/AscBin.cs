using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable once CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AsciiBinary;

/// <summary>
/// 上下文有关的编码, 将任意数据编码到指定的码表字符集范围中.
/// 码表字符集只能使用 ascii 字符.
/// </summary>
public static class AscBin
{
    public const sbyte DefaultMaxCodeLength = 9;
    public const char DefaultCodeLenghtKeyChar = '`';
    public const string DefaultIgnoreCharSet = @"""'\`";
    public const string DefaultCharSet = " !#$%&()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}~";

    public static ulong MaxCode { get; private set; }
    public static sbyte MaxCodeLength { get; private set; } = DefaultMaxCodeLength;
    public static uint CodeRadix => (uint)CharCodeMap.Count;

    public static IReadOnlyDictionary<byte, byte> CharToCode => CharCodeMap;
    public static IReadOnlyDictionary<byte, byte> CodeToChar => CodeCharMap;

    /// <summary>
    /// 可读性更高的字符串编码方式
    /// </summary>
    /// <param name="data"> 字符串 </param>
    /// <returns> 编码结果 </returns>
    public static byte[] EncodeReadable(in ReadOnlySpan<char> data)
    {
        Span<ulong> lData = stackalloc ulong[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var c = data[i];
            if (c < byte.MaxValue)
            {
                if (CharCodeMap.TryGetValue((byte)c, out var code)) lData[i] = code;
                else lData[i] = c + CodeRadix;
            }
            else lData[i] = c + CodeRadix;
        }
        return Encode(lData);
    }

    /// <summary>
    /// 可读性更高的字符串编码方式
    /// </summary>
    /// <param name="data"> 字符串 </param>
    /// <returns> 编码结果 </returns>
    public static byte[] EncodeReadable(in ReadOnlySpan<ulong> data)
    {
        Span<ulong> lData = stackalloc ulong[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var c = data[i];
            if (c < byte.MaxValue)
            {
                if (CharCodeMap.TryGetValue((byte)c, out var code)) lData[i] = code;
                else lData[i] = c + CodeRadix;
            }
            else lData[i] = c + CodeRadix;
        }
        return Encode(lData);
    }

    /// <summary>
    /// 编码数据
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 编码结果 </returns>
    public static byte[] Encode(in ReadOnlySpan<ulong> data)
    {
        var content = new List<byte>();
        Span<byte> unitData = stackalloc byte[MaxCodeLength + 1];

        var curCodeSize = 1;
        for (var i = 0; i < data.Length; i++)
        {
            var c = data[i];
            var cSize = GetUnitDataAndSize(c, ref unitData);

            // 检查并更新码长
            {
                // 单元码长可以小于当前码长
                // 检查一下是否要减短当前码长
                if (cSize < curCodeSize)
                {
                    // 缩减码长的规则:
                    // 未来 n 个单元码长都小于当前码长, n > (目标码长 + 当前码长)
                    // 或
                    // 未来 n 个单元码长与目标码长相同 且 未来 n+1 个单元码长大于当前码长 (n > 1)
                    var isChangeSize = false;
                    var sizeCb = curCodeSize + cSize;
                    for (int j = i, n = 0; j < data.Length; j++, n++)
                    {
                        var futureSize = GetUnitSize(data[j]);
                        if (futureSize > curCodeSize && n > 1)
                        {
                            isChangeSize = true;
                            break;
                        }

                        if (n > sizeCb)
                        {
                            isChangeSize = true;
                            break;
                        }

                        if (futureSize == curCodeSize) break;
                        if (j == data.Length - 1) isChangeSize = true;
                    }

                    if (isChangeSize)
                    {
                        curCodeSize = cSize;
                        for (var j = 0; j < curCodeSize; j++) content.Add((byte)_codeLenghtChar);
                    }
                }
                // 单元码长大于当前码长, 扩充一下
                else if (cSize > curCodeSize)
                {
                    curCodeSize = cSize;
                    for (var j = 0; j < curCodeSize; j++) content.Add((byte)_codeLenghtChar);
                }
            }
            content.AddRange(unitData[..curCodeSize]);
        }
        return content.ToArray();
    }

    /// <summary>
    /// 对可读性更高的编码方式进行解码
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 内容文本 </returns>
    public static string DecodeReadable(in ReadOnlySpan<byte> data)
    {
        var content = new StringBuilder();
        var isCalcCodeSize = false;
        var curCodeSize = 1;
        for (var i = 0; i < data.Length;)
        {
            var u = data[i];
            if (u == _codeLenghtChar)
            {
                if (!isCalcCodeSize) curCodeSize = 0;
                curCodeSize++;
                isCalcCodeSize = true;
                i += 1;
                continue;
            }
            isCalcCodeSize = false;

            if (curCodeSize + i > data.Length) break;
            var unit = GetUnit(data.Slice(i, curCodeSize));
            if (unit < CodeRadix) content.Append((CodeCharMap.TryGetValue((byte)unit, out var v) ? (char)v : '\0'));
            else content.Append((char)(unit - CodeRadix));
            i += curCodeSize;
        }
        return content.ToString();
    }

    /// <summary>
    /// 解码数据
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 内容文本 </returns>
    public static ulong[] Decode(in ReadOnlySpan<byte> data)
    {
        var content = new List<ulong>();
        var isCalcCodeSize = false;
        var curCodeSize = 1;
        for (var i = 0; i < data.Length;)
        {
            var u = data[i];
            if (u == _codeLenghtChar)
            {
                if (!isCalcCodeSize) curCodeSize = 0;
                curCodeSize++;
                isCalcCodeSize = true;
                i += 1;
                continue;
            }
            isCalcCodeSize = false;

            if (curCodeSize + i > data.Length) break;
            content.Add(GetUnit(data.Slice(i, curCodeSize)));
            i += curCodeSize;
        }
        return content.ToArray();
    }

    /// <summary>
    /// 设置码表字符集
    /// </summary>
    /// <param name="charset"> 字符集 </param>
    /// <param name="codeLenghtChar"> 用于标识码长的字符 </param>
    /// <param name="maxCodeLength"> 最大码长 </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void SetCharset(string charset, char codeLenghtChar, byte maxCodeLength = (byte)DefaultMaxCodeLength)
    {
        if (string.IsNullOrEmpty(charset)) throw new ArgumentNullException(nameof(charset));
        if (maxCodeLength > sbyte.MaxValue) throw new ArgumentOutOfRangeException(nameof(maxCodeLength));
        CharCodeMap.Clear();
        CodeCharMap.Clear();
        for (var i = 0; i < charset.Length; i++)
        {
            var c = charset[i];
            if (c > 128)
            {
                Reset();
                throw new ArgumentException($"Invalid charset: {charset}, {c}", nameof(charset));
            }
            CodeCharMap.Add((byte)i, (byte)c);
            CharCodeMap.Add((byte)c, (byte)i);
        }
        MaxCodeLength = (sbyte)maxCodeLength;
        MaxCode = Pow((uint)CharCodeMap.Count, MaxCodeLength);

        if (codeLenghtChar > 128 || CharCodeMap.ContainsKey((byte)codeLenghtChar))
        {
            Reset();
            throw new ArgumentException($"Invalid code lenght char: {codeLenghtChar}, bytecode: {(int)codeLenghtChar}",
                nameof(codeLenghtChar));
        }
        _codeLenghtChar = codeLenghtChar;
    }

    #region Calc
    public static sbyte GetUnitSize(ulong unit)
    {
        sbyte ret = 0;
        var codeCount = (uint)CharCodeMap.Count;
        var t = (ulong)codeCount;
        while (t <= unit)
        {
            t *= codeCount;
            ret += 1;
            if (ret > MaxCodeLength) throw new OverflowException("Unit is too big");
        }
        return (sbyte)(ret + 1);
    }

    public static sbyte GetUnitDataAndSize(ulong unit, ref Span<byte> dst)
    {
        dst.Fill(CodeCharMap[0]);
        var size = GetUnitSize(unit);

        var lUnit = unit;
        var codeCount = (uint)CharCodeMap.Count;
        for (var i = (sbyte)(size - 1); i >= 0; i--)
        {
            var p = Pow(codeCount, i);
            var v = lUnit / p;
            lUnit -= v * p;
            dst[i] = CodeCharMap[(byte)v];
        }
        return size;
    }

    public static ulong GetUnit(in ReadOnlySpan<byte> src)
    {
        if (src.IsEmpty) return 0;
        if (src.Length > MaxCodeLength) throw new OverflowException("Unit is too big");
        ulong ret = 0;
        ulong pow = 1;
        var codeCount = (ulong)CharCodeMap.Count;
        foreach (var t in src)
        {
            ret += pow * (CharCodeMap.TryGetValue(t, out var v) ? v : 0LU);
            pow *= codeCount;
        }
        return ret;
    }

    public static ulong Pow(ulong x, sbyte pow)
    {
        var ret = 1LU;
        while (pow != 0)
        {
            if ((pow & 1) == 1) ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }
    #endregion

    #region
    static AscBin() => Reset();
    private static void Reset()
    {
        _codeLenghtChar = DefaultCodeLenghtKeyChar;
        for (var i = 0; i < DefaultCharSet.Length; i++)
        {
            var c = DefaultCharSet[i];
            CodeCharMap.Add((byte)i, (byte)c);
            CharCodeMap.Add((byte)c, (byte)i);
        }
        MaxCodeLength = DefaultMaxCodeLength;
        MaxCode = Pow((uint)CharCodeMap.Count, MaxCodeLength);
    }

    private static char _codeLenghtChar = DefaultCodeLenghtKeyChar;
    private static readonly Dictionary<byte, byte> CodeCharMap = new ();
    private static readonly Dictionary<byte, byte> CharCodeMap = new();
    #endregion
}