using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;
using RK.DockableWindows.WPF;

namespace UnitTest
{
    [TestClass]
    public class SplitContainerCollectionTest
    {
        /// <summary>
        /// Creates a new canvas element which is used for tests.
        /// </summary>
        /// <returns>A canvas instance with a dock panel as child.</returns>
        private DockCanvas PrepareCanvas()
        {
            // Create a new canvas element
            var canvas = new DockCanvas();

            // Add a dock panel as content element to the canvas
            var panel = new DockPanel();
            canvas.Child = panel;

            return canvas;
        }

        [TestMethod]
        public void TestIndex()
        {
            // Setup canvas and panel
            var canvas = PrepareCanvas();

            // Setup a split container collection and add a split container
            var collection = new SplitContainerCollection(canvas, canvas.Child) { new SplitContainer() };

            // Test that we can get the split container
            var container = collection[0];
            Assert.IsNotNull(container);
            Assert.IsInstanceOfType(container, typeof(SplitContainer));
        }
    }
}
