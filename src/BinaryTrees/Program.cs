﻿/* The Computer Language Benchmarks Game
   http://benchmarksgame.alioth.debian.org/

   contributed by Marek Safar
   *reset*
   concurrency added by Peperud
   minor improvements by Alex Yakunin
*/

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class Program
{
    public const int MinDepth = 4;

    public static void Main(string[] args)
    {
        var n = args.Length == 0 ? 0 : int.Parse(args[0]);
        var maxDepth = n < (MinDepth + 2) ? MinDepth + 2 : n;
        var stretchDepth = maxDepth + 1;

        var stretchDepthTask = Task.Run(() => TreeNode.CreateTree(stretchDepth).CountNodes());
        var maxDepthTask = Task.Run(() => TreeNode.CreateTree(maxDepth).CountNodes());

        var tasks = new Task<string>[(maxDepth - MinDepth) / 2 + 1];
        for (int depth = MinDepth, ti = 0; depth <= maxDepth; depth += 2, ti++) {
            var iterationCount = 1 << (maxDepth - depth + MinDepth);
            var depthCopy = depth; // To make sure closure value doesn't change
            tasks[ti] = Task.Run(() => {
                var count = 0;
                if (depthCopy >= 17) {
                    // Parallelized computation for relatively large tasks
                    var miniTasks = new Task<int>[iterationCount];
                    for (var i = 0; i < iterationCount; i++)
                        miniTasks[i] = Task.Run(() => TreeNode.CreateTree(depthCopy).CountNodes());
                    Task.WaitAll(miniTasks);
                    for (var i = 0; i < iterationCount; i++)
                        count += miniTasks[i].Result;
                }
                else {
                    // Sequential computation for smaller tasks
                    for (var i = 0; i < iterationCount; i++)
                        count += TreeNode.CreateTree(depthCopy).CountNodes();
                }
                return $"{iterationCount}\t trees of depth {depthCopy}\t check: {count}";
            });
        }
        Task.WaitAll(tasks);

        Console.WriteLine("stretch tree of depth {0}\t check: {1}",
            stretchDepth, stretchDepthTask.Result);
        foreach (var task in tasks)
            Console.WriteLine(task.Result);
        Console.WriteLine("long lived tree of depth {0}\t check: {1}",
            maxDepth, maxDepthTask.Result);
    }
}

public struct TreeNode
{
    public sealed class NodeData
    {
        public TreeNode Left, Right;

        public NodeData(TreeNode left, TreeNode right)
        {
            Left = left;
            Right = right;
        }
    }

    public NodeData Data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TreeNode(TreeNode left, TreeNode right)
    {
        Data = new NodeData(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TreeNode CreateTree(int depth)
    {
        return depth <= 0
            ? default(TreeNode)
            : new TreeNode(CreateTree(depth - 1), CreateTree(depth - 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CountNodes()
    {
        if (ReferenceEquals(Data, null))
            return 1;
        return 1 + Data.Left.CountNodes() + Data.Right.CountNodes();
    }
}
