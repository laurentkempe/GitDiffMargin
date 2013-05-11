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
        public void HunkRange_HunkOriginalFile_ExpectHunkOriginalFile(int contextLines)
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"41,0", contextLines);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(40);
            hunkRange.NumberOfLines.ShouldBe(0);
        }

        [Test]
        public void HunkRange_ValidHunk_ExpectHunkNewFile(int contextLines)
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"42,20", contextLines);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(41);
            hunkRange.NumberOfLines.ShouldBe(20);
        }

        [Test]
        public void NumberOfLines_HunkWithoutLineNumber_ExpectDefaultTo1LineNumber(int contextLines)
        {
            //Arrange
            var hunkRange = new HunkRange(@"-18", contextLines);

            //Act
            var numberOfLines = hunkRange.NumberOfLines;

            //Assert
            numberOfLines.ShouldBe(1);
        }

        [Test]
        public void StartingLineNumber_HunkWithoutLineNumber_ExpectLineNumber(int contextLines)
        {
            //Arrange
            var hunkRange = new HunkRange(@"18", contextLines);

            //Act
            var startingLineNumber = hunkRange.StartingLineNumber;

            //Assert
            startingLineNumber.ShouldBe(17);
        }
    }

    // ReSharper restore InconsistentNaming 
}
