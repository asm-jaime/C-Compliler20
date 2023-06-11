using NUnit.Framework;
using OnlineCompiler.Client.Pages;
using OnlineCompiler.Server.Handlers;

namespace test;

[TestFixture]
public class CheckSortedListTests
{
    private static string _invalidSortedListCode = @"";
    private static string _validSortedListCode = TemplateSortedList.SortedListCode;

    [Test]
    public void TestSortedList()
    {
        var keyValue = new KeyValuePair<string, string>("2", "1");
        //Assert.AreEqual(CodeCompileChecker<int>.CheckSortedList(_validSortedListCode, 8), true);
        Assert.AreEqual(CodeCompileChecker<KeyValuePair<string, string>>.CheckSortedList(_validSortedListCode, keyValue), true);
    }
}
