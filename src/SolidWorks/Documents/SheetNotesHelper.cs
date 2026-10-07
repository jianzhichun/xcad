//*********************************************************************
//xCAD solidworksz fork patch
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using Xarial.XCad.SolidWorks.Annotations;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// Selects the sheet pseudo-view and enumerates its note annotations.
    /// Sheet-format and title-block notes live on this pseudo-view, which
    /// SwDrawingAnnotationCollection intentionally skips.
    /// </summary>
    internal static class SheetNotesHelper
    {
        private const int MaximumSheetViews = 10000;

        internal static IEnumerable<ISwNote> GetSheetNotes(object[] allSheetViews, string sheetName,
            bool isCommitted, Func<INote, ISwNote> noteFactory)
        {
            if (!isCommitted)
            {
                throw new InvalidOperationException("Sheet is not committed");
            }

            foreach (object[] sheetViews in allSheetViews ?? new object[0])
            {
                if (sheetViews == null || sheetViews.Length == 0)
                {
                    continue;
                }

                var first = (IView)sheetViews.GetValue(0);

                if (!string.Equals(first.Name, sheetName, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                if (sheetViews.Length - 1 > MaximumSheetViews)
                {
                    throw new InvalidOperationException("Sheet annotation count exceeds safety bound");
                }

                var ann = first.IGetFirstAnnotation2();

                while (ann != null)
                {
                    if ((swAnnotationType_e)ann.GetType() == swAnnotationType_e.swNote)
                    {
                        var note = ann.GetSpecificAnnotation() as INote;

                        if (note == null)
                        {
                            throw new InvalidOperationException("Note-specific annotation dispatch is null");
                        }

                        yield return noteFactory(note);
                    }

                    ann = ann.IGetNext2();
                }

                yield break;
            }

            throw new InvalidOperationException($"Failed to find the pseudo-view of sheet '{sheetName}'");
        }
    }
}
