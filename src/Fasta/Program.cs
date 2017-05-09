using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Fasta
{
    const int LineLength = 60;
    const int IM = 139968;
    const int IA = 3877;
    const int IC = 29573;

    private static int _seed = 42;

    public static void Main(string[] args)
    {
        var n = args.Length > 0 ? int.Parse(args[0]) : 1000;

        MakeCumulative(IUB);
        MakeCumulative(HomoSapiens);

        using (var s = Console.OpenStandardOutput()) {
            MakeRepeatFasta("ONE", "Homo sapiens alu", Encoding.ASCII.GetBytes(ALU), n * 2, s);
            MakeRandomFasta("TWO", "IUB ambiguity codes", IUB, n * 3, s);
            MakeRandomFasta("THREE", "Homo sapiens frequency", HomoSapiens, n * 5, s);
        }
    }

    static void MakeRandomFasta(string id, string desc, Frequency[] a, int n, Stream s)
    {
        var descStr = Encoding.ASCII.GetBytes($">{id} {desc}\n");
        s.Write(descStr, 0, descStr.Length);

        var bufferSize = LineLength * 40;
        var pool = new BufferPool(bufferSize);
        var results = GetSplits(n, bufferSize)
            .AsParallel().AsOrdered()
            .Select(size => pool.Acquire(size))
            .AsEnumerable()
            .Select(buffer => {
                FillRandomly(buffer);
                return buffer;
            })
            .AsParallel().AsOrdered()
            .Select(rnd => new { Buffer = rnd, Nucleotides = SelectNucleotides(a, rnd)});
        foreach (var result in results) {
            var nucleotides = result.Nucleotides;
            s.Write(nucleotides, 0, nucleotides.Length);
            pool.Release(result.Buffer);
        }
    }

    static IEnumerable<int> GetSplits(int n, int s)
    {
        for (var i = 0; i < n; i += s)
            yield return n - i < s ? n - i : s;
    }

    private static unsafe byte[] SelectNucleotides(Frequency[] a, int[] rnd)
    {
        var resLength = (rnd.Length / LineLength) * (LineLength + 1);
        if (rnd.Length % LineLength != 0)
            resLength += rnd.Length % LineLength + 1;

        var result = new byte[resLength];
        fixed (int* pinnedRnd = &rnd[0])
        fixed (byte* pinnedResult = &result[0]) {
            var pRnd = pinnedRnd;
            var pResult = pinnedResult;
            for (var i = 0; i < rnd.Length; i += LineLength) {
                var pRndEnd = pRnd + Math.Min(LineLength, rnd.Length - i);
                for (; pRnd < pRndEnd; pRnd++, pResult++)
                    *pResult = SelectRandom(a, *pRnd);
                *(pResult++) = (byte) '\n';
            }
        }
        return result;
    }

    static void MakeRepeatFasta(string id, string desc, byte[] alu, int n, Stream s)
    {
        var descStr = Encoding.ASCII.GetBytes($">{id} {desc}\n");
        s.Write(descStr, 0, descStr.Length);

        var buffer = new byte[alu.Length * (LineLength + 1) + 1];
        int bufferSize;
        using (var ms = new MemoryStream(buffer)) {
            MakeRepeatFastaBuffer(alu, alu.Length * LineLength, ms);
            bufferSize = (int) ms.Length;
        }
        var leftToWrite = n + n / 60;
        while (leftToWrite >= bufferSize)
        {
            s.Write(buffer, 0, bufferSize);
            leftToWrite -= bufferSize;
        }
        if (leftToWrite <= 0)
            return;
        s.Write(buffer, 0, leftToWrite);
        s.WriteByte((byte)'\n');
    }

    static void MakeRepeatFastaBuffer(byte[] alu, int n, Stream s)
    {
        var aluIdx = 0;
        var aluLength = alu.Length;
        var buf = new byte[1024];
        var bufIdx = 0;
        for (; n > 0; n -= LineLength) {
            var m = n < LineLength ? n : LineLength;
            if (buf.Length - bufIdx < m) {
                s.Write(buf, 0, bufIdx);
                bufIdx = 0;
            }
            for (var i = 0; i < m; i++) {
                if (aluIdx == aluLength)
                    aluIdx = 0;
                buf[bufIdx++] = alu[aluIdx];
                aluIdx++;
            }
            buf[bufIdx++] = (byte)'\n';
        }

        if (bufIdx != 0)
            s.Write(buf, 0, bufIdx);
    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe byte SelectRandom(Frequency[] a, int r)
    {
        fixed (Frequency* pinnedA = &a[0]) {
            var pA = pinnedA;
            var pAEnd = pA + (a.Length - 1);
            for (; pA < pAEnd; pA++) {
                if (r < pA->P)
                    return pA->C;
            }
            return (*pA).C;
        }
    }

    static void MakeCumulative(Frequency[] a)
    {
        double cp = 0;
        for (var i = 0; i < a.Length; i++) {
            cp += a[i].P;
            a[i].P = cp;
        }
    }

    private static void FillRandomly(int[] buffer)
    {
        var s = _seed;
        for (var i = 0; i < buffer.Length; i++) {
            s = (s * IA + IC) % IM;
            buffer[i] = s;
        }
        _seed = s;
    }

    struct Frequency
    {
        public readonly byte C;
        public double P;

        public Frequency(char c, double p)
        {
            this.C = (byte) c;
            this.P = (p * IM);
        }
    }

    class BufferPool
    {
        private readonly int _bufferSize;
        private readonly Stack<int[]> _buffers = new Stack<int[]>();

        public BufferPool(int bufferSize, int bufferCount = 0)
        {
            this._bufferSize = bufferSize;
            for (var i = 0; i < bufferCount; i++)
                _buffers.Push(new int[bufferSize]);
        }

        public int[] Acquire(int bufferSize)
        {
            if (bufferSize != this._bufferSize)
                return new int[bufferSize];
            lock (_buffers) {
                if (_buffers.Count > 0)
                    return _buffers.Pop();
            }
            return new int[this._bufferSize];
        }

        public void Release(int[] buffer)
        {
            if (buffer.Length != _bufferSize)
                return;
            lock (_buffers)
                _buffers.Push(buffer);
        }
    }

    static string ALU =
        "GGCCGGGCGCGGTGGCTCACGCCTGTAATCCCAGCACTTTGG" +
        "GAGGCCGAGGCGGGCGGATCACCTGAGGTCAGGAGTTCGAGA" +
        "CCAGCCTGGCCAACATGGTGAAACCCCGTCTCTACTAAAAAT" +
        "ACAAAAATTAGCCGGGCGTGGTGGCGCGCGCCTGTAATCCCA" +
        "GCTACTCGGGAGGCTGAGGCAGGAGAATCGCTTGAACCCGGG" +
        "AGGCGGAGGTTGCAGTGAGCCGAGATCGCGCCACTGCACTCC" +
        "AGCCTGGGCGACAGAGCGAGACTCCGTCTCAAAAA";

    static Frequency[] IUB = {
        new Frequency ('a', 0.27)
        ,new Frequency ('c', 0.12)
        ,new Frequency ('g', 0.12)
        ,new Frequency ('t', 0.27)

        ,new Frequency ('B', 0.02)
        ,new Frequency ('D', 0.02)
        ,new Frequency ('H', 0.02)
        ,new Frequency ('K', 0.02)
        ,new Frequency ('M', 0.02)
        ,new Frequency ('N', 0.02)
        ,new Frequency ('R', 0.02)
        ,new Frequency ('S', 0.02)
        ,new Frequency ('V', 0.02)
        ,new Frequency ('W', 0.02)
        ,new Frequency ('Y', 0.02)
    };

    static Frequency[] HomoSapiens = {
        new Frequency ('a', 0.3029549426680)
        ,new Frequency ('c', 0.1979883004921)
        ,new Frequency ('g', 0.1975473066391)
        ,new Frequency ('t', 0.3015094502008)
    };
}
