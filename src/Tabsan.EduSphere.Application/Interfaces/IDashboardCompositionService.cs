namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// A single composable dashboard widget description.
/// The client renders each widget in <see cref="Order"/> sequence.
/// </summary>
public sealed record WidgetDescriptor(
    string Key,
    string Title,
    string Icon,
    int    Order
);

/// <summary>
/// Determines which dashboard widgets are shown for a given role and institution context.
/// </summary>
public interface IDashboardCompositionService
{
    /// <summary>
    /// Returns the ordered list of widget descriptors appropriate for the caller's
    /// role and institution policy.
    /// </summary>
    IReadOnlyList<WidgetDescriptor> GetWidgets(
        string role,
        InstitutionPolicySnapshot policy);
}
