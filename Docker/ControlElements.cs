namespace Docker
{
    /// <summary>
    /// Contains control element keys which are used for rendering styled control templates.
    /// </summary>
    public enum ControlElements
    {
        /// <summary>
        /// Background brush of the DockCanvas control.
        /// </summary>
        DockCanvasBackgroundBrush,
        DockControlBorderBrush,


        /// <summary>
        /// Style for the active title bar of a WindowGroup.
        /// </summary>
        ActiveDockedTitleBar,
        /// <summary>
        /// Style for the inactive title bar of a WindowGroup.
        /// </summary>
        InactiveDockedTitleBar,

        /// <summary>
        /// Style for a title bar button of a WindowGroup.
        /// </summary>
        TitleBarButton,
        /// <summary>
        /// Style for a hovered focused title bar button of a WindowGroup.
        /// </summary>
        ActiveTitleBarButtonHot,
        /// <summary>
        /// Style for a hovered unfocused title bar button of a WindowGroup.
        /// </summary>
        InactiveTitleBarButtonHot,
        /// <summary>
        /// Style for a pressed title bar button of a WindowGroup.
        /// </summary>
        ActiveTitleBarButtonPressed,

        /// <summary>
        /// Style for a window group tab container (which contains tabs for each window in a WindowGroup).
        /// </summary>
        WindowGroupTabStripContainer,
        /// <summary>
        /// Style for a WindowGroup tab strip.
        /// </summary>
        WindowGroupTabStrip,

        WindowTab,

        WindowSelectedTab,
    }
}
