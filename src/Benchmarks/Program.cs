using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Program
    {
        public const int TreeDepth = 5;
        public static TreeNode TreeNodeRoot;
        public static TreeNodeStruct TreeNodeStructRoot;

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TreeNodeBenchmark>();
        }
    }

    public class TreeNodeBenchmark
    {
        public const int TreeDepth = 5;
        public TreeNode TreeNodeRoot;
        public TreeNodeStruct TreeNodeStructRoot;

        public TreeNodeBenchmark()
        {
            TreeNodeRoot = TreeNode.CreateTree(TreeDepth);
            TreeNodeStructRoot = TreeNodeStruct.CreateTree(TreeDepth);
            if (TreeNodeRoot.CountNodes() != TreeNodeStructRoot.CountNodes())
                throw new InvalidOperationException("Node count doesn't match.");
        }

        [Benchmark]
        public void CreateTreeNodes()
        {
            TreeNode.CreateTree(TreeDepth);
        }

        [Benchmark]
        public void CreateTreeNodeStructs()
        {
            TreeNodeStruct.CreateTree(TreeDepth);
        }

        [Benchmark]
        public void CountTreeNodes()
        {
            TreeNodeRoot.CountNodes();
        }

        [Benchmark]
        public void CountTreeNodeStructs()
        {
            TreeNodeStructRoot.CountNodes();
        }
    }

    public class TreeNode
    {
        public TreeNode Left, Right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNode(TreeNode left, TreeNode right)
        {
            Left = left;
            Right = right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeNode CreateTree(int depth)
        {
            return depth < 0
                ? null
                : new TreeNode(CreateTree(depth - 1), CreateTree(depth - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountNodes()
        {
            if (ReferenceEquals(Left, null))
                return 1;
            return 1 + Left.CountNodes() + Right.CountNodes();
        }
    }

    public struct TreeNodeStruct
    {
        public sealed class NodeData
        {
            public TreeNodeStruct Left, Right;

            public NodeData(TreeNodeStruct left, TreeNodeStruct right)
            {
                Left = left;
                Right = right;
            }
        }

        public NodeData Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeNodeStruct(TreeNodeStruct left, TreeNodeStruct right)
        {
            Data = new NodeData(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TreeNodeStruct CreateTree(int depth)
        {
            return depth <= 0
                ? default(TreeNodeStruct)
                : new TreeNodeStruct(CreateTree(depth - 1), CreateTree(depth - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountNodes()
        {
            if (ReferenceEquals(Data, null))
                return 1;
            return 1 + Data.Left.CountNodes() + Data.Right.CountNodes();
        }
    }
}

