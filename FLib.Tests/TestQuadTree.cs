using FLib.Collections;

namespace FLib.Tests;

public class TestQuadTree
{
    [Fact]
    public void TotalObjectCountsRemainAccurateAfterSplitRemoveAndMerge()
    {
        var tree = new QuadTree<int>(new FRect(new FVector2(0, 0), new FVector2(100, 100)))
        {
            MaxDepthLimit = 1,
            MaxObjectGroup = 2,
            SplitNodeObjectNumber = 2
        };

        var objectIndexes = new[]
        {
            tree.Add(1, new FVector2(10, 10)),
            tree.Add(2, new FVector2(90, 10), 1),
            tree.Add(3, new FVector2(10, 90)),
            tree.Add(4, new FVector2(90, 90), 1)
        };

        Assert.True(tree.Root.HasChild);
        AssertTotalObjectCounts(tree, 0);

        Assert.True(tree.Remove(objectIndexes[0]));
        AssertTotalObjectCounts(tree, 0);

        tree.Root.MergeChildren();

        Assert.False(tree.Root.HasChild);
        AssertTotalObjectCounts(tree, 0);
        Assert.Equal([1, 2], tree.Root.TotalObjCounts);
        Assert.All(objectIndexes[1..], objectIndex => Assert.Equal(0, tree.GetObj(objectIndex).NodeId));
    }

    [Fact]
    public void RefreshPositionReaddsFromRootAfterMerge()
    {
        var tree = new QuadTree<int>(new FRect(new FVector2(0, 0), new FVector2(100, 100)))
        {
            MaxDepthLimit = 2,
            SplitNodeObjectNumber = 2
        };

        var movedObjectIndex = tree.Add(1, new FVector2(10, 10));
        tree.Add(2, new FVector2(30, 10));

        tree.RefreshPosition(movedObjectIndex, new FVector2(30, 10));

        ref readonly var obj = ref tree.GetObj(movedObjectIndex);
        ref readonly var node = ref tree.GetObjNode(movedObjectIndex);
        Assert.False(tree.Nodes.NodeBuffer[obj.NodeId].IsFreed);
        Assert.Contains(movedObjectIndex, node.ObjIndexes[0]);
    }

    [Fact]
    public void LongRectSplitsAlongLongAxis()
    {
        var tree = new QuadTree<int>(new FRect(new FVector2(0, 0), new FVector2(2000, 15)))
        {
            MaxDepthLimit = 1,
            SplitNodeObjectNumber = 1
        };

        tree.Add(1, new FVector2(100, 7));

        Assert.True(tree.Root.HasChild);
        Assert.Equal(4, tree.Root.Children.Length);
        Assert.All(tree.Root.Children, childIndex =>
        {
            ref readonly var child = ref tree.GetNode(childIndex);
            Assert.True(child.Rect.Height == 15);
            Assert.True(child.Rect.Width == 500);
        });
    }

    private static int[] AssertTotalObjectCounts(QuadTree<int> tree, int nodeIndex)
    {
        ref var node = ref tree.GetNode(nodeIndex);
        var actualCounts = new int[tree.MaxObjectGroup];
        if (node.HasChild)
        {
            foreach (var childIndex in node.Children)
            {
                var childCounts = AssertTotalObjectCounts(tree, childIndex);
                for (var group = 0; group < actualCounts.Length; group++)
                    actualCounts[group] += childCounts[group];
            }
        }
        else if (node.ObjIndexes != null)
        {
            for (var group = 0; group < actualCounts.Length; group++)
                actualCounts[group] = node.ObjIndexes[group].Count;
        }

        Assert.Equal(actualCounts, node.TotalObjCounts ?? new int[tree.MaxObjectGroup]);
        return actualCounts;
    }
}
