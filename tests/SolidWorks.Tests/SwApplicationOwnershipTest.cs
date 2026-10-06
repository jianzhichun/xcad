using Moq;
using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;

namespace SolidWorks.Tests
{
    public class SwApplicationOwnershipTest
    {
        [Test]
        public void DetachDoesNotExitBorrowedApplication()
        {
            var native = new Mock<SldWorks>();
            var application = SwApplicationFactory.FromPointer(native.Object);

            application.Detach();
            application.Detach();

            native.Verify(app => app.ExitApp(), Times.Never);
            native.Verify(app => app.CloseAllDocuments(It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void HiddenStateCanBeRestoredToVisible()
        {
            var native = new Mock<SldWorks>();
            native.SetupProperty(app => app.Visible, true);
            var application = SwApplicationFactory.FromPointer(native.Object);

            application.State = ApplicationState_e.Hidden;
            Assert.AreEqual(ApplicationState_e.Hidden, application.State);
            application.State = ApplicationState_e.Default;
            Assert.IsTrue(native.Object.Visible);
            application.Detach();
        }
    }
}
