using System;
using System.Threading.Tasks;

public static class AltProgram
{
    const int MinDepth = 4;

    public static void Main1(string[] args)
    {
        int n = 0;
        if (args.Length > 0) n = int.Parse(args[0]);

        int maxDepth = n < (MinDepth + 2) ? MinDepth + 2 : n;
        int stretchDepth = maxDepth + 1;

        var tcheck = new[]
        {
            Task.Run(() => TreeNode.CreateTree(stretchDepth).CountNodes()),
            Task.Run(() => TreeNode.CreateTree(maxDepth).CountNodes())
        };

        var results = new Task<string>[(maxDepth - MinDepth) / 2 + 1];

        for (int depth = MinDepth; depth <= maxDepth; depth += 2)
        {
            int iterations = 1 << (maxDepth - depth + MinDepth);

            int check = 0, safeDept = depth;

            results[(safeDept - MinDepth) / 2] = Task.Run(() =>
            {
                //for (var i = 1; i <= iterations; i++)
                //{
                //    check += (TreeNode.BottomUpTree(safeDept)).CountNodes();
                //}

                //
                // This is a somewhat silly, however overparallelizing towards
                // the end gave me a little better numbers on average
                //
                int i = 1;
                while (i <= iterations)
                {
                    if (safeDept > 18)
                    {
                        var split = new[]
                        {
                            Task.Run(() => (TreeNode.CreateTree(safeDept)).CountNodes()),
                            Task.Run(() => (TreeNode.CreateTree(safeDept)).CountNodes())
                        };

                        i += 2;
                        Task.WaitAll(split);
                        check += split[0].Result + split[1].Result;
                    }
                    else
                    {
                        check += (TreeNode.CreateTree(safeDept)).CountNodes();
                        i++;
                    }
                }

                return $"{iterations}\t trees of depth {safeDept}\t check: {check}";
            });
        }

        Console.WriteLine("stretch tree of depth {0}\t check: {1}",
            stretchDepth, tcheck[0].Result);
        foreach (var t in results)
            Console.WriteLine(t.Result);
        Console.WriteLine("long lived tree of depth {0}\t check: {1}",
            maxDepth, tcheck[1].Result);
    }
}
