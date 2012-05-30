using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class HunkRangeTests
    {
        [Test]
        public void HunkRange_HunkOriginalFile_ExpectHunkOriginalFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"41,0");

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(40);
            hunkRange.NumberOfLines.ShouldBe(0);
        }

        [Test]
        public void HunkRange_ValidHunk_ExpectHunkNewFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"42,20");

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(41);
            hunkRange.NumberOfLines.ShouldBe(20);
        }
    }

    // ReSharper restore InconsistentNaming 
}
