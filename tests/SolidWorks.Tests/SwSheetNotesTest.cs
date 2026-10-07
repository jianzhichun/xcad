using Moq;
using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Linq;

namespace SolidWorks.Tests
{
    /// <summary>
    /// Mocked contracts for the solidworksz sheet-note enumeration patch.
    /// Covers the sheet pseudo-view selection, note filtering and explicit
    /// failure semantics via SheetNotesHelper; real COM behavior is qualified
    /// separately on the live VM.
    /// </summary>
    public class SwSheetNotesTest
    {
        private Mock<IView> m_SheetView;
        private Mock<IView> m_ModelView;
        private Mock<Annotation> m_NoteAnnotation;
        private Mock<Note> m_Note;

        [SetUp]
        public void SetUp()
        {
            m_SheetView = new Mock<IView>();
            m_ModelView = new Mock<IView>();
            m_NoteAnnotation = new Mock<Annotation>();
            m_Note = new Mock<Note>();

            m_SheetView.Setup(v => v.Name).Returns("Sheet1");
            m_ModelView.Setup(v => v.Name).Returns("Drawing View1");

            m_NoteAnnotation.Setup(a => a.GetType()).Returns(6); // swAnnotationType_e.swNote (verified from interop)
            m_NoteAnnotation.Setup(a => a.GetSpecificAnnotation()).Returns(m_Note.Object);
            m_NoteAnnotation.Setup(a => a.IGetNext2()).Returns((Annotation)null);
            m_SheetView.Setup(v => v.IGetFirstAnnotation2()).Returns(m_NoteAnnotation.Object);
            m_ModelView.Setup(v => v.IGetFirstAnnotation2()).Returns((Annotation)null);
        }

        private static object[] Views(params IView[] views) => new object[] { views.Cast<object>().ToArray() };

        private System.Collections.Generic.List<Xarial.XCad.SolidWorks.Annotations.ISwNote> Read(object[] sheets)
            => Xarial.XCad.SolidWorks.Documents.SheetNotesHelper.GetSheetNotes(sheets, "Sheet1",
                true, note => new Xarial.XCad.SolidWorks.Annotations.SwNote(
                    note, null, (Xarial.XCad.SolidWorks.SwApplication)Xarial.XCad.SolidWorks.SwApplicationFactory.FromPointer(new Mock<SldWorks>().Object))).ToList();

        [Test]
        public void NotesComeOnlyFromSheetPseudoView()
        {
            var notes = Read(Views(m_SheetView.Object, m_ModelView.Object));
            Assert.AreEqual(1, notes.Count);
            m_ModelView.Verify(v => v.IGetFirstAnnotation2(), Times.Never);
        }

        [Test]
        public void MissingPseudoViewFailsExplicitly()
        {
            m_SheetView.Setup(v => v.Name).Returns("Other");
            Assert.Throws<InvalidOperationException>(() => Read(Views(m_SheetView.Object, m_ModelView.Object)));
        }

        [Test]
        public void NonNoteAnnotationsAreSkipped()
        {
            var dimension = new Mock<Annotation>();
            dimension.Setup(a => a.GetType()).Returns(4); // swDisplayDimension
            dimension.Setup(a => a.IGetNext2()).Returns((Annotation)null);
            m_NoteAnnotation.SetupSequence(a => a.IGetNext2()).Returns(dimension.Object).Returns((Annotation)null);
            Assert.AreEqual(1, Read(Views(m_SheetView.Object)).Count);
        }

        [Test]
        public void UncommittedSheetIsRejected()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Xarial.XCad.SolidWorks.Documents.SheetNotesHelper.GetSheetNotes(Views(m_SheetView.Object),
                    "Sheet1", false, _ => null).ToList());
        }

        [Test]
        public void NullSpecificAnnotationIsRejected()
        {
            m_NoteAnnotation.Setup(a => a.GetSpecificAnnotation()).Returns((Note)null);
            Assert.Throws<InvalidOperationException>(() => Read(Views(m_SheetView.Object)));
        }
    }
}
