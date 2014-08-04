using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace GitDiffMargin.Git
{
    public static class Extensions
    {
        internal static IEnumerable<Document> AllDocuments(this Documents documents)
        {
            return documents.Cast<Document>().Where(x => !x.Path.StartsWith("vstfs://", StringComparison.InvariantCultureIgnoreCase));
        } 
 
    }
}