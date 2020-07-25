namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// Contains dynamic control template types. These can be used to change the whole
    /// appearance of controls, for example if you want to hide or reposition tabs of
    /// the WindowGroup control.
    /// </summary>
    public enum DynamicTemplates
    {
        /// <summary>
        /// WindowGroup control template. A window group is a wrapper around the actual
        /// DockableWindow. A window group contains the title bar and the tab list below.
        /// </summary>
        WindowGroupTemplate,

        DockableWindowTabTemplate,
    }
}
