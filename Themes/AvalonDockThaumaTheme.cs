using System;
using AvalonDock.Themes;

namespace ThaumaStudio.Themes
{
    /// <inheritdoc/>
    public class AvalonDockThaumaTheme : Theme
    {
        /// <inheritdoc/>
        public override Uri GetResourceUri()
        {
            return new Uri(
                "/Themes/AvalonDockThaumaTheme.xaml",
                UriKind.RelativeOrAbsolute);
        }
    }
}
