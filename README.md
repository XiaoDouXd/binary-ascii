# 编码一些数据到 ascii

```csharp
var c = "一些非 ascii 字符串";
var encoded /* byte[] */ = AscBin.EncodeReadable(c);
Console.WriteLine(c); // 一些非 ascii 字符串
Console.WriteLine(encoded.Select(v => (char)v).ToArray()); // ```IH#.J#ob%` ascii ```{o#Jn$}H#
Console.WriteLine(AscBin.DecodeReadable(encoded)); // 一些非 ascii 字符串
```

提供自定义码表的接口:

```csharp
    /// <summary>
    /// 可读性更高的字符串编码方式
    /// </summary>
    /// <param name="data"> 字符串 </param>
    /// <returns> 编码结果 </returns>
    public static byte[] EncodeReadable(in ReadOnlySpan<char> data) { /* ... */}

    /// <summary>
    /// 编码数据
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 编码结果 </returns>
    public static byte[] Encode(in ReadOnlySpan<ulong> data) { /* ... */}

    /// <summary>
    /// 对可读性更高的编码方式进行解码
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 内容文本 </returns>
    public static string DecodeReadable(in ReadOnlySpan<byte> data) { /* ... */}

    /// <summary>
    /// 解码数据
    /// </summary>
    /// <param name="data"> 数据 </param>
    /// <returns> 内容文本 </returns>
    public static ulong[] Decode(in ReadOnlySpan<byte> data) { /* ... */}

    /// <summary>
    /// 设置码表字符集
    /// </summary>
    /// <param name="charset"> 字符集 </param>
    /// <param name="codeLenghtChar"> 用于标识码长的字符 </param>
    /// <param name="maxCodeLength"> 最大码长 </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void SetCharset(string charset, char codeLenghtChar, byte maxCodeLength = (byte)DefaultMaxCodeLength) { /* ... */}
```

