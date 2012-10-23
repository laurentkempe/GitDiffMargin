#region using

using System.Collections.Generic;
using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

#endregion

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class HunkRangeInfoTests
    {
        [Test]
        public void IsAddition_AllDiffLinesWithStartsWithPlusSign_ExpectedTrue()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "+ " }.ToArray());

            //Act
            bool isAddition = hunkRangeInfo.IsAddition;

            //Assert
            isAddition.ShouldBe(true);
        }

        [Test]
        public void IsAddition_NotAllDiffLinesStartsWithPlusSign_ExpectedFalse()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "- " }.ToArray());

            //Act
            bool isAddition = hunkRangeInfo.IsAddition;

            //Assert
            isAddition.ShouldBe(false);
        }

        [Test]
        public void IsModification_DiffLinesStartsWithPlusSignAndWithMinus_ExpectedTrue()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "- " }.ToArray());

            //Act
            bool isModification = hunkRangeInfo.IsModification;

            //Assert
            isModification.ShouldBe(true);
        }

        [Test]
        public void OriginalText_1NewLineAnd1OriginalLine_ExpectedOriginalText()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+New Text", "-Original Text" }.ToArray());

            //Act
            string originalText = hunkRangeInfo.OriginalText[0];

            //Assert
            originalText.ShouldBe("Original Text");
        }

        [Test]
        public void OriginalText_1NewLineAnd1OriginalLineWithLeadingSpaces_ExpectedOriginalText()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ New Text", "-    Original Text" }.ToArray());

            //Act
            string originalText = hunkRangeInfo.OriginalText[0];

            //Assert
            originalText.ShouldBe("    Original Text");
        }

        [Test]
        public void OriginalText_1NewLineAnd1OriginalLineWithLeadingSpacesAndInvertedOrder_ExpectedOriginalText()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-18"), new HunkRange("+18"), new List<string> { "-            it++; // this is just a comment", "+            it--;" }.ToArray());

            //Act
            var originalText = hunkRangeInfo.OriginalText;

            //Assert
            originalText[0].ShouldBe("            it++; // this is just a comment");
        }

        [Test]
        public void IsDeletion_3DeletedLines_ExpectTrue()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-7,3"), new HunkRange("+6,0"), new List<string> { "-using Microsoft.VisualStudio.Shell;", "-using Microsoft.VisualStudio.Text;", "-using Microsoft.VisualStudio.Text.Editor;" }.ToArray());

            //Act
            var isDeletion = hunkRangeInfo.IsDeletion;

            //Assert
            isDeletion.ShouldBe(true);
        }
    }

    // ReSharper restore InconsistentNaming 
}