using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Splat;

namespace PlaylistManager.UserControls
{
    /// <summary>
    /// A panel that acts like a stack
    /// The last child of the panel is the only visible child, the rest are hidden
    /// As the name suggests, useful for navigation (going forward and back)
    /// </summary>
    public class NavigationPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var lastChild = Children.Last();
            lastChild.Measure(availableSize);
            return lastChild.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (i != Children.Count - 1)
                {
                    Children[i].IsVisible = false;
                }
                else
                {
                    Children[i].IsVisible = true;
                    Children[i].Arrange(new Rect(new Point(0, 0), finalSize));
                }
            }
            return finalSize;
        }

        // Registered to splat with a contract name if name exists
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                Locator.CurrentMutable.RegisterConstant(this, typeof(NavigationPanel), Name.Replace(nameof(NavigationPanel), ""));
            }
        }

        /// <summary>
        /// Push a child view
        /// </summary>
        /// <param name="view">View to push</param>
        public void Push(Control? view)
        {
            if (view != null)
            {
                Children.Add(view);
            }
        } 
        
        /// <summary>
        /// Pop the topmost view
        /// </summary>
        public void Pop() => Children.RemoveAt(Children.Count - 1);
    }
}